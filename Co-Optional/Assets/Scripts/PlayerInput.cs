using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;

public class PlayerInput : NetworkBehaviour
{
    [SerializeField] 
    private InputActionReference m_moveActionReference; 
    public Vector2 MoveInput { get; private set; } 
    public event Action OnPickupPressed;
    public event Action OnInteractPressed;

    private Vector2 m_rawInput;
    [SerializeField]
    private float m_smoothTime = 0.1f;


    // Update is called once per frame
    void Update()
    { 
        if(!IsOwner) 
            return;

        m_rawInput = m_moveActionReference.action.ReadValue<Vector2>(); 
        MoveInput = Vector2.MoveTowards
            (MoveInput, m_rawInput, Time.deltaTime / m_smoothTime); 

        if(Keyboard.current.eKey.wasPressedThisFrame)
        {
            OnPickupPressed?.Invoke();
        } 
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnInteractPressed?.Invoke();
        }

    }
}
