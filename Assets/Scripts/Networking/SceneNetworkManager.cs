using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneNetworkManager : NetworkBehaviour
{
    public static SceneNetworkManager Instance;

    // Use a NetworkList that holds a serializable type.
    // Since PlayerNetwork (a MonoBehaviour) isn’t directly serializable,
    // you might instead store a unique identifier (e.g., the player's NetworkObjectId or OwnerClientId)
    public NetworkList<ulong> CurrentPlayerIds;

    private void Awake()
    {
        Instance = this;
        CurrentPlayerIds = new NetworkList<ulong>();
    }

    // Example: A server method to add a player
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ulong playerId)
    {
        if (!CurrentPlayerIds.Contains(playerId))
        {
            CurrentPlayerIds.Add(playerId);
            Debug.Log($"Player with id {playerId} registered");
        }
    }

    // And a corresponding removal method
    [ServerRpc(RequireOwnership = false)]
    public void UnregisterPlayerServerRpc(ulong playerId)
    {
        if (CurrentPlayerIds.Contains(playerId))
        {
            CurrentPlayerIds.Remove(playerId);
            Debug.Log($"Player with id {playerId} unregistered");
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
        foreach (var playerNetwork in GetPlayerNetworks())
        {
            if (playerNetwork.health.Value <= 0 && !playerNetwork.isDead)
            {
                MinigameManager.Instance.CheckPlayerCountServerRpc(playerNetwork.CurrentTeam);
        
                playerNetwork.isDead = true;
                playerNetwork.SetTeamServerRpc(Team.Dead);
        
            }
        }
    }

    public List<PlayerNetwork> GetPlayerNetworks()
    {
        List<PlayerNetwork> playerNetworks = new List<PlayerNetwork>();
        
        foreach (var playerId in CurrentPlayerIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                var playerNetwork = client.PlayerObject.GetComponent<PlayerNetwork>();
                if (playerNetwork != null)
                {
                    playerNetworks.Add(playerNetwork);
                }
            }
        }
        return playerNetworks;
    }
}
