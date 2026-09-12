using UnityEngine;

// Third-person camera that follows the PLAYER (on foot) and orbits with the mouse.
// Put this script on a SEPARATE camera object used only while walking,
// or swap it in on the Main Camera when the player gets out of the car.
public class PlayerCamera : MonoBehaviour
{
    public Transform target;           // Drag the Capsule (Player) here
    public float distance = 6f;        // How far the camera stays behind the player
    public float height = 2f;          // How high above the target the camera sits
    public float mouseSensitivity = 3f;
    public float minPitch = -20f;      // How far down you can look
    public float maxPitch = 60f;       // How far up you can look

    private float yaw;                 // Horizontal rotation (left/right)
    private float pitch = 15f;         // Vertical rotation (up/down)

    void Start()
    {
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

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * distance) + Vector3.up * height;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
    }

    // Gives PlayerController access to the camera's horizontal facing direction
    public float GetYaw()
    {
        return yaw;
    }
}