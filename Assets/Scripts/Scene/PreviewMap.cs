using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using SaintsField;

public class PreviewMap : NetworkBehaviour
{
    public static PreviewMap Instance;
    
    [AssetPreview] public HostList hostList;
    
    //stuff to spawn/inistantiate
    public GameObject corner1Prefab;
    public GameObject corner2Prefab;
    public GameObject sceneCenterPrefab;
    public GameObject linePrefab;
    public Transform lineContainer;

    //the gb that are on the hand itself
    private GameObject _corner1Instance;
    private GameObject _corner2Instance;
    
    public float lineBlockLengthCm = 10;
    public float lineBlockwidthCm = 1;
    public float lineBlockHeightCm = 0.1f;
    public float lineGapCm = 5;
    
    [HideInInspector] public NetworkVariable<Vector3> cor1;
    [HideInInspector] public NetworkVariable<Vector3> cor2;
    [HideInInspector] public NetworkVariable<Vector3> cor3;
    [HideInInspector] public NetworkVariable<Vector3> cor4;
    
    [HideInInspector] public NetworkVariable<Vector3> left;
    [HideInInspector] public NetworkVariable<Vector3> right;
    
    [HideInInspector] public Vector3 cent0;
    [HideInInspector] public Vector3 cent1;
    [HideInInspector] public Vector3 cent2;
    [HideInInspector] public Vector3 cent0A;
    [HideInInspector] public Vector3 cent0B;
        
    //the gb that is underneath the hand and is still there after the next step
    private GameObject _oldCorner1Instance;
    private GameObject _oldCorner2Instance;
    
    private GameObject _sceneCenterInstance;
    private GameObject _corner3Instance;
    private GameObject _corner4Instance;

    
    /*
     * cor1 & cor2
     * set by the host
     *
     * ┌───cent1───┐
     * │cor1   cor3│
     * │   cent0A  │
     * ├───cent0───┤ Left - Right
     * │   cent0B  │
     * │cor4   cor2│
     * └───cent2───┘
     */
    
    private void Awake()
    {
        // Create a singleton instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        MakeDisplayLineRpc(cor1.Value, cor3.Value);
        MakeDisplayLineRpc(cor3.Value, cor2.Value);
        MakeDisplayLineRpc(cor2.Value, cor4.Value);
        MakeDisplayLineRpc(cor4.Value, cor1.Value);
        MakeDisplayLineRpc(GetCenter(cor1.Value, cor4.Value), GetCenter(cor2.Value, cor3.Value));
    }


    public void Calculate()
    {
        if (_sceneCenterInstance == null) // makes a new instance to display to the host where the center is
            _sceneCenterInstance = Instantiate(sceneCenterPrefab, cor1.Value, new Quaternion()); 
        
        // makes a new corner to display to the host where the corners are
        if (_corner3Instance == null) 
            _corner3Instance = Instantiate(sceneCenterPrefab, cor3.Value, new Quaternion()); 
        if (_corner4Instance == null) 
            _corner4Instance = Instantiate(sceneCenterPrefab, cor4.Value, new Quaternion()); 

        cent0 = GetCenter(cor1.Value, cor2.Value);
        cent1 = GetCenter(cor1.Value, cor3.Value);
        cent2 = GetCenter(cor4.Value, cor2.Value);
        cent0A = GetCenter(cent0, cent1);
        cent0B = GetCenter(cent0, cent2);
        
        
        // Assuming cor1 and cor2 are diagonal corners and lie on the same horizontal plane.
        Vector3 mid = (cor1.Value + cor2.Value) * 0.5f;
        Vector3 halfDiagonal = (cor2.Value - cor1.Value) * 0.5f;
        Vector3 perpendicular = new Vector3(-halfDiagonal.z, 0, halfDiagonal.x);
        cor3.Value = mid + perpendicular;
        cor4.Value = mid - perpendicular;

        
        _sceneCenterInstance.transform.position = cent0;
        _corner3Instance.transform.position = cor3.Value;
        _corner4Instance.transform.position = cor4.Value;

        ClearDisplayLinesRpc();
        MakeDisplayLineRpc(cor1.Value, cor3.Value);
        MakeDisplayLineRpc(cor3.Value, cor2.Value);
        MakeDisplayLineRpc(cor2.Value, cor4.Value);
        MakeDisplayLineRpc(cor4.Value, cor1.Value);
        MakeDisplayLineRpc(GetCenter(cor1.Value, cor4.Value), GetCenter(cor2.Value, cor3.Value)); //centerline
    }

    private Vector3 GetCenter(Vector3 posA, Vector3 posB, float yHight = 0)
    {
        Vector3 posC;
        posC.x = posA.x + (posB.x - posA.x) / 2;
        posC.y = yHight;
        posC.z = posA.z + (posB.z - posA.z) / 2;

        return posC;
    }

    public void UpdateHandLogic(int currentHostListPosition)
    {
        if (currentHostListPosition == 0)
        {
            if (_corner1Instance != null) Destroy(_corner1Instance);
            if (_oldCorner1Instance != null) Destroy(_oldCorner1Instance);
        }

        //SetCorner1
        if (currentHostListPosition == 1) 
        {
            if (_corner1Instance == null)// spawns the prefab on the hand
                _corner1Instance = hostList.InitializeObjectAtRightHand(corner1Prefab);
            
            if (_corner2Instance != null) Destroy(_corner2Instance);
            if (_corner3Instance != null) Destroy(_corner3Instance);
            if (_corner4Instance != null) Destroy(_corner4Instance);
            if (_oldCorner2Instance != null) Destroy(_oldCorner2Instance);
            if (_sceneCenterInstance != null) Destroy(_sceneCenterInstance);
            
            if (_oldCorner1Instance == null)// spawns the prefab for on the floor and to use the actual calilations from
            {
                _oldCorner1Instance =
                    Instantiate(corner1Prefab, cor1.Value,
                        new Quaternion()); // makes a new corner to display to the host where the corners are
                _oldCorner1Instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            _oldCorner1Instance.transform.position = new Vector3(
                _corner1Instance.transform.position.x, 0,
                _corner1Instance.transform.position.z
            ); // moves the display corner to the hands posision but wiht y=0
            

            cor1.Value = _oldCorner1Instance.transform.localPosition;
        }
        
        //SetCorner2
        else if (currentHostListPosition == 2) 
        {
            if (_corner2Instance == null)// spawns the prefab on the hand
                _corner2Instance = hostList.InitializeObjectAtRightHand(corner2Prefab);
            
            if (_corner1Instance != null) Destroy(_corner1Instance);
            
            if (_oldCorner2Instance == null)// spawns the prefab for on the floor and to use the actual calilations from
            {
                _oldCorner2Instance =
                    Instantiate(corner2Prefab, cor2.Value,
                        new Quaternion()); // makes a new corner to display to the host where the corners are
                _oldCorner2Instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            _oldCorner2Instance.transform.position = new Vector3(
                _corner2Instance.transform.position.x, 0,
                _corner2Instance.transform.position.z
            ); // moves the display corner to the hands posision but wiht y=0
            

            cor2.Value = _oldCorner2Instance.transform.localPosition;
            Calculate(); // calculates the center and the map
        }
        
        
        //ConfirmRoomSize
        else if (currentHostListPosition == 3) 
        {
            //remove all the 
            if (_corner1Instance != null) Destroy(_corner1Instance); 
            if (_oldCorner1Instance != null) Destroy(_oldCorner1Instance); 
            if (_corner2Instance != null) Destroy(_corner2Instance); 
            if (_oldCorner2Instance != null) Destroy(_oldCorner2Instance); 
            
            if (_sceneCenterInstance != null) Destroy(_sceneCenterInstance); 
            if (_corner3Instance != null) Destroy(_corner3Instance); 
            if (_corner4Instance != null) Destroy(_corner4Instance); 
        }
    }

    [Rpc(SendTo.Everyone)]
    private void MakeDisplayLineRpc(Vector3 posA, Vector3 posB)
    {
        /*
         * ◄──Block──►
         * ◄line►◄gap►
         * ┌────┐     ┌────┐
         */

        // Calculate the total distance (convert to cm)
        float distanceCm = Vector3.Distance(posA, posB) * 100f;

        // Calculate how many blocks (block + gap) fit in the distance
        int amountOfBlocks = Mathf.CeilToInt(distanceCm / (lineBlockLengthCm + lineGapCm));
        // Get the precise spacing per block (including its gap) in cm
        float perBlockSizeCm = distanceCm / amountOfBlocks;

        // Determine the direction from posA to posB
        Vector3 direction = (posB - posA).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        // Place each block along the line, offset by half of perBlockSizeCm, converting cm to meters
        for (int i = 0; i < amountOfBlocks; i++)
        {
            Vector3 pos = posA + direction * ((perBlockSizeCm * i + perBlockSizeCm / 2f) / 100f);
            GameObject instance = Instantiate(linePrefab, pos, rotation);

            // Adjust Y-position (convert height from cm to meters)
            instance.transform.position += new Vector3(0, lineBlockHeightCm / 2f / 100f, 0);
            // Set local scale (convert from cm to meters)
            instance.transform.localScale = new Vector3(lineBlockwidthCm / 100f, lineBlockHeightCm / 100f, lineBlockLengthCm / 100f);

            // Parent the instance to the assigned container
            if (lineContainer != null)
            {
                instance.transform.SetParent(lineContainer, worldPositionStays: true);
            }
        }
    }
    
    [Rpc(SendTo.Everyone)]
    private void ClearDisplayLinesRpc()
    {
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("line"))
        {
            Destroy(line);
        }
    }
}