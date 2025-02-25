using Unity.Netcode;
using UnityEngine;

public class NetcodeSendTransform : NetworkBehaviour
{
    public Transform modelTransform;
    public bool sendPosition = true;
    public bool sendRotation = true;
    public bool sendScale = false;

    public void OnGrab()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        ChangeOwnershipServerRpc(clientId);

        UpdateTransformServerRpc(transform.position, transform.rotation, transform.localScale);
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            UpdateTransformServerRpc(transform.position, transform.rotation, transform.localScale);
            UpdateTransformClientRpc(transform.position, transform.rotation, transform.localScale);
            MoveModelToOwnPosition(transform.position, transform.rotation, transform.localScale);
        }
    }

    void MoveModelToOwnPosition(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        modelTransform.position = newPosition;
        modelTransform.rotation = newRotation;
        modelTransform.localScale = newScale;
    }
    
    [ServerRpc]
    void UpdateTransformServerRpc(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        transform.position = newPosition;
        modelTransform.position = newPosition;
        transform.rotation = newRotation;
        modelTransform.rotation = newRotation;
        transform.localScale = newScale;
        modelTransform.localScale = newScale;
    }

    [ClientRpc]
    void UpdateTransformClientRpc(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        if (IsOwner)
            return;

        if (sendPosition)
        {
            transform.position = newPosition;
            modelTransform.position = newPosition;
        }
        if (sendRotation)
        {
            transform.rotation = newRotation;
            modelTransform.rotation = newRotation;
        }
        if (sendScale)
        {
            transform.localScale = newScale;
            modelTransform.localScale = newScale;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    void ChangeOwnershipServerRpc(ulong newOwnerClientId)
    {
        // Change ownership using the built-in method.
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerClientId);
    }
}
