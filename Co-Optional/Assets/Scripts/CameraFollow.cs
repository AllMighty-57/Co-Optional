using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private Vector3 lookTargetOffset = new Vector3(0f, 1f, 0f);

    private Camera m_camera;
    private Vector3 m_cameraOffset;
    private Vector3 m_originPosition;
    private Quaternion m_originRotation;

    private float yaw;
    private float pitch;

    override public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
            return;
        m_camera = Camera.main;
        if (m_camera == null)
        {
            Debug.LogError("No camera tagged MainCamera was found.");
            return;
        }

        m_originPosition = m_camera.transform.position;
        m_originRotation = m_camera.transform.rotation;

        // Camera position relative to the player.
        m_cameraOffset = m_camera.transform.position - transform.position;

        // Start with the current camera angle.
        Vector3 horizontalOffset = new Vector3(
            m_cameraOffset.x,
            0f,
            m_cameraOffset.z
        );

        yaw = Mathf.Atan2(horizontalOffset.x, horizontalOffset.z) * Mathf.Rad2Deg;

        float horizontalDistance = horizontalOffset.magnitude;
        pitch = Mathf.Atan2(
            m_cameraOffset.y,
            horizontalDistance
        ) * Mathf.Rad2Deg;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    } 
    
    private void LateUpdate()
    {
        if (!IsOwner || m_camera == null)
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        m_camera.transform.position =
            transform.position + cameraRotation * m_cameraOffset;

        Vector3 lookTarget = transform.position + lookTargetOffset;

        m_camera.transform.LookAt(lookTarget);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    override public void OnNetworkDespawn()
    {
        if (IsOwner && m_camera != null)
        {
            m_camera.transform.position = m_originPosition;
            m_camera.transform.rotation = m_originRotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        base.OnNetworkDespawn();
    }
}
