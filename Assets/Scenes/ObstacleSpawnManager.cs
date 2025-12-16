using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class ObstacleLayout
    {
        public string layoutName;
        public List<ObstacleSpawn> obstacles = new List<ObstacleSpawn>();
    }

    [System.Serializable]
    public class ObstacleSpawn
    {
        public GameObject obstaclePrefab; // Prefab dell'ostacolo (muro rimbalzo, power-up, etc.)
        public Vector3 position; // Posizione dove spawnare
        public Vector3 rotation; // Rotazione (opzionale)
    }

    [Header("Spawn Settings")]
    [SerializeField] private float firstSpawnDelay = 10f; // Primo spawn dopo 10 secondi
    [SerializeField] private float spawnInterval = 15f; // Ogni 15 secondi dopo il primo
    [SerializeField] private bool randomizeLayouts = true; // Shuffle l'ordine dei layout

    [Header("Spawn Area")]
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 8f, 0.2f); // X, Y, Z
    [SerializeField] private float minDistanceBetweenObstacles = 2f; // Distanza minima tra ostacoli

    [Header("Visual Indicator")]
    [SerializeField] private GameObject spawnIndicatorPrefab; // Prefab dell'indicatore visivo
    [SerializeField] private float indicatorDuration = 2f; // Quanto tempo mostrare l'indicatore prima dello spawn
    [SerializeField] private Color indicatorColor = Color.yellow;

    [Header("Layouts")]
    [SerializeField] private List<ObstacleLayout> layouts = new List<ObstacleLayout>();

    [Header("Audio (Opzionale)")]
    [SerializeField] private string spawnWarningSoundName = ""; // Suono quando appare l'indicatore
    [SerializeField] private string spawnCompleteSoundName = ""; // Suono quando spawna l'ostacolo

    private List<ObstacleLayout> availableLayouts = new List<ObstacleLayout>();
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private int currentLayoutIndex = 0;
    private bool isSpawning = false;

    // Singleton
    public static ObstacleSpawnManager Instance { get; private set; }

    void Awake()
    {
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
        // Crea copia dei layout disponibili
        availableLayouts = new List<ObstacleLayout>(layouts);

        // Randomizza l'ordine se richiesto
        if (randomizeLayouts)
        {
            ShuffleLayouts();
        }

        // Inizia lo spawn dopo il primo delay
        StartCoroutine(SpawnRoutine());
    }

    void ShuffleLayouts()
    {
        // Fisher-Yates shuffle
        for (int i = availableLayouts.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            ObstacleLayout temp = availableLayouts[i];
            availableLayouts[i] = availableLayouts[randomIndex];
            availableLayouts[randomIndex] = temp;
        }

        Debug.Log($"Layout randomizzati! Ordine: {string.Join(", ", availableLayouts.ConvertAll(l => l.layoutName))}");
    }

    IEnumerator SpawnRoutine()
    {
        // Aspetta prima dello spawn iniziale
        yield return new WaitForSeconds(firstSpawnDelay);

        // Spawna layout uno alla volta
        while (currentLayoutIndex < availableLayouts.Count)
        {
            if (!isSpawning)
            {
                StartCoroutine(SpawnNextLayout());
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("Tutti i layout sono stati spawnati!");
    }

    IEnumerator SpawnNextLayout()
    {
        if (currentLayoutIndex >= availableLayouts.Count)
        {
            Debug.Log("Non ci sono più layout da spawnare!");
            yield break;
        }

        isSpawning = true;

        ObstacleLayout currentLayout = availableLayouts[currentLayoutIndex];
        Debug.Log($"Spawning layout: {currentLayout.layoutName}");

        // Per ogni ostacolo nel layout
        foreach (ObstacleSpawn obstacle in currentLayout.obstacles)
        {
            if (obstacle.obstaclePrefab != null)
            {
                // Mostra indicatore visivo
                yield return StartCoroutine(ShowSpawnIndicator(obstacle.position));

                // Spawna l'ostacolo
                SpawnObstacle(obstacle);

                // Piccola pausa tra ostacoli dello stesso layout (opzionale)
                yield return new WaitForSeconds(0.5f);
            }
        }

        currentLayoutIndex++;
        isSpawning = false;
    }

    IEnumerator ShowSpawnIndicator(Vector3 position)
    {
        GameObject indicator = null;

        // Crea indicatore visivo
        if (spawnIndicatorPrefab != null)
        {
            indicator = Instantiate(spawnIndicatorPrefab, position, Quaternion.identity);
        }
        else
        {
            // Crea indicatore di default se non c'è prefab
            indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.transform.position = position;
            indicator.transform.localScale = new Vector3(1f, 0.1f, 1f);

            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = indicatorColor;
            }

            // Rimuovi collider dall'indicatore
            Collider col = indicator.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        // Suona il suono di warning
        if (!string.IsNullOrEmpty(spawnWarningSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(spawnWarningSoundName);
        }

        // Animazione pulsante (opzionale)
        StartCoroutine(PulseIndicator(indicator));

        // Aspetta
        yield return new WaitForSeconds(indicatorDuration);

        // Distruggi indicatore
        if (indicator != null)
        {
            Destroy(indicator);
        }
    }

    IEnumerator PulseIndicator(GameObject indicator)
    {
        if (indicator == null) yield break;

        Vector3 originalScale = indicator.transform.localScale;
        float elapsed = 0f;

        while (indicator != null && elapsed < indicatorDuration)
        {
            float scale = 1f + Mathf.Sin(elapsed * 5f) * 0.2f;
            indicator.transform.localScale = originalScale * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void SpawnObstacle(ObstacleSpawn obstacle)
    {
        // Verifica che la posizione sia valida
        if (!IsPositionValid(obstacle.position))
        {
            Debug.LogWarning($"Posizione {obstacle.position} non valida! Troppo vicina ad altri ostacoli.");
            return;
        }

        // Spawna l'ostacolo
        Quaternion rotation = Quaternion.Euler(obstacle.rotation);
        GameObject spawnedObstacle = Instantiate(obstacle.obstaclePrefab, obstacle.position, rotation);
        spawnedObstacles.Add(spawnedObstacle);

        // Suona il suono di spawn
        if (!string.IsNullOrEmpty(spawnCompleteSoundName) && SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(spawnCompleteSoundName);
        }

        Debug.Log($"Ostacolo spawnato: {obstacle.obstaclePrefab.name} a {obstacle.position}");
    }

    bool IsPositionValid(Vector3 position)
    {
        // Controlla che sia dentro l'area di spawn
        if (!IsInSpawnArea(position))
        {
            return false;
        }

        // Controlla la distanza dagli altri ostacoli
        foreach (GameObject obstacle in spawnedObstacles)
        {
            if (obstacle != null)
            {
                float distance = Vector3.Distance(position, obstacle.transform.position);
                if (distance < minDistanceBetweenObstacles)
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool IsInSpawnArea(Vector3 position)
    {
        float halfWidth = spawnAreaSize.x / 2f;
        float halfHeight = spawnAreaSize.y / 2f;

        // Controlla sul piano XY (vista laterale) invece di XZ
        return position.x >= spawnAreaCenter.x - halfWidth &&
               position.x <= spawnAreaCenter.x + halfWidth &&
               position.y >= spawnAreaCenter.y - halfHeight &&
               position.y <= spawnAreaCenter.y + halfHeight;
    }

    // Metodi pubblici per controllo
    public void PauseSpawning()
    {
        StopAllCoroutines();
    }

    public void ResumeSpawning()
    {
        StartCoroutine(SpawnRoutine());
    }

    public void ClearAllObstacles()
    {
        foreach (GameObject obstacle in spawnedObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        spawnedObstacles.Clear();
    }

    // Visualizza l'area di spawn nell'editor (piano XY)
    void OnDrawGizmosSelected()
    {
        // Area di spawn - box sul piano XY
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            spawnAreaCenter,
            spawnAreaSize
        );

        // Centro
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spawnAreaCenter, 0.3f);
    }
}