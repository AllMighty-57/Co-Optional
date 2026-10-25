using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    private bool m_winTriggered;


    //[SerializeField]
    //private List<ResourcePallet> m_pallets;

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
