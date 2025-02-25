using Unity.Netcode;
using UnityEngine;

public class NetcodeSendTransform : NetworkBehaviour
{
    public Transform modelTransform;
    public bool sendPosition = true;
    public bool sendRotation = true;
    public bool sendScale = false;

    // Called when a client “grabs” the object.
    public void OnGrab()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        // Request the server to change ownership.
        ChangeOwnershipServerRpc(clientId);

        // Immediately update the transform state.
        UpdateTransformServerRpc(transform.position, transform.rotation, transform.localScale);
    }

    private void FixedUpdate()
    {
        // Only the owner sends transform updates.
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
    // ServerRpc called by the owner to update the transform.
    [ServerRpc]
    void UpdateTransformServerRpc(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        // Optionally update the server’s copy if needed.
        transform.position = newPosition;
        modelTransform.position = newPosition;
        transform.rotation = newRotation;
        modelTransform.rotation = newRotation;
        transform.localScale = newScale;
        modelTransform.localScale = newScale;
    }

    // ClientRpc to update non-owners with the new transform values.
    [ClientRpc]
    void UpdateTransformClientRpc(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        // The owner already has the correct transform.
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

    // ServerRpc to change ownership of this network object.
    [ServerRpc (RequireOwnership = false)]
    void ChangeOwnershipServerRpc(ulong newOwnerClientId)
    {
        // Change ownership using the built-in method.
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerClientId);
    }
}
