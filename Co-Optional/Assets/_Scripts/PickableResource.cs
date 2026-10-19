using UnityEngine;

public class PickableResource : PickableBase
{
    protected override void ApplyAvailabilityState(bool newValue)
    {
        //no code
    }

    protected override void OnPickedUp()
    {
        if(!IsServer)
            return; 
        NetworkObject.Despawn();
    }
}
