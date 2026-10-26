using System;
using UnityEngine;

public class FallenOff : MonoBehaviour
{
    [SerializeField]
    private GameObject m_object;
    private Rigidbody m_objectRb;

    public bool HasFallen;
    public event Action OnFellStateChanged;

    private void Start()
    {
        m_objectRb = m_object.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (m_object.transform.position.y <= -35)
        {
            m_objectRb.constraints = RigidbodyConstraints.FreezePosition;  
            HasFallen = true;

            OnFellStateChanged?.Invoke();

        }
    }
}
