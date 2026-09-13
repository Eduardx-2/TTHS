using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float turnSpeed = 10f;
    public float gravity = -9.81f;
    public PlayerCamera cameraFollow;
    public Animator animator;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Jump Cooldown")]
    public float jumpCooldown = 1.2f;
    private float nextJumpTime = 0f;

    [Header("Aim Pitch (inclinación vertical al apuntar)")]
    public float aimPitchMultiplier = 1f;
    public float aimPitchSmoothing = 15f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private bool isGrounded;
    private float currentSpeed;
    private Transform chestBone;
    private float currentAimPitch;

    [HideInInspector] public bool canMove = true;

    public LayerMask GroundMask => groundMask;

    public void ResetVerticalVelocity()
    {
        verticalVelocity = Vector3.zero;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;

        if (animator != null)
        {
            chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
        }
    }

    void Update()
    {
        if (controller == null)
        {
            return;
        }

        if (!controller.enabled)
        {
            if (!canMove)
            {
                return;
            }

            controller.enabled = true;
        }

        if (!canMove)
        {
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool isAiming = animator != null && animator.GetBool("IsAiming");
        bool isSprinting = !isAiming && Input.GetKey(KeyCode.LeftShift);

        if (isGrounded)
        {
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        }

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f || isAiming)
        {
            float cameraYaw = cameraFollow != null ? cameraFollow.GetYaw() : transform.eulerAngles.y;
            Vector3 targetMoveDirection;

            if (isAiming && inputDirection.magnitude < 0.1f)
            {
                targetMoveDirection = Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward;
            }
            else
            {
                Quaternion cameraRotation = Quaternion.Euler(0f, cameraYaw, 0f);
                targetMoveDirection = cameraRotation * inputDirection;
            }

            if (targetMoveDirection != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(targetMoveDirection.x, targetMoveDirection.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            if (inputDirection.magnitude >= 0.1f)
            {
                controller.Move(targetMoveDirection * currentSpeed * Time.deltaTime);
                float animSpeed = (currentSpeed == sprintSpeed) ? 1f : 0.5f;
                animator.SetFloat("Speed", animSpeed);
            }
            else if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
        }
        else if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        if (isGrounded && Input.GetButtonDown("Jump") && !isAiming && Time.time >= nextJumpTime)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            nextJumpTime = Time.time + jumpCooldown;

            if (animator != null)
            {
                if (inputDirection.magnitude >= 0.1f)
                {
                    if (currentSpeed == sprintSpeed)
                    {
                        animator.SetTrigger("SprintJump");
                    }
                    else
                    {
                        animator.SetTrigger("WalkJump");
                    }
                }
                else
                {
                    animator.SetTrigger("IdleJump");
                }
            }
        }

        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void LateUpdate()
    {
        bool isAiming = animator != null && animator.GetBool("IsAiming");

        float targetAimPitch = (isAiming && cameraFollow != null) ? cameraFollow.GetPitch() : 0f;
        currentAimPitch = Mathf.Lerp(currentAimPitch, targetAimPitch, Time.deltaTime * aimPitchSmoothing);

        if (chestBone != null && Mathf.Abs(currentAimPitch) > 0.01f)
        {
            chestBone.RotateAround(chestBone.position, transform.right, -currentAimPitch * aimPitchMultiplier * 0.1f);
        }
    }
}