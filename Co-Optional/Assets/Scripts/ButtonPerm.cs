using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ButtonPerm : NetworkBehaviour
{
    [SerializeField]
    private ComponentController m_buttonNormal;
    [SerializeField]
    private ComponentController m_buttonDepressed;
    [SerializeField]
    private Animator m_animator;
    [SerializeField]
    private ButtonToggle m_toggleButton;

    private NetworkVariable<bool> activated = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        activated.OnValueChanged += OnActivatedChanged;

        // Apply the current state for late-joining clients.
        ApplyButtonState(activated.Value);
    }

    public override void OnNetworkDespawn()
    {
        activated.OnValueChanged -= OnActivatedChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || activated.Value)
            return;

        if (!other.CompareTag("Player"))
            return;

        activated.Value = true;
    }

    private void OnActivatedChanged(bool oldValue, bool newValue)
    {
        ApplyButtonState(newValue);
    }

    private void ApplyButtonState(bool isActivated)
    {
        m_animator.SetBool("Button_down", isActivated);
        m_buttonDepressed.SetEnabled(isActivated);
        m_buttonNormal.SetEnabled(!isActivated);

        // Prevent the normal button from changing the door state.
        m_toggleButton.permActivated = isActivated;
    }
}