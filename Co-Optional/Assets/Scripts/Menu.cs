using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreen; 
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;

    [Header("Create Room Screen")]
    [SerializeField]
    private TMP_InputField usernameInput;
    [SerializeField]
    private TMP_InputField roomCodeInput;

    [Header("Lobby Screen")]
    [SerializeField]
    private TMP_Text playerCount;
    [SerializeField]
    private TMP_Text playerList;
    [SerializeField]
    private TMP_Text roomCode;
    [SerializeField]
    private Button startGameButton;

    [SerializeField]
    private string gameSceneName = "GameScene1"; 

    private ISession session;
    private bool isStartingGame;

    private void Update()
    {
        if (session != null && playerCount != null)
        {
            playerCount.text = $"Player Count: {session.PlayerCount}/2";
        }
        if (startGameButton == null)
            return;

        var network = NetworkManager.Singleton;
        startGameButton.interactable = session != null && network != null
            && network.IsListening && network.IsHost && !isStartingGame;
    }

    public void StartGame()
    {
        var network = NetworkManager.Singleton;
        if (session == null || network == null || !network.IsListening
            || !network.IsHost || isStartingGame)
            return;

        isStartingGame = true;
        if (startGameButton != null)
            startGameButton.interactable = false;

        // Netcode loads the scene for every connected client, including the host.
        var status = network.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            isStartingGame = false;
            Debug.LogError($"Could not load {gameSceneName}: {status}");
        }
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void StartSessionAsHost()
    {
        try
        {
            await StartSessionAsHostAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    async Task StartSessionAsHostAsync()
    {
        var options = new SessionOptions
        {
            MaxPlayers = 2
        }.WithRelayNetwork(); // or WithDistributedAuthorityNetwork() to use Distributed Authority instead of Relay
        
        session = await MultiplayerService.Instance.CreateSessionAsync(options);
        
        Debug.Log($"Session {session.Id} created! Join code: {session.Code}");
        roomCode.text = "Room Code: " + session.Code;
    }

    public async void JoinSession()
    {
        string joinCode = roomCodeInput.text.Trim();

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            Debug.LogWarning("Enter a join code.");
            return;
        }

        try
        {
            await JoinSessionAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    async Task JoinSessionAsync(string joinCode)
    {
        session = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);
        Debug.Log($"Session {session.Id} Joined!");
    }

    public void QuitGame()
    {
        // Logs a message so you can verify it works in the Editor
        Debug.Log("Quitting game...");

        // Closes the built application
        Application.Quit();

        // Optional: Stops Play Mode if you are testing inside the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
