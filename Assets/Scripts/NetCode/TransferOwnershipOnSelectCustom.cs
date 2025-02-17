using System;
using Meta.XR.MultiplayerBlocks.NGO;
using UnityEngine;
using Oculus.Interaction;
using Unity.Netcode;

public class TransferOwnershipOnSelectCustom : NetworkBehaviour
{
    public ClientNetworkTransform networkTransform;
    //public int 
    //The following ownership permission settings, defined by NetworkObject.OwnershipStatus, are only available when running in distributed authority mode:

    //     None: Ownership of this NetworkObject is considered static and can't be redistributed, requested, or transferred (a Player would have this, for example).
    //     Distributable: Ownership of this NetworkObject is automatically redistributed when a client joins or leaves, as long as ownership is not locked or a request is pending.
    //     Transferable: Ownership of this NetworkObject can be transferred immediately, as long as ownership is not locked and there are no pending requests.
    //     RequestRequired: Ownership of this NetworkObject must be requested before it can be transferred and will always be locked after transfer.
    

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
        Debug.Log("Grab ownership updated across clients.");
    }

    public void HandleUnselect()
    {
        if (IsOwner)
        {
            networkTransform.SetState(transform.position, transform.rotation, transform.localScale, false);
            ReleaseOwnershipServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ReleaseOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        NetworkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        OnReleaseClientRpc();
    }

    [ClientRpc]
    void OnReleaseClientRpc()
    {
        Debug.Log("Object released and ownership reset.");
    }
}
