using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

public class PreviewMap : NetworkBehaviour
{
    [ShowAssetPreview]
    public HostList hostList;
    
    //stuff to spawn/inistantiate
    [ShowAssetPreview]
    public GameObject corner1Prefab;
    [ShowAssetPreview]
    public GameObject corner2Prefab;
    [ShowAssetPreview]
    public GameObject sceneCenterPrefab;
    
    //the gb that are on the hand itself
    private GameObject _corner1Instance;
    private GameObject _corner2Instance;
    
    [HideInInspector] public Vector3 cor1;
    [HideInInspector] public Vector3 cor2;
    [HideInInspector] public Vector3 cor3;
    [HideInInspector] public Vector3 cor4;
    
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
     * ├───cent0───┤
     * │   cent0B  │
     * │cor4   cor2│
     * └───cent2───┘
     */
    
    private void Calculate()
    {
        if (_sceneCenterInstance == null) // makes a new instance to display to the host where the center is
            _sceneCenterInstance = Instantiate(sceneCenterPrefab, cor1, new Quaternion()); 
        
        // makes a new corner to display to the host where the corners are
        if (_corner3Instance == null) 
            _corner3Instance = Instantiate(sceneCenterPrefab, cor3, new Quaternion()); 
        if (_corner4Instance == null) 
            _corner4Instance = Instantiate(sceneCenterPrefab, cor4, new Quaternion()); 

        cent0 = GetCenter(cor1, cor2);
        cent1 = GetCenter(cor1, cor3);
        cent2 = GetCenter(cor4, cor2);
        cent0A = GetCenter(cent0, cent1);
        cent0B = GetCenter(cent0, cent2);
        
        
        // Assuming cor1 and cor2 are diagonal corners and lie on the same horizontal plane.
        Vector3 mid = (cor1 + cor2) * 0.5f;
        Vector3 halfDiagonal = (cor2 - cor1) * 0.5f;
        Vector3 perpendicular = new Vector3(-halfDiagonal.z, 0, halfDiagonal.x);
        cor3 = mid + perpendicular;
        cor4 = mid - perpendicular;

        
        _sceneCenterInstance.transform.position = cent0;
        _corner3Instance.transform.position = cor3;
        _corner4Instance.transform.position = cor4;
        
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
                    Instantiate(corner1Prefab, cor1,
                        new Quaternion()); // makes a new corner to display to the host where the corners are
                _oldCorner1Instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            _oldCorner1Instance.transform.position = new Vector3(
                _corner1Instance.transform.position.x, 0,
                _corner1Instance.transform.position.z
            ); // moves the display corner to the hands posision but wiht y=0
            

            cor1 = _oldCorner1Instance.transform.localPosition;
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
                    Instantiate(corner2Prefab, cor2,
                        new Quaternion()); // makes a new corner to display to the host where the corners are
                _oldCorner2Instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            _oldCorner2Instance.transform.position = new Vector3(
                _corner2Instance.transform.position.x, 0,
                _corner2Instance.transform.position.z
            ); // moves the display corner to the hands posision but wiht y=0
            

            cor2 = _oldCorner2Instance.transform.localPosition;
            Calculate(); // calculates the center and the map
        }
        
        
        //ConfirmRoomSize
        else if (currentHostListPosition == 3) 
        {
            if (_corner2Instance != null)
            {
                Destroy(_corner2Instance); //removes corner 2 sphere
            }
            Calculate();// calculates the center and the map
        }
    }
}