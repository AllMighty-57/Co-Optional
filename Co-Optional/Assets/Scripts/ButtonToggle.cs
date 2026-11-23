using System.Collections.Generic;
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

    private readonly HashSet<Collider> m_colliders = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Crate"))
            return;

        m_colliders.Add(other);
        UpdateButtonState();
    }

    private void OnTriggerExit(Collider other)
    {
        m_colliders.Remove(other);
        UpdateButtonState();
    }

    public void RemoveObject(GameObject objectToRemove)
    {
        m_colliders.RemoveWhere(c =>
            c == null ||
            c.gameObject == objectToRemove ||
            c.transform.root.gameObject == objectToRemove);

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool pressed = m_colliders.Count > 0;

        m_buttonDepressed.SetEnabled(pressed);
        m_buttonNormal.SetEnabled(!pressed);

        if (!permActivated)
            m_animator.SetBool("Button_down", pressed);
    }
}