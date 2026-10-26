using Unity.Netcode.Components;
using UnityEngine;

public class PickableTool : PickableBase
{
    [SerializeField]
    private ComponentController m_componentController;
    [SerializeField] 
    private Rigidbody m_rigidbody;

    protected override void ApplyAvailabilityState(bool newValue)
    {
        if (m_rigidbody != null)
        {
            m_rigidbody.isKinematic = !newValue;
            m_rigidbody.useGravity = newValue;
        }

        if (IsServer && m_componentController != null)
        {
            m_componentController.SetEnabled(newValue);
        }
    }

    protected override void OnPickedUp()
    {
        if (m_rigidbody != null)
        {
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.isKinematic = true;
            m_rigidbody.useGravity = false;
        }
    } 
    public void Drop(Vector3 position)
    {
        if (!IsServer)
        {
            return;
        }
        transform.position = new Vector3(position.x, transform.position.y, position.z);

        if (m_rigidbody != null)
        {
            m_rigidbody.isKinematic = false;
            m_rigidbody.useGravity = true;
        }

        m_isAvailable.Value = true;
        
    }
}
