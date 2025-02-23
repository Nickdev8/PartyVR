using System;
using TMPro;
using Unity.Netcode;
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
     */
    
    private void Awake()
    {
        _cameraRig = FindAnyObjectByType<OVRCameraRig>();

        _currentHostListPosition = new ObservableInt();
        _currentHostListPosition.OnValueChanged += OnCurrentHostListPositionChanged;
    }
    
    private void Update()
    {
        if (!IsServer)
            return;
        
        SpawnListOnHost();
        previewMap.UpdateHandLogic(_currentHostListPosition.Value);
        
        
        if (_currentHostListPosition.Value == -1) imageRenderer.ClearImage();
        if (_currentHostListPosition.Value == 0) imageRenderer.BlinkImage(0, 1, 0.3f);
        if (_currentHostListPosition.Value == 1) imageRenderer.BlinkImage(2, 3, 0.3f);
        if (_currentHostListPosition.Value == 2) imageRenderer.BlinkImage(4, 5, 0.3f);
        if (_currentHostListPosition.Value == 3)
        {
            //runs the spawnPoint maker and returns if successful. if not, rerun
            //spawnPointMaker.ran = spawnPointMaker.SpawnSpawnPoint(_currentHostListPosition.Value,
            //    MinigameManager.Instance.GetCurrentController().endCondition == EndConditionType.TeamBased);
            spawnPointMaker.ran = spawnPointMaker.SpawnSpawnPoint(_currentHostListPosition.Value, true);

            if (!spawnPointMaker.ran)
            {
                imageRenderer.BlinkImage(6, 7, 0.3f);
            }
            else
            {
                imageRenderer.ShowImage(8);
            }
        }
        if (_currentHostListPosition.Value == 4) imageRenderer.ClearImage();
    }

    /// <summary>
    /// Instantiates the ListPrefab as a child of _cameraRig.rightControllerAnchor if not already instantiated.
    /// This runs only on the server.
    /// Sets the listPrefabTextComp to the correct child component
    /// </summary>
    private bool _ranBefore = false;
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
                    imageRenderer = listInstance.GetComponent<ImageRenderer>();
                    
                    Debug.Log("Hostlist: ListPrefab instantiated as child of rightControllerAnchor.");
                    // makes it so it cant be run multiple times
                    _ranBefore = true;
                }
                else
                {
                    Debug.LogError("Hostlist: OVRCameraRig or rightControllerAnchor is null.");
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
    
    public void NextInHostsListUp() {
        if (IsServer)
            _currentHostListPosition.Value++;
    }
    public void UndoLastHostListup() {
        if (IsServer)
            _currentHostListPosition.Value--;
    }

    private void OnCurrentHostListPositionChanged(int newValue) {
        if (listInstance != null) {
            SceneNetworkManager.Instance.MessagePlayers("OnCurrentHostListPositionChanged To " + newValue);
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
