using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int ammoAmount = 1; // Quante munizioni dare
    [SerializeField] private bool destroyOnPickup = true; // Distruggi dopo il pickup
    [SerializeField] private float respawnTime = 5f; // Tempo di respawn (se non distrutto)

    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 100f; // Rotazione per renderlo visibile
    [SerializeField] private float bobSpeed = 2f; // Velocità del movimento su/giù
    [SerializeField] private float bobHeight = 0.3f; // Altezza del movimento su/giù

    [Header("Audio (Opzionale)")]
    [SerializeField] private string pickupSoundName = ""; // Suono quando viene raccolto

    private Vector3 startPosition;
    private bool isActive = true;
    private Renderer objectRenderer;
    private Collider objectCollider;

    void Start()
    {
        startPosition = transform.position;
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (!isActive) return;

        // Rotazione continua
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Movimento su e giù (bobbing)
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Controlla se è un player
        PlayerShootingDirect shooting = other.GetComponent<PlayerShootingDirect>();

        if (shooting != null)
        {
            // Dai munizioni al player
            shooting.AddAmmo(ammoAmount);
            Debug.Log($"{other.gameObject.name} ha raccolto {ammoAmount} munizione/i!");

            // Suona il suono del pickup
            if (!string.IsNullOrEmpty(pickupSoundName) && SoundManager.Instance != null)
            {
                SoundManager.Instance.Play(pickupSoundName);
            }

            // Gestisci il pickup
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                // Respawn dopo un po'
                StartRespawn();
            }
        }
    }

    void StartRespawn()
    {
        isActive = false;

        // Nascondi l'oggetto
        if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }

        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        // Riattiva dopo il tempo di respawn
        Invoke(nameof(Respawn), respawnTime);
    }

    void Respawn()
    {
        isActive = true;

        // Rimostra l'oggetto
        if (objectRenderer != null)
        {
            objectRenderer.enabled = true;
        }

        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }

        // Resetta la posizione
        transform.position = startPosition;
    }
}