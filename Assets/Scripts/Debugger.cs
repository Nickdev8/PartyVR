using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;

public class Debugger : NetworkBehaviour
{
    public int previewNumberPlayers;
    private HostList _hostList;

    private void Awake()
    {
        if (IsHost)
            _hostList = FindAnyObjectByType<HostList>();
    }

    [Button("SpawnSpawnPoints")]
    public void SpawnSpawnPoints()
    {
        if (IsHost)
        {
             SceneNetworkManager.Instance.MessageThisPlayer($"HostList is {_hostList} with {previewNumberPlayers}");
             //_hostList.spawnPointMaker.SpawnSpawnPoint(List<Vector3>, Vector3.forward * 5);
        }
    }
}