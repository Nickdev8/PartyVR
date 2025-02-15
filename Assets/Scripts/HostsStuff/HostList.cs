using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HostList : NetworkBehaviour
{
    public static HostList Instance;
    public GameObject listPrefab;
    public GameObject corner1Prefab;
    public GameObject corner2Prefab;
    public GameObject sceneCenterPrefab;
    
    private readonly string[] _listText =
    {
        "Set Corner 1",
        "Set Corner 2",
        "Make Random Teams \n Press X to \n Use sides of the line",
        "SpawnController",
        "START THE GAME COUNTDOWN!!", // if changes also change line 146
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
    [HideInInspector] public Vector3 leftSide;
    [HideInInspector] public Vector3 rightSide;
    private GameObject _oldCorner1Instance;
    private GameObject _oldCorner2Instance;
    
    private GameObject _sceneCenterInstance;
    private GameObject _corner3Instance;
    private GameObject _corner4Instance;

    
    private void Awake()
    {
        Instance = this;
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
        if (!IsServer) return;

        SpawnListOnHost();
        DoSetuplogicUpdate();
        
        // turn the list off when not needed anymore
        _listPrefabTextComp.gameObject.SetActive(_listText[_currentHostListPosition] != string.Empty);
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
        if (!IsServer) return;
        
        _listPrefabTextComp.color = Color.white;
        
        if (_currentHostListPosition >= _listText.Length)
            return;
        
        _currentHostListPosition++;
        Debug.Log("Pressed A to go to next in host list up");
        
        if (_listInstance != null && _listText[_currentHostListPosition] != String.Empty)
        {
            // 0 = set corner 1
            // 1 = set corner 2
            // 2 aks player if random teams or determent by line
            
            if (_currentHostListPosition == 2);
            {
                if (MinigameManager.Instance.currentController.teamMode == false) _currentHostListPosition = 4;
                
                if (SceneNetworkManager.Instance.PutPlayersInTeams(true) != -1)
                {
                    foreach (PlayerNetwork player in SceneNetworkManager.Instance.GetPlayerNetworksServerRpc())
                    {
                        player.playerLog.AddLog("Error with random team assignment");
                    }
                }
            }
            
            if (_currentHostListPosition == 3) MinigameManager.Instance.StartNextGameServerRpc();
                
            UpdatelistText(); 
        }
    }

    public void PressedXUp()
    {
        if (!IsServer) return;

        if (_currentHostListPosition != 2) return;
        _listPrefabTextComp.color = Color.white;

        int currentTeamSizeA = 0;
        int teamSizeA = Mathf.CeilToInt(SceneNetworkManager.Instance.currentPlayerNetworks.Count * MinigameManager.Instance.currentController.teamSplitRatio);
        while (currentTeamSizeA != teamSizeA)
        {
            currentTeamSizeA = SceneNetworkManager.Instance.PutPlayersInTeams(false);
            
            if (currentTeamSizeA == teamSizeA) break;

            bool tooMany = currentTeamSizeA > teamSizeA;
            
            foreach (PlayerNetwork player in SceneNetworkManager.Instance.GetPlayerNetworksServerRpc())
            {
                if(tooMany)
                    player.playerLog.AddLog($"Team A has too many players, \n {currentTeamSizeA - teamSizeA} players need to go to the other side");
                else
                    player.playerLog.AddLog($"Team B has too many players, \n {teamSizeA - currentTeamSizeA} players need to go to the other side");
            }
        }
    }
    public void PressedXDown()
    {
        if (!IsServer) return;

        if (_currentHostListPosition != 2) return;
        _listPrefabTextComp.color = Color.gray;
    }
    
    // trigger when the player presses the A button down to change text color to gray
    public void NextInHostsListDown()
    {
        if (!IsServer) return;

        Debug.Log("Pressed A to go to next in host list down");
        _listPrefabTextComp.color = Color.gray;
    }

    // trigger when the player presses the B button down to change text color to red
    public void UndoLastHostListdown()
    {
        if (!IsServer) return;

        _listPrefabTextComp.color = Color.red;
    }
    
    // trigger when the player presses the B button up to change text color to white
    public void UndoLastHostListup()
    {
        if (!IsServer) return;

        _listPrefabTextComp.color = Color.white;
        
        if (_currentHostListPosition < -1)
            return;
        
        _currentHostListPosition--;
        
        UpdatelistText();
        
        if (_currentHostListPosition == -1)
            _listPrefabTextComp.text = _starterListText;
    }
    
    private void UpdatelistText()
    {
        if (!IsServer) return;

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
            MoveMinigameController();
        }
        
        
        //ConfirmRoomSize
        else if (_currentHostListPosition == 2) 
        {
            if (_corner2Instance != null)
            {
                Destroy(_corner2Instance); //removes corner 3 sphere
            }
            PreviewMap();// calculates the center and the map
            MoveMinigameController();
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

        leftSide.x = corner1.x + (corner4.x - corner1.x) / 2;
        leftSide.z = corner1.z + (corner4.z - corner1.z) / 2;
        
        rightSide.x = corner2.x + (corner3.x - corner2.x) / 2;
        rightSide.z = corner2.z + (corner3.z - corner2.z) / 2;
        
        _sceneCenterInstance.transform.position = sceneCenter;
        _corner3Instance.transform.position = corner3;
        _corner4Instance.transform.position = corner4;
        
        // corner1        //corner3//
        //
        // leftSide //sceneCenter// rightSide
        //x
        //y z //corner4//   corner2

    }
    
    /// <summary>
    /// Does the logic for the Host list when the player presses the A button
    /// </summary>
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
    
    void MoveMinigameController()
    {
        MinigameManager.Instance.currentController.transform.localScale = new Vector3(
            Vector3.Distance(corner1, corner4), 1, 
            Vector3.Distance(corner2, corner4));
    }
}
