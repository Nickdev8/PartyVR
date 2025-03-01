using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using SaintsField;
using System.Runtime.Serialization;

public class MinigameController : NetworkBehaviour
{
    [Header("Events")]
    public UnityEvent onGameStart;
    public UnityEvent onGameEnd;
    public UnityEvent onGamePaused;
    public UnityEvent onGameResumed;
    public UnityEvent onScoreUpdated;
    public UnityEvent onPlayerJoined;
    public UnityEvent onPlayerLeft;
    
    [Header("Game Settings")]
    public PlayerModes playerModes;
    [Range(0, 1)] public float teamSplitRatio = 0.5f;
    public int minimumPlayerCount;

    [Header("Spawn Settings")]
    public Transform[] objectSpawnPoints;
    public List<NetworkObject> spawnableObjects;
    [SerializeField] private List<GameObject> initialisedObjects;

    [Header("End Game Requirements")]
    public EndConditionType endCondition;
    public float timeLimit;
    public int teamMinimumGameSize;
    public float maxScore;
    
    [ReadOnly] 
    [SerializeField] private float currentScore;

    [Header("Game State")]
    [SerializeField] private bool isGameRunning = false;
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private float currentTime = 0f;
    // Dictionary to track individual player scores (key: clientId, value: score)
    private Dictionary<ulong, float> playerScores = new Dictionary<ulong, float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Subscribe to connection events for managing players
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        }
    }

    // Called when a new player connects
    private void OnPlayerConnected(ulong clientId)
    {
        if (!playerScores.ContainsKey(clientId))
        {
            playerScores.Add(clientId, 0f);
        }
        onPlayerJoined?.Invoke();
    }

    // Called when a player disconnects
    private void OnPlayerDisconnected(ulong clientId)
    {
        if (playerScores.ContainsKey(clientId))
        {
            playerScores.Remove(clientId);
        }
        onPlayerLeft?.Invoke();
    }

    public void InitializeGame()
    {
        onGameStart?.Invoke();
        isGameRunning = true;
        currentTime = 0f;
        currentScore = 0f;
        // Spawn initial objects at designated spawn points
        SpawnInitialObjects();

        if (endCondition == EndConditionType.TimeBased)
        {
            StartCoroutine(GameLoop());
        }
    }

    // Spawns objects randomly at each spawn point
    private void SpawnInitialObjects()
    {
        if (spawnableObjects.Count == 0 || objectSpawnPoints.Length == 0)
            return;

        foreach (Transform spawnPoint in objectSpawnPoints)
        {
            int randomIndex = Random.Range(0, spawnableObjects.Count);
            NetworkObject obj = Instantiate(spawnableObjects[randomIndex], spawnPoint.position, spawnPoint.rotation);
            obj.Spawn();
            initialisedObjects.Add(obj.gameObject);
        }
    }

    // Main game loop for time-based games
    private IEnumerator GameLoop()
    {
        while (isGameRunning && currentTime < timeLimit)
        {
            if (!isGamePaused)
            {
                currentTime += Time.deltaTime;
            }
            yield return null;
        }
        EndGameRpc();
    }

    // Called via RPC to end the game on the server
    [Rpc(SendTo.Server)]
    private void EndGameRpc()
    {
        isGameRunning = false;
        onGameEnd?.Invoke();
        CleanupGame();
    }

    // Clean up objects and reset state after game ends
    private void CleanupGame()
    {
        foreach (GameObject obj in initialisedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        initialisedObjects.Clear();
        playerScores.Clear();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isGameRunning || isGamePaused)
            return;

        switch (endCondition)
        {
            case EndConditionType.TeamBased:
                CheckTeamBasedEnd();
                break;
            case EndConditionType.ScoreBased:
                CheckScoreBasedEnd();
                break;
        }
    }

    // Ends the game if any team falls below the minimum required players
    private void CheckTeamBasedEnd()
    {
        int teamACount = SceneNetworkManager.Instance.CountPlayersOnTeam(Team.A);
        int teamBCount = SceneNetworkManager.Instance.CountPlayersOnTeam(Team.B);

        if (teamACount < teamMinimumGameSize || teamBCount < teamMinimumGameSize)
        {
            EndGameRpc();
        }
    }

    // Ends the game if the overall score reaches the maximum limit
    private void CheckScoreBasedEnd()
    {
        if (currentScore >= maxScore)
        {
            EndGameRpc();
        }
    }

    // Public method to add score to the game and trigger an event update
    public void AddScore(float scoreToAdd)
    {
        if (!isGameRunning)
            return;

        currentScore += scoreToAdd;
        onScoreUpdated?.Invoke();

        if (endCondition == EndConditionType.ScoreBased)
        {
            CheckScoreBasedEnd();
        }
    }

    // Networked method to update an individual player's score
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public void UpdatePlayerScoreRpc(ulong clientId, float scoreDelta)
    {
        if (playerScores.ContainsKey(clientId))
        {
            playerScores[clientId] += scoreDelta;
            // Optionally update the overall game score too
            currentScore += scoreDelta;
            onScoreUpdated?.Invoke();
        }
    }

    // Pause the game, stopping the game loop and invoking the pause event
    public void PauseGame()
    {
        if (!isGameRunning)
            return;
        isGamePaused = true;
        onGamePaused?.Invoke();
    }

    // Resume the game from a paused state and invoke the resume event
    public void ResumeGame()
    {
        if (!isGameRunning)
            return;
        isGamePaused = false;
        onGameResumed?.Invoke();
    }

    // Reset the game state so a new round can begin
    public void ResetGame()
    {
        StopAllCoroutines();
        isGameRunning = false;
        isGamePaused = false;
        currentTime = 0f;
        currentScore = 0f;
        playerScores.Clear();
        CleanupGame();
    }
}
