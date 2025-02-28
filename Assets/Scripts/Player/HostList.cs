using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HostList : NetworkBehaviour
{
    public GameObject listPrefab;
    public PreviewMap previewMap;
    public SpawnPointMaker spawnPointMaker;
    public RoomCollision roomCollision;
    [HideInInspector] public GameObject listInstance;
    [HideInInspector] public ImageRenderer imageRenderer;
    
    private OVRCameraRig _cameraRig;
    private string _starterListText;
    private ObservableInt _currentHostListPosition;
    private TMP_Text _listPrefabTextComp;
    
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
     *
     * _currentHostListPosition.value == on changed
     *  0 = show the blinking press the A button
     *  1 = show Corner 1 blinking
     *  2 = show Corner 2 blinking
     *  3 = show start next game
     *  4 = show calculates the spawn locations for the playerObjects
     * 
     */
    
    private void Awake()
    {
        _currentHostListPosition = new ObservableInt();
    }
    private void Start()
    {
        _currentHostListPosition.OnValueChanged += OnCurrentHostListPositionChanged;
        NetworkManager.Singleton.OnServerStarted += SpawnListOnHost;
        _cameraRig = FindAnyObjectByType<OVRCameraRig>();
    }
    
    private void Update()
    {
        if (!IsServer)
            return;
        
        previewMap.UpdateHandLogic(_currentHostListPosition.Value);
    }


    /// <summary>
    /// Instantiates the ListPrefab as a child of _cameraRig.rightControllerAnchor if not already instantiated.
    /// This runs only on the server.
    /// Sets the listPrefabTextComp to the correct child component
    /// </summary>
    private void SpawnListOnHost()
    {
        if (IsServer)
        {
            // Only instantiate once.
            if (listInstance == null)
            {
                if (_cameraRig != null && _cameraRig.rightControllerAnchor != null)
                {
                    // Instantiate ListPrefab as a child of rightControllerAnchor.
                    listInstance = InitializeObjectAtRightHand(listPrefab);
                    imageRenderer = listInstance.GetComponent<ImageRenderer>();
                    
                    OnCurrentHostListPositionChanged(0);
                    
                    Debug.Log("Hostlist: ListPrefab instantiated as child of rightControllerAnchor.");
                }
                else
                {
                    Debug.LogWarning("Hostlist: OVRCameraRig or rightControllerAnchor is null.");
                    _cameraRig = FindAnyObjectByType<OVRCameraRig>();
                    SpawnListOnHost();
                }
            }
        }
    }
    
    private void OnCurrentHostListPositionChanged(int newValue) 
    {
        if (listInstance != null) {
            SceneNetworkManager.Instance.MessagePlayersRpc("OnCurrentHostListPositionChanged To " + newValue);
        }
        
        if (newValue == -1) imageRenderer.ClearImage();
        else if (newValue == 0) imageRenderer.BlinkImage(0, 1); // runs because line 93
        else if (newValue == 1) imageRenderer.BlinkImage(2, 3); // show Corner 1 blinking
        else if (newValue == 2) imageRenderer.BlinkImage(4, 5); // show Corner 2 blinking
        else if (newValue == 3) { // show start game button and do the collision calulations
            previewMap.Calculate();
            roomCollision.MakeRoomCollisionRpc(
                previewMap.cor1.Value, previewMap.cor2.Value, previewMap.cor3.Value, previewMap.cor4.Value);
            imageRenderer.ShowImage(9);
        }
        else if (newValue == 4) // start the game
        {
            MinigameManager.Instance.StartNextGame();
            StartCoroutine(MinigameManager.Instance.BlinkImageForTeamDivition(spawnPointMaker, imageRenderer));
            // the correct images are shown in function above.
        } 
        else if (newValue == 5) imageRenderer.ClearImage();
        
        else // is somethings wrong show error
        {
            imageRenderer.ShowImage(-1);
        }
    }
    
    public void OnSpawnPointRanValueChanged(bool ran)
    {
        if (_currentHostListPosition.Value == 4)
        {
            if (!ran)
            {
                imageRenderer.BlinkImage(6, 7);
                return;
            }

            imageRenderer.ShowImage(8);
        }
    }

    // <--- this is for functions that can be called from anywhere --->
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
    public void NextInHostsListUp() {
        if (IsServer)
        {
            if (MinigameManager.Instance.wrongTeamRatio == false)
            {
                _currentHostListPosition.Value++;
            }
            else
            {
                SceneNetworkManager.Instance.MessagePlayersRpc("Teams are not divided correctly");
            }
        }
    }
    public void UndoLastHostListUp() {
        if (IsServer && _currentHostListPosition.Value > 0)
        {
            _currentHostListPosition.Value--;
        }
    }
}

public class ObservableInt
{
    private int _value;
    public event Action<int> OnValueChanged;

    public int Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }
}
