using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private string ownerTag = ""; // Tag del player che ha sparato (per evitare auto-danni)

    private bool hasHit = false;

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return; // Già colpito qualcosa

        // Non colpire il proprio player
        if (!string.IsNullOrEmpty(ownerTag) && collision.gameObject.CompareTag(ownerTag))
        {
            return;
        }

        hasHit = true;

        // Colpo FATALE - uccide con 1 colpo
        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.Die();
            Debug.Log($"Proiettile ha UCCISO: {collision.gameObject.name}");
        }
        else
        {
            Debug.Log($"Proiettile ha colpito: {collision.gameObject.name}");
        }

        // Distruggi il proiettile quando colpisce qualcosa
        Destroy(gameObject);
    }

    // Imposta chi ha sparato questo proiettile
    public void SetOwner(string tag)
    {
        ownerTag = tag;
    }
}