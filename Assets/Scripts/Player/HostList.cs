using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HostList : NetworkBehaviour
{
    public GameObject listPrefab;
    [HideInInspector] public GameObject listInstance;
    
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
    private TMP_Text _listPrefabTextComp;
    
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
            if (listInstance == null)
            {
                if (_cameraRig != null && _cameraRig.rightControllerAnchor != null)
                {
                    // Instantiate ListPrefab as a child of rightControllerAnchor.
                    listInstance = InitializeObjectAtRightHand(listPrefab);
                    _listPrefabTextComp = listInstance.transform.GetComponentInChildren<TMP_Text>();

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

    public GameObject InitializeObjectAtRightHand(GameObject prefab)
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
        
        if (listInstance != null && _listText[_currentHostListPosition] != String.Empty)
        {
            // 0 = set corner 1
            // 1 = set corner 2
            //if (_currentHostListPosition == 3) runthisfunciton();
                
            UpdateListText(); 
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

    public void PressedXUp()
    {
        // if (!IsServer) return;
        //
        // if (_currentHostListPosition != 2) return;
        // _listPrefabTextComp.color = Color.white;
        //
        // int currentTeamSizeA = 0;
        // int teamSizeA = Mathf.CeilToInt(SceneNetworkManager.Instance.Players.Count * MinigameManager.Instance.currentController.teamSplitRatio);
        // while (currentTeamSizeA != teamSizeA)
        // {
        //     currentTeamSizeA = SceneNetworkManager.Instance.PutPlayersInTeams(false);
        //     
        //     if (currentTeamSizeA == teamSizeA) break;
        //
        //     bool tooMany = currentTeamSizeA > teamSizeA;
        //     
        //     foreach (PlayerNetwork player in SceneNetworkManager.Instance.GetPlayerNetworksServerRpc())
        //     {
        //         if(tooMany)
        //             player.logger.LogErrorText($"Team A has too many Players, \n {currentTeamSizeA - teamSizeA} Players need to go to the other side");
        //         else
        //             player.logger.LogErrorText($"Team B has too many Players, \n {teamSizeA - currentTeamSizeA} Players need to go to the other side");
        //     }
        // }
    }
    public void PressedXDown()
    {
        if (!IsServer) return;

        if (_currentHostListPosition != 2) return;
        _listPrefabTextComp.color = Color.gray;
    }
    
    /// <summary>
    /// trigger when the player presses the B button up to change text color to white
    /// </summary>
    public void UndoLastHostListup()
    {
        _listPrefabTextComp.color = Color.white;
        _currentHostListPosition--;
        UpdateListText();
    }
    public void UndoLastHostListdown()
    {
        _listPrefabTextComp.color = Color.red;
    }
    
    private void UpdateListText()
    {
        string text = $"Press A to \n {_listText[_currentHostListPosition]}";
        if (_listText[_currentHostListPosition + 1] != String.Empty) 
            text += $"\n \n Next: \n Press A to \n {_listText[_currentHostListPosition + 1]}";

        _listPrefabTextComp.text = text;
    }
}
