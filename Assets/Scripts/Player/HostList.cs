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
        if (IsHost)
            this.enabled = false;
            
        _currentHostListPosition = new ObservableInt();
    }
    
    private void Start()
    {
        _currentHostListPosition.OnValueChanged += OnCurrentHostListPositionChanged;
        NetworkManager.Singleton.OnServerStarted += SpawnListOnHost;
        _cameraRig = FindAnyObjectByType<OVRCameraRig>();

        imageRenderer.BlinkImage(0, 1); // show the blinking press a button
    }
    
    
    private void Update()
    {
        if (!IsServer)
            return;
        previewMap.UpdateHandLogic(_currentHostListPosition.Value);

        if (_currentHostListPosition.Value == 4)
        {
            // runs the spawnPoint maker and returns if successful. if not, rerun
            spawnPointMaker.ran = spawnPointMaker.SpawnSpawnPoint();
            
            SceneNetworkManager.Instance.MessagePlayersRpc($"{spawnPointMaker.ran}");
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
        else if (newValue == 0) imageRenderer.BlinkImage(0, 1); // this is also on 41:9
        else if (newValue == 1) imageRenderer.BlinkImage(2, 3); // show Corner 1 blinking
        else if (newValue == 2) imageRenderer.BlinkImage(4, 5); // show Corner 2 blinking
        else if (newValue == 3) imageRenderer.BlinkImage(9, 10);// start next game
        else if (newValue == 4) imageRenderer.BlinkImage(6, 7); // calculates the spawn locations for the playerObjects
        else if (newValue == 5) imageRenderer.ClearImage();
        else
        {
            imageRenderer.ShowImage(-1);
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
            _currentHostListPosition.Value++;
    }
    public void UndoLastHostListup() {
        if (IsServer)
            _currentHostListPosition.Value--;
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
