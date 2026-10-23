using UnityEngine;

/// <summary>
/// AgentMover doesnt need to ask "IsOwner" question becuase the Input is only provided for the Owner. So while we could
/// make it a NetworkBehaviour and do the IsOwner check, it is not really necessary and we can save some performance by keeping it as a regular MonoBehaviour.
/// </summary>
public class AgentMover : MonoBehaviour
{
    [SerializeField]
    private CharacterController m_characterController;
    [SerializeField]
    private Animator m_animator;

    [Header("Movement")]
    [SerializeField]
    private float m_movementSpeed = 6f;

    [SerializeField]
    private float m_rotationSpeed = 250f;

    [Header("Gravity")]
    [SerializeField]
    private float m_gravity = -15f;

    [SerializeField]
    private float m_maxFallSpeed = 30f;

    private float m_verticalVelocity;

    public void Move(Vector2 movementInput)
    {
        transform.Rotate(
            Vector3.up, movementInput.x * Time.deltaTime * m_rotationSpeed);

        // Ground check
        if (m_characterController.isGrounded &&
            m_verticalVelocity < 0f)
        {
            m_verticalVelocity = -2f;
        }

        // Apply gravity with maximum fall speed
        m_verticalVelocity += m_gravity * Time.deltaTime;

        m_verticalVelocity = Mathf.Max(
            m_verticalVelocity,
            -m_maxFallSpeed
        );

        // Calculate horizontal movement
        Vector3 horizontalMovement =
            m_characterController.transform.forward *
            movementInput.y *
            m_movementSpeed;

        // Combine horizontal and vertical movement
        Vector3 finalMovement = horizontalMovement;

        finalMovement.y = m_verticalVelocity;

        // Apply movement once per frame
        m_characterController.Move(
            finalMovement * Time.deltaTime
        );

        m_animator.SetFloat("Movement", Mathf.Abs(movementInput.y));
    }
}
