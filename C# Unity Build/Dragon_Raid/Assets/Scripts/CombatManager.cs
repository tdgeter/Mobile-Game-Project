using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public enum CombatState { START, PLAYERTURN, ENEMYTURN, WIN, LOSE }


[System.Serializable]
public class EnemySpawn
{
    public GameObject enemyPrefab;
    public EnemyData enemyData;   
}


[System.Serializable]
public class Round
{
    public string roundName;
    public EnemySpawn[] enemiesInThisRound; 
}

public class CombatManager : MonoBehaviour
{
    public CombatState state;

    [Header("Prefabs & Spawns")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints; 

    [Header("Damage Formula Tuning")]
  
    public float randMinMultiplier = 1f;
    public float randMaxMultiplier = 1f;

    // (ATK * atkPotency) / (DEF * defDivisor)
    public float attackPotency = 1f;
    public float defenseDivisor = 1f; // (Set to 1 to avoid divide-by-zero)

    public float defensePotency = 1f;
    public float defensePenaltyMultiplier = 1f;
    public float defensePenaltyScalar = 1f;
    public float defensePenaltyToggle = 0f;

    // + flatBonus
    public float flatDamageBonus = 0f;

    public float lucktoCrit = 0.5f; // Chance to crit per LCK point
    public float critDamageMultiplier = 1.5f; // Crit damage multiplier
    
    [Header("Round Control")]
    public Round[] allRounds; 
    private int currentRoundIndex = 0; 
    
    [Header("Combat Tuning")]
    public int apRegenPerTurn = 15; // AP to give player each turn

    // These are set at runtime
    private List<EnemyAI> activeEnemies = new List<EnemyAI>();
    private PlayerController player;

    /**
     * Called when the scene starts. Begins the battle setup.
     */
    void Start()
    {
        // Use a Coroutine to set up the battle with delays
        StartCoroutine(SetupBattle());
    }
    
    /**
     * Spawns the player, gives them references, and starts Round 1.
     */
    IEnumerator SetupBattle()
    {
        state = CombatState.START;
        
        GameObject playerGO = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        
        player = playerGO.GetComponent<PlayerController>();

        player.combatManager = this;
        
        player.SetActionPanel(false);

        yield return null; 
        
        // Start the first round
        StartRound(currentRoundIndex);
    }

    /**
     * Spawns all enemies for a given round based on the 'allRounds' array.
     */
    void StartRound(int roundIndex)
    {
        activeEnemies.Clear();
        Round round = allRounds[roundIndex];
        
        // Loop through all enemies defined for this round
        for (int i = 0; i < round.enemiesInThisRound.Length; i++)
        {
            // Only spawn if we have an available spawn point
            if (i < enemySpawnPoints.Length)
            {
                EnemySpawn spawnInfo = round.enemiesInThisRound[i];
                
                // 2. Spawn the correct prefab
                GameObject newEnemyGO = Instantiate(spawnInfo.enemyPrefab, enemySpawnPoints[i].position, Quaternion.identity);
                
                // 3. Get its AI script
                EnemyAI newEnemyAI = newEnemyGO.GetComponent<EnemyAI>();
                
                newEnemyAI.Setup(spawnInfo.enemyData, this, player); 
                
                // 5. Add it to our list of active enemies
                activeEnemies.Add(newEnemyAI);
            }
        }
        
        Debug.Log("Starting " + round.roundName);

        // The round is set up, begin the player's turn
        state = CombatState.PLAYERTURN;
        PlayerTurnStart();
    }
    
    
    /**
     * Called at the beginning of the player's turn.
     * Regenerates AP and shows the action UI.
     */
    void PlayerTurnStart()
    {
        Debug.Log("Player's turn!");

        // Regenerate AP
        player.currentAP += apRegenPerTurn;
        if (player.currentAP > player.maxAP)
        {
            player.currentAP = player.maxAP;
        }
        Debug.Log("Player AP regenerated to " + player.currentAP);
       
        player.SetActionPanel(true);
    }

    /**
     * This is called by the PlayerController when a skill button is pressed.
     * It spends the AP and starts the attack sequence.
     */
    public void OnPlayerSkill(SkillData skill, int attackerAttack, int attackerLuck)
    {
        if (state != CombatState.PLAYERTURN) return; // Safety check

        player.currentAP -= skill.apCost;
        Debug.Log("Player used " + skill.skillName + ", AP remaining: " + player.currentAP);


        EndPlayerTurn(); 

        if (activeEnemies.Count > 0)
        {
            StartCoroutine(PlayerAttackSequence(skill, attackerAttack, attackerLuck, activeEnemies[0]));
        }
    }

    /**
     * Coroutine that handles the player's attack, including multi-hits.
     */
    IEnumerator PlayerAttackSequence(SkillData skill, int attackerAttack, int attackerLuck, EnemyAI target)
    {
        // Loop for each hit in the skill
        for (int i = 0; i < skill.hitCount; i++)
        {
            if (target != null) // Check if target is still alive
            {
                Debug.Log("Hit " + (i + 1) + "!");
                
                // Calculate the final power for this one hit
                int finalPower = (int)(skill.power * skill.damageMultiplier);
                
                // Tell the target to take damage
                target.TakeDamage(attackerAttack, attackerLuck, finalPower);
                
                yield return new WaitForSeconds(0.2f); // Short pause between hits
            }
            else
            {
                break; // Stop attacking if target died mid-combo
            }
        }
        
        // After all hits, start the enemy's turn
        StartCoroutine(EnemyTurn());
    }

    /**
     * Called when the player's action is finished.
     * Hides the UI and sets the state to ENEMYTURN.
     */
    void EndPlayerTurn()
    {
        state = CombatState.ENEMYTURN;
        Debug.Log("Ending player turn.");
        player.SetActionPanel(false);
    }
    
    /**
     * Coroutine that loops through each living enemy and has them take their turn.
     */
    IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy turn!");
        
        // We use a copy of the list in case an enemy dies mid-turn
        List<EnemyAI> enemiesThisTurn = new List<EnemyAI>(activeEnemies);

        foreach(EnemyAI enemy in enemiesThisTurn)
        {
            if (enemy != null) // Check if enemy is still alive
            {
                yield return new WaitForSeconds(1f); // Pause between enemy attacks
                enemy.TakeTurn();
            }
        }

        // After all enemies have moved, end the enemy phase
        EndEnemyTurnPhase();
    }
    
    /**
     * Called after all enemies have taken their turn.
     * Switches the state back to the player.
     */
    public void EndEnemyTurnPhase()
    {
        // Only switch back if we are still in the enemy turn
        if (state == CombatState.ENEMYTURN)
        {
            state = CombatState.PLAYERTURN;
            PlayerTurnStart();
        }
    }

    
    public void EnemyDied(GameObject deadEnemy)
    {
        EnemyAI ai = deadEnemy.GetComponent<EnemyAI>();
        if (activeEnemies.Contains(ai))
        {
            activeEnemies.Remove(ai); // Remove from our active list
        }
        
        // If all enemies are dead, end the round
        if (activeEnemies.Count == 0)
        {
            Debug.Log("Round " + allRounds[currentRoundIndex].roundName + " complete!");
            StartNextRound();
        }
    }

    /**
     * This function is called when all enemies in a round are defeated.
     * It checks if the completed round was a "reward round".
     */
    void StartNextRound()
    {
        bool isRewardRound = false;
        int roundJustFinished = currentRoundIndex; 

        // Check if the round we JUST finished is a reward round
        if (roundJustFinished == 0) 
        {
            isRewardRound = true;
        }
        else if (roundJustFinished == 2) 
        {
            isRewardRound = true;
        }
        else if (roundJustFinished == 4) 
        {
            StartCoroutine(ShowRewardScreen(true)); 
            return; // Stop here. The coroutine will handle the win.
        }

        if (isRewardRound)
        {
            StartCoroutine(ShowRewardScreen(false));
            return; // Stop here. The coroutine will advance the round.
        }

        // If it's NOT a reward round
        
        // Move to the next round index
        currentRoundIndex++; 
        StartCoroutine(RoundTransition());
    }

    /**
     * This coroutine pauses the game and simulates a reward screen.
     * It now takes a parameter to know if it's the final round.
     */
    IEnumerator ShowRewardScreen(bool isFinalRound)
    {
        state = CombatState.START; // Use START as a "paused" state
        Debug.Log($"--- ROUND {currentRoundIndex + 1} COMPLETE ---");
        Debug.Log("Showing Reward Screen (Item & Skill)... (Game is Paused)");
        
       
        yield return new WaitForSeconds(5f); 
        
        Debug.Log("Reward chosen!");

        if (isFinalRound)
        {
            // This was the last round. Win the level.
            state = CombatState.WIN;
            Debug.Log("YOU WIN THE BATTLE!");
            
        }
        else
        {
            // Not the final round. Move to the next one.
            currentRoundIndex++; // Move to the next round index
            Debug.Log($"Continuing to Round {currentRoundIndex + 1}.");
            StartCoroutine(RoundTransition());
        }
    }
    
    /**
     * A simple delay coroutine for between rounds
     */
    IEnumerator RoundTransition()
    {
        Debug.Log("Get ready for the next round...");
        yield return new WaitForSeconds(2f);
        StartRound(currentRoundIndex);
    }
}