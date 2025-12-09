package com.yourgame.combat
import korlibs.korge.input.mouse
import korlibs.korge.input.keys
import korlibs.korge.scene.Scene
import korlibs.korge.view.*
import korlibs.korge.view.Text
import korlibs.korge.view.Image
import korlibs.image.color.Colors
import korlibs.io.file.std.resourcesVfs
import korlibs.image.format.readBitmap
import korlibs.math.geom.Point
import com.yourgame.data.SkillData
import com.yourgame.enemy.EnemyAI
import com.yourgame.player.PlayerController
import com.yourgame.data.EnemyData
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import kotlin.random.Random

enum class CombatState { START, PLAYERTURN, ENEMYTURN, WIN, LOSE }

data class EnemySpawn(
    val enemyViewClass: String,  // Reference by class name or use factory functions
    val enemyData: EnemyData
)

data class Round(
    val roundName: String,
    val enemiesInThisRound: List<EnemySpawn>
)

class CombatManager(private val container: SContainer) {

    var state: CombatState = CombatState.START

    lateinit var playerSpawnPoint: Point
    val enemySpawnPoints = mutableListOf<Point>()

    private var targetIndicator: View? = null
    private var currentTargetIndex = 0

    private var animatorContainer: Container? = null

    var attackPotency: Float = 1f
    var randMinMultiplier: Float = 0.8f
    var randMaxMultiplier: Float = 1.2f
    var defenseDivisor: Float = 1f // (Set to 1 to avoid divide-by-zero)
    var defensePotency: Float = 1f
    var defensePenaltyMultiplier: Float = 1f
    var defensePenaltyScalar: Float = 1f
    var defensePenaltyToggle: Float = 0f
    var flatDamageBonus: Float = 0f

    var lucktoCrit: Float = 0.5f // Chance to crit per LCK point
    var critDamageMultiplier: Float = 1.5f // Crit damage multiplier

    var allRounds: List<Round> = emptyList()
    private var currentRoundIndex = 0

    var apRegenPerTurn: Int = 15 // AP to give player each turn

    // UI & bar Settings
    var enemyHealthText: Text? = null
    var playerHealthText: Text? = null
    var playerApText: Text? = null
    var barLength: Int = 10
    var fillChar: Char = '█'
    var emptyChar: Char = '░'

    // runtime references
    val activeEnemies = mutableListOf<EnemyAI>()
    lateinit var player: PlayerController

    suspend fun setupBattle() {
        state = CombatState.START

        //spawn player at position
        player = createPlayer() // Your player factory function
        player.combatManager = this
        player.setActionPanel(false)
        player.setSkillPanel(false)

        enemyHealthText?.text = ""

        //create target indicator
        targetIndicator = container.image(resourcesVfs["common/assets/target.png"].readBitmap())
        (targetIndicator as? Image)?.smoothing = false
        targetIndicator?.let { it.visible = false }

        delay(100) //unity's yield return null equivalent

        //begin the first round
        startRound(currentRoundIndex)
    }

    /**
     * spawns all enemies for next round.
     */
    fun startRound(roundIndex: Int) {
        activeEnemies.clear()
        val round = allRounds[roundIndex]
        round.enemiesInThisRound.forEachIndexed { idx, spawnInfo ->
            if (idx < enemySpawnPoints.size) {
                val enemyAI = createEnemy(spawnInfo.enemyViewClass, enemySpawnPoints[idx])
                enemyAI.setup(spawnInfo.enemyData, this, player)
                activeEnemies.add(enemyAI)
            }
        }

        println("Starting ${round.roundName}")

        // The round is set up, begin the player's turn
        state = CombatState.PLAYERTURN
        playerTurnStart()
    }

    /**
     * call player's turn.
     * regenerates AP
     */
    fun playerTurnStart() {
        println("Player's turn!")

        // Regenerate AP
        player.currentAP += apRegenPerTurn
        if (player.currentAP > player.maxAP) {
            player.currentAP = player.maxAP
        }
        player.updateUI()
        playerHealthText?.text = player.healthText?.text ?: ""
        playerApText?.text = player.apText?.text ?: ""
        println("Player AP regenerated to ${player.currentAP}")

        currentTargetIndex = 0
        targetIndicator?.let { it.visible = true }
        updateTargetIndicator()

        updateEnemyUI()

        player.setActionPanel(true)
        player.setSkillPanel(false)
    }

    /**
     * playerController.kt called.
     * attack sequence.
     */
    fun onPlayerSkill(skill: SkillData, attackerAttack: Int, attackerLuck: Int) {
        if (state != CombatState.PLAYERTURN) return // Safety check

        when (skill.skillName) {
            "Heal" -> {
                player.currentHealth += player.maxHealth / 4 // heal for 25% of max
                println("Player Heals to: ${player.currentHealth}")
                player.currentAP -= skill.apCost
            }
            else -> {
                if (activeEnemies.isNotEmpty() && activeEnemies[currentTargetIndex] != null) {
                    player.currentAP -= skill.apCost
                    player.updateUI()
                    println("Player used ${skill.skillName}, AP remaining: ${player.currentAP}")

                    val targetEnemy = activeEnemies[currentTargetIndex]!!
                    playerAttackSequence(skill, attackerAttack, attackerLuck, targetEnemy)
                } else {
                    println("No enemies to attack!")
                    return
                }
            }
        }

        //end turn and start enemy turn
        endPlayerTurn()
        enemyTurn()
    }

    private fun updateTargetIndicator() {
        // Skipped: requires enemy views with positions
    }

    fun cycleTarget() {
        if (activeEnemies.isNotEmpty()) {
            currentTargetIndex = (currentTargetIndex + 1) % activeEnemies.size
            updateTargetIndicator()
        }
    }

    private fun updateEnemyUI() {
        val target = activeEnemies.getOrNull(currentTargetIndex)
        target?.let { enemy ->
            enemyHealthText?.let { it.text = generateBarString("Enemy HP", enemy.currentHealth, enemy.baseStats.maxHealth) }
        }
    }

    fun generateBarString(label: String, current: Int, max: Int): String {
        if (max == 0) return "$label: ERROR"
        val clampedCurrent = kotlin.math.min(current, max).coerceAtLeast(0)
        val percent = clampedCurrent.toFloat() / max
        val fillCount = kotlin.math.round(percent * barLength).toInt()
        val fillString = fillChar.toString().repeat(fillCount)
        val emptyString = emptyChar.toString().repeat(barLength - fillCount)
        return "$label: [$fillString$emptyString]"
    }

    private fun playerAttackSequence(skill: SkillData, attack: Int, luck: Int, target: EnemyAI) {
        //animate attack here (tweens, effects)
        target.takeDamage(attack, luck, target.baseStats.power)
    }

    private fun enemyTurn() {
        state = CombatState.ENEMYTURN
        activeEnemies.forEach { enemy -> enemy.takeTurn() }
        state = CombatState.PLAYERTURN
        playerTurnStart()
    }

    private fun endPlayerTurn() {
        player.setActionPanel(false)
        player.setSkillPanel(false)
        targetIndicator?.let { it.visible = false }

    }

    fun endTurn() {
        endPlayerTurn()
        enemyTurn()
    }

    private fun createPlayer(): PlayerController {
        return PlayerController()
    }

    private fun createEnemy(enemyViewClass: String, spawnPoint: Point): EnemyAI {
        val ai = EnemyAI()
        // You can attach a view here if required; for now just return AI
        return ai
    }

    fun killAllEnemiesInCurrentRound() {
        activeEnemies.forEach { it.forceKill() }
        activeEnemies.clear()
    }
}
