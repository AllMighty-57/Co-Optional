using System;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerNameUi : NetworkBehaviour
{
    [SerializeField]
    private UIDocument m_uiPlayerNameDocument;

    private Label m_playerNameLabel;

    private NetworkVariable<FixedString32Bytes> m_playerName = new NetworkVariable<FixedString32Bytes>( 
        string.Empty, NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        m_playerNameLabel = m_uiPlayerNameDocument.rootVisualElement.Q<Label>("NameLabel");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_playerName.OnValueChanged += HandlePlayerNameChanged;
        
        // The owner entered this name in the menu. Send it to the server so
        // the NetworkVariable can display it on every player's screen.
        if (IsOwner)
        {
            string playerName = AuthenticationService.Instance.PlayerName;
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = $"Player {OwnerClientId + 1}";

            SubmitPlayerNameServerRpc(RemoveNameDiscriminator(playerName));
        }

        // Apply the already-synchronized value on every client
        m_playerNameLabel.text = m_playerName.Value.ToString();
    }
    
    [ServerRpc]
    private void SubmitPlayerNameServerRpc(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = $"Player {OwnerClientId + 1}";

        m_playerName.Value = RemoveNameDiscriminator(playerName);
    }

    private string RemoveNameDiscriminator(string playerName)
    {
        int discriminatorIndex = playerName.IndexOf('#');
        if (discriminatorIndex >= 0)
            playerName = playerName.Substring(0, discriminatorIndex);

        return playerName.Trim();
    }

    private void HandlePlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        m_playerNameLabel.text = newValue.ToString();
    }
    public override void OnNetworkDespawn()
    {
        m_playerName.OnValueChanged -= HandlePlayerNameChanged;
        base.OnNetworkDespawn();
    }
}
