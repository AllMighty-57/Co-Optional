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
using static UnityEngine.Analytics.AnalyticsSessionInfo;

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
    private bool isLeavingSession;

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
        await SetAuthenticatedPlayerNameAsync();
        var options = new SessionOptions
        {
            MaxPlayers = 2
        }.WithRelayNetwork()
        .WithPlayerName(VisibilityPropertyOptions.Member); ; 
        
        session = await MultiplayerService.Instance.CreateSessionAsync(options);
        SubscribeToSessionEvents();
        SubscribeToPlayerEvents();
        SetScreen(lobbyScreen);
        RefreshPlayerList();

        Debug.Log($"Session {session.Id} created! Join code: {session.Code}");
        if(roomCode != null)
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
        await SetAuthenticatedPlayerNameAsync();

        // Player-name synchronization must also be enabled on the joining
        // client's options, not only when the host creates the session.
        var joinOptions = new JoinSessionOptions()
            .WithPlayerName(VisibilityPropertyOptions.Member);

        session = await MultiplayerService.Instance.JoinSessionByCodeAsync(
            joinCode,
            joinOptions);
        SubscribeToSessionEvents();
        SubscribeToPlayerEvents();
        SetScreen(lobbyScreen);
        RefreshPlayerList();
        Debug.Log($"Session {session.Id} Joined!");
    }

    private async Task SetAuthenticatedPlayerNameAsync()
    {
        string enteredName = usernameInput != null
            ? usernameInput.text.Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(enteredName))
            enteredName = $"Player {AuthenticationService.Instance.PlayerId}";

        await AuthenticationService.Instance.UpdatePlayerNameAsync(enteredName);
    }


    /// <summary>
    /// Hook the session lifecycle so clients return to the room screen if the
    /// host deletes the session or otherwise removes this player.
    /// </summary>
    private void SubscribeToSessionEvents()
    {
        session.StateChanged += OnSessionStateChanged;
    }

    private void UnsubscribeFromSessionEvents()
    {
        if (session != null)
            session.StateChanged -= OnSessionStateChanged;
    }
    private void SubscribeToPlayerEvents()
    {
        session.PlayerJoined += OnPlayerListChanged;
        session.PlayerHasLeft += OnPlayerListChanged;
        session.PlayerPropertiesChanged += RefreshPlayerList;
    }

    private void UnsubscribeFromPlayerEvents()
    {
        if (session == null)
            return;

        session.PlayerJoined -= OnPlayerListChanged;
        session.PlayerHasLeft -= OnPlayerListChanged;
        session.PlayerPropertiesChanged -= RefreshPlayerList;
    }

    private void OnPlayerListChanged(string playerId)
    {
        RefreshPlayerList();
    }

    private void RefreshPlayerList()
    {
        if (session == null || playerList == null)
            return;

        string names = string.Empty;
        for (int i = 0; i < session.Players.Count; i++)
        {
            var player = session.Players[i];
            string name = player.GetPlayerName();

            if (string.IsNullOrWhiteSpace(name))
                name = $"Player {i + 1}";
            else
            {
                // Unity may append a unique discriminator such as "#1234"
                // to synchronized player names. Hide it in the lobby UI.
                int discriminatorIndex = name.IndexOf('#');
                if (discriminatorIndex >= 0)
                    name = name.Substring(0, discriminatorIndex).TrimEnd();
            }

            if (i > 0)
                names += "\n";

            names += name;
        }

        playerList.text = names;
    }

    private void OnSessionStateChanged(SessionState state)
    {
        if (state != SessionState.Disconnected && state != SessionState.Deleted)
            return;

        ReturnToCreateRoomScreen();
    }
    /// <summary>
    /// Assign this method to the lobby Back button.
    /// A client leaves only itself. A host deletes the session, which removes
    /// and disconnects every player currently in it.
    /// </summary>
    public async void OnBackButton()
    {
        if (isLeavingSession)
            return;

        if (session == null)
        {
            ReturnToCreateRoomScreen();
            return;
        }

        isLeavingSession = true;
        ISession sessionToLeave = session;

        try
        {
            if (sessionToLeave.IsHost)
                await sessionToLeave.AsHost().DeleteAsync();
            else
                await sessionToLeave.LeaveAsync();
        }
        catch (SessionException e)
        {
            Debug.LogError($"Could not leave session {sessionToLeave.Id}: {e}");

            // The session may already have been deleted or disconnected. Do
            // not leave the local player stranded on the lobby screen.
            ReturnToCreateRoomScreen();
        }
        finally
        {
            // Usually StateChanged handles this. The fallback also covers a
            // successful API call whose state event arrives later.
            if (session == sessionToLeave)
                ReturnToCreateRoomScreen();

            isLeavingSession = false;
        }
    }

    private void ReturnToCreateRoomScreen()
    {
        UnsubscribeFromSessionEvents();

        session = null;
        isStartingGame = false;

        SetScreen(createRoomScreen);
    }
    private void OnDestroy()
    {
        UnsubscribeFromSessionEvents();
    }

    void SetScreen(GameObject screen)
    {
        // disable all other screens
        startScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        // activate the requested screen
        screen.SetActive(true);
    } 

    public void OnPlayButton()
    {
        SetScreen(createRoomScreen);
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
