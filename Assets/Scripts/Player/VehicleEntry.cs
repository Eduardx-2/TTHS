using UnityEngine;

public class VehicleEntry : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public Transform exitPoint;

    [Header("Cameras")]
    public GameObject playerCamera; 
    public GameObject carCamera; 

    [Header("Salida en movimiento")]
    public float minEjectSpeed = 4f;
    public float lateralEjectForce = 4f;
    public float upwardEjectForce = 2f;
    public float forwardEjectForce = 2f;

    private CarController carController;
    private Rigidbody vehicleRigidbody;
    private GameObject nearbyPlayer;  
    private GameObject currentPlayer;  
    private PlayerController playerController;
    private PlayerRagdollController playerRagdollController;

    private bool isDriving = false;

    void Start()
    {
        carController = GetComponent<CarController>();
        vehicleRigidbody = GetComponent<Rigidbody>();

        carController.enabled = false;
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = true;
        }

        if (playerCamera != null) playerCamera.SetActive(true);
        if (carCamera != null) carCamera.SetActive(false);
    }

    void Update()
    {
        if (!isDriving && nearbyPlayer != null && Input.GetKeyDown(interactKey))
        {
            EnterCar(nearbyPlayer);
        }
        else if (isDriving && Input.GetKeyDown(interactKey))
        {
            ExitCar();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController foundController = other.GetComponentInParent<PlayerController>();
        if (foundController != null)
        {
            nearbyPlayer = foundController.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController foundController = other.GetComponentInParent<PlayerController>();
        if (foundController != null && foundController.gameObject == nearbyPlayer)
        {
            nearbyPlayer = null;
        }
    }

    void EnterCar(GameObject player)
    {
        isDriving = true;
        currentPlayer = player;
        playerController = player.GetComponent<PlayerController>();
        playerRagdollController = player.GetComponent<PlayerRagdollController>();

        if (playerController == null)
        {
            Debug.LogError("VehicleEntry: no PlayerController found on " + player.name);
            isDriving = false;
            return;
        }

        playerController.canMove = false;
        player.SetActive(false);

        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = false;
        }

        carController.enabled = true;

        if (playerCamera != null) playerCamera.SetActive(false);
        if (carCamera != null) carCamera.SetActive(true);
    }

    void ExitCar()
    {
        if (currentPlayer == null || playerController == null)
        {
            return;
        }

        isDriving = false;
        Vector3 vehicleVelocity = carController != null ? carController.WorldVelocity : Vector3.zero;
        float vehicleSpeed = vehicleVelocity.magnitude;

        currentPlayer.SetActive(true);

        bool shouldEject = vehicleSpeed >= minEjectSpeed && playerRagdollController != null;
        if (shouldEject)
        {
            if (exitPoint != null)
            {
                currentPlayer.transform.position = exitPoint.position;
            }

            Vector3 sideDirection = Vector3.Cross(Vector3.up, transform.forward).normalized;
            Vector3 impulse = sideDirection * lateralEjectForce + transform.forward * forwardEjectForce + Vector3.up * upwardEjectForce;
            playerRagdollController.EnableRagdoll(vehicleVelocity, impulse, gameObject);
        }
        else
        {
            PlacePlayerOutsideVehicle(currentPlayer);
            playerController.canMove = true;
        }

        carController.enabled = false;
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = true;
        }

        if (carCamera != null) carCamera.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);

        currentPlayer = null;
    }

    void PlacePlayerOutsideVehicle(GameObject player)
    {
        CharacterController characterController = player.GetComponent<CharacterController>();
        Vector3 spawnPosition = exitPoint != null ? exitPoint.position : transform.position - transform.right * 2.5f;

        if (characterController != null)
        {
            spawnPosition = FindClearExitPosition(spawnPosition, characterController);
            characterController.enabled = false;
            player.transform.position = spawnPosition;
            characterController.enabled = true;
            return;
        }

        player.transform.position = spawnPosition;
    }

    Vector3 FindClearExitPosition(Vector3 preferredPosition, CharacterController characterController)
    {
        Vector3 side = -transform.right;
        float[] distances = { 0f, 0.5f, 1f, 1.5f, 2f };

        foreach (float distance in distances)
        {
            Vector3 candidate = preferredPosition + side * distance;
            if (!IsCapsuleBlockedByVehicle(candidate, characterController))
            {
                return candidate;
            }
        }

        return preferredPosition + side * 2.5f;
    }

    bool IsCapsuleBlockedByVehicle(Vector3 position, CharacterController characterController)
    {
        Vector3 worldCenter = position + characterController.center;
        float cylinderHalf = Mathf.Max(0f, characterController.height * 0.5f - characterController.radius);
        Vector3 bottom = worldCenter - Vector3.up * cylinderHalf;
        Vector3 top = worldCenter + Vector3.up * cylinderHalf;
        float radius = characterController.radius * 0.95f;

        Collider[] hits = Physics.OverlapCapsule(bottom, top, radius, ~0, QueryTriggerInteraction.Ignore);
        foreach (Collider hit in hits)
        {
            if (hit == null || hit.isTrigger)
            {
                continue;
            }

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }
}