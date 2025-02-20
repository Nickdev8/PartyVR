using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Meta.XR.MultiplayerBlocks.NGO;
using Unity.Netcode.Components;
using Oculus.Interaction;

public class TransferOwnershipOnSelectCustom : NetworkBehaviour
{
    public NetworkObject networkObject;
    public ClientNetworkTransform NetworkTransform;
    private Rigidbody _rb;

    private void Start()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
    }

    public void EventPickup()
    {
        //if (isOwned == false)
        //{
            ResetInteractableVelocity();
            PickupServerRpc();
        //}
    }
    
    public void EventDrop()
    {
        // technically dont need to pass auth when dropping, only remove auth when another player grabs, or current grabber disconnects
        // doing it this way stops jitter when passing auth of moving objects (due to ping difference of positions)
        ///*
        if (IsOwner == true)
        {
            //ResetInteractableVelocity();
            DropServerRpc();
            //_rb.velocity,_rb.angularVelocity
        }
        //*/
    }
    [ServerRpc(RequireOwnership = false)]
    private void PickupServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ResetInteractableVelocity();

        var clientId = serverRpcParams.Receive.SenderClientId;
        networkObject.ChangeOwnership(clientId);
        SceneNetworkManager.Instance.MessagePlayers("You Lost ownership of this object " + networkObject.OwnerClientId, clientId);
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropServerRpc(ServerRpcParams serverRpcParams = default) //Vector3 _velocity, Vector3 _angualarVelocity,
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        
        SceneNetworkManager.Instance.MessagePlayers("You Lost ownership of this object " + networkObject.OwnerClientId, clientId);

        ResetInteractableVelocity();
        networkObject.RemoveOwnership();
        
        SceneNetworkManager.Instance.MessagePlayers("You now own this object " + networkObject.OwnerClientId, networkObject.OwnerClientId);
    }

    private void ResetInteractableVelocity()
    {
        if (_rb.isKinematic)
            return;
        
        // Unitys interactable types need some adjustments to stop them behaving weird over network
        // Without this you may notice some pickups rapidly fall through the floor
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // we can use this check apply different behaviour depending on interactable type
        // if (xRGrabInteractable.movementType == XRBaseInteractable.MovementType.VelocityTracking) { }
    }
}