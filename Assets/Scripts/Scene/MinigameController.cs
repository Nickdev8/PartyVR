using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using SaintsField;

public class MinigameController : NetworkBehaviour
{
    public UnityEvent onGameStart;
    public UnityEvent onGameEnd;
    
    [Header("Settings")]
    public PlayerModes playerModes;
    [Range(0, 1)] public float teamSplitRatio = 0.5f;
    public int minimumPlayerCount;
    
    [Header("SpawnSettings")]
    public Transform[] objectSpawnPoints;
    public List<NetworkObject> spawnableObjects;
    
    [SerializeField] private List<GameObject> initialisedObjects;
    
    [Header("EndGameRequirements")]
    public EndConditionType endCondition;
    
    [OptionalField] public float timeLimit;
    [OptionalField] public int teamMinimumGameSize;
    [OptionalField] public float maxScore;
    [ReadOnly] [SerializeField] private float currentScore;

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

        EndGameRpc();
    }

    [Rpc(SendTo.Server)]
    private void EndGameRpc()
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
                EndGameRpc();
            if (teamBCount < 1)
                EndGameRpc();
        }


        else if (endCondition == EndConditionType.ScoreBased)
        {
            if (currentScore >= maxScore)
            {
                EndGameRpc();
            }
        }
    }
}