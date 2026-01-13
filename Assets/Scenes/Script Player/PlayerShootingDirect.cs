using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShootingDirect : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] private int gamepadIndex = 0; // 0 per Player 1, 1 per Player 2

    [Header("References")]
    [SerializeField] private Transform cannonTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Shooting Settings")]
    [SerializeField] private float minProjectileSpeed = 5f;
    [SerializeField] private float maxProjectileSpeed = 20f;
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float shootCooldown = 0.2f;

    [Header("Cannon Settings")]
    [SerializeField] private float cannonRotationSpeed = 200f; // Gradi per secondo
    [SerializeField] private bool useIncrementalRotation = true; // Rotazione relativa invece che assoluta
    [SerializeField] private bool limitCannonAngle = true; // Limita angolo di mira
    [SerializeField] private float minCannonAngle = -90f; // Angolo minimo (sarà invertito per P2)
    [SerializeField] private float maxCannonAngle = 90f; // Angolo massimo (sarà invertito per P2)

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private int startingAmmo = 5;
    [SerializeField] private bool infiniteAmmo = false;

    [Header("UI References")]
    [SerializeField] private Text ammoText;
    [SerializeField] private Slider chargeBar;

    [Header("Audio (Opzionale)")]
    [SerializeField] private string shootSoundName = ""; // Nome del suono nello SoundManager
    [SerializeField] private string chargingSoundName = ""; // Suono loop mentre carica
    [SerializeField] private string chargeCompleteSoundName = ""; // Suono quando carica è completa

    private int currentAmmo;
    private bool isCharging = false;
    private float chargeStartTime;
    private float lastShootTime;
    private Vector2 currentAimDirection = Vector2.right;
    private bool hasPlayedChargeComplete = false; // Flag per suonare charge complete una volta sola
    private float currentCannonAngle = 0f; // Angolo corrente del cannone (per rotazione incrementale)

    // Proprietà pubblica per sapere se sta caricando
    public bool IsCharging => isCharging;

    void Start()
    {
        currentAmmo = startingAmmo;
        UpdateAmmoUI();

        if (chargeBar != null)
        {
            chargeBar.gameObject.SetActive(false);
        }

        // Inizializza angolo cannone in base al player
        // Player 1 (gamepadIndex 0) parte a destra (0°)
        // Player 2 (gamepadIndex 1) parte a sinistra (180°)
        currentCannonAngle = (gamepadIndex == 0) ? 0f : 180f;
    }

    void Update()
    {
        // Leggi input DIRETTAMENTE dal gamepad specifico
        Vector2 aimInput = Vector2.zero;
        float fireInput = 0f;

        var gamepads = Gamepad.all;
        if (gamepadIndex < gamepads.Count)
        {
            Gamepad gamepad = gamepads[gamepadIndex];
            aimInput = gamepad.rightStick.ReadValue();
            fireInput = gamepad.rightTrigger.ReadValue();
        }

        // Aggiorna aim direction
        if (aimInput.magnitude > 0.1f)
        {
            currentAimDirection = aimInput.normalized;
        }

        UpdateCannonRotation();

        // Gestione shooting
        if (fireInput > 0.1f && !isCharging)
        {
            StartCharging();
        }

        if (isCharging)
        {
            if (fireInput < 0.1f)
            {
                Fire();
            }
            else
            {
                UpdateChargeUI();
            }
        }
    }

    void UpdateCannonRotation()
    {
        if (cannonTransform == null) return;

        Vector3 originalScale = cannonTransform.localScale;

        if (useIncrementalRotation)
        {
            // ROTAZIONE INCREMENTALE (più fluida)
            // Lo stick destro controlla la VELOCITÀ di rotazione, non la posizione assoluta

            var gamepads = Gamepad.all;
            if (gamepadIndex < gamepads.Count)
            {
                Gamepad gamepad = gamepads[gamepadIndex];
                Vector2 aimInput = gamepad.rightStick.ReadValue();

                if (aimInput.magnitude > 0.1f)
                {
                    // Calcola angolo target dallo stick
                    float stickAngle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;

                    // Calcola differenza angolare più corta
                    float angleDiff = Mathf.DeltaAngle(currentCannonAngle, stickAngle);

                    // Ruota progressivamente verso il target
                    float rotationStep = cannonRotationSpeed * Time.deltaTime * aimInput.magnitude;
                    currentCannonAngle += Mathf.Clamp(angleDiff, -rotationStep, rotationStep);

                    // Normalizza angolo tra -180 e 180
                    currentCannonAngle = Mathf.Repeat(currentCannonAngle + 180f, 360f) - 180f;
                }
            }
        }
        else
        {
            // ROTAZIONE ASSOLUTA (sistema vecchio)
            float targetAngle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
            currentCannonAngle = Mathf.LerpAngle(currentCannonAngle, targetAngle, cannonRotationSpeed * Time.deltaTime);
        }

        // Applica limiti angolari
        if (limitCannonAngle)
        {
            float minAngle, maxAngle;

            if (gamepadIndex == 0)
            {
                // Player 1: può mirare solo a destra (-90° a +90°)
                minAngle = minCannonAngle;
                maxAngle = maxCannonAngle;
            }
            else
            {
                // Player 2: può mirare solo a sinistra (90° a 270°, ovvero 90° a -90° passando per 180°)
                minAngle = 90f;
                maxAngle = 270f;

                // Converti angolo per confronto
                float adjustedAngle = currentCannonAngle;
                if (adjustedAngle < 0) adjustedAngle += 360f;

                // Clamp nell'intervallo 90-270
                if (adjustedAngle < 90f)
                {
                    currentCannonAngle = 90f;
                }
                else if (adjustedAngle > 270f)
                {
                    currentCannonAngle = -90f; // 270° = -90°
                }
            }

            // Applica limiti per Player 1
            if (gamepadIndex == 0)
            {
                currentCannonAngle = Mathf.Clamp(currentCannonAngle, minAngle, maxAngle);
            }
        }

        // Aggiorna rotazione cannone
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, currentCannonAngle);
        cannonTransform.localRotation = targetRotation;

        // Aggiorna anche currentAimDirection per lo sparo
        currentAimDirection = new Vector2(
            Mathf.Cos(currentCannonAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentCannonAngle * Mathf.Deg2Rad)
        );

        cannonTransform.localScale = originalScale;
    }

    void StartCharging()
    {
        if (Time.time < lastShootTime + shootCooldown) return;

        isCharging = true;
        chargeStartTime = Time.time;
        hasPlayedChargeComplete = false;

        // Suona il suono di carica (loop)
        if (!string.IsNullOrEmpty(chargingSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(chargingSoundName);
        }

        if (chargeBar != null)
        {
            chargeBar.gameObject.SetActive(true);
            chargeBar.value = 0f;
        }
    }

    void Fire()
    {
        if (!infiniteAmmo && currentAmmo <= 0)
        {
            isCharging = false;

            // Ferma il suono di carica se stava suonando
            if (!string.IsNullOrEmpty(chargingSoundName) && SoundManager.Instance != null)
            {
                SoundManager.Instance.Stop(chargingSoundName);
            }

            if (chargeBar != null) chargeBar.gameObject.SetActive(false);
            return;
        }

        float chargeTime = Mathf.Min(Time.time - chargeStartTime, maxChargeTime);
        float chargePercent = chargeTime / maxChargeTime;
        float projectileSpeed = Mathf.Lerp(minProjectileSpeed, maxProjectileSpeed, chargePercent);

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 shootDirection = new Vector3(currentAimDirection.x, currentAimDirection.y, 0f).normalized;
                rb.linearVelocity = shootDirection * projectileSpeed;
            }

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetOwner(gameObject.tag);
            }

            // Suona l'audio dello sparo se specificato
            if (!string.IsNullOrEmpty(shootSoundName) && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWithRandomPitch(shootSoundName, 0.95f, 1.05f);
            }
        }

        // Ferma il suono di carica
        if (!string.IsNullOrEmpty(chargingSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Stop(chargingSoundName);
        }

        if (!infiniteAmmo)
        {
            currentAmmo--;
            UpdateAmmoUI();
        }

        isCharging = false;
        lastShootTime = Time.time;

        if (chargeBar != null)
        {
            chargeBar.gameObject.SetActive(false);
        }
    }

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

    void UpdateChargeUI()
    {
        if (chargeBar != null)
        {
            float chargeTime = Time.time - chargeStartTime;
            float chargePercent = Mathf.Min(chargeTime / maxChargeTime, 1f);
            chargeBar.value = chargePercent;

            // Suona il suono di carica completa quando raggiungi il 100%
            if (chargePercent >= 1f && !hasPlayedChargeComplete)
            {
                hasPlayedChargeComplete = true;

                // Ferma il suono di carica loop
                if (!string.IsNullOrEmpty(chargingSoundName) && SoundManager.Instance != null)
                {
                    SoundManager.Instance.Stop(chargingSoundName);
                }

                // Suona il suono di carica completa
                if (!string.IsNullOrEmpty(chargeCompleteSoundName) && SoundManager.Instance != null)
                {
                    SoundManager.Instance.Play(chargeCompleteSoundName);
                }
            }
        }
    }

    public void AddAmmo(int amount)
    {
        if (infiniteAmmo) return;

        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        UpdateAmmoUI();
    }

    public void RefillAmmo()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }
}