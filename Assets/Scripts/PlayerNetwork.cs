using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Player Variables:")]
    // A networked health value (default 100).
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    
    
    [HideInInspector]
    public bool isDead = false;
    
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

    public override void OnNetworkDespawn()
    {
        if (IsServer && SceneNetworkManager.Instance != null)
        {
            SceneNetworkManager.Instance.UnregisterPlayer(this);
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

        health.Value -= damage;
        if (health.Value <= 0)
        {
            health.Value = 0;
            // Optionally add additional death logic here.
        }
    }
}
