using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [Header("Lives Settings")]
    [SerializeField] private int startingLives = 0; // Parti con 0 vite
    [SerializeField] private int livesToWin = 5; // Vite necessarie per vincere

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
        // Crea le icone per Player 1 (tutte disabilitate all'inizio - 0 vite)
        if (player1LivesContainer != null && lifeIconPrefab != null)
        {
            for (int i = 0; i < livesToWin; i++)
            {
                GameObject lifeIcon = Instantiate(lifeIconPrefab, player1LivesContainer);
                lifeIcon.SetActive(false); // Parte disabilitata (0 vite)
                player1LifeIcons.Add(lifeIcon);
            }
        }

        // Crea le icone per Player 2 (tutte disabilitate all'inizio - 0 vite)
        if (player2LivesContainer != null && lifeIconPrefab != null)
        {
            for (int i = 0; i < livesToWin; i++)
            {
                GameObject lifeIcon = Instantiate(lifeIconPrefab, player2LivesContainer);
                lifeIcon.SetActive(false); // Parte disabilitata (0 vite)
                player2LifeIcons.Add(lifeIcon);
            }

            // IMPORTANTE: Inverti la lista di Player 2 per matchare l'ordine visivo
            // Se usi Reverse Arrangement nel layout, le icone sono al contrario visivamente
            // player1LifeIcons.Reverse();
            Debug.Log("[SCORE] Lista icone Player 2 invertita per matchare UI");
        }
    }

    // Chiamato quando un player muore
    public void OnPlayerDeath(string victimTag, string killerTag)
    {
        if (gameEnded) return;

        Debug.Log($"[SCORE] Death: Victim={victimTag}, Killer={killerTag}");
        Debug.Log($"[SCORE] Vite PRIMA: P1={player1Lives}, P2={player2Lives}");

        // IMPORTANTE: Se vittima == killer (autocolpimento), nessuno guadagna vite!
        if (victimTag == killerTag)
        {
            Debug.LogWarning($"[SCORE] {victimTag} si è autocolpito! Nessuno guadagna vita.");
            return;
        }

        // Il KILLER guadagna una vita (non la vittima che perde!)
        if (killerTag == "Player1" || killerTag == "Player 1")
        {
            // Player 1 ha ucciso → guadagna +1 vita
            player1Lives++;
            Debug.Log($"[SCORE] Player 1 guadagna vita! Totale: {player1Lives}/{livesToWin}");
            AddLifeIcon(player1LifeIcons);
        }
        else if (killerTag == "Player2" || killerTag == "Player 2")
        {
            // Player 2 ha ucciso → guadagna +1 vita
            player2Lives++;
            Debug.Log($"[SCORE] Player 2 guadagna vita! Totale: {player2Lives}/{livesToWin}");
            AddLifeIcon(player2LifeIcons); // Usa lista già invertita!
        }
        else
        {
            Debug.LogWarning($"[SCORE] Killer tag sconosciuto: '{killerTag}'");
        }

        Debug.Log($"[SCORE] Vite DOPO: P1={player1Lives}, P2={player2Lives}");
        CheckForWinner();
    }

    void AddLifeIcon(List<GameObject> lifeIcons)
    {
        // Attiva la prossima icona disabilitata (sempre da index 0 in su)
        // Per P2, la lista è già stata invertita quindi 0 = destra visivamente
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (!lifeIcons[i].activeSelf)
            {
                lifeIcons[i].SetActive(true);
                Debug.Log($"Icona vita index {i} attivata!");
                return;
            }
        }
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
        Debug.Log($"[SCORE] CheckForWinner: P1={player1Lives}/{livesToWin}, P2={player2Lives}/{livesToWin}");

        // Vince chi raggiunge per primo livesToWin (es: 5 vite)
        if (player1Lives >= livesToWin)
        {
            Debug.Log($"[SCORE] Player 1 HA VINTO! ({player1Lives} >= {livesToWin})");
            EndGame("Player 1");
        }
        else if (player2Lives >= livesToWin)
        {
            Debug.Log($"[SCORE] Player 2 HA VINTO! ({player2Lives} >= {livesToWin})");
            EndGame("Player 2");
        }
        else
        {
            Debug.Log($"[SCORE] Nessun vincitore ancora.");
        }
    }

    void EndGame(string winner)
    {
        gameEnded = true;
        Debug.Log($"{winner} WINS! Gioco bloccato.");

        // BLOCCA il gioco
        Time.timeScale = 1f; // Ferma completamente il tempo di gioco

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
        player1Lives = startingLives; // 0
        player2Lives = startingLives; // 0
        gameEnded = false;

        // Disabilita tutte le icone (parti con 0 vite)
        foreach (GameObject icon in player1LifeIcons)
        {
            icon.SetActive(false);
        }

        foreach (GameObject icon in player2LifeIcons)
        {
            icon.SetActive(false);
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