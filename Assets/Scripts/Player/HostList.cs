using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HostList : NetworkBehaviour
{
    [Header("Dependencies")]
    public GameObject listPrefab;
    public PreviewMap previewMap;
    public SpawnPointMaker spawnPointMaker;
    public RoomCollision roomCollision;

    [HideInInspector] public GameObject listInstance;
    [HideInInspector] public ImageRenderer imageRenderer;

    private string _starterListText;
    private ObservableInt _currentHostListPosition;
    private TMP_Text _listPrefabTextComp;

    /*
     * Host List Positions:
     *  0 = Show blinking "Press A" button
     *  1 = Show Corner 1 blinking
     *  2 = Show Corner 2 blinking
     *  3 = Start next game
     *  4 = Calculate spawn locations for player objects
     */

    private void Awake()
    {
        _currentHostListPosition = new ObservableInt();
    }

    private void Start()
    {
        _currentHostListPosition.OnValueChanged += OnCurrentHostListPositionChanged;
        NetworkManager.Singleton.OnServerStarted += SpawnListOnHost;
    }

    private void Update()
    {
        if (!IsServer) return;

        previewMap.UpdateHandLogic(_currentHostListPosition.Value);
    }

    /// <summary>
    /// Instantiates the listPrefab at the player's hand if not already instantiated.
    /// Runs only on the server.
    /// </summary>
    private void SpawnListOnHost()
    {
        if (!IsServer || listInstance != null) return;

        listInstance = PlayerNetwork.Instance.InitializeObjectAtHand(listPrefab, true);
        imageRenderer = listInstance.GetComponent<ImageRenderer>();

        OnCurrentHostListPositionChanged(0);
    }

    /// <summary>
    /// Handles UI updates based on the host list position.
    /// </summary>
    private void OnCurrentHostListPositionChanged(int newValue)
    {
        if (listInstance != null)
        {
            SceneNetworkManager.Instance.MessagePlayersRpc($"OnCurrentHostListPositionChanged To {newValue}");
        }

        switch (newValue)
        {
            case -1:
                imageRenderer.ClearImage();
                break;
            case 0:
                previewMap.ClearDisplayLinesRpc();
                imageRenderer.BlinkImage(0, 1);
                break;
            case 1:
                imageRenderer.BlinkImage(2, 3); // Corner 1 blinking
                break;
            case 2:
                imageRenderer.BlinkImage(4, 5); // Corner 2 blinking
                break;
            case 3:
                previewMap.Calculate(); // show the final room size pos
                roomCollision.MakeRoomCollisionRpc( // make the collisions for the room
                    previewMap.cor1.Value, previewMap.cor2.Value, previewMap.cor3.Value, previewMap.cor4.Value);
                
                MinigameManager.Instance.StartNextGame();
                StartCoroutine(MinigameManager.Instance.BlinkImageForTeamDivition(spawnPointMaker, imageRenderer));
                break;
            case 4:
                imageRenderer.ShowImage(9);
                break;
            case 5:
                imageRenderer.ClearImage();
                break;
            default: 
                imageRenderer.ShowImage(-1);
                break;
        }
    }

    public void NextInHostsListUp()
    {
        if (!IsServer) return;

        if (!MinigameManager.Instance.wrongTeamRatio || MinigameManager.Instance.overrideWrongTeamRatio)
        {
            _currentHostListPosition.Value++;
        }
        else
        {
            MinigameManager.Instance.overrideWrongTeamRatio = true;
        }
    }

    public void UndoLastHostListUp()
    {
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
