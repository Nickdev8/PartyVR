using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomCollision : NetworkBehaviour
{
    public GameObject borderParticlePrefab;
    public BoxCollider floorCollider;
    public float wallHeight = 3f;
    public float wallThickness = 0.1f;
    public float margin  = 0.1f;

    [HideInInspector] public GameObject borderParticleInstance;
    [HideInInspector] public List<GameObject> currentWalls;
    
    /*
     * cor1 & cor2
     * set by the host
     *
     * ┌───cent1───┐
     * │cor1   cor3│
     * │   cent0A  │
     * ├───cent0───┤left - right
     * │   cent0B  │
     * │cor4   cor2│
     * └───cent2───┘
     */
    
    [Rpc(SendTo.Everyone)]
    public void MakeRoomCollisionRpc(Vector3 cor1, Vector3 cor2, Vector3 cor3, Vector3 cor4) 
    {
        
        CreateWall(cor1, cor3);
        CreateWall(cor3, cor2);
        CreateWall(cor2, cor4);
        CreateWall(cor4, cor1);
        
        float distance = Vector3.Distance(cor1, cor2) + margin;
        Vector3 floorPosition = GetCenter(cor1, cor2);
        floorPosition.y -= wallThickness/2;
        floorCollider.size = new Vector3(distance, wallThickness, distance);
        floorCollider.transform.position = floorPosition;
        
        
        if (borderParticleInstance != null)
            Destroy(borderParticleInstance);
    
        Vector3 cent1 = GetCenter(cor1, cor3);
        cent1.y = floorPosition.y;
        
        Vector3 direction = (cent1 - floorPosition).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        distance = (Vector3.Distance(cor1, cor3) + margin);
    
        borderParticleInstance = Instantiate(borderParticlePrefab, floorPosition, rotation);
        var sh = borderParticleInstance.GetComponent<ParticleSystem>().shape;
        sh.scale = new Vector3(distance, 0.1f, distance);
        //borderParticleInstance.GetComponent<ParticleSystem>().shape = sh;
    }
    
    private void CreateWall(Vector3 start, Vector3 end)
    {
        Vector3 difference = end - start;
        float wallLength = difference.magnitude;

        if (Mathf.Approximately(wallLength, 0f))
        {
            Debug.LogWarning("Cannot create a wall with zero length. Check your input vectors.");
            return;
        }

        Vector3 wallCenter = (start + end) / 2f;
        GameObject wall = new GameObject("Wall");
        wallCenter.y = wallHeight/2 - margin;
        wall.transform.position = wallCenter;
    
        Vector3 direction = difference / wallLength; // normalized direction
        wall.transform.rotation = Quaternion.LookRotation(direction);
    
        BoxCollider collider = wall.AddComponent<BoxCollider>();
        // z = length, y = height, x = thickness
        collider.size = new Vector3(wallThickness, wallHeight + margin, wallLength + margin);
    
        wall.transform.parent = transform;
        currentWalls.Add(wall);
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