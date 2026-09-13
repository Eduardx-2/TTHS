using UnityEngine;

public class M1911 : MonoBehaviour
{
    [Header("Estadísticas de la M1911")]
    public float damage = 25f;
    public float range = 50f;
    public int maxAmmo = 7;
    public float fireRate = 0.2f;

    [Header("Referencias")]
    public Transform gunBarrel;
    public Camera mainCamera;
    public Animator animator;

    private int currentAmmo;
    private float nextTimeToFire = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;

        // Si no asignas una cámara, buscará automáticamente tu cámara principal
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        // GetButtonDown requiere que hagas clic por cada disparo (semiautomática)
        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                nextTimeToFire = Time.time + fireRate;
                Shoot();
            }
            else
            {
                Debug.Log("¡Sin munición! Presiona R para recargar.");
            }
        }

        // Recarga manual
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            Reload();
        }
    }

    private void Shoot()
    {
        currentAmmo--;
        Debug.Log("Disparando M1911 | Balas restantes: " + currentAmmo);

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        RaycastHit hit;
        // Lanza un rayo invisible desde el centro de la cámara hacia adelante
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, range))
        {
            Debug.Log("Le diste a: " + hit.transform.name);

            // Aquí es donde añadiremos el daño a los enemigos más adelante
        }
    }

    private void Reload()
    {
        Debug.Log("Recargando M1911...");
        currentAmmo = maxAmmo;

        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }
    }
}