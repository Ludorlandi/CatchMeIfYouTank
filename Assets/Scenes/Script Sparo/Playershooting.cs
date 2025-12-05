using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Cannon Settings")]
    [SerializeField] private Transform cannonTransform; // La "canna" che ruota
    [SerializeField] private Transform firePoint; // Il punto da cui parte il proiettile
    [SerializeField] private float cannonRotationSpeed = 10f;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 10; // Munizioni massime
    [SerializeField] private int startingAmmo = 10; // Munizioni iniziali (può essere diverso da maxAmmo)
    private int currentAmmo; // Munizioni attuali (private, gestito internamente)
    [SerializeField] private bool infiniteAmmo = false; // Per testing

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // Il prefab del proiettile
    [SerializeField] private float minProjectileSpeed = 5f; // Velocità minima (senza carica)
    [SerializeField] private float maxProjectileSpeed = 30f; // Velocità massima (carica completa)
    [SerializeField] private float maxChargeTime = 2f; // Tempo per raggiungere la carica massima
    [SerializeField] private float fireRate = 0.5f; // Tempo minimo tra un colpo e l'altro

    [Header("Aiming Settings")]
    [SerializeField] private float minAimMagnitude = 0.2f; // Sensibilità minima della levetta

    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Text ammoText; // Testo per mostrare le munizioni
    [SerializeField] private UnityEngine.UI.Slider chargeBar; // Barra di carica

    private Vector2 aimInput;
    private Vector3 currentAimDirection;
    private float lastFireTime = 0f;
    private PlayerInput playerInput;

    // Variabili per la carica
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private float currentCharge = 0f; // 0 a 1

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (cannonTransform == null)
        {
            Debug.LogWarning("Cannon Transform non assegnato! Usando il transform del player.");
            cannonTransform = transform;
        }

        if (firePoint == null)
        {
            Debug.LogWarning("Fire Point non assegnato! Usando la posizione della canna.");
            firePoint = cannonTransform;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab non assegnato! Lo sparo non funzionerà.");
        }

        // Inizializza la direzione di mira (verso destra di default)
        currentAimDirection = Vector3.right;

        // Inizializza le munizioni con startingAmmo
        currentAmmo = startingAmmo;

        // Inizializza UI
        UpdateAmmoUI();
        UpdateChargeUI();
    }

    // Chiamato automaticamente dal Input System per la levetta destra
    public void OnAim(InputValue value)
    {
        aimInput = value.Get<Vector2>();
    }

    // Chiamato automaticamente dal Input System per RT
    public void OnFire(InputValue value)
    {
        float triggerValue = value.Get<float>(); // RT va da 0 a 1

        // Inizia a caricare quando premi RT
        if (triggerValue > 0.1f && !isCharging)
        {
            StartCharging();
        }
        // Rilascia e spara quando lasci RT
        else if (triggerValue <= 0.1f && isCharging)
        {
            ReleaseShot();
        }
    }

    void Update()
    {
        UpdateAimDirection();
        RotateCannon();
        UpdateCharge();
    }

    void UpdateAimDirection()
    {
        // Solo se la levetta è mossa abbastanza
        if (aimInput.magnitude > minAimMagnitude)
        {
            // Converti l'input 2D in direzione 3D (sul piano XY)
            Vector3 aimDirection = new Vector3(aimInput.x, aimInput.y, 0f).normalized;
            currentAimDirection = aimDirection;
        }
        // Se non muovi la levetta, mantieni l'ultima direzione
    }

    void RotateCannon()
    {
        if (cannonTransform == null) return;

        // IMPORTANTE: Mantieni la scala originale del cannon per evitare distorsioni
        // quando il player si scala
        Vector3 originalScale = cannonTransform.localScale;

        // Calcola l'angolo dalla direzione
        float targetAngle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;

        // Rotazione smooth verso l'angolo target
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        cannonTransform.rotation = Quaternion.Lerp(
            cannonTransform.rotation,
            targetRotation,
            cannonRotationSpeed * Time.deltaTime
        );

        // Ripristina la scala locale se è stata modificata
        cannonTransform.localScale = originalScale;
    }

    void StartCharging()
    {
        // Controlla se è passato abbastanza tempo dall'ultimo colpo
        if (Time.time - lastFireTime < fireRate)
        {
            return; // Troppo presto per caricare di nuovo
        }

        // Controlla se ha munizioni
        if (!infiniteAmmo && currentAmmo <= 0)
        {
            Debug.Log("Niente munizioni!");
            return;
        }

        isCharging = true;
        chargeStartTime = Time.time;
        currentCharge = 0f;
        Debug.Log("Inizio carica...");
    }

    void UpdateCharge()
    {
        if (!isCharging) return;

        // Calcola la carica corrente (da 0 a 1)
        float chargeTime = Time.time - chargeStartTime;
        currentCharge = Mathf.Clamp01(chargeTime / maxChargeTime);

        // Aggiorna UI della barra di carica
        UpdateChargeUI();

        // Feedback visivo opzionale (puoi aggiungere effetti qui)
        // Per esempio, cambiare il colore della canna in base alla carica
    }

    void ReleaseShot()
    {
        if (!isCharging) return;

        isCharging = false;

        // Nascondi la barra di carica
        if (chargeBar != null)
        {
            chargeBar.gameObject.SetActive(false);
        }

        if (projectilePrefab == null)
        {
            Debug.LogError("Impossibile sparare: Projectile Prefab non assegnato!");
            return;
        }

        // Calcola la velocità in base alla carica
        float shotSpeed = Mathf.Lerp(minProjectileSpeed, maxProjectileSpeed, currentCharge);

        // Spara!
        Fire(shotSpeed);

        // Consuma una munizione
        if (!infiniteAmmo)
        {
            currentAmmo--;
            UpdateAmmoUI();
        }

        lastFireTime = Time.time;

        Debug.Log($"Sparato con carica {currentCharge:F2} (velocità: {shotSpeed:F1}). Munizioni rimaste: {currentAmmo}");
    }

    void Fire(float speed)
    {
        // Crea il proiettile
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        // Imposta la velocità del proiettile in base alla carica
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = currentAimDirection * speed;
        }
        else
        {
            Debug.LogWarning("Il proiettile non ha un Rigidbody! Aggiungi un Rigidbody al prefab.");
        }

        // Imposta il proprietario del proiettile (per evitare autolesioni)
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetOwner(gameObject.tag);
        }
    }

    // Aggiorna UI delle munizioni
    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            if (infiniteAmmo)
            {
                ammoText.text = "∞";
            }
            else
            {
                ammoText.text = $"{currentAmmo} / {maxAmmo}";
            }
        }
    }

    // Aggiorna UI della barra di carica
    void UpdateChargeUI()
    {
        if (chargeBar != null)
        {
            if (isCharging)
            {
                chargeBar.gameObject.SetActive(true);
                chargeBar.value = currentCharge;
            }
            else
            {
                chargeBar.gameObject.SetActive(false);
            }
        }
    }

    // Metodi pubblici per gestire le munizioni
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        UpdateAmmoUI();
        Debug.Log($"Munizioni aggiunte! Totale: {currentAmmo}");
    }

    public void RefillAmmo()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        Debug.Log("Munizioni ricaricate!");
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    // Metodo opzionale per visualizzare la direzione di mira e la carica nell'editor
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Disegna la direzione di mira
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;

        // Colore basato sulla carica
        if (isCharging)
        {
            Gizmos.color = Color.Lerp(Color.yellow, Color.red, currentCharge);
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(startPos, startPos + currentAimDirection * 2f);
        Gizmos.DrawSphere(startPos + currentAimDirection * 2f, 0.1f + (currentCharge * 0.2f));
    }
}