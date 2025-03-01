using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork Instance;

    [Header("Collider")]
    public CapsuleCollider capsule;
    public float margin = 0.2f;
    
    [Header("Assets")]
    public Logger logger;
    public Material teamColor;
    public GameObject leftHandPrefab;
    
    [Header("Team Colors")]
    public Color32 colorA;
    public Color32 colorB;
    
    [HideInInspector] public GameObject leftHandInstance;
    [HideInInspector] public ImageRenderer leftHandImageRenderer;
    
    private OVRCameraRig _cameraRig;

    [Header("Player Variables")]
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    private readonly NetworkVariable<Team> _currentTeam = new NetworkVariable<Team>();
    public Team CurrentTeam => _currentTeam.Value;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cameraRig = FindAnyObjectByType<OVRCameraRig>();
        if (_cameraRig == null)
        {
            logger.LogErrorText("OVRCameraRig not found in scene.");
            return;
        }

        // Instantiate the left hand object.
        leftHandInstance = InitializeObjectAtHand(leftHandPrefab, right: false);
        if (leftHandInstance != null)
        {
            leftHandImageRenderer = leftHandInstance.GetComponent<ImageRenderer>();
        }
    }

    public override void OnNetworkSpawn()
    {
        _currentTeam.OnValueChanged += OnTeamChanged;
    }

    public override void OnNetworkDespawn()
    {
        _currentTeam.OnValueChanged -= OnTeamChanged;
    }

    public void TakeDamage(int damage)
    {
        if (CurrentTeam != Team.Dead)
        {
            health.Value = Mathf.Max(health.Value - damage, 0);
            if (health.Value <= 0)
            {
                SetTeam(Team.Dead);
            }
        }
    }

    public void SetTeam(Team team)
    {
        _currentTeam.Value = team;
    }

    private void OnTeamChanged(Team previous, Team current)
    {
        // Update the team color based on the current team.
        teamColor.color = current == Team.A ? colorA : colorB;
        logger.LogErrorText($"Player {OwnerClientId} team changed to {current}");
    }

    public GameObject InitializeObjectAtHand(GameObject prefab, bool right = true)
    {
        if (_cameraRig == null)
        {
            logger.LogErrorText("CameraRig not initialized.");
            return null;
        }

        Transform anchor = right ? _cameraRig.rightControllerAnchor : _cameraRig.leftControllerAnchor;
        if (anchor == null)
        {
            logger.LogErrorText($"{(right ? "Right" : "Left")} controller anchor not found.");
            return null;
        }

        // Instantiate the prefab as a child of the selected controller anchor.
        GameObject newObject = Instantiate(prefab, anchor);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;
        return newObject;
    }

    private void Update()
    {
        UpdateCapsuleCollider(_cameraRig.leftControllerAnchor.position, 
            _cameraRig.rightControllerAnchor.position, _cameraRig.centerEyeAnchor.position);
    }

    /// <summary>
    /// Updates the capsule collider based on the positions of the two hands and head.
    /// The collider’s bottom is fixed at y=0 and its top is at head.y + margin.
    /// The horizontal center (x,z) is the average of the three positions.
    /// The radius is chosen to ensure that all points (hands and head) are inside, with extra margin.
    /// </summary>
    /// <param name="leftHand">Left hand world position.</param>
    /// <param name="rightHand">Right hand world position.</param>
    /// <param name="head">Head world position.</param>
    private void UpdateCapsuleCollider(Vector3 leftHand, Vector3 rightHand, Vector3 head)
    {
        float centerX = (leftHand.x + rightHand.x + head.x) / 3f;
        float centerZ = (leftHand.z + rightHand.z + head.z) / 3f;

        capsule.transform.position = new Vector3(centerX, 0f, centerZ);

        float colliderHeight = head.y + margin;
        float colliderCenterY = colliderHeight / 2f;
        capsule.height = colliderHeight;
        capsule.center = new Vector3(0f, colliderCenterY, 0.15f);

        Vector2 center2D = new Vector2(centerX, centerZ);
        float distLeft = Vector2.Distance(center2D, new Vector2(leftHand.x, leftHand.z));
        float distRight = Vector2.Distance(center2D, new Vector2(rightHand.x, rightHand.z));
        float distHead = Vector2.Distance(center2D, new Vector2(head.x, head.z));

        float newRadius = Mathf.Max(distLeft, distRight, distHead) + margin;
        capsule.radius = newRadius;
    }
}
