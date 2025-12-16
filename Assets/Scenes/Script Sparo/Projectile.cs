using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private string ownerTag = ""; // Tag del player che ha sparato (per evitare auto-danni)
    [SerializeField] private float minVelocity = 3f; // Velocità minima per evitare che si fermi
    [SerializeField] private int damage = 1; // Danno del proiettile (default 1 vita)

    private bool hasHit = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Mantieni una velocità minima per evitare che il proiettile si fermi
        if (rb != null && rb.linearVelocity.magnitude < minVelocity)
        {
            // Se è troppo lento, mantieni la direzione ma aumenta la velocità
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
            // Ha colpito un PLAYER - infliggi danno e distruggiti
            if (hasHit) return; // Evita colpi multipli
            hasHit = true;

            health.TakeDamage(damage, ownerTag); // Infliggi danno
            Debug.Log($"Proiettile ha colpito {collision.gameObject.name} per {damage} danno!");

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

    // Imposta il danno di questo proiettile
    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }
}