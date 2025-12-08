package com.yourgame.data
import kotlinx.serialization.Serializable
@Serializable
data class EnemyData(
    val enemyName: String = "",
    val maxHealth: Int = 0,
    val attack: Int = 0,  // (ATK)
    val defense: Int = 0, // (DEF)
    val luck: Int = 0,    // (LCK)
    val power: Int = 0    // (PWR)
)
