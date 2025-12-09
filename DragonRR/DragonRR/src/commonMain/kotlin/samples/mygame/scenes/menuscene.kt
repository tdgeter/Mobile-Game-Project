package samples.scenes

import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.sprite
import korlibs.korge.view.text
import korlibs.korge.view.position
import korlibs.io.file.std.resourcesVfs
import korlibs.image.format.readBitmap
import com.yourgame.enemy.EnemyAI
import com.yourgame.combat.CombatManager
import com.yourgame.player.PlayerController
import com.yourgame.data.EnemyData
import com.yourgame.data.EnemyRepository

class MenuScene : Scene() {
    override suspend fun SContainer.sceneMain() {
        val enemyRepo = EnemyRepository(resourcesVfs)
        enemyRepo.loadAll()

        val goblinData = enemyRepo.getEnemy("goblin")!!
        val enemyView = sprite(resourcesVfs["common/assets/goblin.png"].readBitmap()) { /* position etc */ }
        val combatManager = com.yourgame.combat.CombatManager(this)
        val playerController = com.yourgame.player.PlayerController()
        val enemyAI = EnemyAI()
        enemyAI.setup(goblinData, combatManager, playerController)
        enemyAI.healthText = text("") { position(200, 10) }

        // Optionally store references on your own structures
    }
}
