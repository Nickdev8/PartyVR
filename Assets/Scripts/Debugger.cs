using System;
using System.Collections.Generic;
using SaintsField;
using SaintsField.Playa;
using Unity.Netcode;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    [Dropdown] public HostList hostList;
    public int previewNumberPlayers;
    
    private void Awake()
    {
        // _networkObject = GetComponent<NetworkObject>();
    }

    [Button]
    public void SpawnSpawnPoints()
    {
        if (hostList.isActiveAndEnabled || !NetworkManager.Singleton.IsHost)
            return;
        
        // only runs when there is a hostList and its active
        SceneNetworkManager.Instance.MessageThisPlayerRpc($"HostList is {hostList} with {previewNumberPlayers}");
        hostList.spawnPointMaker.SpawnSpawnPoint(true, previewNumberPlayers, true);
    }

    [Button] [Rpc(SendTo.Server)]
    public void WriteTestToHostRpc()
    {
        SceneNetworkManager.Instance.MessageThisPlayerRpc("this is the host");
    }
    [Button] [Rpc(SendTo.NotMe)]
    public void WriteTestNotMeRpc()
    {
        SceneNetworkManager.Instance.MessageThisPlayerRpc("this is not the sender");
    }
}