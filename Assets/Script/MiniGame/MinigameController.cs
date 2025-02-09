using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;
using System.Collections;
using System.Linq;

public class MinigameController : NetworkBehaviour
{
    [Header("Settings")]
    public bool TeamMode;
    [Range(0, 1)] public float TeamSplitRatio = 0.5f;
    public Transform[] SpawnPoints;
    public List<NetworkObject> CustomObjects;
    public float gameTime;

    [Header("Events")]
    public UnityEvent OnGameStart;
    public UnityEvent OnGameEnd;

    private List<TestVRPlayer> _players = new();

    public void InitializeGame()
    {
        AssignPlayers();
        SpawnObjects();
        OnGameStart?.Invoke();
        StartCoroutine(GameLoop());
    }

    private void AssignPlayers()
    {
        // Get all connected VR players
        _players = FindObjectsOfType<TestVRPlayer>().ToList();

        if (TeamMode)
        {
            int teamASize = Mathf.RoundToInt(_players.Count * TeamSplitRatio);
            SplitTeams(teamASize);
        }
    }

    [ServerRpc]
    private void SplitTeams(int teamASize)
    {
        // Shuffle and split players
        _players = _players.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < _players.Count; i++)
        {
            _players[i].SetTeamServer(i < teamASize ? Team.A : Team.B);
        }
    }

    [ServerRpc]
    private void SpawnObjects()
    {
        foreach (var obj in CustomObjects)
        {
            NetworkObject spawnedObj = Instantiate(obj);
            spawnedObj.Spawn();
        }
    }

    private IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(gameTime);

        EndGame();
    }

    [ServerRpc]
    public void EndGame()
    {
        OnGameEnd?.Invoke();
        MinigameManager.Instance.StartNextGameServerRpc();
    }
}