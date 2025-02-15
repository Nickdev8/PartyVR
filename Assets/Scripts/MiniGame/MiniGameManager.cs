using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class MinigameManager : NetworkBehaviour
{
    public static MinigameManager Instance;

    [SerializeField] private List<GameObject> _minigameQueue = new();
    private GameObject _currentMinigame;
    private MinigameController _currentController;
    
    private void Awake() => Instance = this;

    [ServerRpc]
    public void StartNextGameServerRpc()
    {
        if (_minigameQueue.Count == 0) return;
        
        // Destroy previous minigame
        if (_currentMinigame != null)
        {
            var oldNetworkObject = _currentMinigame.GetComponent<NetworkObject>();
            if (oldNetworkObject != null)
            {
                oldNetworkObject.Despawn();
            }
            Destroy(_currentMinigame);
        }
        
        
        // Spawn new minigame
        _currentMinigame = Instantiate(_minigameQueue[0]);
        var networkObject = _currentMinigame.GetComponent<NetworkObject>();
        networkObject.Spawn();
        _currentController = _currentMinigame.GetComponent<MinigameController>();

        _minigameQueue.RemoveAt(0);
        
        // Cancels miniGame checks are not suffient
        if (!CheckMinigame()){ _currentController.CancelGameServerRpc(); return;}
        
        // Initialize game
        _currentController.InitializeGame();
    }

    bool CheckMinigame()
    {
        //checks if the minigame can be played with the current player count
        if (SceneNetworkManager.Instance.CurrentPlayerIds.Count < _currentController.minimumPlayerCount) {
            Debug.LogError("MinigameManager::StartNextGameServerRpc: Players count is too small");
            return false;
        }
        return true;
    }
    
    [ServerRpc]
    public void CheckPlayerCountServerRpc(Team team)
    {
        foreach (var playerId in SceneNetworkManager.Instance.CurrentPlayerIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            {
                var playerNetwork = client.PlayerObject.GetComponent<PlayerNetwork>();
                if (playerNetwork != null && playerNetwork.CurrentTeam == team)
                {
                    //log that there is still somewone on the team so the game does not have to be ended
                }
            }
        }
    }
}