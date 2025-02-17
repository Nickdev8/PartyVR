using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
// This bypases the default custom editor for NetworkTransform
// and lets you modify your custom NetworkTransform's properties
// within the inspector view
[CustomEditor(typeof(TransformMotionController), true)]
public class TransformMotionControllerEditor : Editor
{
}
#endif
public class TransformMotionController : NetworkTransform
{

    public enum AuthorityModes
    {
        Server,
        Owner
    }
    public AuthorityModes AuthorityMode;

    [Range(0.01f, 10.0f)]
    public float MovementSpeed = 3.0f;

    protected Vector2 m_MoveDir;

    protected override bool OnIsServerAuthoritative()
    {
        return AuthorityMode == AuthorityModes.Server;
    }

    protected virtual void OnOwnerUpdate()
    {
        m_MoveDir = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) m_MoveDir.y = +1;
        if (Input.GetKey(KeyCode.S)) m_MoveDir.y = -1;
        if (Input.GetKey(KeyCode.A)) m_MoveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) m_MoveDir.x = +1f;

        // If server authoritative and we are not the server, then send the movement
        // direction to the server. Otherwise, if owner authoritative then it will
        // just be applied and the updates to the transform will be propagated.
        if (OnIsServerAuthoritative() && !IsServer && m_MoveDir.magnitude > 0)
        {
            UpdateMoveDirServerRpc(m_MoveDir);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (IsServer)
            {
                var minClientCount = IsHost ? 2 : 1;
                if (NetworkManager.ConnectedClientsIds.Count >= minClientCount)
                {
                    // Just selecting the first remote client, you can adjust as needed
                    OnSwitchOwner(NetworkManager.ConnectedClientsIds[minClientCount - 1]);
                }
            }
            else
            {
                SwitchOwnerServerRPC();
            }
        }
    }

    protected virtual void OnUpdateMotion()
    {
        if (m_MoveDir.magnitude > 0)
        {
            var motion = Vector3.zero;
            motion.x = m_MoveDir.x;
            motion.z = m_MoveDir.y;
            transform.position += motion * MovementSpeed * Time.deltaTime;
            m_MoveDir = Vector2.zero;
        }
    }

    protected override void Update()
    {
        // If the instance is not spawned, exit early
        if (!IsSpawned)
        {
            return;
        }

        // Owner will control motion and switching of ownership
        if (IsOwner)
        {
            OnOwnerUpdate();
        }

        // Authority (determined by OnIsServerAuthoritative) will apply the motion
        if (CanCommitToTransform)
        {
            OnUpdateMotion();
        }

        // You must invoke the base Update for Interpolation to work properly
        base.Update();
    }

    /// <summary>
    /// Sending only the axis that need to be sent to reduce bandwidth consumption.
    /// Use the ServerRpcParams to get the client identifier of the sender (this is automatically set for you)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    protected void UpdateMoveDirServerRpc(Vector2 moveDir, ServerRpcParams serverRpcParams = default)
    {
        if (serverRpcParams.Receive.SenderClientId != OwnerClientId)
        {
            return;
        }
        m_MoveDir = moveDir;
    }


    [ServerRpc(RequireOwnership = false)]
    protected void SwitchOwnerServerRPC()
    {
        OnSwitchOwner(NetworkManager.ServerClientId);
    }

    protected virtual void OnSwitchOwner(ulong clientId)
    {
        // If already set or not running in the server context, then do nothing
        if (OwnerClientId == clientId || !IsServer)
        {
            return;
        }
        Debug.Log($"Original Owner: {OwnerClientId}");
        NetworkObject.ChangeOwnership(clientId);
        Debug.Log($"New Owner: {OwnerClientId}");
    }
}