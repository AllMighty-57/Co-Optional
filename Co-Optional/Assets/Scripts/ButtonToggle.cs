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

    public bool permActivated;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Crate"))
        { 
            m_buttonDepressed.SetEnabled(true);
            m_buttonNormal.SetEnabled(false);
            if (permActivated)
                return;
            m_animator.SetBool("Button_down", true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Crate"))
        {
            m_buttonDepressed.SetEnabled(false);
            m_buttonNormal.SetEnabled(true);
            if (permActivated)
                return;
            m_animator.SetBool("Button_down", false);
        }
    }


}