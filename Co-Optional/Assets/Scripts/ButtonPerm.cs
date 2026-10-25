using Unity.Netcode.Components;
using UnityEngine;

public class ButtonPerm : MonoBehaviour
{
    [SerializeField]
    private ComponentController m_buttonNormal;
    [SerializeField]
    private ComponentController m_buttonDepressed;
    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private ButtonToggle m_toggleButton;

    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        activated = true;

        m_animator.SetBool("Button_down", true);
        m_buttonDepressed.SetEnabled(true);
        m_buttonNormal.SetEnabled(false);

        // Prevent the normal button from changing the door state.
        m_toggleButton.enabled = false;
    }
}