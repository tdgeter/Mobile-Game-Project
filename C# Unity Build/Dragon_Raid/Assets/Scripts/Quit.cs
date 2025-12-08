using UnityEngine;
using UnityEngine.InputSystem;


public class Quit : MonoBehaviour
{
    [Header("Confirmation")]
    public GameObject confirmPanel;   // confirm quit panel
    public bool useConfirmation = false;

    [Header("Back Button")]
    public bool backButtonTriggersQuit = true; // Android Back button handling

    void Awake()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void Update()
    {
        if (!backButtonTriggersQuit) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (useConfirmation && confirmPanel != null)
            {
                confirmPanel.SetActive(true);
            }
            else
            {
                QuitApp();
            }
        }
    }

    public void OnQuitButtonPressed()
    {
        if (useConfirmation && confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
        else
        {
            QuitApp();
        }
    }

    public void ConfirmQuit()
    {
        QuitApp();
    }
    public void CancelQuit()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

        public void QuitApp()
    {
#if UNITY_EDITOR
        // Stop play mode in Editor
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID

        Application.Quit();
#else
        Application.Quit();
#endif
    }
}
