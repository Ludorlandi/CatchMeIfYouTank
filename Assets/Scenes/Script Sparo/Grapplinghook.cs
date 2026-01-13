using UnityEngine;

/// <summary>
/// Script per il proiettile-gancio che vola e si attacca agli oggetti
/// </summary>
public class GrapplingHook : MonoBehaviour
{
    private GrapplingSystem grapplingSystem;
    private float maxDistance;
    private Vector3 startPosition;
    private bool hasAttached = false;

    public void Initialize(GrapplingSystem system, float maxDist, Vector3 startPos)
    {
        grapplingSystem = system;
        maxDistance = maxDist;
        startPosition = startPos;
    }

    void Update()
    {
        if (hasAttached) return;

        // Controlla se ha superato distanza massima
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled > maxDistance)
        {
            // Troppo lontano - mancato
            grapplingSystem.OnHookMissed();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasAttached) return;

        // Controlla se può essere agganciato
        if (other.CompareTag("Grabbable"))
        {
            // Controlla se già preso da qualcun altro
            GrabbableObject grabbable = other.GetComponent<GrabbableObject>();
            if (grabbable != null && grabbable.IsGrabbed())
            {
                Debug.Log("Oggetto già agganciato da qualcun altro!");
                grapplingSystem.OnHookMissed();
                Destroy(gameObject);
                return;
            }

            // Attacca!
            hasAttached = true;

            // Ferma movimento
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // Parenta al target così segue l'oggetto
            transform.SetParent(other.transform);

            // Notifica sistema
            grapplingSystem.OnHookAttached(other.gameObject);

            Debug.Log($"Gancio attaccato a: {other.name}");
        }
        else
        {
            // Oggetto non grabbable - annulla
            Debug.Log($"Colpito oggetto non grabbable: {other.name}");
            grapplingSystem.OnHookMissed();
            Destroy(gameObject);
        }
    }
}