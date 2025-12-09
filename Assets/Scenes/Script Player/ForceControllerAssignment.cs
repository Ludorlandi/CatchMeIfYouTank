using UnityEngine;
using UnityEngine.InputSystem;

public class ForceControllerAssignment : MonoBehaviour
{
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    void Start()
    {
        StartCoroutine(SetupControllers());
    }

    System.Collections.IEnumerator SetupControllers()
    {
        Debug.Log("=== SETUP CONTROLLER CON CONTROL SCHEMES ===");

        var gamepads = Gamepad.all;
        Debug.Log($"Gamepads trovati: {gamepads.Count}");

        if (gamepads.Count < 2)
        {
            Debug.LogError("Servono 2 controller!");
            yield break;
        }

        // Aspetta un frame per permettere l'inizializzazione
        yield return new WaitForEndOfFrame();

        PlayerInput input1 = player1?.GetComponent<PlayerInput>();
        PlayerInput input2 = player2?.GetComponent<PlayerInput>();

        // ASSEGNA usando i Control Schemes che hai creato
        if (input1 != null)
        {
            try
            {
                input1.SwitchCurrentControlScheme("Gamepad1", gamepads[0]);
                Debug.Log($"✓ Player 1 assegnato a Gamepad1 con controller: {gamepads[0].displayName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Errore assegnazione Player 1: {e.Message}");
            }
        }

        if (input2 != null)
        {
            try
            {
                input2.SwitchCurrentControlScheme("Gamepad2", gamepads[1]);
                Debug.Log($"✓ Player 2 assegnato a Gamepad2 con controller: {gamepads[1].displayName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Errore assegnazione Player 2: {e.Message}");
            }
        }

        Debug.Log("=== SETUP COMPLETO ===");
    }
}