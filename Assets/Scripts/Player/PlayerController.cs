using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float turnSpeed = 10f;
    public float gravity = -9.81f;
    public PlayerCamera cameraFollow;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    [HideInInspector] public bool canMove = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!canMove)
        {
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float cameraYaw = cameraFollow != null ? cameraFollow.GetYaw() : transform.eulerAngles.y;
            Quaternion cameraRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            Vector3 moveDirection = cameraRotation * inputDirection;

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}