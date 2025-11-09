using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PlayerController : MonoBehaviour
{
    public GameObject actionCanvas;
    public GameObject skillCanvas;
    
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
        if (healthText != null && combatManager != null)
        {
            healthText.text = GenerateBarString("HP", currentHealth, maxHealth);
        }

        // Update the AP Bar
        if (apText != null)
        {
            apText.text = GenerateBarString("AP", currentAP, maxAP);
        }
    }


    public string GenerateBarString(string label, int current, int max)
    {
        int barLength = 10;
        char fillChar = '█';
        char emptyChar = '░';

        // Safety check to prevent divide-by-zero
        if (max == 0) return $"{label}: ERROR";

        // Clamp current value just in case
        current = Mathf.Clamp(current, 0, max);
        
        // Calculate the percentage and fill amount
        float percent = (float)current / max;
        int fillCount = Mathf.RoundToInt(percent * barLength);
        int emptyCount = barLength - fillCount;

        // Build the strings
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
                currentAP -= skill.apCost;
            }
            else
            {
                Debug.Log("Not enough AP to use " + skill.skillName);
            }
        }
    }

    public void OnCycleTarget()
    {
        if (combatManager != null && combatManager.state == CombatState.PLAYERTURN)
        {
            combatManager.CycleTarget();
        }
    }

    public void TakeDamage(int attackerAttack, int attackerLuck, int attackerPower)
    {
        float randMin = combatManager.randMinMultiplier;
        float randMax = combatManager.randMaxMultiplier;
        float atkPotency = combatManager.attackPotency;
        float defDivisor = combatManager.defenseDivisor;
        float defPotency = combatManager.defensePotency;
        float defPenalty = combatManager.defensePenaltyMultiplier;
        float defScalar = combatManager.defensePenaltyScalar;
        float defToggle = combatManager.defensePenaltyToggle;
        float flatBonus = combatManager.flatDamageBonus;

        float critChancePerLuck = combatManager.lucktoCrit;
        float critMultiplier = combatManager.critDamageMultiplier;

        // Formula Breakdown:
        float ATK = attackerAttack;
        float PWR = attackerPower;
        float DEF = this.defense; 

        float randValue = Random.Range(randMin * PWR, randMax * PWR);

        if (defDivisor == 0) defDivisor = 1f;
        
        float mainMultiplier = (ATK * atkPotency) / (DEF * defDivisor);
        float penalty = (DEF * defPotency * defPenalty) * defScalar * defToggle;

        int baseDamage = Mathf.FloorToInt(randValue * (mainMultiplier - penalty) + flatBonus);

        float critChance = attackerLuck * critChancePerLuck;
        float critRoll = Random.Range(0f, 100f);

        int finalDamage;
        if (critRoll < critChance)
        {
            finalDamage = (int)(Mathf.FloorToInt(baseDamage * critMultiplier));
            Debug.Log("Critical Hit!");
        }
        else
        {
            finalDamage = baseDamage;
        }
        
        if (finalDamage < 1) finalDamage = 1;
    
        currentHealth -= finalDamage;
        Debug.Log("Player takes " + finalDamage + " damage!");

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

    public void SetSkillPanel(bool show)
    {
        if (skillCanvas != null) skillCanvas.SetActive(show);
    }
}