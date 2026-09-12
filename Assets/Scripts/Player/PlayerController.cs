using UnityEngine;

// Controls the character on foot.
// Requires a CharacterController component on the same object.
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float turnSpeed = 10f;
    public float gravity = -9.81f;
    public PlayerCamera cameraFollow;  // Drag the Main Camera here (needs the CameraFollow script)

    private CharacterController controller;
    private Vector3 verticalVelocity;

    // This gets turned off when the player enters the car, so they stop moving on foot
    [HideInInspector] public bool canMove = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!canMove)
        {
            // If they're driving the car, skip on-foot movement
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Turn the raw input into a direction relative to where the camera is facing,
            // instead of always moving along the world's fixed X/Z axes
            float cameraYaw = cameraFollow != null ? cameraFollow.GetYaw() : transform.eulerAngles.y;
            Quaternion cameraRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            Vector3 moveDirection = cameraRotation * inputDirection;

            // Turn the character to face the direction it's moving
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            controller.Move(moveDirection * walkSpeed * Time.deltaTime);
        }

        // Simple gravity so the character doesn't float
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}