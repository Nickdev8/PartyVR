using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerNetwork : NetworkBehaviour
{
    [FormerlySerializedAs("playerLog")] [Header("Game Objects")]
    public Logger logger;
    //later also add player nametag for color changes
    
    [Header("Player Variables:")]
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    private readonly NetworkVariable<Team> _currentTeam = new NetworkVariable<Team>();
    public Team CurrentTeam => _currentTeam.Value; // to set use SetTeamServerRpc(Team) to get use this "CurrentTeam"
    
    public override void OnNetworkSpawn()
    {
        // Automatically react to team changes
        _currentTeam.OnValueChanged += OnTeamChanged;
        
        if (SceneNetworkManager.Instance != null)
        {
            SceneNetworkManager.Instance.RegisterPlayer(this);

            Debug.Log($"Added player {gameObject.name} to server");
        }
        else
        {
            Debug.LogError("SceneNetworkManager.Instance is null. Make sure the SceneNetworkManager is in the scene and active.");
        }
        
    }

    public override void OnNetworkDespawn()
    {
        if (SceneNetworkManager.Instance != null)
        {
            SceneNetworkManager.Instance.UnregisterPlayer();
        }
    }
    
    public void TakeDamage(int damage)
    {
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
        logger.LogErrorText($"Player {OwnerClientId} team changed to {current}");
    }
}