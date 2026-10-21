using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    private Camera m_camera;
    Vector3 m_offsetFromPlayer;
    Vector3 m_originPosition;

    override public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
            return;
        m_camera = Camera.main;
        m_originPosition = m_camera.transform.position;
        m_offsetFromPlayer = transform.position - m_camera.transform.position;
    } 
    override public void OnNetworkDespawn()
    {
        if (IsOwner && m_camera != null)
            m_camera.transform.position = m_originPosition; 
        base.OnNetworkDespawn();
    }
    private void LateUpdate()
    {
        if (IsOwner && m_camera != null)
            m_camera.transform.position = transform.position - m_offsetFromPlayer;
    }
}
