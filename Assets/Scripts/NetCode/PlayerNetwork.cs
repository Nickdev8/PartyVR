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
        _currentTeam.OnValueChanged += OnTeamChanged;
    }
    
    public void TakeDamage(int damage)
    {
        health.Value -= damage;
        if (health.Value <= 0)
        {
            health.Value = 0;
            SetTeam(Team.Dead);
        }
    }
    
    public void SetTeam(Team team)
    {
        _currentTeam.Value = team;
    }

    private void OnTeamChanged(Team previous, Team current)
    {
        // Update visuals or UI when team changes
        logger.LogErrorText($"Player {OwnerClientId} team changed to {current}");
    }
}