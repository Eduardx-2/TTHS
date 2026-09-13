using UnityEngine;

public class WeaponEquipController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject ak47Model;
    public Animator animator;
    public Transform muzzle;
    public PlayerController playerController;

    [Header("Configuración")]
    public KeyCode equipKey = KeyCode.F;
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode aimKey = KeyCode.Mouse1;
    public float transitionSpeed = 10f;

    [Header("Soltar / Recoger")]
    public KeyCode dropKey = KeyCode.X;       // G ya la usa el cambio de marchas del auto
    public KeyCode pickupKey = KeyCode.F;     // Misma tecla que equipar
    public float pickupRange = 2f;
    public float dropForwardForce = 2f;

    [Header("Munición")]
    public int magazineSize = 25;
    public float fireRate = 10f;
    public float reloadTime = 1.4f;
    public float shootRange = 80f;

    [Header("Estado Actual (Debug)")]
    public bool isEquipped = true;
    public bool isDropped = false;
    public int currentAmmo = 25;

    private int weaponLayerIndex;
    private float nextFireTime;
    private float reloadFinishTime;
    private bool isReloading;
    private Camera playerCam;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Rigidbody weaponRigidbody;
    private Collider weaponCollider;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        playerCam = Camera.main;
        currentAmmo = magazineSize;
        weaponLayerIndex = animator != null ? animator.GetLayerIndex("Weapon Layer") : -1;

        if (ak47Model != null)
        {
            ak47Model.SetActive(isEquipped);

            // Guardamos dónde estaba originalmente el arma (en la mano) para poder
            // devolverla ahí exactamente cuando se recoja.
            originalParent = ak47Model.transform.parent;
            originalLocalPosition = ak47Model.transform.localPosition;
            originalLocalRotation = ak47Model.transform.localRotation;

            weaponRigidbody = ak47Model.GetComponent<Rigidbody>();
            weaponCollider = ak47Model.GetComponent<Collider>();
        }

        if (animator != null && weaponLayerIndex >= 0)
        {
            float initialWeight = isEquipped ? 1f : 0f;
            animator.SetLayerWeight(weaponLayerIndex, initialWeight);
            animator.SetBool("IsAiming", false);
            animator.SetBool("IsFiring", false);
        }
    }

    void Update()
    {
        if (animator == null)
        {
            return;
        }

        // Soltar el arma al piso
        if (isEquipped && !isDropped && Input.GetKeyDown(dropKey))
        {
            DropWeapon();
        }

        // Recoger el arma del piso, si estás lo bastante cerca
        if (isDropped && Input.GetKeyDown(pickupKey))
        {
            float distance = Vector3.Distance(transform.position, ak47Model.transform.position);
            if (distance <= pickupRange)
            {
                PickupWeapon();
            }
        }

        bool canUseWeapon = isEquipped && !isDropped && (playerController == null || playerController.canMove);

        if (!isDropped && Input.GetKeyDown(equipKey) && (playerController == null || playerController.canMove))
        {
            isEquipped = !isEquipped;

            if (ak47Model != null)
            {
                ak47Model.SetActive(isEquipped);
            }

            if (!isEquipped)
            {
                animator.SetBool("IsAiming", false);
                animator.SetBool("IsFiring", false);
                isReloading = false;
            }
        }

        if (weaponLayerIndex >= 0)
        {
            float targetWeight = isEquipped ? 1f : 0f;
            float currentWeight = animator.GetLayerWeight(weaponLayerIndex);
            float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * transitionSpeed);
            animator.SetLayerWeight(weaponLayerIndex, newWeight);
        }

        if (!canUseWeapon)
        {
            animator.SetBool("IsAiming", false);
            animator.SetBool("IsFiring", false);
            return;
        }

        bool isAiming = Input.GetKey(aimKey);
        animator.SetBool("IsAiming", isAiming);

        bool wantsToFire = Input.GetKey(fireKey);

        if (isReloading)
        {
            animator.SetBool("IsFiring", false);
            if (Time.time >= reloadFinishTime)
            {
                currentAmmo = magazineSize;
                isReloading = false;
            }

            return;
        }

        if (wantsToFire && currentAmmo > 0 && Time.time >= nextFireTime)
        {
            Fire();
        }
        else if (!wantsToFire)
        {
            animator.SetBool("IsFiring", false);
            if (currentAmmo <= 0)
            {
                StartReload();
            }
        }
        else
        {
            animator.SetBool("IsFiring", false);
        }
    }

    void Fire()
    {
        currentAmmo--;
        nextFireTime = Time.time + (1f / Mathf.Max(0.1f, fireRate));
        animator.SetBool("IsFiring", true);

        Vector3 origin;
        Vector3 direction;
        if (muzzle != null)
        {
            origin = muzzle.position;
            direction = muzzle.forward;
        }
        else if (playerCam != null)
        {
            origin = playerCam.transform.position;
            direction = playerCam.transform.forward;
        }
        else
        {
            origin = transform.position + Vector3.up * 1.5f;
            direction = transform.forward;
        }

        Physics.Raycast(origin, direction, out _, shootRange, ~0, QueryTriggerInteraction.Ignore);
    }

    void StartReload()
    {
        isReloading = true;
        reloadFinishTime = Time.time + reloadTime;
        animator.SetBool("IsFiring", false);
    }

    void DropWeapon()
    {
        if (ak47Model == null)
        {
            return;
        }

        isEquipped = false;
        isDropped = true;
        isReloading = false;

        if (animator != null)
        {
            animator.SetBool("IsAiming", false);
            animator.SetBool("IsFiring", false);
        }

        // La desprendemos de la mano y la dejamos en el mundo, en su posición/rotación actual
        ak47Model.transform.SetParent(null, true);

        if (weaponRigidbody == null)
        {
            weaponRigidbody = ak47Model.AddComponent<Rigidbody>();
        }
        weaponRigidbody.isKinematic = false;
        weaponRigidbody.useGravity = true;

        if (weaponCollider == null)
        {
            weaponCollider = ak47Model.AddComponent<BoxCollider>();
        }
        weaponCollider.enabled = true;

        weaponRigidbody.AddForce(transform.forward * dropForwardForce, ForceMode.VelocityChange);
    }

    void PickupWeapon()
    {
        if (ak47Model == null)
        {
            return;
        }

        if (weaponRigidbody != null)
        {
            weaponRigidbody.isKinematic = true;
            weaponRigidbody.useGravity = false;
        }
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }

        ak47Model.transform.SetParent(originalParent, false);
        ak47Model.transform.localPosition = originalLocalPosition;
        ak47Model.transform.localRotation = originalLocalRotation;

        isDropped = false;
        isEquipped = true;
        ak47Model.SetActive(true);

        // Recargamos la munición al recogerla de nuevo (opcional, quítalo si no lo quieres así)
        currentAmmo = magazineSize;
    }
}