using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Meta.XR.MultiplayerBlocks.NGO;
using Unity.Netcode.Components;

public class TransferOwnershipOnSelectCustom : NetworkBehaviour
{
    public NetworkTransform networkTransform;
    
    private void Update()
    {
        // Only allow the owner to control the transform
        if (!IsOwner || IsServer) return;
        

        // Send the updated transform to the server periodically or when significant changes occur.
        // You might want to throttle these calls in a real project.
        UpdateTransformServerRpc(transform.position, transform.rotation);
    }

    [ServerRpc]
    void UpdateTransformServerRpc(Vector3 newPosition, Quaternion newRotation)
    {
        // Optionally, you could do some validation here

        // Update the transform on the server
        transform.position = newPosition;
        transform.rotation = newRotation;

        // Let the NetworkTransform component know about the change (if needed)
        networkTransform.Teleport(newPosition, newRotation, transform.localScale);
        SceneNetworkManager.Instance.MessagePlayers(
            $"Set position: {newPosition}");
        
    }

    public void HandleSelect()
    {
        if (!IsOwner)
        {
            RequestOwnershipServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        OnGrabClientRpc();
    }

    [ClientRpc]
    void OnGrabClientRpc()
    {
        SceneNetworkManager.Instance.MessagePlayers("Grab ownership updated across clients.");
    }

    public void HandleUnselect()
    {
        if (IsOwner)
        {
            // Force the final state update as a teleport (immediate sync)
            networkTransform.SetState(transform.position, transform.rotation, transform.localScale, true);
            // Delay releasing ownership by one frame to ensure the state update propagates
            StartCoroutine(DelayedReleaseOwnership());
        }
    }

    private IEnumerator DelayedReleaseOwnership()
    {
        yield return null; // wait one frame
        ReleaseOwnershipServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ReleaseOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        NetworkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        OnReleaseClientRpc();
        SceneNetworkManager.Instance.MessagePlayers("Changed ownership to " + NetworkManager.Singleton.LocalClientId);
    }

    [ClientRpc]
    void OnReleaseClientRpc()
    {
        SceneNetworkManager.Instance.MessagePlayers("Object released and ownership reset.");
    }
}
