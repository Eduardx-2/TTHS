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
    private float moveInput;
    private float currentSpeed;        // Velocidad ACTUAL del auto, va cambiando poco a poco
    private float turnInput;

    public float CurrentSpeed => currentSpeed;
    public float SteerInput => turnInput;
    public float TurnSpeed => turnSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Bloqueamos que el auto se voltee de lado o hacia adelante (X y Z),
        // pero dejamos libre el eje Y para poder girar con angularVelocity.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        float rawVertical = Input.GetAxis("Vertical");

        // targetSpeed es la velocidad a la que QUEREMOS llegar (no la actual)
        float targetSpeed = rawVertical > 0 ? rawVertical * speed : rawVertical * reverseSpeed;

        // Si el jugador está presionando algo, aceleramos; si no, desaceleramos
        if (Mathf.Abs(rawVertical) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, aceleracion * Time.deltaTime);
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
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float giroFinal = turnInput;

            if (moveInput < 0)
            {
                giroFinal = -turnInput;
            }

            // angularVelocity se integra junto con el resto de la física en el mismo paso,
            // en vez de aplicar un salto de rotación separado (MoveRotation), lo que suele
            // sentirse más fluido en curvas continuas.
            rb.angularVelocity = Vector3.up * (giroFinal * Mathf.Deg2Rad);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }

        // Calculamos la velocidad usando la dirección actual (ya actualizada por angularVelocity
        // del paso anterior, ya que la rotación y la velocidad ahora se integran juntas).
        Vector3 moveDirection = transform.forward * moveInput;
        moveDirection.y = rb.linearVelocity.y;
        rb.linearVelocity = moveDirection;
    }
}