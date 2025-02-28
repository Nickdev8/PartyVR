using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using SaintsField;
using UnityEngine.Serialization;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    public List<GameObject> minigameQueue;
    [NonReorderable] [SerializeField] private List<GameObject> doneGames;
    
    [ReadOnly] public GameObject currentMinigame;
    [ReadOnly] public MinigameController currentController;
    
    public NetworkVariable<int> onSideA;
    public NetworkVariable<int> onSideB;
    public bool wrongTeamRatio;

    private void Awake() => Instance = this;

    public void StartNextGame()
    {
        wrongTeamRatio = false; // resets for next check
        
        if (minigameQueue.Count == 0)
        {
            ResetAllMinigames(); return;
        }

        // Destroy previous minigame
        if (currentMinigame != null)
        {
            var oldNetworkObject = currentMinigame.GetComponent<NetworkObject>();
            if (oldNetworkObject != null)
            {
                oldNetworkObject.Despawn();
            }
            Destroy(currentMinigame);
            minigameQueue.RemoveAt(0);
        }
        
        
        // Cancels miniGame checks are not sufficient
        if (!CheckMinigame())
        {
            CancelGameServerRpc(); 
            doneGames.Add(minigameQueue[0]);
            minigameQueue.RemoveAt(0);
            StartNextGame(); 
            return;
        }

        // Spawn new minigame
        
        currentMinigame = Instantiate(minigameQueue[0]);
        NetworkObject networkObject = currentMinigame.GetComponent<NetworkObject>();
        SpawnMinigameRpc(networkObject);
        currentController = currentMinigame.GetComponent<MinigameController>();

        doneGames.Add(minigameQueue[0]);
    }

    [Rpc(SendTo.Everyone)]
    void SpawnMinigameRpc(NetworkObject networkObject)
    {
        networkObject.Spawn();
    }
    
    bool CheckMinigame()
    {
        //checks if the minigame can be played with the current player count
        if (SceneNetworkManager.Instance.ConnectedClientsCount() < currentController.minimumPlayerCount) {
            Debug.LogError("MinigameManager::StartNextGameServerRpc: Players count is too small");
            return false;
        }
        //
        // add more checks if needed
        //
        
        return true;
    }
    
    private void CancelGameServerRpc()
    {
        var oldNetworkObject = currentController.GetComponent<NetworkObject>();
        if (oldNetworkObject != null)
        {
            oldNetworkObject.Despawn();
        }
        
        Destroy(currentController.gameObject);
    }

    private void ResetAllMinigames()
    {
        foreach (GameObject minigame in doneGames)
        {
            minigameQueue.Add(minigame);
        }
        doneGames.Clear();
    }

    public MinigameController GetCurrentController()
    {
        if (currentController == null)
            return null;
        
        return currentController;
        
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void GetTeamsOnSideRpc(Team team)
    {
        foreach (PlayerNetwork player in FindObjectsOfType<PlayerNetwork>())
        {
            var A = PreviewMap.Instance.left.Value;
            var B = PreviewMap.Instance.right.Value;

            var playerPos = player.logger.cameraAnchor.position;
            
            var lineDir = (B - A).normalized;
            var toPlayer = playerPos - A;

            float cross = Vector3.Cross(lineDir, toPlayer).y;

            if (cross > 0)
            {
                onSideA.Value++;
                player.SetTeam(Team.A);
            }
            else
            {
                onSideB.Value++;
                player.SetTeam(Team.B);
            }
        }
    }

    public IEnumerator BlinkImageForTeamDivition(SpawnPointMaker spawnPointMaker, ImageRenderer imageRenderer)
    {
        while (spawnPointMaker.ran)
        {
            if (currentMinigame == null)
                yield break;
            if (currentController.playerModes == PlayerModes.Teams)
            {
                GetTeamsOnSideRpc(Team.A);
                GetTeamsOnSideRpc(Team.B);

                float actualRatio = (float)onSideA.Value / (onSideA.Value + onSideB.Value);

                if (Mathf.Approximately(actualRatio, currentController.teamSplitRatio))
                {
                    wrongTeamRatio = false;
                    imageRenderer.BlinkImage(); 
                    //do the right image here for if teams are correnct and can continue
                }
                else
                {
                    wrongTeamRatio = true;
                    imageRenderer.BlinkImage();
                    //do the right image here for if teams are Not correnct
                }

            }
            
            // runs the spawnPoint maker and returns if successful. if not, rerun
            // spawnPointMaker.ran = spawnPointMaker.SpawnSpawnPoint();
            
            yield return new WaitForSeconds(0.1f);
        }
        
        yield break;
    }
}