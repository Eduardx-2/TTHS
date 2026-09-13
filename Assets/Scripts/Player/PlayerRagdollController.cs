using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRagdollController : MonoBehaviour
{
    [Header("Ragdoll")]
    public float ragdollDuration = 1.4f;
    public float recoverUprightOffset = 0.9f;
    public float groundRayHeight = 8f;

    private PlayerController playerController;
    private CharacterController characterController;
    private Animator animator;
    private Rigidbody rootRigidbody;
    private Collider rootCollider;

    private readonly List<Rigidbody> ragdollBodies = new List<Rigidbody>();
    private readonly List<Collider> ragdollColliders = new List<Collider>();
    private readonly List<Collider> ignoredCarColliders = new List<Collider>();
    private Coroutine recoverRoutine;
    private bool usingRootFallback;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        rootRigidbody = GetComponent<Rigidbody>();
        rootCollider = GetComponent<Collider>();

        CacheRagdollParts();
        SetRagdollState(false);
    }

    void CacheRagdollParts()
    {
        ragdollBodies.Clear();
        ragdollColliders.Clear();

        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody body in bodies)
        {
            if (body.gameObject == gameObject)
            {
                continue;
            }

            ragdollBodies.Add(body);
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject || col is CharacterController)
            {
                continue;
            }

            ragdollColliders.Add(col);
        }
    }

    public void EnableRagdoll(Vector3 inheritVelocity, Vector3 impulse, GameObject ignoreCollisionsWith = null)
    {
        if (recoverRoutine != null)
        {
            StopCoroutine(recoverRoutine);
        }

        if (playerController != null)
        {
            playerController.canMove = false;
            playerController.ResetVerticalVelocity();
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        EnsureFallbackRootBodyIfNeeded();
        SetRagdollState(true);
        SetIgnoreCarCollisions(ignoreCollisionsWith, true);

        inheritVelocity.y = Mathf.Max(inheritVelocity.y, 0f);
        impulse.y = Mathf.Max(impulse.y, 1.5f);

        if (ragdollBodies.Count > 0)
        {
            foreach (Rigidbody rb in ragdollBodies)
            {
                rb.linearVelocity = inheritVelocity;
            }

            ragdollBodies[0].AddForce(impulse, ForceMode.VelocityChange);
        }
        else if (rootRigidbody != null)
        {
            rootRigidbody.linearVelocity = inheritVelocity;
            rootRigidbody.AddForce(impulse, ForceMode.VelocityChange);
            rootRigidbody.AddTorque(transform.right * 8f, ForceMode.VelocityChange);
        }

        recoverRoutine = StartCoroutine(RecoverAfterDelay());
    }

    IEnumerator RecoverAfterDelay()
    {
        yield return new WaitForSeconds(ragdollDuration);
        DisableRagdollAtCurrentPose();
        recoverRoutine = null;
    }

    public void DisableRagdollAtCurrentPose()
    {
        Vector3 recoverPosition = transform.position;
        if (ragdollBodies.Count > 0)
        {
            recoverPosition = ragdollBodies[0].position;
        }
        else if (rootRigidbody != null)
        {
            recoverPosition = rootRigidbody.position;
        }

        SetRagdollState(false);
        SetIgnoreCarCollisions(null, false);

        recoverPosition = SnapToGround(recoverPosition);
        transform.position = recoverPosition;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        if (animator != null)
        {
            animator.enabled = true;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (playerController != null)
        {
            playerController.ResetVerticalVelocity();
            playerController.canMove = true;
        }
    }

    Vector3 SnapToGround(Vector3 position)
    {
        LayerMask mask = playerController != null ? playerController.GroundMask : ~0;
        Vector3 origin = new Vector3(position.x, position.y + groundRayHeight, position.z);
        float controllerHeight = characterController != null ? characterController.height * 0.5f : recoverUprightOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayHeight + 20f, mask, QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * (controllerHeight + 0.08f);
        }

        return new Vector3(position.x, Mathf.Max(position.y, 0.1f) + recoverUprightOffset, position.z);
    }

    void SetIgnoreCarCollisions(GameObject car, bool ignore)
    {
        if (!ignore)
        {
            foreach (Collider carCol in ignoredCarColliders)
            {
                if (carCol == null)
                {
                    continue;
                }

                ApplyIgnore(carCol, false);
            }

            ignoredCarColliders.Clear();
            return;
        }

        if (car == null)
        {
            return;
        }

        ignoredCarColliders.Clear();
        Collider[] carCols = car.GetComponentsInChildren<Collider>(true);
        foreach (Collider carCol in carCols)
        {
            if (carCol == null || carCol.isTrigger)
            {
                continue;
            }

            ignoredCarColliders.Add(carCol);
            ApplyIgnore(carCol, true);
        }
    }

    void ApplyIgnore(Collider carCol, bool ignore)
    {
        if (characterController != null)
        {
            Physics.IgnoreCollision(characterController, carCol, ignore);
        }

        if (rootCollider != null)
        {
            Physics.IgnoreCollision(rootCollider, carCol, ignore);
        }

        foreach (Collider playerCol in ragdollColliders)
        {
            if (playerCol == null)
            {
                continue;
            }

            Physics.IgnoreCollision(playerCol, carCol, ignore);
        }
    }

    void SetRagdollState(bool enabled)
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = !enabled;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = enabled;
        }

        if (rootRigidbody != null)
        {
            rootRigidbody.isKinematic = !enabled;
            rootRigidbody.useGravity = enabled;
        }

        if (rootCollider != null)
        {
            rootCollider.enabled = enabled && usingRootFallback;
        }
    }

    void EnsureFallbackRootBodyIfNeeded()
    {
        usingRootFallback = ragdollBodies.Count == 0;
        if (!usingRootFallback)
        {
            return;
        }

        if (rootRigidbody == null)
        {
            rootRigidbody = gameObject.AddComponent<Rigidbody>();
            rootRigidbody.mass = 70f;
        }

        if (rootCollider == null)
        {
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 1.8f;
            capsule.radius = 0.35f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            rootCollider = capsule;
        }
    }
}
