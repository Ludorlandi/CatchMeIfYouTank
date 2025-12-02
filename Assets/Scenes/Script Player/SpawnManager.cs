using UnityEngine;

public class SimplePlayerSpawner : MonoBehaviour
{
    [Header("Assign the Player GameObjects from the scene")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    [Header("Spawn Positions")]
    [SerializeField] private Vector3 player1SpawnPosition = new Vector3(-8.5f, 0.75f, -0.1f);
    [SerializeField] private Vector3 player2SpawnPosition = new Vector3(8.5f, 0.75f, -0.1f);

    void Start()
    {
        // Posiziona i player alle coordinate giuste
        if (player1 != null)
        {
            player1.transform.position = player1SpawnPosition;
            Debug.Log($"Player 1 positioned at {player1SpawnPosition}");
        }
        else
        {
            Debug.LogWarning("Player 1 not assigned in SimplePlayerSpawner!");
        }

        if (player2 != null)
        {
            player2.transform.position = player2SpawnPosition;
            Debug.Log($"Player 2 positioned at {player2SpawnPosition}");
        }
        else
        {
            Debug.LogWarning("Player 2 not assigned in SimplePlayerSpawner!");
        }
    }
}