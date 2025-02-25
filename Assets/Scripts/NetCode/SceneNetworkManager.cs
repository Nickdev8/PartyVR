using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneNetworkManager : NetworkBehaviour
{
    // Singleton instance for easy access.
    public static SceneNetworkManager Instance;

    // Lists to hold all Players, and team-specific Players.
    //public Dictionary<ulong, PlayerNetwork> PlayerScripts = new Dictionary<ulong, PlayerNetwork>();
    
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

    
    public int CountPlayersOnTeam(Team team)
    {
        // counts the players on a specific team (so i cant use PlayerScripts.Count)
        int count = 0;

        foreach (PlayerNetwork player in FindObjectsOfType<PlayerNetwork>())
        {
            if (player.CurrentTeam == team)
            {
                count++;
            }
        }

        return count;
    }
    public int ConnectedClientsCount()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        return playerCount;
    } 
    
    /// <summary>
    /// Messages All the players or the players on a specific team
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void MessagePlayersRpc(String message)
    {
        foreach (PlayerNetwork player in FindObjectsOfType<PlayerNetwork>())
        {
            player.logger.LogErrorText(message);
        }
    }
}
