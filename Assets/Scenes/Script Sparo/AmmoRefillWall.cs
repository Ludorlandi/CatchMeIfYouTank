using UnityEngine;

public class AmmoRefillWall : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int playerNumber = 1; // 1 o 2 - quale player riceve le munizioni
    [SerializeField] private int ammoToGive = 1; // Quante munizioni dare

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private Color flashColor = Color.green;
    [SerializeField] private float flashDuration = 0.2f;

    private Renderer wallRenderer;
    private Color originalColor;

    void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            originalColor = wallRenderer.material.color;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Controlla se è un proiettile
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            // Trova il player corretto
            GameObject player = GameObject.Find($"Player {playerNumber}");

            if (player == null)
            {
                // Prova con nomi alternativi
                player = GameObject.Find($"Player{playerNumber}");
            }

            if (player != null)
            {
                // Trova il componente PlayerShooting
                PlayerShooting shooting = player.GetComponent<PlayerShooting>();

                if (shooting != null)
                {
                    // Aggiungi munizioni
                    shooting.AddAmmo(ammoToGive);
                    Debug.Log($"Muro ha dato {ammoToGive} munizione/i al Player {playerNumber}");

                    // Feedback visivo
                    FlashWall();
                }
                else
                {
                    Debug.LogWarning($"PlayerShooting non trovato su Player {playerNumber}");
                }
            }
            else
            {
                Debug.LogWarning($"Player {playerNumber} non trovato nella scena!");
            }

            // Distruggi il proiettile
            Destroy(collision.gameObject);
        }
    }

    void FlashWall()
    {
        if (wallRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashCoroutine());
        }
    }

    System.Collections.IEnumerator FlashCoroutine()
    {
        // Cambia colore
        wallRenderer.material.color = flashColor;

        // Aspetta
        yield return new WaitForSeconds(flashDuration);

        // Torna al colore originale
        wallRenderer.material.color = originalColor;
    }
}