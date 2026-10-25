using System;
using UnityEngine;

public class EndGoalReached : MonoBehaviour
{
     
    public bool IsGoalReached;
    public event Action OnGoalStateChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsGoalReached = true;
            OnGoalStateChanged?.Invoke();

            Debug.Log("Half way complete.");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsGoalReached = false;
            OnGoalStateChanged?.Invoke(); 

            Debug.Log("Need two people");
        }
    }
}
