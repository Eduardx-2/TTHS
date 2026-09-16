using UnityEngine;

public class WeaponEquipController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject weaponModel;
    public WeaponData weaponData;
    public WeaponInventory weaponInventory;
    public Animator animator;
    public Transform muzzle;
    public PlayerController playerController;

    [Header("Configuración")]
    public KeyCode equipKey = KeyCode.F;
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode aimKey = KeyCode.Mouse1;
    public float transitionSpeed = 10f;

    [Header("Soltar / Recoger")]
    public KeyCode dropKey = KeyCode.X;
    public KeyCode pickupKey = KeyCode.F;
    public float pickupRange = 2f;
    public float dropForwardForce = 2f;

    [Header("Munición (fallback si weaponData es null)")]
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

    private int EffectiveMagazineSize => weaponData != null ? weaponData.magazineSize : magazineSize;
    private float EffectiveFireRate => weaponData != null ? weaponData.fireRate : fireRate;
    private float EffectiveReloadTime => weaponData != null ? weaponData.reloadTime : reloadTime;
    private float EffectiveShootRange => weaponData != null ? weaponData.shootRange : shootRange;
    private FireMode EffectiveFireMode => weaponData != null ? weaponData.fireMode : FireMode.Auto;

    void OnEnable()
    {
        if (weaponInventory != null)
        {
            weaponInventory.OnWeaponChanged += HandleWeaponChanged;
        }
    }

    void OnDisable()
    {
        if (weaponInventory != null)
        {
            weaponInventory.OnWeaponChanged -= HandleWeaponChanged;
        }
    }

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

        if (weaponInventory == null)
        {
            weaponInventory = GetComponent<WeaponInventory>();
        }

        playerCam = Camera.main;
        RefreshWeaponData();
        currentAmmo = EffectiveMagazineSize;
        weaponLayerIndex = animator != null ? animator.GetLayerIndex("Weapon Layer") : -1;

        if (weaponModel != null)
        {
            weaponModel.SetActive(isEquipped);

            originalParent = weaponModel.transform.parent;
            originalLocalPosition = weaponModel.transform.localPosition;
            originalLocalRotation = weaponModel.transform.localRotation;

            weaponRigidbody = weaponModel.GetComponent<Rigidbody>();
            weaponCollider = weaponModel.GetComponent<Collider>();
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

        if (isEquipped && !isDropped && Input.GetKeyDown(dropKey))
        {
            DropWeapon();
        }

        if (isDropped && weaponModel != null && Input.GetKeyDown(pickupKey))
        {
            float distance = Vector3.Distance(transform.position, weaponModel.transform.position);
            if (distance <= pickupRange)
            {
                PickupWeapon();
            }
        }

        bool canUseWeapon = isEquipped && !isDropped && (playerController == null || playerController.canMove);

        if (!isDropped && Input.GetKeyDown(equipKey) && (playerController == null || playerController.canMove))
        {
            isEquipped = !isEquipped;

            if (weaponModel != null)
            {
                weaponModel.SetActive(isEquipped);
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

        bool wantsToFire = EffectiveFireMode == FireMode.Semi
            ? Input.GetKeyDown(fireKey)
            : Input.GetKey(fireKey);

        if (isReloading)
        {
            animator.SetBool("IsFiring", false);
            if (Time.time >= reloadFinishTime)
            {
                currentAmmo = EffectiveMagazineSize;
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

    void RefreshWeaponData()
    {
        if (weaponInventory != null)
        {
            WeaponData inventoryWeapon = weaponInventory.GetCurrentWeapon();
            if (inventoryWeapon != null)
            {
                weaponData = inventoryWeapon;
            }
        }
    }

    void HandleWeaponChanged(WeaponData newWeapon)
    {
        if (newWeapon == null || isDropped)
        {
            return;
        }

        weaponData = newWeapon;
        isReloading = false;
        currentAmmo = EffectiveMagazineSize;
    }

    void Fire()
    {
        currentAmmo--;
        nextFireTime = Time.time + (1f / Mathf.Max(0.1f, EffectiveFireRate));
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

        Physics.Raycast(origin, direction, out _, EffectiveShootRange, ~0, QueryTriggerInteraction.Ignore);
    }

    void StartReload()
    {
        isReloading = true;
        reloadFinishTime = Time.time + EffectiveReloadTime;
        animator.SetBool("IsFiring", false);
    }

    void DropWeapon()
    {
        if (weaponModel == null)
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

        weaponModel.transform.SetParent(null, true);

        if (weaponRigidbody == null)
        {
            weaponRigidbody = weaponModel.AddComponent<Rigidbody>();
        }
        weaponRigidbody.isKinematic = false;
        weaponRigidbody.useGravity = true;

        if (weaponCollider == null)
        {
            weaponCollider = weaponModel.AddComponent<BoxCollider>();
        }
        weaponCollider.enabled = true;

        weaponRigidbody.AddForce(transform.forward * dropForwardForce, ForceMode.VelocityChange);
    }

    public void PickupWeapon()
    {
        if (weaponModel == null)
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

        weaponModel.transform.SetParent(originalParent, false);
        weaponModel.transform.localPosition = originalLocalPosition;
        weaponModel.transform.localRotation = originalLocalRotation;

        isDropped = false;
        isEquipped = true;
        weaponModel.SetActive(true);

        currentAmmo = EffectiveMagazineSize;
    }
}