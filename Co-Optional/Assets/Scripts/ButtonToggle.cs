using Unity.Netcode.Components;
using UnityEngine;

public class ButtonToggle : MonoBehaviour
{
    [SerializeField]
    private ComponentController m_buttonNormal;
    [SerializeField]
    private ComponentController m_buttonDepressed;

    [SerializeField]
    private Animator m_animator;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Crate"))
        {
            m_animator.SetBool("Button_down", true);
            m_buttonDepressed.SetEnabled(true);
            m_buttonNormal.SetEnabled(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Crate"))
        {
            m_animator.SetBool("Button_down", false);
            m_buttonDepressed.SetEnabled(false);
            m_buttonNormal.SetEnabled(true);
        }
    }


}