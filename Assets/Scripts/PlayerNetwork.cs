using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // A networked health value (default 100).
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);

    // Prefab to be instantiated.
    public GameObject ListPrefab;

    // Flag to avoid processing death multiple times.
    [HideInInspector]
    public bool IsDead = false;

    // Reference to the OVRCameraRig (assumed to be in the scene).
    private OVRCameraRig _cameraRig;

    // Holds the instantiated ListPrefab.
    private GameObject listInstance;

    private void Awake()
    {
        // Finds the first OVRCameraRig in the scene.
        _cameraRig = FindAnyObjectByType<OVRCameraRig>();
    }

    // Called once the network object is fully spawned.
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (SceneNetworkManager.Instance != null)
            {
                SceneNetworkManager.Instance.RegisterPlayer(this);

                // Optionally assign to a team based on your logic.
                // For example, you might assign by position:
                if (transform.position.x < 0)
                    SceneNetworkManager.Instance.AddPlayerToTeam(1, this);
                else
                    SceneNetworkManager.Instance.AddPlayerToTeam(2, this);

                Debug.LogWarning($"Added player {gameObject.name} to server");
            }
            else
            {
                Debug.LogError("SceneNetworkManager.Instance is null. Make sure the SceneNetworkManager is in the scene and active.");
            }
        }
    }

    /// <summary>
    /// Reduces the player's health by the given damage amount.
    /// This should only be called on the server.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!IsServer)
            return;

        Health.Value -= damage;
        if (Health.Value <= 0)
        {
            Health.Value = 0;
            // Optionally add additional death logic here.
        }
    }

    // Called when the network object is despawned.
    public override void OnNetworkDespawn()
    {
        if (IsServer && SceneNetworkManager.Instance != null)
        {
            SceneNetworkManager.Instance.UnregisterPlayer(this);
        }
    }

    private void Update()
    {
        DoListOnHost();
    }

    /// <summary>
    /// Instantiates the ListPrefab as a child of _cameraRig.rightControllerAnchor if not already instantiated.
    /// This runs only on the server.
    /// </summary>
    private void DoListOnHost()
    {
        if (IsServer)
        {
            // Only instantiate once.
            if (listInstance == null)
            {
                if (_cameraRig != null && _cameraRig.rightControllerAnchor != null)
                {
                    // Instantiate ListPrefab as a child of rightControllerAnchor.
                    listInstance = Instantiate(ListPrefab, _cameraRig.rightControllerAnchor);

                    // Reset local position/rotation if needed.
                    listInstance.transform.localPosition = Vector3.zero;
                    listInstance.transform.localRotation = Quaternion.identity;

                    Debug.Log("ListPrefab instantiated as child of rightControllerAnchor.");
                }
                else
                {
                    Debug.LogError("OVRCameraRig or rightControllerAnchor is null.");
                }
            }
        }
    }

    public void NextInHostsList()
    {
        Debug.Log("Pressed A");
    }

    /// <summary>
    /// Provides access to the instantiated list instance.
    /// </summary>
    public GameObject GetListInstance()
    {
        return listInstance;
    }
}
