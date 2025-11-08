using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;


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

    [Header("Targeting")]
    public GameObject targetIndicatorPrefab;
    public Vector3 targetOffset = new Vector3(0, 2.0f, 0);

    private GameObject targetIndicatorInstance;
    private int currentTargetIndex = 0;


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

    [Header("UI & Bar Settings")]
    public TextMeshProUGUI enemyHealthText; 
    public int barLength = 10;
    public char fillChar = '█';
    public char emptyChar = '░';

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

    void Update()
    {
        if (state == CombatState.PLAYERTURN)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleClick();
            }
        }

        // press F9 to kill all enemies in the current round
        if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
        {
            KillAllEnemiesInCurrentRound();
        }
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

        if (enemyHealthText != null)
            enemyHealthText.text = "";

        targetIndicatorInstance = Instantiate(targetIndicatorPrefab);
        targetIndicatorInstance.SetActive(false);

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
        player.UpdateUI();
        Debug.Log("Player AP regenerated to " + player.currentAP);

        currentTargetIndex = 0;
        targetIndicatorInstance.SetActive(true);
        UpdateTargetIndicator();

        UpdateEnemyUI();

        player.SetActionPanel(true);
    }

    /**
     * This is called by the PlayerController when a skill button is pressed.
     * It spends the AP and starts the attack sequence.
     */
    public void OnPlayerSkill(SkillData skill, int attackerAttack, int attackerLuck)
    {
        if (state != CombatState.PLAYERTURN) return; // Safety check

        if (activeEnemies.Count > 0 && activeEnemies[currentTargetIndex] != null)
        {
            player.currentAP -= skill.apCost;
            player.UpdateUI();
            Debug.Log("Player used " + skill.skillName + ", AP remaining: " + player.currentAP);

            EndPlayerTurn();

            EnemyAI targetEnemy = activeEnemies[currentTargetIndex];
            StartCoroutine(PlayerAttackSequence(skill, attackerAttack, attackerLuck, targetEnemy));

        }
        else
        {
            Debug.Log("No enemies to attack!");
            return;
        }
    }

    void HandleClick()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            EnemyAI clickedEnemy = hit.collider.GetComponent<EnemyAI>();
            if (clickedEnemy != null)
            {
                SetTarget(clickedEnemy);
            }
        }
    }

    public void SetTarget(EnemyAI newTarget)
    {
        if (state != CombatState.PLAYERTURN) return; //These technically aren't needed but they stop bugs from happening from it not being the right turn

        int newIndex = activeEnemies.IndexOf(newTarget);
        if (newIndex != -1)
        {
            currentTargetIndex = newIndex;
            UpdateTargetIndicator();
            UpdateEnemyUI();
            Debug.Log("Target changed to " + newTarget.baseStats.enemyName);
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

        if (enemyHealthText != null)
            enemyHealthText.text = "";

        targetIndicatorInstance.SetActive(false);
        player.SetActionPanel(false);
    }

    public string GenerateBarString(string label, int current, int max)
    {
        if (max == 0) return $"{label}: ERROR";
        current = Mathf.Clamp(current, 0, max);

        float percent = (float)current / max;
        int fillCount = Mathf.RoundToInt(percent * barLength);
        int emptyCount = barLength - fillCount;

        string fillString = new string(fillChar, fillCount);
        string emptyString = new string(emptyChar, emptyCount);

        return label + ": [" + fillString + emptyString + "]";

    }
    
    void UpdateEnemyUI()
    {
        if (enemyHealthText == null) return; // No UI to update

        // Check if we have a valid target
        if (activeEnemies.Count > 0 && currentTargetIndex < activeEnemies.Count && activeEnemies[currentTargetIndex] != null)
        {
            EnemyAI target = activeEnemies[currentTargetIndex];
            enemyHealthText.text = GenerateBarString(target.baseStats.enemyName, target.currentHealth, target.baseStats.maxHealth);
        }
        else
        {
            // No target, clear the text
            enemyHealthText.text = "";
        }
    }


    public void CycleTarget()
    {
        if (state != CombatState.PLAYERTURN) return; // Only allow during player's turn

        currentTargetIndex++;

        if (currentTargetIndex >= activeEnemies.Count)
        {
            currentTargetIndex = 0; // Wrap around to first enemy
        }

        UpdateTargetIndicator();
        UpdateEnemyUI();
    }

    // This is inside CombatManager.cs

private void UpdateTargetIndicator()
{
    // Check if we have any valid targets
    if (activeEnemies.Count == 0 || currentTargetIndex >= activeEnemies.Count || activeEnemies[currentTargetIndex] == null)
    {
        targetIndicatorInstance.SetActive(false);
        return;
    }
    
    // 1. Get the current target
    EnemyAI target = activeEnemies[currentTargetIndex];
    
    Collider2D targetCollider = target.GetComponent<Collider2D>();

    if (targetCollider == null)
    {
        // Failsafe: if no collider, just put it at their center (the old, buggy way)
        Debug.LogWarning("Target has no Collider2D, indicator position may be wrong.");
        targetIndicatorInstance.transform.position = target.transform.position + targetOffset;
        return;
    }

    float topOfEnemy = targetCollider.bounds.max.y; // The highest Y-point of the collider
    float centerOfEnemy = targetCollider.bounds.center.x; // The center X-point of the collider
    
    // 4. Set the indicator's position
    // Place it at the top-center, plus our visual offset
    targetIndicatorInstance.transform.position = new Vector3(
        centerOfEnemy + targetOffset.x, 
        topOfEnemy + targetOffset.y,
        0
    );
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

    // === Developer/Console utilities ===
    // Immediately kill all enemies in the current round. Triggers normal round-complete flow.
    public void KillAllEnemiesInCurrentRound()
    {
        if (activeEnemies == null || activeEnemies.Count == 0) return;

        // Work on a copy as the list will be mutated by EnemyDied during Die()
        List<EnemyAI> toKill = new List<EnemyAI>(activeEnemies);
        foreach (var ai in toKill)
        {
            if (ai != null)
            {
                ai.ForceKill();
            }
        }

        Debug.Log("[Dev] KillAllEnemiesInCurrentRound executed.");
    }
}