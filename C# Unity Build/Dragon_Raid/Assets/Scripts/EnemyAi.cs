using UnityEngine;
using TMPro;
using Unity.VisualScripting;


public class EnemyAI : MonoBehaviour
{
    public EnemyData baseStats;
    public int currentHealth;
    public int currentAttack;
    public int currentDefense;
    public int currentLuck;
    public int currentPower; // Enemy basic attack power

    public CombatManager combatManager;
    private PlayerController playerTarget;

    [Header("UI")]
    public TextMeshProUGUI healthText; // Drag your new text object here
    
    public int barLength = 10;
    public char fillChar = '█';
    public char emptyChar = '░';
    // -------------------------

    public void Setup(EnemyData data, CombatManager manager, PlayerController player)
    {
        baseStats = data;
        combatManager = manager;
        playerTarget = player;

        currentHealth = baseStats.maxHealth;
        currentAttack = baseStats.attack;
        currentDefense = baseStats.defense;
        currentLuck = baseStats.luck;
        currentPower = baseStats.power;

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerTarget != null && healthText != null && combatManager != null)
        {
            healthText.text = combatManager.GenerateBarString(baseStats.enemyName,currentHealth, baseStats.maxHealth);
        }
    }

    // This is the enemy's basic attack
    public void TakeTurn()
    {
        if (playerTarget != null)
        {
            Debug.Log(baseStats.enemyName + " attacks the player!");



            // The enemy just uses its basic stats for its turn
            playerTarget.TakeDamage(currentAttack, currentLuck, currentPower);
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


        // Formula stats
        float ATK = attackerAttack;
        float PWR = attackerPower;
        float DEF = this.currentDefense; // This is the enemy's defense

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
        Debug.Log(baseStats.enemyName + " takes " + finalDamage + " damage.");

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }


    }



    void Die()
    {
        combatManager.EnemyDied(this.gameObject);
        Destroy(this.gameObject);
    }

    // console command to immediately kill this enemy
    public void ForceKill()
    {
        currentHealth = 0;
        Die();
    }
}