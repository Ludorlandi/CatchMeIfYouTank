using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementDirect : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] private int gamepadIndex = 0; // 0 per Player 1, 1 per Player 2

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private bool moveOnXYPlane = true;
    [SerializeField] private float chargingSpeedMultiplier = 0.5f; // Velocità ridotta durante carica (50%)
    [SerializeField] private float speedTransitionSpeed = 5f; // Velocità della transizione

    [Header("Movement Constraints")]
    [SerializeField] private bool constrainToScreen = true;
    [SerializeField] private float minX = -15f;
    [SerializeField] private float maxX = 15f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    private Rigidbody rb;
    private Vector3 currentVelocity = Vector3.zero;
    private Quaternion initialRotation;
    private PlayerShootingDirect shootingScript;
    private float currentSpeedMultiplier = 1f; // Inizia a velocità normale

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        shootingScript = GetComponent<PlayerShootingDirect>();

        // Salva la rotazione iniziale (es: Cannon X=-90)
        initialRotation = transform.rotation;

        // Configura il Rigidbody
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // Leggi input DIRETTAMENTE dal gamepad specifico
        Vector2 moveInput = Vector2.zero;

        var gamepads = Gamepad.all;
        if (gamepadIndex < gamepads.Count)
        {
            Gamepad gamepad = gamepads[gamepadIndex];
            moveInput = gamepad.leftStick.ReadValue();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Gamepad {gamepadIndex} non trovato!");
        }

        // Converti il movimento 2D in 3D
        Vector3 targetDirection;

        if (moveOnXYPlane)
        {
            targetDirection = new Vector3(moveInput.x, moveInput.y, 0f);
        }
        else
        {
            targetDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        }

        // Calcola la velocità target
        Vector3 targetVelocity = targetDirection * maxSpeed;

        // Transizione GRADUALE della velocità quando carica
        float targetMultiplier = 1f; // Default: velocità normale

        if (shootingScript != null && shootingScript.IsCharging)
        {
            targetMultiplier = chargingSpeedMultiplier; // Rallenta
        }

        // Interpolazione smooth verso il multiplier target
        currentSpeedMultiplier = Mathf.Lerp(
            currentSpeedMultiplier,
            targetMultiplier,
            speedTransitionSpeed * Time.fixedDeltaTime
        );

        // Applica il multiplier corrente
        targetVelocity *= currentSpeedMultiplier;

        // Accelera o decelera
        if (targetDirection.magnitude > 0.01f)
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        // Applica velocità
        rb.linearVelocity = currentVelocity;
        rb.angularVelocity = Vector3.zero;

        // Limita movimento
        if (constrainToScreen)
        {
            Vector3 pos = rb.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            if (moveOnXYPlane)
            {
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
            }
            else
            {
                pos.z = Mathf.Clamp(pos.z, minY, maxY);
            }

            rb.position = pos;
        }
    }

    void LateUpdate()
    {
        // Mantieni la rotazione iniziale
        transform.rotation = initialRotation;
    }
}