package com.yourgame.data
//represents data for a skill, similar to Unity's ScriptableObject but as a plain Kotlin data class.
data class SkillData(
    var skillName: String = "",

    //skill properties
    var power: Int = 10,
    var hitCount: Int = 1,
    var damageMultiplier: Float = 1.0f,

    var apCost: Int = 0
)
