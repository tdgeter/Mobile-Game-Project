package com.yourgame.enemy
import com.soywiz.korge.view.*
import com.soywiz.kmmuix.ui.text.Text
import kotlin.random.Random

class EnemyAI(
    val views: Views,
    var combatManager: CombatManager? = null,
    var playerTarget: PlayerController? = null
) {

    lateinit var baseStats: EnemyData

    var currentHealth: Int = 0
    var currentAttack: Int = 0
    var currentDefense: Int = 0
    var currentLuck: Int = 0
    var currentPower: Int = 0  // Enemy basic attack power

    // UI
    var healthText: Text? = null

    var barLength: Int = 10
    var fillChar: Char = '█'
    var emptyChar: Char = '░'
    // -------------------------

    fun setup(data: EnemyData, manager: CombatManager, player: PlayerController) {
        baseStats = data
        combatManager = manager
        playerTarget = player

        currentHealth = baseStats.maxHealth
        currentAttack = baseStats.attack
        currentDefense = baseStats.defense
        currentLuck = baseStats.luck
        currentPower = baseStats.power

        updateUI()
    }

    fun updateUI() {
        val manager = combatManager ?: return
        val player = playerTarget ?: return
        healthText?.text = manager.generateBarString(baseStats.enemyName, currentHealth, baseStats.maxHealth)
    }

    //enemy's basic attack
    fun takeTurn() {
        val player = playerTarget ?: return
        println("${baseStats.enemyName} attacks the player!")

        //enemy uses its basic stats for its turn
        player.takeDamage(currentAttack, currentLuck, currentPower)
    }

    fun takeDamage(attackerAttack: Int, attackerLuck: Int, attackerPower: Int) {
        val manager = combatManager ?: return

        val randMin = manager.randMinMultiplier
        val randMax = manager.randMaxMultiplier
        val atkPotency = manager.attackPotency
        val defDivisor = manager.defenseDivisor.coerceAtLeast(1f)
        val defPotency = manager.defensePotency
        val defPenalty = manager.defensePenaltyMultiplier
        val defScalar = manager.defensePenaltyScalar
        val defToggle = manager.defensePenaltyToggle
        val flatBonus = manager.flatDamageBonus

        val critChancePerLuck = manager.lucktoCrit
        val critMultiplier = manager.critDamageMultiplier

        val ATK = attackerAttack
        val PWR = attackerPower
        val DEF = currentDefense  // This is the enemy's defense

        val randValue = Random.nextFloat() * (randMax - randMin) * PWR + randMin * PWR

        val mainMultiplier = (ATK * atkPotency) / (DEF * defDivisor)
        val penalty = (DEF * defPotency * defPenalty) * defScalar * defToggle

        val baseDamage = kotlin.math.floor(randValue * (mainMultiplier - penalty) + flatBonus).toInt()

        val critChance = attackerLuck * critChancePerLuck
        val critRoll = Random.nextFloat() * 100f

        val finalDamage = if (critRoll < critChance) {
            println("Critical Hit!")
            kotlin.math.floor(baseDamage * critMultiplier).toInt()
        } else {
            baseDamage
        }.coerceAtLeast(1)

        currentHealth -= finalDamage
        println("${baseStats.enemyName} takes $finalDamage damage.")

        updateUI()

        if (currentHealth <= 0) {
            die()
        }
    }

    private fun die() {
        combatManager?.enemyDied()
        //remove the enemy view
    }
    //console command to immediately kill this enemy
    fun forceKill() {
        currentHealth = 0
        die()
    }
}
