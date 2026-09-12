using UnityEngine;

// This script goes on the CAR. It detects when the player is nearby
// and lets them get in/out by pressing a key, switching cameras accordingly.
public class VehicleEntry : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public Transform exitPoint; // An empty object next to the car where the player appears when getting out

    [Header("Cameras")]
    public GameObject playerCamera; // The camera used while walking (has PlayerCameraFollow)
    public GameObject carCamera;    // The camera used while driving (has CameraFollow, targets the Car)

    private CarController carController;
    private GameObject nearbyPlayer;   // Reference to the player ONLY while inside the detection area
    private GameObject currentPlayer;  // Reference to the player WHILE they're driving
    private PlayerController playerController;

    private bool isDriving = false;

    void Start()
    {
        carController = GetComponent<CarController>();

        // While nobody is driving, the car shouldn't respond to input
        carController.enabled = false;

        // Start with the player camera active, car camera off
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
        // Look for PlayerController on this object OR any of its parents,
        // in case the collider that touched the trigger is a child (like the Capsule)
        // instead of the top-level Player object that holds the script.
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

        if (playerController == null)
        {
            Debug.LogError("VehicleEntry: no PlayerController found on " + player.name);
            isDriving = false;
            return;
        }

        // Turn off on-foot movement and hide the character (they're now "inside" the car)
        playerController.canMove = false;
        player.SetActive(false);

        // Turn on car control
        carController.enabled = true;

        // Switch cameras: hide the player's camera, show the car's camera
        if (playerCamera != null) playerCamera.SetActive(false);
        if (carCamera != null) carCamera.SetActive(true);
    }

    void ExitCar()
    {
        isDriving = false;

        // Reappear the player at the exit point (next to the car)
        currentPlayer.SetActive(true);
        if (exitPoint != null)
        {
            currentPlayer.transform.position = exitPoint.position;
        }
        playerController.canMove = true;

        // Turn off car control so it doesn't keep moving on its own
        carController.enabled = false;

        // Switch cameras back: hide the car's camera, show the player's camera
        if (carCamera != null) carCamera.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);

        currentPlayer = null;
    }
}