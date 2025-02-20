using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class MinigameController : NetworkBehaviour
{
    
    [Header("Settings")]
    public PlayerModes playerModes;
    [Range(0, 1)] public float teamSplitRatio = 0.5f;
    public int minimumPlayerCount;
    public int teamMinimumGameSize;
    
    [Header("Spawn Settings")]
    public Transform[] objectSpawnPoints;
    public List<NetworkObject> spawnableObjects;
    
    [Header("EndGame Requirements")]
    public EndConditionType endCondition;
    
    [SerializeField] private List<GameObject> initialisedObjects;
    
    [Foldout("Events")] public UnityEvent onGameStart;
    [Foldout("Events")] public UnityEvent onGameEnd;
    
    
    [Foldout("End condition variabels")] public float timeLimit;
    [Foldout("End condition variabels")] public float maxScore;
    [Foldout("End condition variabels")] [ReadOnly] [SerializeField] private float currentScore;

    public void InitializeGame()
    {
        onGameStart?.Invoke();
        
        if (endCondition == EndConditionType.TimeBased)
            StartCoroutine(GameLoop());
    }
    

    // ends the game after some time
    private IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(timeLimit);

        EndGameServerRpc();
    }

    [ServerRpc]
    private void EndGameServerRpc()
    {
        // log team won(team team)
        
        onGameEnd?.Invoke();
    }

    private void Update()
    {
        if (endCondition == EndConditionType.TeamBased)
        {
            int teamACount = SceneNetworkManager.Instance.CountPlayersOnTeam(Team.A);
            int teamBCount = SceneNetworkManager.Instance.CountPlayersOnTeam(Team.B);

            if (teamACount < 1)
                EndGameServerRpc();
            if (teamBCount < 1)
                EndGameServerRpc();
        }


        else if (endCondition == EndConditionType.ScoreBased)
        {
            if (currentScore >= maxScore)
            {
                EndGameServerRpc();
            }
        }
    }
}