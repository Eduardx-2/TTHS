using UnityEngine;
using TMPro;

public class TrackVell : MonoBehaviour
{
    [Header("UI Marcha")]
    public TextMeshProUGUI gearText;

    [Header("Cambios de marcha")]
    public KeyCode gearUpKey = KeyCode.T;
    public KeyCode gearDownKey = KeyCode.G;
    public float shiftCooldown = 0.2f;

    [Header("Rango de marchas")]
    [Tooltip("-1 = Reversa, 0 = Neutro, 1..N = marchas hacia adelante")]
    public int minGear = -1;
    public int maxGear = 5;
    public int currentGear = 1;

    [Header("Velocidad por marcha")]
    public float reverseSpeedCap = 7f;
    public float[] forwardSpeedCaps = { 8f, 14f, 20f, 26f, 32f };

    [Header("Aceleracion por marcha")]
    public float reverseAcceleration = 12f;
    public float neutralAcceleration = 0f;
    public float[] forwardAccelerations = { 16f, 18f, 20f, 22f, 24f };

    private float lastShiftTime = -999f;
    private CarController carController;

    void Awake()
    {
        carController = GetComponent<CarController>();
        currentGear = Mathf.Clamp(currentGear, minGear, maxGear);

        if (gearText != null)
        {
            gearText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (carController != null && !carController.enabled)
        {
            if (gearText != null && gearText.gameObject.activeSelf)
            {
                gearText.gameObject.SetActive(false);
            }
            return;
        }

        if (gearText != null && !gearText.gameObject.activeSelf)
        {
            gearText.gameObject.SetActive(true);
        }

        UpdateGearUI();

        if (!CanShift())
        {
            return;
        }

        if (Input.GetKeyDown(gearUpKey))
        {
            ShiftUp();
        }
        else if (Input.GetKeyDown(gearDownKey))
        {
            ShiftDown();
        }
    }

    void UpdateGearUI()
    {
        if (gearText == null) return;

        string gearDisplay;
        if (currentGear < 0)
        {
            gearDisplay = "R";
        }
        else if (currentGear == 0)
        {
            gearDisplay = "N";
        }
        else
        {
            gearDisplay = currentGear.ToString();
        }

        gearText.text = $"MARCHA: {gearDisplay}";
    }

    bool CanShift()
    {
        return Time.time >= lastShiftTime + shiftCooldown;
    }

    public void ShiftUp()
    {
        if (currentGear >= maxGear)
        {
            return;
        }

        currentGear++;
        lastShiftTime = Time.time;
    }

    public void ShiftDown()
    {
        if (currentGear <= minGear)
        {
            return;
        }

        currentGear--;
        lastShiftTime = Time.time;
    }

    public void SetGear(int gear)
    {
        currentGear = Mathf.Clamp(gear, minGear, maxGear);
    }

    public int GetCurrentGear()
    {
        return currentGear;
    }

    public bool IsReverseGear()
    {
        return currentGear < 0;
    }

    public bool IsNeutralGear()
    {
        return currentGear == 0;
    }

    public float GetMaxForwardSpeedForGear()
    {
        if (currentGear <= 0)
        {
            return 0f;
        }
        if (forwardSpeedCaps == null || forwardSpeedCaps.Length == 0)
        {
            return 0f;
        }

        int index = Mathf.Clamp(currentGear - 1, 0, forwardSpeedCaps.Length - 1);
        return forwardSpeedCaps[index];
    }

    public float GetMaxReverseSpeed()
    {
        return reverseSpeedCap;
    }

    public float GetAccelerationForGear()
    {
        if (currentGear < 0)
        {
            return reverseAcceleration;
        }

        if (currentGear == 0)
        {
            return neutralAcceleration;
        }
        if (forwardAccelerations == null || forwardAccelerations.Length == 0)
        {
            return neutralAcceleration;
        }

        int index = Mathf.Clamp(currentGear - 1, 0, forwardAccelerations.Length - 1);
        return forwardAccelerations[index];
    }
}