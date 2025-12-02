using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f; // Quanto velocemente accelera
    [SerializeField] private float deceleration = 15f; // Quanto velocemente decelera quando lasci la levetta

    [Header("Movement Plane")]
    [SerializeField] private bool moveOnXYPlane = true; // true = XY (come nella tua vista), false = XZ

    [Header("Optional: Movement Constraints")]
    [SerializeField] private bool constrainToScreen = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private PlayerInput playerInput;
    private Vector3 currentVelocity = Vector3.zero; // Velocità attuale del personaggio

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody non trovato! Aggiungilo al GameObject.");
        }

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component non trovato! Aggiungilo al GameObject.");
        }

        // Congela la rotazione per evitare che il personaggio cada o ruoti
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Se muovi sul piano XY, congela anche la Z
            if (moveOnXYPlane)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            }
        }
    }

    // Questo metodo viene chiamato automaticamente dal nuovo Input System
    // Usa InputValue per compatibilità con Send Messages
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // Converti il movimento 2D in 3D a seconda del piano scelto
        Vector3 targetDirection;

        if (moveOnXYPlane)
        {
            // Movimento sul piano XY (vista frontale/laterale)
            targetDirection = new Vector3(moveInput.x, moveInput.y, 0f);
        }
        else
        {
            // Movimento sul piano XZ (vista dall'alto)
            targetDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        }

        // Calcola la velocità target in base all'input della levetta
        // La magnitudine dell'input (0-1) determina quanto veloce vuoi andare
        Vector3 targetVelocity = targetDirection * maxSpeed;

        // Accelera o decelera gradualmente verso la velocità target
        if (targetDirection.magnitude > 0.01f)
        {
            // Sta accelerando - muove la levetta
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // Sta decelerando - levetta rilasciata
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        // Calcola la nuova posizione
        Vector3 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;

        // Opzionale: limita il movimento entro certi confini
        if (constrainToScreen)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

            if (moveOnXYPlane)
            {
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            }
            else
            {
                newPosition.z = Mathf.Clamp(newPosition.z, minY, maxY);
            }
        }

        rb.MovePosition(newPosition);
    }

    // Metodo opzionale per visualizzare i confini nell'editor
    private void OnDrawGizmosSelected()
    {
        if (constrainToScreen)
        {
            Gizmos.color = Color.yellow;

            if (moveOnXYPlane)
            {
                // Disegna i confini sul piano XY
                Vector3 bottomLeft = new Vector3(minX, minY, transform.position.z);
                Vector3 bottomRight = new Vector3(maxX, minY, transform.position.z);
                Vector3 topLeft = new Vector3(minX, maxY, transform.position.z);
                Vector3 topRight = new Vector3(maxX, maxY, transform.position.z);

                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }
            else
            {
                // Disegna i confini sul piano XZ
                Vector3 bottomLeft = new Vector3(minX, transform.position.y, minY);
                Vector3 bottomRight = new Vector3(maxX, transform.position.y, minY);
                Vector3 topLeft = new Vector3(minX, transform.position.y, maxY);
                Vector3 topRight = new Vector3(maxX, transform.position.y, maxY);

                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }
        }
    }
}