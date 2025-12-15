using UnityEngine;
using System.Collections;

public class PowerUpWall : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private int damageMultiplier = 2; // Quante vite toglie il proiettile potenziato
    [SerializeField] private Color powerUpColor = Color.red; // Colore del proiettile potenziato
    [SerializeField] private float sizeMultiplier = 1.5f; // Quanto più grande diventa

    [Header("Visual Feedback")]
    [SerializeField] private Color wallColor = Color.magenta;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Audio (Opzionale)")]
    [SerializeField] private string powerUpSoundName = ""; // Suono quando potenzia

    private Renderer wallRenderer;
    private Color originalWallColor;

    void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            originalWallColor = wallRenderer.material.color;
            // Colora il muro per distinguerlo
            wallRenderer.material.color = wallColor;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Controlla se è un proiettile
        Projectile projectile = other.GetComponent<Projectile>();

        if (projectile != null)
        {
            PowerUpProjectile(projectile);

            // Feedback visivo
            FlashWall();

            // Suona il suono del power-up
            if (!string.IsNullOrEmpty(powerUpSoundName) && SoundManager.Instance != null)
            {
                SoundManager.Instance.Play(powerUpSoundName);
            }

            Debug.Log("Proiettile POTENZIATO! Ora toglie 2 vite!");
        }
    }

    void PowerUpProjectile(Projectile projectile)
    {
        // Imposta il danno del proiettile
        projectile.SetDamage(damageMultiplier);

        // Cambia l'aspetto del proiettile
        Renderer projRenderer = projectile.GetComponent<Renderer>();
        if (projRenderer != null)
        {
            projRenderer.material.color = powerUpColor;
        }

        // Aumenta la dimensione
        projectile.transform.localScale *= sizeMultiplier;

        // Opzionale: aggiungi un trail o particelle per mostrare che è potenziato
        // (puoi aggiungere qui se vuoi)
    }

    void FlashWall()
    {
        if (wallRenderer != null)
        {
            StartCoroutine(FlashCoroutine());
        }
    }

    IEnumerator FlashCoroutine()
    {
        wallRenderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        wallRenderer.material.color = wallColor;
    }
}