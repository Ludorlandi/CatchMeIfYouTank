using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private bool respawnOnDeath = true;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private Vector3 respawnPosition;

    private bool isDead = false;

    void Start()
    {
        // Salva la posizione iniziale come respawn point
        if (respawnPosition == Vector3.zero)
        {
            respawnPosition = transform.position;
        }
    }

    public void Die()
    {
        if (isDead) return; // Già morto

        isDead = true;
        Debug.Log($"{gameObject.name} è MORTO!");

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

    void DisablePlayer()
    {
        // Disabilita i componenti di controllo
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;

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
    }

    void EnablePlayer()
    {
        // Riabilita i componenti di controllo
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = true;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = true;

        // Rimostra il player
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }

        // Riabilita le collisioni
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }

    // Metodo pubblico per impostare la posizione di respawn
    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
    }
}