using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HostList : NetworkBehaviour
{
    public GameObject listPrefab;
    public GameObject corner1Prefab;
    public GameObject corner2Prefab;
    public GameObject sceneCenterPrefab;
    
    private readonly string[] _listText =
    {
        "Set Corner 1",
        "Set Corner 2",
        "Confirm Current Players",
        "Confirm Teams",
        "Confirm BlockLayoutPositions",
        "START THE GAME COUNTDOWN!!",
        "",
    };
    
    private OVRCameraRig _cameraRig;
    
    private string _starterListText;
    private int _currentHostListPosition = -1;
    private GameObject _listInstance;
    private TMP_Text _listPrefabTextComp;
    
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

    // corner1        //corner2//
    //
    //      //sceneCenter//
    //
    // //corner4//      corner2
    
    private void Awake()
    {
        _cameraRig = FindAnyObjectByType<OVRCameraRig>();
        
        _starterListText = 
            "This is the Host List, \n" +
            "for controlling the session. \n" +
            "Press A to go next in list. \n \n" +
            "Next: \n" +
            $"Press A to \n {_listText[0]}";
    }
    
    private void Update()
    {
        SpawnListOnHost();
        DoSetuplogicUpdate();
    }

    /// <summary>
    /// Instantiates the ListPrefab as a child of _cameraRig.rightControllerAnchor if not already instantiated.
    /// This runs only on the server.
    /// Sets the listPrefabTextComp to the correct child component
    /// </summary>
    private bool _ranBefore;
    private void SpawnListOnHost()
    {
        if (!_ranBefore && IsServer)
        {
            // Only instantiate once.
            if (_listInstance == null)
            {
                if (_cameraRig != null && _cameraRig.rightControllerAnchor != null)
                {
                    // Instantiate ListPrefab as a child of rightControllerAnchor.
                    _listInstance = InitializeObjectAtRightHand(listPrefab);
                    _listPrefabTextComp = _listInstance.transform.GetComponentInChildren<TMP_Text>();

                    _currentHostListPosition = -1;
                    _listPrefabTextComp.text = _starterListText;
                    
                    Debug.Log("ListPrefab instantiated as child of rightControllerAnchor.");
                    
                    // makes it so it cant be run multiple times
                    _ranBefore = true;
                }
                else
                {
                    Debug.LogError("OVRCameraRig or rightControllerAnchor is null.");
                }
            }
        }
    }

    private GameObject InitializeObjectAtRightHand(GameObject prefab)
    {
        if (_cameraRig != null && _cameraRig.rightControllerAnchor != null)
        {
            // Instantiate ListPrefab as a child of rightControllerAnchor.
            GameObject newObject = Instantiate(prefab, _cameraRig.rightControllerAnchor);

            // Reset local position/rotation if needed.
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;

            return newObject;
        }
        return null;
    }
    
    /// <summary>
    /// When player Presses A button up than that variable does logic depending on the _currentHostListPosition & ListText.
    /// And setting the right text to listPrefabTextComp.
    /// </summary>
    public void NextInHostsListUp()
    {
        _currentHostListPosition++;
        
        Debug.Log("Pressed A to go to next in host list up");
        _listPrefabTextComp.color = Color.white;
        
        if (_listInstance != null && _listText[_currentHostListPosition] != String.Empty)
        {
            // 0 = set corner 1
            // 1 = set corner 2
            if (_currentHostListPosition == 3) ConfirmCurrentPlayers();
            if (_currentHostListPosition == 4) ConfirmTeams();
            if (_currentHostListPosition == 5) ConfirmBlockLayoutPositions();
            if (_currentHostListPosition == 6) StartGame();
                
            UpdatelistText(); 
        }
    }
    
    /// <summary>
    /// trigger when the player presses the A button down to change text color to gray
    /// </summary>
    public void NextInHostsListDown()
    {
        Debug.Log("Pressed A to go to next in host list down");
        _listPrefabTextComp.color = Color.gray;
    }

    /// <summary>
    /// trigger when the player presses the B button down to change text color to red
    /// </summary>
    public void UndoLastHostListdown()
    {
        _listPrefabTextComp.color = Color.red;
    }
    
    /// <summary>
    /// trigger when the player presses the B button up to change text color to white
    /// </summary>
    public void UndoLastHostListup()
    {
        _listPrefabTextComp.color = Color.white;
        _currentHostListPosition--;
        UpdatelistText();
    }
    
    private void UpdatelistText()
    {
        string text = $"Press A to \n {_listText[_currentHostListPosition]}";
        if (_listText[_currentHostListPosition + 1] != String.Empty) 
            text += $"\n \n Next: \n Press A to \n {_listText[_currentHostListPosition + 1]}";

        _listPrefabTextComp.text = text;
    }

    /// <summary>
    /// Does the logic for the Host list every frame
    /// </summary>
    private void DoSetuplogicUpdate() //_currentHostListPosition is what's currently being shown on the text label
    {
        if (_listInstance == null)
            return;
        
        //SetCorner1
        if (_currentHostListPosition == 0) 
        {
            if (_corner1Instance == null)// spawns the prefab on the hand
                _corner1Instance = InitializeObjectAtRightHand(corner1Prefab);
            
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
        else if (_currentHostListPosition == 1) 
        {
            if (_corner1Instance != null)
            {
                Destroy(_corner1Instance);//remove corner 1 sphere
            }

            if (_corner2Instance == null)
                _corner2Instance = InitializeObjectAtRightHand(corner2Prefab); // replace with net corene
            if (_oldCorner2Instance == null)// spawns the prefab for on the floor and to use the actual calilations from
            {
                _oldCorner2Instance =
                    Instantiate(corner2Prefab, corner2,
                        new Quaternion()); // makes a new corner to display to the host where the corners are
                _oldCorner2Instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }

            _oldCorner2Instance.transform.position = new Vector3(
                _corner2Instance.transform.position.x, 0,
                _corner2Instance.transform.position.z
            ); // moves the display corner to the hands posision but wiht y=0

            
            corner2 = _oldCorner2Instance.transform.position;
            PreviewMap(); // calculates the center and the map
        }
        
        
        //ConfirmRoomSize
        else if (_currentHostListPosition == 2) 
        {
            if (_corner2Instance != null)
            {
                Destroy(_corner2Instance); //removes corner 3 sphere
            }
            PreviewMap();// calculates the center and the map
        }
    }
    
    /// <summary>
    /// Gets the Corner1 & 3 and calculates the center
    /// And makes corner 4 the oposite posiion relative to the ceneter of 1&3
    /// </summary>
    private void PreviewMap()
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
    
    /// <summary>
    /// Does the logic for the Host list when the player presses the A button
    /// </summary>
    
    void ConfirmCurrentPlayers()
    {
        Debug.Log("Confirm Current Players");
    }
    void ConfirmTeams()
    {
        Debug.Log("Confirm Teams");
    }
    void ConfirmBlockLayoutPositions()
    {
        Debug.Log("Confirm BlockLayoutPositions");
    }
    
    void StartGame()
    {
        Debug.Log("Start Game");
    }
}
