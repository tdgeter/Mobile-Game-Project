package com.yourgame.data
import korlibs.io.file.VfsFile
import korlibs.io.file.std.resourcesVfs

class EnemyRepository(private val root: VfsFile = resourcesVfs) {
    private val enemies = mutableMapOf<String, EnemyData>()

    suspend fun loadAll() {
        enemies["goblin"] = EnemyData(
            enemyName = "Goblin",
            maxHealth = 30,
            attack = 8,
            defense = 3,
            luck = 2,
            power = 1,
            spritePath = "enemies/goblin.png"
        )
        enemies["slime"] = EnemyData(
            enemyName = "Slime",
            maxHealth = 20,
            attack = 5,
            defense = 1,
            luck = 1,
            power = 1,
            spritePath = "enemies/slime.png"
        )
        enemies["ooze"] = EnemyData(
            enemyName = "Ooze",
            maxHealth = 35,
            attack = 7,
            defense = 2,
            luck = 1,
            power = 1,
            spritePath = "enemies/ooze.png"
        )
    }

    fun getEnemy(key: String): EnemyData? = enemies[key]
}
