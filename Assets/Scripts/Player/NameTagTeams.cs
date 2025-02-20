using System.Collections;
using Meta.XR.MultiplayerBlocks.NGO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NameTagTeams : NetworkBehaviour
{
    [SerializeField] private Color32 teamColorA;
    [SerializeField] private Color32 teamColorB;
    [SerializeField] private GameObject nameTagPanel;

    private PlayerNameTagNGO _playerNameTag;

    private void Awake()
    {
        _playerNameTag = gameObject.GetComponent<PlayerNameTagNGO>();
    }

    private IEnumerator Start()
    {
        var playerNetwork = SceneNetworkManager.Instance.GetPlayerNetwork();
        
        if (playerNetwork != null && _playerNameTag != null && playerNetwork.CurrentTeam != Team.None)
        {
            if (playerNetwork.CurrentTeam == Team.A) nameTagPanel.GetComponent<Image>().color = teamColorA;
            if (playerNetwork.CurrentTeam == Team.A) nameTagPanel.GetComponent<Image>().color = teamColorB;
        }
        
        // refresh nameTag panel
        yield return new WaitForFixedUpdate();
    }
}