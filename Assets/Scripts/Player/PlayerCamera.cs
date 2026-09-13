using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float height = 2f;
    public float mouseSensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Aim Settings")]
    public Vector3 aimOffset = new Vector3(0.6f, 1.4f, -1.2f);
    public float normalFOV = 60f;
    public float aimFOV = 40f;
    public float smoothSpeed = 15f;

    private float yaw;
    private float pitch = 15f;
    private Camera cam;
    [HideInInspector] public bool isAiming;

    void Start()
    {
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Press Escape once to free the cursor permanently (for clicking Editor UI, like Pause).
        if (Input.GetKeyDown(KeyCode.Escape) && Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

#if UNITY_EDITOR
        // Press P to pause the Editor instantly via keyboard, without needing to
        // release the mouse button (so you can pause mid-aim without the pose changing first).
        if (Input.GetKeyDown(KeyCode.P))
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
#endif

        if (Cursor.lockState != CursorLockMode.Locked) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        isAiming = Input.GetMouseButton(1);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition;

        if (isAiming)
        {
            targetPosition = target.position + targetRotation * aimOffset;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, aimFOV, Time.deltaTime * smoothSpeed);
            target.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        else
        {
            targetPosition = target.position - (targetRotation * Vector3.forward * distance) + Vector3.up * height;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalFOV, Time.deltaTime * smoothSpeed);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);

        if (!isAiming)
        {
            transform.LookAt(target.position + Vector3.up * height * 0.5f);
        }
    }

    public float GetYaw()
    {
        return yaw;
    }
}