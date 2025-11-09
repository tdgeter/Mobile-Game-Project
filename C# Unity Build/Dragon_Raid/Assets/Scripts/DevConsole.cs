using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// dev console, use /help for commands
public class DevConsole : MonoBehaviour
{
    [Header("UI References")]
    public GameObject consolePanel;        // Container panel to show/hide console
    public TMP_InputField inputField;      // Input field to type commands
    public TextMeshProUGUI outputText;     // echo output here

    private bool visible = false;

    void Awake()
    {
        SetVisible(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            Toggle();
        }

        // Quick convenience: Ctrl+K triggers killall even without opening console
        if (Keyboard.current != null && Keyboard.current.ctrlKey.isPressed && Keyboard.current.kKey.wasPressedThisFrame)
        {
            ExecuteCommand("killall");
        }
    }

    public void Toggle()
    {
        SetVisible(!visible);
    }

    private void SetVisible(bool show)
    {
        visible = show;
        if (consolePanel != null) consolePanel.SetActive(show);

        if (show && inputField != null)
        {
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }
    }

    // Hook this to TMP_InputField.onSubmit or call from a UI Button
    public void OnSubmitCommand()
    {
        if (inputField == null) return;
        string cmd = inputField.text;
        ExecuteCommand(cmd);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    private void ExecuteCommand(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;

        string cmd = raw.Trim().ToLowerInvariant();
        Log($"> {raw}");

        switch (cmd)
        {
            case "help":
            case "?":
                Log("Commands: help | killall (aliases: kill all, kill_enemies, slay)");
                break;
            case "killall":
            case "kill all":
            case "kill_enemies":
            case "slay":
                CombatManager cm = null;
#if UNITY_2023_1_OR_NEWER
                cm = Object.FindFirstObjectByType<CombatManager>();
#else
                cm = Object.FindObjectOfType<CombatManager>();
#endif
                if (cm != null)
                {
                    cm.KillAllEnemiesInCurrentRound();
                    Log("All enemies eliminated.");
                }
                else
                {
                    Log("CombatManager not found in scene.");
                }
                break;
            default:
                Log($"Unknown command: {raw}");
                break;
        }
    }

    private void Log(string msg)
    {
        Debug.Log($"[DevConsole] {msg}");
        if (outputText != null)
        {
            outputText.text += (outputText.text.Length > 0 ? "\n" : string.Empty) + msg;
        }
    }
}
// blah blah commit comment just to test