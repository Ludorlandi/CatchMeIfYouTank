using UnityEngine;

public class PortalWall : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private PortalWall linkedPortal; // L'altro portale collegato
    [SerializeField] private float teleportOffset = 1f; // Distanza dal portale di uscita

    [Header("Visual Feedback")]
    [SerializeField] private Color portalColor = Color.cyan;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Projectile Adjustment")]
    [SerializeField] private bool maintainEntryDirection = true; // Mantiene direzione di entrata
    [SerializeField] private float exitOffset = 1f; // Distanza dal portale di uscita

    [Header("Audio (Opzionale)")]
    [SerializeField] private string teleportSoundName = ""; // Suono quando teletrasporta

    private Renderer wallRenderer;
    private Color originalColor;
    private bool isTeleporting = false; // Previene loop infiniti

    void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            originalColor = wallRenderer.material.color;
            // Colora il portale per distinguerlo
            wallRenderer.material.color = portalColor;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Controlla se è un proiettile
        Projectile projectile = other.GetComponent<Projectile>();

        if (projectile != null && !isTeleporting)
        {
            TeleportProjectile(other.gameObject);
        }
    }

    void TeleportProjectile(GameObject projectile)
    {
        if (linkedPortal == null)
        {
            Debug.LogWarning($"Portale {gameObject.name} non ha un portale collegato!");
            return;
        }

        Debug.Log($"[PORTAL] ===== TELEPORT START =====");
        Debug.Log($"[PORTAL] Da: {gameObject.name} (pos: {transform.position}, rot: {transform.rotation.eulerAngles})");
        Debug.Log($"[PORTAL] A: {linkedPortal.gameObject.name} (pos: {linkedPortal.transform.position}, rot: {linkedPortal.transform.rotation.eulerAngles})");

        // Ottieni il Rigidbody del proiettile
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Salva la velocità attuale (direzione di entrata!)
            Vector3 entryVelocity = rb.linearVelocity;
            Vector3 entryDirection = entryVelocity.normalized;
            float speed = entryVelocity.magnitude;

            Debug.Log($"[PORTAL] Entry velocity: {entryVelocity}, direction: {entryDirection}");

            // Spawna esattamente alla posizione del portale di uscita
            Vector3 newPosition = linkedPortal.transform.position;

            Debug.Log($"[PORTAL] Portal position: {linkedPortal.transform.position}");
            Debug.Log($"[PORTAL] New position: {newPosition}");

            // PRIMA teletrasporta
            projectile.transform.position = newPosition;

            // POI blocca entrambi i portali per evitare loop
            isTeleporting = true;
            linkedPortal.isTeleporting = true;

            // Suona il suono del teletrasporto se specificato
            if (!string.IsNullOrEmpty(teleportSoundName) && SoundManager.Instance != null)
            {
                SoundManager.Instance.Play(teleportSoundName);
            }

            // Mantiene la STESSA direzione e velocità di entrata
            if (maintainEntryDirection)
            {
                rb.linearVelocity = entryVelocity; // Stessa identica velocità (direzione + magnitudine)
            }

            Debug.Log($"[PORTAL] Proiettile esce con velocity: {rb.linearVelocity}");
            Debug.Log($"Proiettile teletrasportato da {gameObject.name} a {linkedPortal.gameObject.name}");

            // Feedback visivo
            FlashPortal();
            linkedPortal.FlashPortal();
        }

        // Dopo un breve delay, riabilita il teletrasporto
        Invoke(nameof(ResetTeleport), 0.3f); // Aumentato da 0.1 a 0.3
    }

    void ResetTeleport()
    {
        isTeleporting = false;
        if (linkedPortal != null)
        {
            linkedPortal.isTeleporting = false;
        }
    }

    void FlashPortal()
    {
        if (wallRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashCoroutine());
        }
    }

    System.Collections.IEnumerator FlashCoroutine()
    {
        // Cambia a colore bianco brillante
        wallRenderer.material.color = Color.white;

        // Aspetta
        yield return new WaitForSeconds(flashDuration);

        // Torna al colore del portale
        wallRenderer.material.color = portalColor;
    }

    // Metodo per visualizzare la connessione nell'editor
    void OnDrawGizmos()
    {
        if (linkedPortal != null)
        {
            Gizmos.color = portalColor;
            Gizmos.DrawLine(transform.position, linkedPortal.transform.position);
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
    }

    // Metodo pubblico per collegare portali dopo lo spawn
    public void SetLinkedPortal(PortalWall portal)
    {
        linkedPortal = portal;
        Debug.Log($"[PORTAL] {gameObject.name} collegato a {portal.gameObject.name}");
    }
}