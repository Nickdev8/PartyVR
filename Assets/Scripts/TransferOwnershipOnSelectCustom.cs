using UnityEngine;
using Oculus.Interaction;
using Unity.Netcode;

public class TransferOwnershipOnSelectCustom : NetworkBehaviour
{
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
