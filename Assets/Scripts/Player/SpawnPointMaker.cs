using System.Collections.Generic;
using SaintsField;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPointMaker : MonoBehaviour
{
    public static SpawnPointMaker Instance;
    
    [AssetPreview] public GameObject spawnPointPrefab;
    public Transform spawnPointParent;

    public HostList hostList;
    private PreviewMap pm;
    
    /*
     * cor1 & cor2
     * set by the host
     *
     * ┌───cent1───┐
     * │cor1   cor3│
     * │   cent0A  │
     * ├───cent0───┤
     * │   cent0B  │
     * │cor4   cor2│
     * └───cent2───┘
     */
    
    private void Awake()
    {
        Instance = this;
        pm = hostList.GetComponent<PreviewMap>();
    }

    private bool _ran;
    public bool ran {
        get { return _ran; }
        set {
            // Only act if the value really changes
            if (_ran != value) {
                _ran = value;
                hostList.OnSpawnPointRanValueChanged(_ran);
            }
        }
    }
    public bool SpawnSpawnPoint(bool teams = false, int teamCount = -1,  bool reRun = false)
    {
        if (!reRun)
            if (ran) return false;
        
        if (teamCount == -1)
            teamCount = SceneNetworkManager.Instance.ConnectedClientsCount();

        if (teams == false)
            teams = MinigameManager.Instance.GetCurrentController().playerModes == PlayerModes.Teams;

        
        //check if currentgame uses teams EndConditionType endConditionType = MinigameManager.Instance.GetCurrentController().endCondition;
        
        //int playerCount = SceneNetworkManager.Instance.PlayerScripts.Count;
        //int teamCountDead = SceneNetworkManager.Instance.CountPlayersOnTeam(Team.Dead);
        Vector3 cent0 = pm.cent0;
        // pm.cor1;
        // pm.cor2;
        // pm.cor3;
        // pm.cor4;
        // pm.cent1;
        // pm.cent2;
        
        List<Vector3> spawnPoints = new List<Vector3>();
        
        if (teams && teamCount >= 2)
        {
            foreach (Vector3 spawnPoint in CalculateOnTeamSide(Team.A, teamCount)) {
                spawnPoints.Add(spawnPoint);
            }
            foreach (Vector3 spawnPoint in CalculateOnTeamSide(Team.B, teamCount)) {
                spawnPoints.Add(spawnPoint);
            }
        }
        else
        {
            return false;
        }
        
        // Do logic
        // teams == true
        // then spawn on sides
        // else spawn in a array spread out over the area

        SpawnAllPoints(spawnPoints, cent0);
        ran = true;
        return true;
    }

    List<Vector3> CalculateOnTeamSide(Team team, int teamCount)
    {
        List<Vector3> spawnPoints = new List<Vector3>();
        
        // get the amount on one side rounded up. for team a and down for team b 
        if (team == Team.A)
            teamCount = Mathf.CeilToInt(teamCount / 2); 
        if (team == Team.B)
            teamCount = Mathf.FloorToInt(teamCount / 2); 

        if (teamCount == 1)
        {
            spawnPoints.Add(team == Team.A ? pm.cent0A : pm.cent0B); // adds the center of a team side
        }
        else if (teamCount == 2)
        {
            spawnPoints.Add(GetCenter(pm.cent0, team == Team.A ? pm.cor1.Value : pm.cor4.Value)); // adds the center of the quarter of the map
            spawnPoints.Add(GetCenter(pm.cent0, team == Team.A ? pm.cor3.Value : pm.cor2.Value));
        }
        else if (teamCount == 3) // now i did both
        {
            spawnPoints.Add(team == Team.A ? pm.cent0A : pm.cent0B);
            spawnPoints.Add(GetCenter(pm.cent0, team == Team.A ? pm.cor1.Value : pm.cor4.Value)); 
            spawnPoints.Add(GetCenter(pm.cent0, team == Team.A ? pm.cor3.Value : pm.cor2.Value));
        }
        else if (teamCount >= 4)
        {
            /*
            * ┌───cent1───┐cor3
            * │cor1       │
            * │   cent0A Left/Right │corCentL/R
            * │           │
            * ├───cent0───┤centR/L
            */

            Vector3 centR = GetCenter(pm.cor3.Value, pm.cor2.Value);
            Vector3 centL = GetCenter(pm.cor1.Value, pm.cor4.Value);
            Vector3 corCentR = GetCenter(centR, team == Team.A ? pm.cor3.Value : pm.cor2.Value);
            Vector3 corCentL = GetCenter(centL, team == Team.A ? pm.cor1.Value : pm.cor4.Value);
            Vector3 right = GetCenter(corCentR, team == Team.A ? pm.cent0A : pm.cent0B);
            Vector3 left = GetCenter(corCentL, team == Team.A ? pm.cent0A : pm.cent0B);
            
            float dist = Vector3.Distance(right, left);
            float partPerObject = dist / teamCount;

            for (int i = 0; i < teamCount; i++)
            {
                spawnPoints.Add(left + right * (partPerObject * i));
            }
        }
        
        return spawnPoints;
    }

    /// <summary>
    /// Instantiates the prefab at the parent's position and rotates it so that its forward
    /// direction aligns with the provided direction (with Y forced to 0).
    /// </summary>
    /// <param name="targetDirection">The direction to face (will use X and Z only).</param>
    public void SpawnAllPoints(List<Vector3> spawnPoints, Vector3 targetDirection)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            targetDirection.y = 0f;

            if (targetDirection == Vector3.zero)
            {
                Debug.LogWarning("Target direction is zero. Cannot determine rotation.");
                return;
            }

            Quaternion rotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            GameObject instance = Instantiate(spawnPointPrefab, spawnPointParent.position + spawnPoint, rotation, spawnPointParent);
            
            if (instance.GetComponent<DebugRay>() == null)
                instance.AddComponent<DebugRay>();
        }
    }
    
    private Vector3 GetCenter(Vector3 posA, Vector3 posB, float yHight = 0)
    {
        Vector3 posC;
        posC.x = posA.x + (posB.x - posA.x) / 2;
        posC.y = yHight;
        posC.z = posA.z + (posB.z - posA.z) / 2;

        return posC;
    }
}