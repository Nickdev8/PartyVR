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

    private List<GameObject> _initialisedObjects;

    public void InitializeGame()
    {
        SpawnObjectsServerRpc();
        onGameStart?.Invoke();
        
        if (useGameTime)
            StartCoroutine(GameLoop());
    }

    [ServerRpc]
    private void SpawnObjectsServerRpc()
    {
        foreach (Transform _transform in objectSpawnPoints)
        {
            //spawns random obj at rand spawnpoint
            int randomIndex = Random.Range(0, objectSpawnPoints.Length);
            GameObject spawnableObj = spawnableObjects[randomIndex].gameObject;
            _initialisedObjects.Add(Instantiate(spawnableObj, _transform.position, _transform.rotation));
        }
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
        onGameEnd?.Invoke();
        
        foreach (GameObject obj in _initialisedObjects)
        {
            Destroy(obj);
        }
        
        MinigameManager.Instance.StartNextGameServerRpc();
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
            int teamACount = 0;
            int teamBCount = 0;
            foreach (PlayerNetwork player in SceneNetworkManager.Instance.GetPlayerNetworksServerRpc())
            {
                if (player.CurrentTeam == Team.A)
                    teamACount++;
                else if (player.CurrentTeam == Team.B)
                    teamBCount++;
            }

            if (teamACount < teamMinimumGameSize || teamBCount < teamMinimumGameSize)
                EndGameServerRpc();
        }
    }
}