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

        // Initialize game
        _currentController.InitializeGame();
    }
}