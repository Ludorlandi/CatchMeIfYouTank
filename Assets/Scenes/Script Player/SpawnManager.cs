using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

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

        // Assegna i controller specifici a ciascun player
        AssignControllersToPlayers();
    }

    void AssignControllersToPlayers()
    {
        // Trova tutti i gamepad connessi
        var gamepads = Gamepad.all;

        Debug.Log($"Trovati {gamepads.Count} gamepad connessi");

        if (gamepads.Count == 0)
        {
            Debug.LogError("Nessun gamepad trovato! Connetti almeno un controller.");
            return;
        }

        // Mostra info sui gamepad
        for (int i = 0; i < gamepads.Count; i++)
        {
            Debug.Log($"Gamepad {i}: {gamepads[i].displayName} (Device ID: {gamepads[i].deviceId})");
        }

        // Assegna il primo gamepad a Player 1
        if (player1 != null)
        {
            PlayerInput input1 = player1.GetComponent<PlayerInput>();
            if (input1 != null && gamepads.Count > 0)
            {
                // Unpair tutti i dispositivi prima
                InputUser.PerformPairingWithDevice(gamepads[0], input1.user, InputUserPairingOptions.UnpairCurrentDevicesFromUser);
                Debug.Log($"Player 1 ora usa SOLO: {gamepads[0].displayName}");
            }
        }

        // Assegna il secondo gamepad a Player 2 (SE DISPONIBILE)
        if (player2 != null)
        {
            PlayerInput input2 = player2.GetComponent<PlayerInput>();
            if (input2 != null)
            {
                if (gamepads.Count > 1)
                {
                    // Due controller: Player 2 usa il secondo
                    InputUser.PerformPairingWithDevice(gamepads[1], input2.user, InputUserPairingOptions.UnpairCurrentDevicesFromUser);
                    Debug.Log($"Player 2 ora usa SOLO: {gamepads[1].displayName}");
                }
                else
                {
                    // Un solo controller: disabilita Player 2
                    Debug.LogWarning("Solo 1 controller trovato - Player 2 DISABILITATO");
                    player2.SetActive(false);
                }
            }
        }
    }
}