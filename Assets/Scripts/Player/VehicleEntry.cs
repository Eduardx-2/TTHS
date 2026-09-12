using UnityEngine;

public class VehicleEntry : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public Transform exitPoint;

    [Header("Cameras")]
    public GameObject playerCamera; 
    public GameObject carCamera; 
    private CarController carController;
    private GameObject nearbyPlayer;  
    private GameObject currentPlayer;  
    private PlayerController playerController;

    private bool isDriving = false;

    void Start()
    {
        carController = GetComponent<CarController>();

        carController.enabled = false;

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

        if (playerController == null)
        {
            Debug.LogError("VehicleEntry: no PlayerController found on " + player.name);
            isDriving = false;
            return;
        }

        playerController.canMove = false;
        player.SetActive(false);

        // Turn on car control
        carController.enabled = true;

        if (playerCamera != null) playerCamera.SetActive(false);
        if (carCamera != null) carCamera.SetActive(true);
    }

    void ExitCar()
    {
        isDriving = false;

        currentPlayer.SetActive(true);
        if (exitPoint != null)
        {
            currentPlayer.transform.position = exitPoint.position;
        }
        playerController.canMove = true;

        carController.enabled = false;

        if (carCamera != null) carCamera.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);

        currentPlayer = null;
    }
}