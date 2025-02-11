using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneNetworkManager : NetworkBehaviour
{
    // Singleton instance for easy access.
    public static SceneNetworkManager Instance;

    // Lists to hold all players, and team-specific players.
    public List<PlayerNetwork> allPlayers = new List<PlayerNetwork>();
    public List<PlayerNetwork> team1Players = new List<PlayerNetwork>();
    public List<PlayerNetwork> team2Players = new List<PlayerNetwork>();

    // Lists to hold players that have “died” (i.e. health reached 0).
    public List<PlayerNetwork> deadTeam1Players = new List<PlayerNetwork>();
    public List<PlayerNetwork> deadTeam2Players = new List<PlayerNetwork>();

    private void Awake()
    {
        // Create a singleton instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when a player is spawned.
    /// </summary>
    public void RegisterPlayer(PlayerNetwork player)
    {
        if (!allPlayers.Contains(player))
        {
            allPlayers.Add(player);
            Debug.Log($"Player {player.gameObject.name} Registered to AllPlayers");
        }
    }

    /// <summary>
    /// Optionally add players to teams. You can call this from PlayerNetwork.OnNetworkSpawn().
    /// </summary>
    public void AddPlayerToTeam(int teamNumber, PlayerNetwork player)
    {
        if (teamNumber == 1)
        {
            if (!team1Players.Contains(player))
            {
                team1Players.Add(player);
                Debug.Log($"Player {player.gameObject.name} added to Team 1");
            }
        }
        else if (teamNumber == 2)
        {
            if (!team2Players.Contains(player))
            {
                team2Players.Add(player);
                Debug.Log($"Player {player.gameObject.name} added to Team 2");
            }
        }
        else
        {
            Debug.LogError($"Incorrect team number: {teamNumber} for player {player.gameObject.name}");
        }
    }

    /// <summary>
    /// Call this method when a player leaves the game or is destroyed.
    /// </summary>
    public void UnregisterPlayer(PlayerNetwork player)
    {
        allPlayers.Remove(player);
        team1Players.Remove(player);
        team2Players.Remove(player);
        deadTeam1Players.Remove(player);
        deadTeam2Players.Remove(player);
        Debug.Log($"Player {player.gameObject.name} unregistered");
    }

    /// <summary>
    /// Checks every registered player's health.
    /// If a player’s health is zero or below and they are not already marked as dead,
    /// they are added to the appropriate dead team list.
    /// </summary>
    public void CheckPlayerHealth()
    {
        foreach (var player in allPlayers)
        {
            if (player.Health.Value <= 0 && !player.IsDead)
            {
                player.IsDead = true;

                if (team1Players.Contains(player))
                {
                    if (!deadTeam1Players.Contains(player))
                    {
                        team1Players.Remove(player);
                        deadTeam1Players.Add(player);
                        Debug.Log($"Player {player.gameObject.name} moved to Dead Team 1");
                    }
                }
                else if (team2Players.Contains(player))
                {
                    if (!deadTeam2Players.Contains(player))
                    {
                        team2Players.Remove(player);
                        deadTeam2Players.Add(player);
                        Debug.Log($"Player {player.gameObject.name} moved to Dead Team 2");
                    }
                }
                else
                {
                    Debug.LogError($"Player {player.gameObject.name} is not in any team or group");
                }
            }
        }
    }

    private void Update()
    {
        // Only the server should perform health checking.
        if (IsServer)
        {
            CheckPlayerHealth();
        }
    }
}
