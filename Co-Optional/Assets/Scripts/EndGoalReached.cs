using System;
using Unity.Netcode.Components;
using UnityEngine;

public class EndGoalReached : MonoBehaviour
{
    [SerializeField]
    private ComponentController m_buttonNoPlayer;
    [SerializeField]
    private ComponentController m_buttonHasPlayer;

    public bool IsGoalReached;
    public event Action OnGoalStateChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            IsGoalReached = true;
            OnGoalStateChanged?.Invoke();
            m_buttonNoPlayer.SetEnabled(false);
            m_buttonHasPlayer.SetEnabled(true);

            Debug.Log("Half way complete.");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsGoalReached = false;
            OnGoalStateChanged?.Invoke();
            m_buttonNoPlayer.SetEnabled(true);
            m_buttonHasPlayer.SetEnabled(false);

            Debug.Log("Need two people");
        }
    }
}
