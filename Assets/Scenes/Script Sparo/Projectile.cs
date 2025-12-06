using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private string ownerTag = ""; // Tag del player che ha sparato (per evitare auto-danni)
    [SerializeField] private float minVelocity = 3f; // Velocit� minima per evitare che si fermi

    private bool hasHit = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Mantieni una velocit� minima per evitare che il proiettile si fermi
        if (rb != null && rb.linearVelocity.magnitude < minVelocity)
        {
            // Se � troppo lento, mantieni la direzione ma aumenta la velocit�
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * minVelocity;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Controlla se ha colpito un player
        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

        if (health != null)
        {
            // Ha colpito un PLAYER - uccidilo e distruggiti
            if (hasHit) return; // Evita colpi multipli
            hasHit = true;

            health.Die(ownerTag); // Passa chi ha sparato per il punteggio
            Debug.Log($"Proiettile ha UCCISO: {collision.gameObject.name}");

            // Distruggi il proiettile
            Destroy(gameObject);
        }
        else
        {
            // Ha colpito un MURO o altro oggetto
            // NON distruggere - lascia che rimbalzi o si teletrasporti
            Debug.Log($"Proiettile ha colpito: {collision.gameObject.name} (rimbalza)");
        }
    }

    // Imposta chi ha sparato questo proiettile
    public void SetOwner(string tag)
    {
        ownerTag = tag;
    }
}