using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneNetworkManager : NetworkBehaviour
{
    // Singleton instance for easy access.
    public static SceneNetworkManager Instance;

    // Lists to hold all Players, and team-specific Players.
    public Dictionary<ulong, PlayerNetwork> PlayerScripts = new Dictionary<ulong, PlayerNetwork>();
    
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
    public void RegisterPlayer(ulong playerId, PlayerNetwork playerNetwork)
    {
        if (PlayerScripts.TryAdd(playerId, playerNetwork))
        {
            Debug.Log($"Playerid {playerId} & Registered to ClientScripts");
        }
    }


    /// <summary>
    /// Call this method when a player leaves the game or is destroyed.
    /// </summary>
    public void UnregisterPlayer(ulong playerId)
    {
        PlayerScripts.Remove(playerId);
        Debug.Log($"Playerid {playerId} unregistered");
    }
    
    public int CountPlayersOnTeam(Team team)
    {
        int count = 0;
        foreach (ulong playerId in PlayerScripts.Keys)
        {
            if (PlayerScripts.TryGetValue(playerId, out PlayerNetwork playerNetwork))
            {
                if (playerNetwork.CurrentTeam == team)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Messages All the players or the players on a specific team
    /// </summary>
    /// <param name="message"></param>
    public void MessagePlayers(String message)
    {
        foreach (ulong playerId in PlayerScripts.Keys)
        {
            if (PlayerScripts.TryGetValue(playerId, out PlayerNetwork playerNetwork))
            {
                //playerNetwork.logger.LogErrorText(message);
            }
        }
    }
    public void MessagePlayers(String message, Team team)
    {
        foreach (ulong playerId in PlayerScripts.Keys)
        {
            if (PlayerScripts.TryGetValue(playerId, out PlayerNetwork playerNetwork))
            {
                if (playerNetwork.CurrentTeam == team)
                {
                    //playerNetwork.logger.LogErrorText(message);
                }
            }
        }
    }
    public void MessagePlayers(String message, ulong playerId)
    {
        if (PlayerScripts.TryGetValue(playerId, out PlayerNetwork playerNetwork))
        {
            //playerNetwork.logger.LogErrorText(message);
        }
    }
}
