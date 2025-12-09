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

        // Aspetta 1 frame per permettere ai PlayerInput di inizializzarsi
        StartCoroutine(AssignControllersDelayed());
    }

    System.Collections.IEnumerator AssignControllersDelayed()
    {
        // Aspetta la fine del frame
        yield return new WaitForEndOfFrame();

        // Assegna i controller
        AssignControllersToPlayers();
    }

    void AssignControllersToPlayers()
    {
        // Trova tutti i gamepad connessi
        var gamepads = Gamepad.all;

        Debug.Log($"=== INIZIO ASSEGNAZIONE CONTROLLER ===");
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

        // NUOVO APPROCCIO: Disabilita entrambi i PlayerInput prima
        PlayerInput input1 = player1?.GetComponent<PlayerInput>();
        PlayerInput input2 = player2?.GetComponent<PlayerInput>();

        if (input1 != null) input1.enabled = false;
        if (input2 != null) input2.enabled = false;

        Debug.Log("PlayerInput disabilitati temporaneamente");

        // Assegna Controller 0 a Player 1
        Debug.Log("--- Assegna Controller 0 a Player 1 ---");
        if (player1 != null && input1 != null && gamepads.Count > 0)
        {
            // Riabilita Player 1
            input1.enabled = true;

            // Aspetta un frame per permettere l'inizializzazione
            StartCoroutine(PairDeviceDelayed(input1, gamepads[0], "Player 1"));
        }

        // Assegna Controller 1 a Player 2 (SE DISPONIBILE)
        Debug.Log("--- Assegna Controller 1 a Player 2 ---");
        if (player2 != null && input2 != null)
        {
            if (gamepads.Count > 1)
            {
                // Riabilita Player 2
                input2.enabled = true;

                // Assegna il secondo gamepad
                StartCoroutine(PairDeviceDelayed(input2, gamepads[1], "Player 2"));
            }
            else
            {
                // Un solo controller: lascia Player 2 disabilitato
                Debug.LogWarning("Solo 1 controller trovato - Player 2 DISABILITATO");
                player2.SetActive(false);
            }
        }

        Debug.Log($"=== FINE ASSEGNAZIONE CONTROLLER ===");
    }

    System.Collections.IEnumerator PairDeviceDelayed(PlayerInput playerInput, InputDevice device, string playerName)
    {
        // Aspetta qualche frame per permettere l'inizializzazione
        yield return new WaitForSeconds(0.1f);

        try
        {
            InputUser.PerformPairingWithDevice(
                device,
                playerInput.user,
                InputUserPairingOptions.UnpairCurrentDevicesFromUser
            );
            Debug.Log($"✓ {playerName} ora usa: {device.displayName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Errore pairing {playerName}: {e.Message}");
        }
    }
}