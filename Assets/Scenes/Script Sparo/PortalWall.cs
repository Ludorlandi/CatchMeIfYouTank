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
    [SerializeField] private bool maintainVelocity = true; // Mantiene la velocit�
    [SerializeField] private bool flipDirection = false; // Inverte la direzione (opzionale)

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

    void OnCollisionEnter(Collision collision)
    {
        // Controlla se � un proiettile
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();

        if (projectile != null && !isTeleporting)
        {
            TeleportProjectile(collision.gameObject, collision);
        }
    }

    void TeleportProjectile(GameObject projectile, Collision collision)
    {
        if (linkedPortal == null)
        {
            Debug.LogWarning($"Portale {gameObject.name} non ha un portale collegato!");
            return;
        }

        // Previeni che il portale di destinazione riporti indietro il proiettile
        linkedPortal.isTeleporting = true;

        // Ottieni il Rigidbody del proiettile
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Salva la velocit� attuale
            Vector3 currentVelocity = rb.linearVelocity;

            // Calcola la nuova posizione
            // Posiziona il proiettile davanti al portale di uscita
            Vector3 exitDirection = linkedPortal.transform.forward;

            // Se flipDirection � attivo, inverti la direzione
            if (flipDirection)
            {
                exitDirection = -exitDirection;
            }

            Vector3 newPosition = linkedPortal.transform.position + (exitDirection * teleportOffset);

            // Teletrasporta il proiettile
            projectile.transform.position = newPosition;

            // Regola la velocit�
            if (maintainVelocity)
            {
                // Mantiene la velocit� ma nella direzione del portale di uscita
                float speed = currentVelocity.magnitude;
                rb.linearVelocity = exitDirection * speed;
            }
            else
            {
                // Mantiene la velocit� originale (direzione e tutto)
                rb.linearVelocity = currentVelocity;
            }

            Debug.Log($"Proiettile teletrasportato da {gameObject.name} a {linkedPortal.gameObject.name}");

            // Feedback visivo
            FlashPortal();
            linkedPortal.FlashPortal();
        }

        // Dopo un breve delay, riabilita il teletrasporto
        Invoke(nameof(ResetTeleport), 0.1f);
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
}