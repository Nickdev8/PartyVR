using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class MinigameManager : NetworkBehaviour
{
    public static MinigameManager Instance;

    [SerializeField] private List<GameObject> _minigameQueue = new();
    private GameObject _currentMinigame;
    public MinigameController currentController;
    
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
        currentController = _currentMinigame.GetComponent<MinigameController>();

        _minigameQueue.RemoveAt(0);
        
        // Cancels miniGame checks are not suffient
        if (!CheckMinigame()){ currentController.CancelGameServerRpc(); return;}
        
        // Initialize game
        currentController.InitializeGame();
    }

    bool CheckMinigame()
    {
        //checks if the minigame can be played with the current player count
        if (SceneNetworkManager.Instance.currentPlayerNetworks.Count < currentController.minimumPlayerCount) {
            Debug.LogError("MinigameManager::StartNextGameServerRpc: Players count is too small");
            return false;
        }
        return true;
    }
    
    
    //called from SceneNetworkManager.cs when player dies
    [ServerRpc]
    public void CheckPlayerCountServerRpc(Team team)
    {
        foreach (PlayerNetwork player in SceneNetworkManager.Instance.currentPlayerNetworks)
        {
            if (player.CurrentTeam == team)
            {
                //log that there is still someone on the team so the game does not have to be ended
                // logs to all the clients on a team
                player.playerLog.AddLog($"A player was eliminated at team {team}");
                currentController.PlayerDiedServerRpc();
            }
        }
    }
}