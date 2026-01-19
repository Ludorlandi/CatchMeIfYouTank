using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private string ownerTag = ""; // Tag del player che ha sparato (per evitare auto-danni)
    [SerializeField] private float minVelocity = 3f; // Velocità minima per evitare che si fermi
    [SerializeField] private int damage = 1; // Danno del proiettile (default 1 vita)

    [Header("Visual Rotation")]
    [SerializeField] private bool rotateTowardsDirection = true; // Ruota verso direzione movimento
    [SerializeField] private float rotationSpeed = 20f; // Velocità rotazione (più alto = più veloce)

    [Header("Audio (Opzionale)")]
    [SerializeField] private string[] bounceSoundNames = new string[10]; // Array di 10 suoni rimbalzo

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

        // ROTAZIONE VISIVA - ruota verso la direzione del movimento
        if (rotateTowardsDirection && rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            // Calcola angolo target basato sulla velocità
            float targetAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;

            // Rotazione target (sull'asse Z per giochi 2D)
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

            // Interpola smooth verso la rotazione target
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Controlla se ha colpito un player
        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

        if (health != null)
        {
            // IMPORTANTE: Ignora il proprietario (no autocolpimento!)
            if (collision.gameObject.CompareTag(ownerTag))
            {
                Debug.Log($"Proiettile ignora il proprietario: {collision.gameObject.name}");
                return; // Non fare nulla se colpisci te stesso
            }

            // Ha colpito un PLAYER AVVERSARIO - infliggi danno e distruggiti
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

            // Suona un suono di rimbalzo casuale
            PlayRandomBounceSound();

            Debug.Log($"Proiettile ha colpito: {collision.gameObject.name} (rimbalza)");
        }
    }

    void PlayRandomBounceSound()
    {
        if (SoundManager.Instance == null) return;

        // Filtra solo i nomi non vuoti
        List<string> validSounds = new List<string>();
        foreach (string soundName in bounceSoundNames)
        {
            if (!string.IsNullOrEmpty(soundName))
            {
                validSounds.Add(soundName);
            }
        }

        // Se ci sono suoni validi, scegline uno casuale
        if (validSounds.Count > 0)
        {
            int randomIndex = Random.Range(0, validSounds.Count);
            string randomSound = validSounds[randomIndex];

            // Suona alla posizione del proiettile
            SoundManager.Instance.PlayAtPosition(randomSound, transform.position);
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