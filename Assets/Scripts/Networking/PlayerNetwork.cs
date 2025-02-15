using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Game Objects")]
    public Logger playerLog;
    //later also add player nametag for color changes
    
    [Header("Player Variables:")]
    // A networked health value (default 100).
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    private readonly NetworkVariable<Team> _currentTeam = new NetworkVariable<Team>();
    public Team CurrentTeam => _currentTeam.Value;
    
    
    [HideInInspector]
    public bool isDead = false;
    
    public override void OnNetworkSpawn()
    {
        // Automatically react to team changes
        _currentTeam.OnValueChanged += OnTeamChanged;
        
        if (IsServer)
        {
            if (SceneNetworkManager.Instance != null)
            {
                SceneNetworkManager.Instance.RegisterPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

                // Optionally assign to a team based on your logic.
                // For example, you might assign by position:
                if (transform.position.x < 0)
                    SetTeamServerRpc(Team.A);
                else
                    SetTeamServerRpc(Team.B);

                Debug.Log($"Added player {gameObject.name} to server");
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
            SceneNetworkManager.Instance.UnregisterPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer)
            return;

        health.Value -= damage;
        if (health.Value <= 0)
        {
            health.Value = 0;
            SetTeamServerRpc(Team.Dead);
        }
    }
    
    
    [ServerRpc]
    public void SetTeamServerRpc(Team team)
    {
        // Server authority - only the server can change the team
        _currentTeam.Value = team;
    }

    private void OnTeamChanged(Team previous, Team current)
    {
        // Update visuals or UI when team changes
        playerLog.AddLog($"Player {OwnerClientId} team changed to {current}");
    }
}

public enum Team { A, B, Dead }
