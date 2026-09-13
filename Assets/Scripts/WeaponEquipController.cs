using UnityEngine;

public class WeaponEquipController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject ak47Model;
    public Animator animator;

    [Header("Configuración")]
    public KeyCode equipKey = KeyCode.F;
    private KeyCode aimKey = KeyCode.Mouse1; // Click derecho para apuntar
    public float transitionSpeed = 10f;

    [Header("Estado Actual (Debug)")]
    public bool isEquipped = true;
    private int weaponLayerIndex;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        // Busca el índice de la capa del arma (asegúrate de que se llame exactamente igual en el Animator)
        weaponLayerIndex = animator.GetLayerIndex("Weapon Layer");

        if (ak47Model != null)
            ak47Model.SetActive(isEquipped);

        // Estado inicial del peso y del parámetro de apuntado
        float initialWeight = isEquipped ? 1f : 0f;
        animator.SetLayerWeight(weaponLayerIndex, initialWeight);
        animator.SetBool("IsAiming", false);
    }

    void Update()
    {
        // 1. Detectar si equipamos o guardamos el arma con la tecla F
        if (Input.GetKeyDown(equipKey))
        {
            isEquipped = !isEquipped;
            Debug.Log(">>> La tecla F fue presionada. Nuevo estado de isEquipped: " + isEquipped);

            if (ak47Model != null)
            {
                ak47Model.SetActive(isEquipped);
            }

            // Si guardamos el arma, nos aseguramos de cancelar el estado de apuntado
            if (!isEquipped && animator != null)
            {
                animator.SetBool("IsAiming", false);
            }
        }

        // 2. Transición suave del peso de la capa del arma (de 0 a 1 o de 1 a 0)
        float targetWeight = isEquipped ? 1f : 0f;
        float currentWeight = animator.GetLayerWeight(weaponLayerIndex);
        float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * transitionSpeed);
        animator.SetLayerWeight(weaponLayerIndex, newWeight);

        // 3. Detectar si estamos apuntando (solo si el arma está equipada)
        if (isEquipped && animator != null)
        {
            bool isAiming = Input.GetKey(aimKey);
            animator.SetBool("IsAiming", isAiming);
        }
    }
}