using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema di screen shake per feedback visivo intenso (morte, esplosioni, etc)
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float defaultShakeDuration = 0.3f;
    [SerializeField] private float defaultShakeIntensity = 0.5f;
    [SerializeField] private float defaultShakeFrequency = 25f;

    private Vector3 originalPosition;
    private bool isShaking = false;

    // Singleton per accesso globale
    public static CameraShake Instance { get; private set; }

    void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[CAMERA SHAKE] Instance creata!");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        originalPosition = transform.localPosition;
        Debug.Log($"[CAMERA SHAKE] Posizione originale salvata: {originalPosition}");
    }

    /// <summary>
    /// Avvia lo shake con parametri di default
    /// </summary>
    public void Shake()
    {
        Debug.Log("[CAMERA SHAKE] Shake() chiamato con parametri default!");
        Shake(defaultShakeDuration, defaultShakeIntensity);
    }

    /// <summary>
    /// Avvia lo shake con parametri custom
    /// </summary>
    public void Shake(float duration, float intensity)
    {
        Debug.Log($"[CAMERA SHAKE] Shake chiamato! Duration: {duration}, Intensity: {intensity}");

        if (isShaking)
        {
            // Se sta già shakeando, riavvia con nuovi parametri
            StopAllCoroutines();
        }

        StartCoroutine(ShakeCoroutine(duration, intensity));
    }

    IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        isShaking = true;
        float elapsed = 0f;

        Debug.Log($"[CAMERA SHAKE] Coroutine iniziata! Original pos: {originalPosition}");

        while (elapsed < duration)
        {
            // Shake random in tutte le direzioni
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            Vector3 shakeOffset = new Vector3(x, y, 0f);
            transform.localPosition = originalPosition + shakeOffset;

            if (elapsed < 0.1f) // Debug solo all'inizio
            {
                Debug.Log($"[CAMERA SHAKE] Shake offset: {shakeOffset}, New pos: {transform.localPosition}");
            }

            elapsed += Time.deltaTime;

            // Diminuisce intensità nel tempo (fade out)
            float fadeOut = 1f - (elapsed / duration);
            intensity *= 0.99f; // Decay graduale

            yield return null;
        }

        // Ritorna alla posizione originale
        transform.localPosition = originalPosition;
        isShaking = false;

        Debug.Log($"[CAMERA SHAKE] Shake completato! Posizione finale: {transform.localPosition}");
    }

    /// <summary>
    /// Ferma immediatamente lo shake e ritorna alla posizione originale
    /// </summary>
    public void StopShake()
    {
        StopAllCoroutines();
        transform.localPosition = originalPosition;
        isShaking = false;
    }

    /// <summary>
    /// Aggiorna la posizione originale (utile se la camera si muove)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (!isShaking)
        {
            originalPosition = transform.localPosition;
        }
    }
}