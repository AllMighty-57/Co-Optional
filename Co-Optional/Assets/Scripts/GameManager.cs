using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private MultiplayerUI m_multiplayerUI;
    [SerializeField]
    private GameObject m_playerPrefab;

    [SerializeField]
    private EndGoalReached m_endGoal1;
    [SerializeField]
    private EndGoalReached m_endGoal2;

    [SerializeField]
    private FallenOff HasFallenOff;

    [SerializeField]
    private GameObject m_permButton;

    [SerializeField]
    private Transform m_permButtonSpawn;

    private bool m_winTriggered; 
    private bool potentialSoftlock;
    
    

    private void Start()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.OnStartHost += StartHost;
            m_multiplayerUI.OnStartClient += StartClient;
            m_multiplayerUI.OnDiconnectClient += DisconnectClient;
        } 
    } 

    override public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
            return;
        NetworkManager.OnClientConnectedCallback += SpawnPlayer;
        NetworkManager.SceneManager.OnLoadEventCompleted += HandleSceneLoadCompleted;

        m_endGoal1.OnGoalStateChanged += CheckWinCondition;
        m_endGoal2.OnGoalStateChanged += CheckWinCondition;
        HasFallenOff.OnFellStateChanged += AntiSoftlockProtocol;

    } 

    private void AntiSoftlockProtocol()
    { 
        if (!IsServer || potentialSoftlock) 
            return;
        
        if (HasFallenOff.HasFallen)
        {
            potentialSoftlock = true;
            m_permButton.transform.SetPositionAndRotation(
                m_permButtonSpawn.position,
                m_permButtonSpawn.rotation
            );
        }
    }

    private void CheckWinCondition()
    {
        if (!IsServer || m_winTriggered)
            return;

        if (m_endGoal1.IsGoalReached && m_endGoal2.IsGoalReached)
        {
            m_winTriggered = true;

            NetworkManager.SceneManager.LoadScene(
                SceneManager.GetActiveScene().name,
                LoadSceneMode.Single);
        }
    }

    private void HandleSceneLoadCompleted(string sceneName, 
        LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(ulong clientId in clientsCompleted)
        {
            SpawnPlayer(clientId);
        }
    }

    override public void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= SpawnPlayer; 
            NetworkManager.SceneManager.OnLoadEventCompleted -= HandleSceneLoadCompleted;

            m_endGoal1.OnGoalStateChanged -= CheckWinCondition;
            m_endGoal2.OnGoalStateChanged -= CheckWinCondition;
            HasFallenOff.OnFellStateChanged -= AntiSoftlockProtocol;

        }

        base.OnNetworkDespawn();
    }

    private void SpawnPlayer(ulong clientID)
    {
        if (NetworkManager.ConnectedClients[clientID].PlayerObject != null)
            return;
        GameObject player = Instantiate(m_playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
    }

    private void DisconnectClient()
    {
        m_multiplayerUI.EnableButtons(); 
        NetworkManager.Shutdown();
    }

    private void StartClient()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartClient();
    }

    private void StartHost()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartHost();
    }
}
