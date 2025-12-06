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
    [SerializeField] private GameObject winPanel; // Panel che mostra chi ha vinto
    [SerializeField] private Text winnerText; // Testo "Player X Wins!"
    [SerializeField] private Text restartText; // Testo "Premi R per ricominciare"

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

        if (winPanel != null)
        {
            winPanel.SetActive(false);
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
        Debug.Log($"{winner} WINS!");

        // Mostra pannello vittoria
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            winnerText.text = $"{winner} WINS!";
        }

        if (restartText != null)
        {
            restartText.text = "Premi R per ricominciare";
        }

        // NON riavvia automaticamente - aspetta input
        Debug.Log("Premi R per ricominciare");
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

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }
}