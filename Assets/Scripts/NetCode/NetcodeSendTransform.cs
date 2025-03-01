using System;
using Unity.Netcode;
using UnityEngine;

public class NetcodeSendTransform : NetworkBehaviour
{
    public Transform modelTransform;
    [HideInInspector] public bool sendPosition = true;
    [HideInInspector] public bool sendRotation = true;
    [HideInInspector] public bool sendScale = false;
    [HideInInspector] public bool useGravity = true;
    
    [HideInInspector] public Vector3 velocity = Vector3.zero; 

    private Team _currentOwnersTeam;
    private Rigidbody _rb;
    private Vector3 previousPosition;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = useGravity;
        
        previousPosition = modelTransform.position;
        NetworkManager.Singleton.OnServerStarted += SpawnListOnHost;
    }

    private void SpawnListOnHost()
    {
        if (IsHost)
        {
            _rb.isKinematic = !useGravity;
            velocity = Vector3.zero;
        }
    }
    
    public void OnGrab()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        ChangeOwnershipServerRpc(clientId);
        
        foreach (PlayerNetwork player in FindObjectsOfType<PlayerNetwork>())
        {
            _currentOwnersTeam = player.CurrentTeam;
        }
        _rb.isKinematic = true;
    }

    // On Release / unselected: set the velocity and allow physics simulation.
    public void OnLettingObjectFallDown()
    {
        if(useGravity)
        {
            _rb.isKinematic = false;
            _rb.velocity = velocity;
        }
        else
        {
            _rb.isKinematic = true;
            _rb.velocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
            previousPosition = transform.position;
            
            UpdateTransformServerRpc(transform.position, transform.rotation, transform.localScale);
            UpdateTransformClientRpc(transform.position, transform.rotation, transform.localScale);
            MoveModelToOwnPosition(transform.position, transform.rotation, transform.localScale);
        }
        else
        {
            _rb.isKinematic = true;
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

    [ServerRpc(RequireOwnership = false)]
    void ChangeOwnershipServerRpc(ulong newOwnerClientId)
    {
        // Change ownership using the built-in method.
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerClientId);

        GrabSettings grabSettings = transform.parent.GetComponent<GrabSettings>();
        grabSettings.lastOwnerId = newOwnerClientId;
    }
}
