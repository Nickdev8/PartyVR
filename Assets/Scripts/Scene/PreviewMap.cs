using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PreviewMap : NetworkBehaviour
{
    public HostList HostList;
    
    public GameObject corner1Prefab;
    public GameObject corner2Prefab;
    public GameObject sceneCenterPrefab;
    
    private GameObject _corner1Instance;
    private GameObject _corner2Instance;
    [HideInInspector] public Vector3 corner1;
    [HideInInspector] public Vector3 corner2;
    [HideInInspector] public Vector3 corner3;
    [HideInInspector] public Vector3 corner4;
    [HideInInspector] public Vector3 sceneCenter;
    private GameObject _oldCorner1Instance;
    private GameObject _oldCorner2Instance;
    
    private GameObject _sceneCenterInstance;
    private GameObject _corner3Instance;
    private GameObject _corner4Instance;


    private void Update()
    {
        if (HostList.listInstance == null)
            return;
    }

    /// <summary>
    /// Gets the Corner1 & 3 and calculates the center
    /// And makes corner 4 the oposite posiion relative to the ceneter of 1&3
    /// </summary>
    private void Calculate()
    {
        if (_sceneCenterInstance == null) // makes a new instance to display to the host where the center is
            _sceneCenterInstance = Instantiate(sceneCenterPrefab, corner1, new Quaternion()); 
        
        // makes a new corner to display to the host where the corners are
        if (_corner3Instance == null) 
            _corner3Instance = Instantiate(sceneCenterPrefab, corner3, new Quaternion()); 
        if (_corner4Instance == null) 
            _corner4Instance = Instantiate(sceneCenterPrefab, corner4, new Quaternion()); 

        sceneCenter.x = corner1.x + (corner2.x - corner1.x) / 2;
        sceneCenter.z = corner1.z + (corner2.z - corner1.z) / 2;
        
        // Assuming corner1 and corner2 are diagonal corners and lie on the same horizontal plane.
        Vector3 mid = (corner1 + corner2) * 0.5f;
        Vector3 halfDiagonal = (corner2 - corner1) * 0.5f;
        Vector3 perpendicular = new Vector3(-halfDiagonal.z, 0, halfDiagonal.x);
        corner3 = mid + perpendicular;
        corner4 = mid - perpendicular;

        
        _sceneCenterInstance.transform.position = sceneCenter;
        _corner3Instance.transform.position = corner3;
        _corner4Instance.transform.position = corner4;
        
    }

    public void UpdateHandLogic(int currentHostListPosition)
    {
        //SetCorner1
        if (currentHostListPosition == 0) 
        {
            if (_corner1Instance == null)// spawns the prefab on the hand
                _corner1Instance = HostList.InitializeObjectAtRightHand(corner1Prefab);
            
            if (_corner2Instance != null) Destroy(_corner2Instance);
            if (_corner3Instance != null) Destroy(_corner3Instance);
            if (_corner4Instance != null) Destroy(_corner4Instance);
            if (_oldCorner2Instance != null) Destroy(_oldCorner2Instance);
            if (_sceneCenterInstance != null) Destroy(_sceneCenterInstance);
            
            if (_oldCorner1Instance == null)// spawns the prefab for on the floor and to use the actual calilations from
            {
                _oldCorner1Instance =
                    Instantiate(corner1Prefab, corner1,
                        new Quaternion()); // makes a new corner to display to the host where the corners are
                _oldCorner1Instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            _oldCorner1Instance.transform.position = new Vector3(
                _corner1Instance.transform.position.x, 0,
                _corner1Instance.transform.position.z
            ); // moves the display corner to the hands posision but wiht y=0
            

            corner1 = _oldCorner1Instance.transform.localPosition;
        }
        
        //SetCorner2
        else if (currentHostListPosition == 1) 
        {
            if (_corner1Instance == null)// spawns the prefab on the hand
                _corner1Instance = HostList.InitializeObjectAtRightHand(corner1Prefab);
            
            if (_corner2Instance != null) Destroy(_corner2Instance);
            if (_corner3Instance != null) Destroy(_corner3Instance);
            if (_corner4Instance != null) Destroy(_corner4Instance);
            if (_oldCorner2Instance != null) Destroy(_oldCorner2Instance);
            if (_sceneCenterInstance != null) Destroy(_sceneCenterInstance);
            
            if (_oldCorner1Instance == null)// spawns the prefab for on the floor and to use the actual calilations from
            {
                _oldCorner1Instance =
                    Instantiate(corner1Prefab, corner1,
                        new Quaternion()); // makes a new corner to display to the host where the corners are
                _oldCorner1Instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            _oldCorner1Instance.transform.position = new Vector3(
                _corner1Instance.transform.position.x, 0,
                _corner1Instance.transform.position.z
            ); // moves the display corner to the hands posision but wiht y=0
            

            corner1 = _oldCorner1Instance.transform.localPosition;
            Calculate(); // calculates the center and the map
        }
        
        
        //ConfirmRoomSize
        else if (currentHostListPosition == 2) 
        {
            if (_corner2Instance != null)
            {
                Destroy(_corner2Instance); //removes corner 2 sphere
            }
            Calculate();// calculates the center and the map
        }
    }
}