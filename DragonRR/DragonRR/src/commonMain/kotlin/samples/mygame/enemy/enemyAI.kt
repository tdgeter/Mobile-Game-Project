package com.yourgame.enemy
import korlibs.korge.view.*
import korlibs.korge.view.Text
import com.yourgame.combat.CombatManager
import com.yourgame.player.PlayerController
import com.yourgame.data.EnemyData
import kotlin.math.floor
import kotlin.random.Random

class EnemyAI(
    var combatManager: CombatManager? = null,
    var playerTarget: PlayerController? = null
) {
    var view: View? = null

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

    fun attachView(enemyView: View) {
        view = enemyView
        enemyView.name = baseStats.enemyName
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

        val baseDamage = floor((randValue * (mainMultiplier - penalty) + flatBonus).toDouble()).toInt()

        val critChance = attackerLuck * critChancePerLuck
        val critRoll = Random.nextFloat() * 100f

        val finalDamage = if (critRoll < critChance) {
            println("Critical Hit!")
            floor((baseDamage.toDouble() * critMultiplier.toDouble())).toInt()
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
        // TODO: notify manager if needed
        //remove the enemy view
    }
    //console command to immediately kill this enemy
    fun forceKill() {
        currentHealth = 0
        die()
    }
}
