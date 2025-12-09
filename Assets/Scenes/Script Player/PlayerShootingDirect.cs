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
    [SerializeField] private float cannonRotationSpeed = 10f;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private int startingAmmo = 5;
    [SerializeField] private bool infiniteAmmo = false;

    [Header("UI References")]
    [SerializeField] private Text ammoText;
    [SerializeField] private Slider chargeBar;

    private int currentAmmo;
    private bool isCharging = false;
    private float chargeStartTime;
    private float lastShootTime;
    private Vector2 currentAimDirection = Vector2.right;

    void Start()
    {
        currentAmmo = startingAmmo;
        UpdateAmmoUI();

        if (chargeBar != null)
        {
            chargeBar.gameObject.SetActive(false);
        }
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

        float targetAngle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        cannonTransform.localRotation = Quaternion.Lerp(
            cannonTransform.localRotation,
            targetRotation,
            cannonRotationSpeed * Time.deltaTime
        );

        cannonTransform.localScale = originalScale;
    }

    void StartCharging()
    {
        if (Time.time < lastShootTime + shootCooldown) return;

        isCharging = true;
        chargeStartTime = Time.time;

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
                ammoText.text = "?";
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