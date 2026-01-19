using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private bool respawnOnDeath = true;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private Vector3 respawnPosition;

    [Header("Renderer Settings")]
    [SerializeField] private GameObject[] objectsToKeepDisabled; // Oggetti che rimangono disabilitati (es: cubo, cannone vecchio)

    [Header("Invincibility Settings")]
    [SerializeField] private bool invincibleOnRespawn = true; // Invincibile dopo respawn
    [SerializeField] private float invincibilityDuration = 3f; // Durata invincibilità (secondi)
    [SerializeField] private float flickerSpeed = 0.1f; // Velocità lampeggio

    [Header("Ammo Settings")]
    [SerializeField] private int ammoOnHit = 1; // Munizioni che ricevi quando vieni colpito

    [Header("Audio (Opzionale)")]
    [SerializeField] private string deathSoundName = ""; // Suono quando muore (SoundManager)
    [SerializeField] private AudioClip deathSoundClip; // Clip diretto per morte (bypass SoundManager)
    [SerializeField] private string respawnSoundName = ""; // Suono quando respawna

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab; // Prefab esplosione animata
    [SerializeField] private float explosionLifetime = 2f; // Durata esplosione prima di distruggersi

    [Header("Screen Shake on Death")]
    [SerializeField] private bool shakeOnDeath = true; // Shake quando muore
    [SerializeField] private float shakeDuration = 0.4f; // Durata shake
    [SerializeField] private float shakeIntensity = 0.8f; // Intensità shake

    private bool isDead = false;
    private bool isInvincible = false; // Stato invincibilità

    // Proprietà pubblica per controllare invincibilità
    public bool IsInvincible => isInvincible;

    void Start()
    {
        // Salva la posizione iniziale come respawn point
        if (respawnPosition == Vector3.zero)
        {
            respawnPosition = transform.position;
        }
    }

    public void Die(string killerTag = "")
    {
        if (isDead) return; // Già morto

        isDead = true;
        Debug.Log($"{gameObject.name} è MORTO! Ucciso da: {killerTag}");

        // Suona il suono della morte se specificato
        if (!string.IsNullOrEmpty(deathSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(deathSoundName);
        }

        // Notifica lo ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPlayerDeath(gameObject.tag, killerTag);
        }

        // Disabilita il player
        DisablePlayer();

        // Respawn dopo un delay
        if (respawnOnDeath)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            // Se non respawna, distruggi il GameObject o gestisci il game over
            Debug.Log($"{gameObject.name} NON respawnerà");
        }
    }

    // Nuovo metodo per gestire danno variabile
    public void TakeDamage(int damage, string killerTag = "")
    {
        if (isDead) return;

        // Se invincibile, ignora danno
        if (isInvincible)
        {
            Debug.Log($"{gameObject.name} è INVINCIBILE! Danno ignorato.");
            return;
        }

        Debug.Log($"{gameObject.name} ha preso {damage} danno da {killerTag}");

        // Ricarica munizioni quando vieni colpito
        if (ammoOnHit > 0)
        {
            PlayerShootingDirect shooting = GetComponent<PlayerShootingDirect>();
            if (shooting != null)
            {
                shooting.AddAmmo(ammoOnHit);
                Debug.Log($"{gameObject.name} ha ricevuto {ammoOnHit} munizione/i dopo essere stato colpito!");
            }
        }

        // MORTE - Notifica lo ScoreManager UNA SOLA VOLTA
        // Nel nuovo sistema: killer guadagna +1 vita
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPlayerDeath(gameObject.tag, killerTag);
            Debug.Log($"[DEATH] {gameObject.name} ucciso da {killerTag}");
        }

        isDead = true;

        // ESPLOSIONE VISIVA!
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // Distruggi l'esplosione dopo un po' (se non ha script che si autodistrugge)
            Destroy(explosion, explosionLifetime);

            Debug.Log($"[DEATH] Esplosione spawnata a {transform.position}");
        }

        // SCREEN SHAKE!
        if (shakeOnDeath)
        {
            // Prova a trovare CameraShake
            CameraShake shaker = CameraShake.Instance;

            if (shaker == null)
            {
                Debug.LogWarning("[HEALTH] CameraShake.Instance è null! Cerco con FindObjectOfType...");
                shaker = FindObjectOfType<CameraShake>();
            }

            if (shaker != null)
            {
                Debug.Log($"[HEALTH] Chiamando CameraShake! Duration: {shakeDuration}, Intensity: {shakeIntensity}");
                shaker.Shake(shakeDuration, shakeIntensity);
            }
            else
            {
                Debug.LogError("[HEALTH] CameraShake NON TROVATO! Aggiungi il componente CameraShake alla Main Camera!");
            }
        }

        // Suona il suono della morte PRIMA di disabilitare
        // Prova prima con AudioClip diretto, poi fallback a SoundManager
        if (deathSoundClip != null)
        {
            // METODO DIRETTO - Crea AudioSource temporaneo che suona e si autodistrugge
            AudioSource.PlayClipAtPoint(deathSoundClip, transform.position, 1f);
            Debug.Log($"[DEATH SOUND] Suonato clip diretto: {deathSoundClip.name}");
        }
        else if (!string.IsNullOrEmpty(deathSoundName) && SoundManager.Instance != null)
        {
            Debug.Log($"[DEATH SOUND] Chiamando Play('{deathSoundName}')");
            // Fallback a SoundManager
            SoundManager.Instance.Play(deathSoundName);
        }
        else
        {
            Debug.LogWarning("[DEATH SOUND] Nessun suono di morte assegnato!");
        }

        // Disabilita il player (dopo aver suonato il suono!)
        DisablePlayer();

        // Respawn dopo un delay
        if (respawnOnDeath)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
    }

    void DisablePlayer()
    {
        // Disabilita i componenti di controllo (vecchi)
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;

        // Disabilita i componenti di controllo (NUOVI - Direct)
        PlayerMovementDirect movementDirect = GetComponent<PlayerMovementDirect>();
        if (movementDirect != null) movementDirect.enabled = false;

        PlayerShootingDirect shootingDirect = GetComponent<PlayerShootingDirect>();
        if (shootingDirect != null) shootingDirect.enabled = false;

        // Disabilita il grappling system
        GrapplingSystem grappling = GetComponent<GrapplingSystem>();
        if (grappling != null) grappling.enabled = false;

        // Opzionale: Nascondi il player
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        // Opzionale: Disabilita le collisioni
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    void Respawn()
    {
        isDead = false;
        Debug.Log($"{gameObject.name} è RESPAWNATO!");

        // Teletrasporta alla posizione di respawn
        transform.position = respawnPosition;

        // Riabilita il player
        EnablePlayer();

        // Attiva invincibilità temporanea
        if (invincibleOnRespawn)
        {
            StartCoroutine(InvincibilityCoroutine());
        }

        // Suona il suono del respawn se specificato
        if (!string.IsNullOrEmpty(respawnSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(respawnSoundName);
        }
    }

    void EnablePlayer()
    {
        // Riabilita i componenti di controllo (vecchi)
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = true;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = true;

        // Riabilita i componenti di controllo (NUOVI - Direct)
        PlayerMovementDirect movementDirect = GetComponent<PlayerMovementDirect>();
        if (movementDirect != null) movementDirect.enabled = true;

        PlayerShootingDirect shootingDirect = GetComponent<PlayerShootingDirect>();
        if (shootingDirect != null) shootingDirect.enabled = true;

        // Riabilita il grappling system
        GrapplingSystem grappling = GetComponent<GrapplingSystem>();
        if (grappling != null) grappling.enabled = true;

        // Riabilita tutti i renderer
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }

        // POI disabilita esplicitamente solo i MeshRenderer degli oggetti da escludere
        if (objectsToKeepDisabled != null)
        {
            foreach (GameObject obj in objectsToKeepDisabled)
            {
                if (obj != null)
                {
                    // Disabilita SOLO il MeshRenderer, non tutto il GameObject
                    MeshRenderer meshRend = obj.GetComponent<MeshRenderer>();
                    if (meshRend != null)
                    {
                        meshRend.enabled = false;
                        Debug.Log($"[HEALTH] MeshRenderer disabilitato: {obj.name}");
                    }
                }
            }
        }

        // Riabilita le collisioni
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }

    // Helper per controllare se un GameObject è figlio di un altro
    bool IsChildOf(GameObject child, GameObject parent)
    {
        if (child == parent) return true;

        Transform current = child.transform;
        while (current != null)
        {
            if (current.gameObject == parent) return true;
            current = current.parent;
        }
        return false;
    }

    // Metodo pubblico per impostare la posizione di respawn
    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
    }

    // Coroutine per invincibilità temporanea con effetto lampeggio
    System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsedTime = 0f;

        Debug.Log($"{gameObject.name} è INVINCIBILE per {invincibilityDuration} secondi!");

        // Ottieni tutti i renderer MA escludi quelli da mantenere disabilitati
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Crea lista di renderer da far lampeggiare (esclusi quelli da mantenere disabilitati)
        System.Collections.Generic.List<Renderer> flickerRenderers = new System.Collections.Generic.List<Renderer>();

        foreach (Renderer rend in renderers)
        {
            bool shouldExclude = false;

            // Controlla se questo renderer appartiene a un oggetto da escludere
            if (objectsToKeepDisabled != null)
            {
                foreach (GameObject excludedObj in objectsToKeepDisabled)
                {
                    if (excludedObj != null)
                    {
                        Renderer excludedRenderer = excludedObj.GetComponent<Renderer>();
                        if (excludedRenderer == rend)
                        {
                            shouldExclude = true;
                            break;
                        }
                    }
                }
            }

            if (!shouldExclude)
            {
                flickerRenderers.Add(rend);
            }
        }

        // Lampeggia per tutta la durata dell'invincibilità
        while (elapsedTime < invincibilityDuration)
        {
            // Alterna visibilità SOLO per i renderer non esclusi
            bool visible = (Mathf.FloorToInt(elapsedTime / flickerSpeed) % 2 == 0);

            foreach (Renderer rend in flickerRenderers)
            {
                rend.enabled = visible;
            }

            yield return new WaitForSeconds(flickerSpeed);
            elapsedTime += flickerSpeed;
        }

        // Assicurati che siano tutti visibili alla fine (tranne quelli esclusi)
        foreach (Renderer rend in flickerRenderers)
        {
            rend.enabled = true;
        }

        // Mantieni disabilitati gli oggetti esclusi
        if (objectsToKeepDisabled != null)
        {
            foreach (GameObject obj in objectsToKeepDisabled)
            {
                if (obj != null)
                {
                    MeshRenderer meshRend = obj.GetComponent<MeshRenderer>();
                    if (meshRend != null)
                    {
                        meshRend.enabled = false;
                    }
                }
            }
        }

        isInvincible = false;
        Debug.Log($"{gameObject.name} NON è più invincibile!");
    }
}