using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [Header("Lives Settings")]
    [SerializeField] private int startingLives = 5;

    [Header("Player 1 Lives UI")]
    [SerializeField] private Transform player1LivesContainer; // Parent che contiene le icone vite
    [SerializeField] private GameObject lifeIconPrefab; // Prefab dell'icona vita (Image o Sprite)

    [Header("Player 2 Lives UI")]
    [SerializeField] private Transform player2LivesContainer;

    [Header("Win Panel")]
    [SerializeField] private GameObject player1WinPanel; // Panel quando vince Player 1
    [SerializeField] private GameObject player2WinPanel; // Panel quando vince Player 2

    [Header("Game Objects to Disable on Victory")]
    [SerializeField] private GameObject[] objectsToDisableOnVictory; // Player, spawner, etc

    [Header("Audio (Opzionale)")]
    [SerializeField] private string victorySoundName = ""; // Suono quando qualcuno vince

    private List<GameObject> player1LifeIcons = new List<GameObject>();
    private List<GameObject> player2LifeIcons = new List<GameObject>();

    private int player1Lives;
    private int player2Lives;
    private bool gameEnded = false;

    // Singleton per accesso globale
    public static ScoreManager Instance { get; private set; }

    void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        player1Lives = startingLives;
        player2Lives = startingLives;

        CreateLifeIcons();

        // Nascondi entrambi i pannelli all'inizio
        if (player1WinPanel != null)
        {
            player1WinPanel.SetActive(false);
        }

        if (player2WinPanel != null)
        {
            player2WinPanel.SetActive(false);
        }
    }

    void CreateLifeIcons()
    {
        // Crea le icone per Player 1
        if (player1LivesContainer != null && lifeIconPrefab != null)
        {
            for (int i = 0; i < startingLives; i++)
            {
                GameObject lifeIcon = Instantiate(lifeIconPrefab, player1LivesContainer);
                player1LifeIcons.Add(lifeIcon);
            }
        }

        // Crea le icone per Player 2
        if (player2LivesContainer != null && lifeIconPrefab != null)
        {
            for (int i = 0; i < startingLives; i++)
            {
                GameObject lifeIcon = Instantiate(lifeIconPrefab, player2LivesContainer);
                player2LifeIcons.Add(lifeIcon);
            }
        }
    }

    // Chiamato quando un player muore
    public void OnPlayerDeath(string victimTag, string killerTag)
    {
        if (gameEnded) return;

        Debug.Log($"Death: Victim={victimTag}, Killer={killerTag}");

        // Determina chi perde una vita
        if (victimTag == "Player1" || victimTag == "Player 1")
        {
            // Player 1 perde una vita
            player1Lives--;
            RemoveLifeIcon(player1LifeIcons);
            Debug.Log($"Player 1 perde una vita! Vite rimaste: {player1Lives}");
        }
        else if (victimTag == "Player2" || victimTag == "Player 2")
        {
            // Player 2 perde una vita
            player2Lives--;
            RemoveLifeIcon(player2LifeIcons);
            Debug.Log($"Player 2 perde una vita! Vite rimaste: {player2Lives}");
        }

        CheckForWinner();
    }

    void RemoveLifeIcon(List<GameObject> lifeIcons)
    {
        // Trova l'ultima icona attiva e disabilitala
        for (int i = lifeIcons.Count - 1; i >= 0; i--)
        {
            if (lifeIcons[i].activeSelf)
            {
                lifeIcons[i].SetActive(false);
                return;
            }
        }
    }

    void CheckForWinner()
    {
        if (player1Lives <= 0)
        {
            EndGame("Player 2");
        }
        else if (player2Lives <= 0)
        {
            EndGame("Player 1");
        }
    }

    void EndGame(string winner)
    {
        gameEnded = true;
        Debug.Log($"{winner} WINS! Gioco bloccato.");

        // BLOCCA il gioco
        Time.timeScale = 0f; // Ferma completamente il tempo di gioco

        // Disabilita tutti gli oggetti di gioco (player, spawner, etc)
        DisableGameplay();

        // Suona il suono della vittoria se specificato
        if (!string.IsNullOrEmpty(victorySoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(victorySoundName);
        }

        // Mostra il pannello corretto in base al vincitore
        if (winner == "Player 1" && player1WinPanel != null)
        {
            player1WinPanel.SetActive(true);
            Debug.Log("[VICTORY] Mostrato WinPanel 1");
        }
        else if (winner == "Player 2" && player2WinPanel != null)
        {
            player2WinPanel.SetActive(true);
            Debug.Log("[VICTORY] Mostrato WinPanel 2");
        }

        Debug.Log("Premi R per ricominciare");
    }

    void DisableGameplay()
    {
        // Disabilita player - cerca con entrambe le varianti del tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player 1");
        foreach (GameObject player in players)
        {
            DisablePlayerComponents(player);
        }

        players = GameObject.FindGameObjectsWithTag("Player 2");
        foreach (GameObject player in players)
        {
            DisablePlayerComponents(player);
        }

        // Disabilita oggetti specifici (spawner, etc)
        if (objectsToDisableOnVictory != null)
        {
            foreach (GameObject obj in objectsToDisableOnVictory)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"[VICTORY] Disabilitato: {obj.name}");
                }
            }
        }
    }

    void DisablePlayerComponents(GameObject player)
    {
        // Disabilita movimento e sparo
        PlayerMovementDirect movement = player.GetComponent<PlayerMovementDirect>();
        if (movement != null) movement.enabled = false;

        PlayerShootingDirect shooting = player.GetComponent<PlayerShootingDirect>();
        if (shooting != null) shooting.enabled = false;

        GrapplingSystem grappling = player.GetComponent<GrapplingSystem>();
        if (grappling != null) grappling.enabled = false;

        Debug.Log($"[VICTORY] Player disabilitato: {player.name}");
    }

    void Update()
    {
        // Se il gioco è finito, ascolta il tasto R
        if (gameEnded && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        // Riabilita il tempo
        Time.timeScale = 1f;

        // Ricarica la scena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Metodi pubblici per debugging/testing
    public int GetPlayer1Lives()
    {
        return player1Lives;
    }

    public int GetPlayer2Lives()
    {
        return player2Lives;
    }

    public void ResetLives()
    {
        player1Lives = startingLives;
        player2Lives = startingLives;
        gameEnded = false;

        // Riattiva tutte le icone
        foreach (GameObject icon in player1LifeIcons)
        {
            icon.SetActive(true);
        }

        foreach (GameObject icon in player2LifeIcons)
        {
            icon.SetActive(true);
        }

        // Nascondi entrambi i pannelli
        if (player1WinPanel != null)
        {
            player1WinPanel.SetActive(false);
        }

        if (player2WinPanel != null)
        {
            player2WinPanel.SetActive(false);
        }
    }
}