using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel; 
    
    public GameObject zoneSelectPanel; 

    void Start()
    {
        mainMenuPanel.SetActive(true);
        zoneSelectPanel.SetActive(false);
    }

    public void ShowZoneSelect()
    {
        // Hide the main menu
        mainMenuPanel.SetActive(false);
        
        // Show the zone select menu
        zoneSelectPanel.SetActive(true);
    }

    // This will be for the "Back" button in the zone select menu
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        
        zoneSelectPanel.SetActive(false);
    }

    public void LoadZone(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("QUITTING GAME..."); 
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}