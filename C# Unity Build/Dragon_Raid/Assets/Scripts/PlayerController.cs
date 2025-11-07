using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PlayerController : MonoBehaviour
{
    public GameObject actionCanvas;
    
    [HideInInspector]
    public CombatManager combatManager; 

    [Header("Player Stats")]
    public int maxHealth = 50; 
    public int currentHealth;
    public int maxAP = 50;     
    public int currentAP;      
    public int attack = 10;  
    public int defense = 5; 
    public int luck = 5;     

    [Header("UI References")]
    public TextMeshProUGUI healthText; 
    public TextMeshProUGUI apText;     

    // Bar Style Settings
    [Header("ASCII Bar Settings")]
    public int barLength = 10;      // How many characters wide the bar is
    public char fillChar = '█';     // A solid block
    public char emptyChar = '░';    // A light shaded block

    void Start()
    {
        actionCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
        currentHealth = maxHealth;
        currentAP = maxAP; 
        
        UpdateUI(); // Set the initial values
    }
    
    /**
     * A single function to update all our UI elements
     */
    public void UpdateUI()
    {
        // Update the Health Bar
        if (healthText != null)
        {
            healthText.text = GenerateBarString("HP", currentHealth, maxHealth);
        }

        // Update the AP Bar
        if (apText != null)
        {
            apText.text = GenerateBarString("AP", currentAP, maxAP);
        }
    }

   
    private string GenerateBarString(string label, int current, int max)
    {
        // Safety check to prevent divide-by-zero
        if (max == 0) return $"{label}: ERROR";

        // Clamp current value just in case
        current = Mathf.Clamp(current, 0, max);
        
        // Calculate the percentage and fill amount
        float percent = (float)current / max;
        int fillCount = Mathf.RoundToInt(percent * barLength);
        int emptyCount = barLength - fillCount;

        // Build the string
        string fillString = new string(fillChar, fillCount);
        string emptyString = new string(emptyChar, emptyCount);

        // Return the final formatted string
        return label + ": [" + fillString + emptyString + "]";
        
    }


    // This is called by your buttons in the Inspector
    public void UseSkill(SkillData skill)
    {
        if (combatManager != null && combatManager.state == CombatState.PLAYERTURN)
        {
            if (currentAP >= skill.apCost)
            {
                combatManager.OnPlayerSkill(skill, attack, luck);
            }
            else
            {
                Debug.Log("Not enough AP to use " + skill.skillName);
            }
        }
    }

    public void TakeDamage(int attackerAttack, int attackerLuck, int attackerPower)
    {
        
        int finalDamage = 1; // Default to minimum damage

        currentHealth -= finalDamage;
        Debug.Log("Player takes " + finalDamage + " damage!");
        
        UpdateUI(); // Update the UI after taking damage

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has been defeated!");
        combatManager.state = CombatState.LOSE;
    }
    
    public void SetActionPanel(bool show)
    {
        if (actionCanvas != null) actionCanvas.SetActive(show);
    }
}