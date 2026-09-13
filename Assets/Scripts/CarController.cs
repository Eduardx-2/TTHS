using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 15f;          // Velocidad máxima hacia adelante
    public float reverseSpeed = 7f;    // Velocidad máxima en reversa
    public float turnSpeed = 100f;

    [Header("Aceleración / Desaceleración")]
    public float aceleracion = 20f;    // Qué tan rápido llega a la velocidad máxima (unidades por segundo)
    public float desaceleracion = 15f; // Qué tan rápido frena cuando sueltas la tecla

    private Rigidbody rb;
    private TrackVell trackVell;
    private float moveInput;
    private float currentSpeed;        // Velocidad ACTUAL del auto, va cambiando poco a poco
    private float turnInput;

    public float CurrentSpeed => currentSpeed;
    public float SteerInput => turnInput;
    public float TurnSpeed => turnSpeed;
    public Vector3 WorldVelocity => rb != null ? rb.linearVelocity : Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        trackVell = GetComponent<TrackVell>();
        rb.freezeRotation = true;

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    void Update()
    {
        float rawVertical = Input.GetAxis("Vertical");

        if (trackVell != null && Mathf.Abs(currentSpeed) < 0.6f)
        {
            if (rawVertical < -0.1f)
            {
                trackVell.SetGear(-1);
            }
            else if (rawVertical > 0.1f && (trackVell.IsReverseGear() || trackVell.IsNeutralGear()))
            {
                trackVell.SetGear(1);
            }
        }

        float activeForwardSpeed = speed;
        float activeReverseSpeed = reverseSpeed;
        float activeAcceleration = aceleracion;

        if (trackVell != null)
        {
            activeForwardSpeed = trackVell.GetMaxForwardSpeedForGear();
            activeReverseSpeed = trackVell.GetMaxReverseSpeed();
            activeAcceleration = trackVell.GetAccelerationForGear();
            if (trackVell.IsReverseGear())
            {
                activeAcceleration = Mathf.Max(activeAcceleration, 12f);
            }
        }

        // targetSpeed es la velocidad a la que QUEREMOS llegar (no la actual)
        float targetSpeed = 0f;
        if (rawVertical > 0f)
        {
            targetSpeed = rawVertical * Mathf.Max(0f, activeForwardSpeed);
        }
        else if (rawVertical < 0f)
        {
            if (trackVell == null || trackVell.IsReverseGear())
            {
                targetSpeed = rawVertical * Mathf.Max(0f, activeReverseSpeed);
            }
        }

        // Si el jugador está presionando algo, aceleramos; si no, desaceleramos
        if (Mathf.Abs(rawVertical) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, activeAcceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, desaceleracion * Time.deltaTime);
        }

        moveInput = currentSpeed;
        turnInput = Input.GetAxis("Horizontal") * turnSpeed;
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = transform.forward * moveInput;
        moveDirection.y = rb.linearVelocity.y;
        rb.linearVelocity = moveDirection;

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float giroFinal = turnInput;

            if (moveInput < 0)
            {
                giroFinal = -turnInput;
            }

            Quaternion turnRotation = Quaternion.Euler(0f, giroFinal * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
}