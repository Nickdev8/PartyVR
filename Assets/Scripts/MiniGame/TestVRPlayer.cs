using UnityEngine;
using Unity.Netcode;
/*
 * script is no longer used
 * please use PlayerNetwork.cs
 * 
 */
public class TestVRPlayer : NetworkBehaviour
{
    private readonly NetworkVariable<Team> _currentTeam = new NetworkVariable<Team>();

    public Team CurrentTeam => _currentTeam.Value;

    [ServerRpc]
    public void SetTeamServerRpc(Team team)
    {
        // Server authority - only the server can change the team
        _currentTeam.Value = team;
    }

    public override void OnNetworkSpawn()
    {
        // Automatically react to team changes
        _currentTeam.OnValueChanged += OnTeamChanged;
    }

    private void OnTeamChanged(Team previous, Team current)
    {
        // Update visuals or UI when team changes
        Debug.Log($"Player {OwnerClientId} team changed to {current}"); // TODO: Change to UI stuff later
    }
}