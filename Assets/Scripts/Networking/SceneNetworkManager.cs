using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneNetworkManager : NetworkBehaviour
{
    public static SceneNetworkManager Instance;

    // Use a NetworkList that holds a serializable type.
    // Since PlayerNetwork (a MonoBehaviour) isn’t directly serializable,
    // you might instead store a unique identifier (e.g., the player's NetworkObjectId or OwnerClientId)
    public List<PlayerNetwork> currentPlayerNetworks;

    public int teamSizeA;
    public int teamSizeB;

    private void Awake()
    {
        Instance = this;
    }

    // Example: A server method to add a player
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ulong playerNetworkObjectId)
    {
        // Retrieve the player object using the NetworkManager's SpawnManager.
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var networkObject))
        {
            var player = networkObject.GetComponent<PlayerNetwork>();
            if (player != null && !currentPlayerNetworks.Contains(player))
            {
                currentPlayerNetworks.Add(player);
                Debug.Log($"Player with id {playerNetworkObjectId} registered");
            }
        }
    }


    // And a corresponding removal method
    [ServerRpc(RequireOwnership = false)]
    public void UnregisterPlayerServerRpc(PlayerNetwork player)
    {
        if (currentPlayerNetworks.Contains(player))
        {
            currentPlayerNetworks.Remove(player);
            Debug.Log($"Player with id {player} unregistered");
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
    
    private void CheckPlayerHealth()
    {
        foreach (var playerNetwork in GetPlayerNetworksServerRpc())
        {
            if (playerNetwork.health.Value <= 0 && !playerNetwork.isDead)
            {
                //MinigameManager.Instance.CheckPlayerCountServerRpc(playerNetwork.CurrentTeam);
        
                playerNetwork.isDead = true;
                playerNetwork.SetTeamServerRpc(Team.Dead);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public List<PlayerNetwork> GetPlayerNetworksServerRpc()
    {
        // List<PlayerNetwork> playerNetworks = new List<PlayerNetwork>();
        //
        // foreach (var playerId in currentPlayerNetworks)
        // {
        //     if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
        //     {
        //         var playerNetwork = client.PlayerObject.GetComponent<PlayerNetwork>();
        //         if (playerNetwork != null)
        //         {
        //             playerNetworks.Add(playerNetwork);
        //         }
        //     }
        // }
        
        List<PlayerNetwork> playerNetworks = new List<PlayerNetwork>();

        foreach (var player in currentPlayerNetworks)
        {
            playerNetworks.Add(player);
        }
        
        return playerNetworks;
    }
    
    

    public int PutPlayersInTeams(bool useRandom)
    {
        /*
        if (IsServer)
        {
            teamSizeA = Mathf.CeilToInt(currentPlayerNetworks.Count *
                                        MinigameManager.Instance.currentController.teamSplitRatio);
            teamSizeB = currentPlayerNetworks.Count - teamSizeA;

            if (useRandom)
            {
                List<PlayerNetwork> players = GetPlayerNetworksServerRpc();

                // Shuffle and split players
                players = players.OrderBy(x => Random.value).ToList();

                for (int i = 0; i < players.Count; i++)
                {
                    players[i].SetTeamServerRpc(i < teamSizeA ? Team.A : Team.B);
                }

                return -1;
            }
            else
            {
                Vector3 lineDirection = (HostList.Instance.leftSide - HostList.Instance.rightSide).normalized;
                // Calculate a perpendicular to the line (works for a flat XZ plane)
                Vector3 normal = Vector3.Cross(lineDirection, Vector3.up);

                int currentTeamSizeA = 0;

                foreach (PlayerNetwork player in GetPlayerNetworksServerRpc())
                {
                    // Determine which side of the line the player is on.
                    if (Vector3.Dot(player.transform.position - HostList.Instance.rightSide, normal) > 0)
                    {
                        player.SetTeamServerRpc(Team.A);
                        currentTeamSizeA++;
                    }
                    else
                    {
                        player.SetTeamServerRpc(Team.B);
                    }
                }

                return currentTeamSizeA;
            }
        }
        */
        return 0;
    }
}
