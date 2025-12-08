package com.yourgame.player
import com.soywiz.korge.view.*
import com.soywiz.kmmuix.ui.text.Text
import com.soywiz.korge.scene.Scene
import kotlin.random.Random

/**
 *player controller class for korGE
 */
class PlayerController(
    val views: Views,  //korGE Views context
    var combatManager: CombatManager? = null  //will be set from scene
) {

    //player stats
    var maxHealth: Int = 50
    var currentHealth: Int = 50
    var maxAP: Int = 50
    var currentAP: Int = 50
    var attack: Int = 10
    var defense: Int = 5
    var luck: Int = 5

    // UI References - KorGE Text views instead of TextMeshProUGUI
    var healthText: Text? = null
    var apText: Text? = null

    suspend fun init() {
        currentHealth = maxHealth
        currentAP = maxAP
        updateUI()
    }
    /**
     * A single function to update all our UI elements
     */
    fun updateUI() {
        // Update the Health Bar
        healthText?.text = generateBarString("HP", currentHealth, maxHealth)
        apText?.text = generateBarString("AP", currentAP, maxAP)
    }
    fun generateBarString(label: String, current: Int, max: Int): String {
        val barLength = 10
        val fillChar = '█'
        val emptyChar = '░'

        //safety check to prevent divide-by-zero
        if (max == 0) return "$label: ERROR"

        //clamp current value just in case
        val clampedCurrent = kotlin.math.minOf(current, max).coerceAtLeast(0)

        //calculate the percentage and fill amount
        val percent = clampedCurrent.toFloat() / max
        val fillCount = kotlin.math.roundToInt(percent * barLength)
        val emptyCount = barLength - fillCount

        //build the strings
        val fillString = fillChar.toString().repeat(fillCount)
        val emptyString = emptyChar.toString().repeat(emptyCount)

        //return the final formatted string
        return "$label: [$fillString$emptyString]"
    }

    //this is called by your buttons in KorGE
    fun useSkill(skill: SkillData) {
        val manager = combatManager ?: return
        if (manager.state == CombatState.PLAYERTURN && currentAP >= skill.apCost) {
            manager.onPlayerSkill(skill, attack, luck)
        } else {
            println("Not enough AP to use ${skill.skillName}")
        }
    }

    fun onCycleTarget() {
        combatManager?.let { manager ->
            if (manager.state == CombatState.PLAYERTURN) {
                manager.cycleTarget()
            }
        }
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

        // Formula Breakdown:
        val ATK = attackerAttack
        val PWR = attackerPower
        val DEF = defense

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
        println("Player takes $finalDamage damage!")

        if (currentHealth <= 0) {
            die()
        }
    }
    private fun die() {
        println("Player has been defeated!")
        combatManager?.state = CombatState.LOSE
    }
    fun setActionPanel(show: Boolean) {
        // In KorGE: toggle visibility of your action UI container View
        // actionContainer?.visible = show
    }
    fun setSkillPanel(show: Boolean) {
        // in KorGE: toggle visibility of your skill UI container View
        // skillContainer?.visible = show
    }
}
