using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;
using System.Collections;

public class MinigameController : NetworkBehaviour
{
    [Header("Settings")]
    public bool teamMode;
    [Range(0, 1)] public float teamSplitRatio = 0.5f;
    public int minimumPlayerCount;
    
    [Header("Spawn Settings")]
    public Transform[] objectSpawnPoints;
    public List<NetworkObject> spawnableObjects;
    
    [Header("EndGame Requirements")]
    public bool useGameTime;
    public float gameTime;
    public bool useTeamDead;
    public int teamMinimumGameSize;
    
    [Header("Events")]
    public UnityEvent onGameStart;
    public UnityEvent onGameEnd;


    public void InitializeGame()
    {
        onGameStart?.Invoke();
        
        if (useGameTime)
            StartCoroutine(GameLoop());
    }
    

    // ends the game after some time
    private IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(gameTime);

        EndGameServerRpc();
    }

    // ends the game
    [ServerRpc]
    private void EndGameServerRpc()
    {
        //if (team == Team.A)
        //    SceneNetworkManager.Instance.MessagePlayers("");
        
        onGameEnd?.Invoke();
    }
    
    //cancel the game (may only be called before initialize game is called)
    [ServerRpc]
    public void CancelGameServerRpc()
    {
        Destroy(this.gameObject);
    }

    [ServerRpc]
    public void PlayerDiedServerRpc()
    {
        if (useTeamDead)
        {
            int teamACount = SceneNetworkManager.Instance.CountPlayersOnTeam(Team.A);
            int teamBCount = SceneNetworkManager.Instance.CountPlayersOnTeam(Team.B);

            if (teamACount < 1)
                EndGameServerRpc();
            if (teamBCount < 1)
                EndGameServerRpc();
        }
    }
}