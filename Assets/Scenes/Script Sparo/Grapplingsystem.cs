using UnityEngine;

/// <summary>
/// Sistema di aggancio tipo grappling hook:
/// 1. Premi L2 → Spara un proiettile
/// 2. Proiettile colpisce oggetto → Si attacca
/// 3. Raggio visibile collega player a oggetto
/// 4. Muovi stick destro → Oggetto segue direzione
/// 5. Rilasci L2 → Sgancia
/// </summary>
public class GrapplingSystem : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private int gamepadIndex = 0; // 0 = Player 1, 1 = Player 2

    [Header("Aim Settings")]
    [SerializeField] private float aimRotationSpeed = 200f; // Gradi per secondo
    [SerializeField] private bool useIncrementalAim = true; // Rotazione relativa invece che assoluta
    [SerializeField] private bool limitAimAngle = true; // Limita angolo di mira
    [SerializeField] private float minAimAngle = -90f; // Angolo minimo
    [SerializeField] private float maxAimAngle = 90f; // Angolo massimo

    [Header("Grapple Projectile")]
    [SerializeField] private GameObject grapplingHookPrefab; // Prefab del "gancio" (pallino)
    [SerializeField] private float hookSpeed = 20f; // Velocità proiettile
    [SerializeField] private float maxDistance = 15f; // Distanza massima

    [Header("Object Movement")]
    [SerializeField] private float pullSpeed = 5f; // Velocità movimento oggetto
    [SerializeField] private float minDistance = 2f; // Distanza minima
    [SerializeField] private float maxHoldDistance = 10f; // Distanza massima

    [Header("Visual")]
    [SerializeField] private LineRenderer rope; // Il "filo"
    [SerializeField] private Color ropeColor = Color.cyan;
    [SerializeField] private float ropeWidth = 0.1f;

    [Header("Audio")]
    [SerializeField] private string shootSoundName = "";
    [SerializeField] private string attachSoundName = "";
    [SerializeField] private string detachSoundName = "";

    // Stato
    private GameObject hookProjectile; // Il proiettile in volo
    private GameObject attachedObject; // L'oggetto agganciato
    private Rigidbody attachedRb;
    private bool isAttached = false;
    private bool isShooting = false;
    private Vector2 aimDirection = Vector2.right;
    private float currentAimAngle = 0f; // Angolo corrente per rotazione incrementale
    private float currentDistance;

    //射击脚本引用
    private PlayerShootingDirect shootingScript;

    void Awake()
    {
        shootingScript = GetComponent<PlayerShootingDirect>();

        // Setup rope
        if (rope == null)
        {
            rope = gameObject.AddComponent<LineRenderer>();
        }
        rope.enabled = false;
        rope.startWidth = ropeWidth;
        rope.endWidth = ropeWidth;
        rope.startColor = ropeColor;
        rope.endColor = ropeColor;
        rope.positionCount = 2;

        // Inizializza angolo mira in base al player
        currentAimAngle = (gamepadIndex == 0) ? 0f : 180f;
    }

    void Update()
    {
        // Leggi input
        var gamepads = UnityEngine.InputSystem.Gamepad.all;
        if (gamepadIndex >= gamepads.Count)
        {
            Debug.LogWarning($"[GRAPPLE] Gamepad {gamepadIndex} non trovato!");
            return;
        }
        var gamepad = gamepads[gamepadIndex];

        // Aggiorna aim con rotazione incrementale o assoluta
        Vector2 aimInput = gamepad.rightStick.ReadValue();

        if (useIncrementalAim && aimInput.magnitude > 0.1f)
        {
            // ROTAZIONE INCREMENTALE (come il cannone)
            float stickAngle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
            float angleDiff = Mathf.DeltaAngle(currentAimAngle, stickAngle);
            float rotationStep = aimRotationSpeed * Time.deltaTime * aimInput.magnitude;
            currentAimAngle += Mathf.Clamp(angleDiff, -rotationStep, rotationStep);
            currentAimAngle = Mathf.Repeat(currentAimAngle + 180f, 360f) - 180f;

            // Applica limiti angolari
            if (limitAimAngle)
            {
                if (gamepadIndex == 0)
                {
                    // Player 1: solo destra (-90° a +90°)
                    currentAimAngle = Mathf.Clamp(currentAimAngle, minAimAngle, maxAimAngle);
                }
                else
                {
                    // Player 2: solo sinistra (90° a 270°)
                    float adjustedAngle = currentAimAngle;
                    if (adjustedAngle < 0) adjustedAngle += 360f;

                    if (adjustedAngle < 90f)
                        currentAimAngle = 90f;
                    else if (adjustedAngle > 270f)
                        currentAimAngle = -90f;
                }
            }

            // Converti angolo in direzione
            aimDirection = new Vector2(
                Mathf.Cos(currentAimAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAimAngle * Mathf.Deg2Rad)
            );
        }
        else if (!useIncrementalAim && aimInput.magnitude > 0.1f)
        {
            // ROTAZIONE ASSOLUTA (sistema vecchio)
            aimDirection = aimInput.normalized;
        }

        // Input L2
        float grabInput = gamepad.leftTrigger.ReadValue();

        if (grabInput > 0.01f)
        {
            Debug.Log($"[GRAPPLE] L2 premuto: {grabInput}, isAttached: {isAttached}, isShooting: {isShooting}");
        }

        if (grabInput > 0.5f && !isAttached && !isShooting)
        {
            // Spara gancio
            Debug.Log("[GRAPPLE] Tentativo di sparare gancio!");
            ShootHook();
        }
        else if (grabInput < 0.3f && isAttached)
        {
            // Sgancia
            Detach();
        }

        // Aggiorna oggetto agganciato
        if (isAttached && attachedObject != null)
        {
            UpdateAttachedObject();
        }

        // Aggiorna rope
        if (isAttached && attachedObject != null)
        {
            rope.SetPosition(0, transform.position);
            rope.SetPosition(1, attachedObject.transform.position);
        }
        else if (isShooting && hookProjectile != null)
        {
            // Mostra rope anche mentre il gancio vola
            rope.SetPosition(0, transform.position);
            rope.SetPosition(1, hookProjectile.transform.position);
        }
    }

    void ShootHook()
    {
        if (grapplingHookPrefab == null)
        {
            Debug.LogError("Grappling Hook Prefab non assegnato!");
            return;
        }

        isShooting = true;

        // Spawna proiettile-gancio alla stessa Z del player (importante per giochi 2D!)
        Vector3 shootDir = new Vector3(aimDirection.x, aimDirection.y, 0f).normalized;
        Vector3 spawnPos = transform.position; // Usa la Z del player
        hookProjectile = Instantiate(grapplingHookPrefab, spawnPos, Quaternion.identity);

        Debug.Log($"[GRAPPLE] Hook spawnato a posizione: {spawnPos}");

        // Dai velocità
        Rigidbody hookRb = hookProjectile.GetComponent<Rigidbody>();
        if (hookRb != null)
        {
            hookRb.linearVelocity = shootDir * hookSpeed;
            Debug.Log($"[GRAPPLE] Hook velocity: {hookRb.linearVelocity}");
        }
        else
        {
            Debug.LogError("[GRAPPLE] Hook non ha Rigidbody!");
        }

        // Ignora collisione con il player che ha sparato
        Collider hookCollider = hookProjectile.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (hookCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(hookCollider, playerCollider);
        }

        // Aggiungi component che gestisce collisione
        GrapplingHook hookScript = hookProjectile.GetComponent<GrapplingHook>();
        if (hookScript == null)
        {
            hookScript = hookProjectile.AddComponent<GrapplingHook>();
        }
        hookScript.Initialize(this, maxDistance, transform.position);

        // Mostra rope
        rope.enabled = true;

        // Disabilita sparo
        if (shootingScript != null)
        {
            shootingScript.enabled = false;
        }

        // Suono
        if (!string.IsNullOrEmpty(shootSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(shootSoundName);
        }

        Debug.Log("Gancio sparato!");
    }

    public void OnHookAttached(GameObject target)
    {
        // Chiamato dal GrapplingHook quando si attacca
        isShooting = false;
        isAttached = true;
        attachedObject = target;
        attachedRb = target.GetComponent<Rigidbody>();

        currentDistance = Vector3.Distance(transform.position, target.transform.position);
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxHoldDistance);

        // Disabilita gravità
        if (attachedRb != null)
        {
            attachedRb.useGravity = false;
            attachedRb.isKinematic = false; // IMPORTANTE: permette movimento!
            Debug.Log($"[GRAPPLE] RB configurato: useGravity=false, isKinematic=false");
        }

        // Marca come preso
        GrabbableObject grabbable = target.GetComponent<GrabbableObject>();
        if (grabbable == null)
        {
            grabbable = target.AddComponent<GrabbableObject>();
        }
        grabbable.SetGrabbed(true, gameObject);

        // Disabilita script che potrebbero muovere l'oggetto (come AmmoPickup)
        MonoBehaviour[] scripts = target.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script.GetType().Name == "AmmoPickup")
            {
                script.enabled = false;
                Debug.Log("[GRAPPLE] Disabilitato script AmmoPickup");
            }
        }

        // Suono
        if (!string.IsNullOrEmpty(attachSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(attachSoundName);
        }

        Debug.Log($"Agganciato: {target.name}");
    }

    public void OnHookMissed()
    {
        // Chiamato quando il gancio non colpisce nulla
        isShooting = false;
        rope.enabled = false;

        // Riabilita sparo
        if (shootingScript != null)
        {
            shootingScript.enabled = true;
        }

        Debug.Log("Gancio mancato!");
    }

    void UpdateAttachedObject()
    {
        if (attachedObject == null || attachedRb == null)
        {
            Debug.LogWarning("[GRAPPLE] UpdateAttached ma oggetto o RB è null!");
            return;
        }

        // Calcola direzione target basata sull'aim
        Vector3 aimDir3D = new Vector3(aimDirection.x, aimDirection.y, 0f).normalized;

        Debug.Log($"[GRAPPLE] Aim direction: {aimDir3D}");

        // Calcola posizione target
        Vector3 targetPosition = transform.position + aimDir3D * currentDistance;

        Debug.Log($"[GRAPPLE] Target pos: {targetPosition}, Object pos: {attachedObject.transform.position}");

        // Muovi oggetto verso target
        Vector3 toTarget = targetPosition - attachedObject.transform.position;

        Debug.Log($"[GRAPPLE] Distance to target: {toTarget.magnitude}");

        if (toTarget.magnitude > 0.1f)
        {
            Vector3 moveDirection = toTarget.normalized;
            Vector3 newPosition = attachedObject.transform.position + moveDirection * pullSpeed * Time.deltaTime;

            // Limita distanza
            float distanceFromPlayer = Vector3.Distance(transform.position, newPosition);
            if (distanceFromPlayer < minDistance)
            {
                newPosition = transform.position + (newPosition - transform.position).normalized * minDistance;
            }
            else if (distanceFromPlayer > maxHoldDistance)
            {
                newPosition = transform.position + (newPosition - transform.position).normalized * maxHoldDistance;
            }

            Debug.Log($"[GRAPPLE] Moving to: {newPosition}");
            Debug.Log($"[GRAPPLE] RB isKinematic: {attachedRb.isKinematic}, useGravity: {attachedRb.useGravity}");

            attachedRb.MovePosition(newPosition);
            attachedRb.linearVelocity = Vector3.zero;
        }
    }

    void Detach()
    {
        if (attachedObject == null) return;

        // Riabilita gravità
        if (attachedRb != null)
        {
            attachedRb.useGravity = true;
            attachedRb.isKinematic = true; // Torna kinematic = fermo
        }

        // Marca come rilasciato
        GrabbableObject grabbable = attachedObject.GetComponent<GrabbableObject>();
        if (grabbable != null)
        {
            grabbable.SetGrabbed(false, null);
        }

        // Riabilita script disabilitati
        MonoBehaviour[] scripts = attachedObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script.GetType().Name == "AmmoPickup")
            {
                script.enabled = true;
                Debug.Log("[GRAPPLE] Riabilitato script AmmoPickup");
            }
        }

        // Distruggi gancio
        if (hookProjectile != null)
        {
            Destroy(hookProjectile);
        }

        // Nascondi rope
        rope.enabled = false;

        // Riabilita sparo
        if (shootingScript != null)
        {
            shootingScript.enabled = true;
        }

        // Suono
        if (!string.IsNullOrEmpty(detachSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(detachSoundName);
        }

        Debug.Log($"Sganciato: {attachedObject.name}");

        isAttached = false;
        attachedObject = null;
        attachedRb = null;
    }

    void OnDisable()
    {
        // Auto-detach se player muore
        if (isAttached)
        {
            Detach();
        }
    }
}