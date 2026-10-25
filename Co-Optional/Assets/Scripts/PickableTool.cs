using Unity.Netcode.Components;
using UnityEngine;

public class PickableTool : PickableBase
{
    [SerializeField]
    private ComponentController m_componentController;
    protected override void ApplyAvailabilityState(bool newValue)
    {
        if(IsServer)
            m_componentController.SetEnabled(newValue);
    }

    protected override void OnPickedUp()
    {
        //no code
    } 
    public void Drop(Vector3 position)
    {
        if (!IsServer)
        {
            return;
        }
        transform.position = new Vector3(position.x, transform.position.y, position.z + 2);
        m_isAvailable.Value = true;
        
    }
}
