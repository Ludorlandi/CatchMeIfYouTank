using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementDirect : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] private int gamepadIndex = 0; // 0 per Player 1, 1 per Player 2

    [Header("Tank Tracks (Cingoli)")]
    [SerializeField] private Transform tankBody; // Corpo del tank che ruota (con i cingoli attaccati)
    [SerializeField] private float bodyRotationSpeed = 5f; // Velocità rotazione corpo
    [SerializeField] private Vector3 modelRotationOffset = Vector3.zero; // Offset per correggere orientamento modello (es: 0, 90, 0)

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private bool moveOnXYPlane = true;
    [SerializeField] private float chargingSpeedMultiplier = 0.5f; // Velocità ridotta durante carica (50%)
    [SerializeField] private float speedTransitionSpeed = 5f; // Velocità della transizione

    [Header("Audio (Opzionale)")]
    [SerializeField] private string accelerationStartSoundName = ""; // Suono quando inizi ad accelerare
    [SerializeField] private string movementLoopSoundName = ""; // Suono loop mentre ti muovi
    [SerializeField] private string decelerationStartSoundName = ""; // Suono quando inizi a decelerare
    [SerializeField] private float minSpeedForSound = 2f; // Velocità minima per considerare "in movimento"

    [Header("Movement Constraints")]
    [SerializeField] private bool constrainToScreen = true;
    [SerializeField] private float minX = -15f;
    [SerializeField] private float maxX = 15f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    private Rigidbody rb;
    private Vector3 currentVelocity = Vector3.zero;
    private Quaternion initialRotation;
    private Quaternion tankBodyInitialRotation; // Rotazione iniziale del corpo tank
    private PlayerShootingDirect shootingScript;
    private float currentSpeedMultiplier = 1f; // Inizia a velocità normale

    // Movement sound states
    private bool wasMoving = false;
    private bool isPlayingMovementLoop = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        shootingScript = GetComponent<PlayerShootingDirect>();

        // Salva la rotazione iniziale del player
        initialRotation = transform.rotation;

        // Salva la rotazione iniziale del corpo del tank (importante!)
        if (tankBody != null)
        {
            tankBodyInitialRotation = tankBody.localRotation;
        }

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

        // Ruota i cingoli verso la direzione del movimento
        UpdateTrackRotation(targetDirection);

        // Gestisci suoni movimento
        HandleMovementSounds();

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

    void UpdateTrackRotation(Vector3 movementDirection)
    {
        if (tankBody == null) return;
        if (movementDirection.magnitude < 0.1f) return; // Non ruotare se fermo

        // Calcola angolo target basato sulla direzione del movimento
        float targetAngle;
        Quaternion directionRotation;

        if (moveOnXYPlane)
        {
            // Piano XY (side view): direzione su X e Y
            targetAngle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
            // Rotazione sull'asse Z per giochi 2D side-view
            directionRotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            // Piano XZ (top down): direzione su X e Z
            targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;
            // Rotazione sull'asse Y (verticale) per giochi top-down
            directionRotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        // Applica l'offset del modello
        Quaternion offsetRotation = Quaternion.Euler(modelRotationOffset);
        directionRotation = directionRotation * offsetRotation;

        // IMPORTANTE: Applica la rotazione RELATIVA alla rotazione iniziale
        // Così manteniamo l'orientamento originale del modello
        Quaternion targetRotation = directionRotation * tankBodyInitialRotation;

        // Interpola smooth verso la rotazione target
        tankBody.localRotation = Quaternion.Slerp(
            tankBody.localRotation,
            targetRotation,
            bodyRotationSpeed * Time.fixedDeltaTime
        );
    }

    void HandleMovementSounds()
    {
        if (SoundManager.Instance == null) return;

        bool isMovingNow = currentVelocity.magnitude > minSpeedForSound;

        // Transizione: Fermo → In movimento
        if (isMovingNow && !wasMoving)
        {
            // Suona accelerazione start
            if (!string.IsNullOrEmpty(accelerationStartSoundName))
            {
                SoundManager.Instance.Play(accelerationStartSoundName);
            }

            // Avvia loop movimento (dopo un attimo)
            if (!string.IsNullOrEmpty(movementLoopSoundName))
            {
                Invoke(nameof(StartMovementLoop), 0.1f);
            }
        }
        // Transizione: In movimento → Fermo
        else if (!isMovingNow && wasMoving)
        {
            // Ferma loop movimento
            StopMovementLoop();

            // Suona decelerazione start
            if (!string.IsNullOrEmpty(decelerationStartSoundName))
            {
                SoundManager.Instance.Play(decelerationStartSoundName);
            }
        }

        wasMoving = isMovingNow;
    }

    void StartMovementLoop()
    {
        if (SoundManager.Instance != null && !string.IsNullOrEmpty(movementLoopSoundName))
        {
            SoundManager.Instance.Play(movementLoopSoundName);
            isPlayingMovementLoop = true;
        }
    }

    void StopMovementLoop()
    {
        if (SoundManager.Instance != null && !string.IsNullOrEmpty(movementLoopSoundName) && isPlayingMovementLoop)
        {
            SoundManager.Instance.Stop(movementLoopSoundName);
            isPlayingMovementLoop = false;
        }
    }

    void LateUpdate()
    {
        // Mantieni la rotazione iniziale
        transform.rotation = initialRotation;
    }
}