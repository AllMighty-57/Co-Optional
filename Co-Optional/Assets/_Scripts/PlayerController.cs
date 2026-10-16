using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private PlayerInput m_playerInput;
    [SerializeField]
    private AgentMover m_agentMover;

    [SerializeField]
    private InteractionDetector m_interactionDetector;
    [SerializeField]
    private Animator m_animator;
    [SerializeField]
    private AnimationEvents m_animationEvents;

    private bool m_isInteracting;

    private void OnEnable()
    {
        m_playerInput.OnPickupPressed += HandlePickUpPressed;
    }
    private void OnDisable()
    {
        m_playerInput.OnPickupPressed -= HandlePickUpPressed;
    }

    private void HandlePickUpPressed()
    {
        if(m_isInteracting)
            return; 
        if(m_interactionDetector.ClosestInteractable == null)
            return; 

        m_animator.SetBool("Interact", true);
        m_isInteracting = true;
    }

    override public void OnNetworkSpawn()
    { 
        base.OnNetworkSpawn();
        m_interactionDetector.Initialize(IsOwner); 
        if (IsOwner)
        {
            m_animationEvents.OnInteract += HandleInteractAction;
            m_animationEvents.OnAnimationDone += HandleAnimationDone;
        }
        else
        {
            
        }
    }

    private void HandleAnimationDone()
    {
        m_isInteracting = false;
    }

    private void HandleInteractAction()
    {
        if(m_interactionDetector.ClosestInteractable is PickableBase)
        {
            RequestPickUpServerRpc(
                m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestPickUpServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects
            .TryGetValue(networkObjectId, out NetworkObject target))
        {
            return;
        }
        if (!target.TryGetComponent(out PickableBase pickableItem))
        {
            return;
        }
        if (!pickableItem.CanBePickedUp)
        {
            return;
        }

        pickableItem.PickUp();
    }

    void Update()
    {
        if (!IsOwner)
            return; 
        
        Vector2 moveInput = m_playerInput.MoveInput;
        m_agentMover.Move(moveInput);
    }
}
