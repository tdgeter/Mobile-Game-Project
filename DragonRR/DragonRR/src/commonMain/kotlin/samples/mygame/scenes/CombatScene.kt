package samples.scenes

import korlibs.korge.scene.Scene
import korlibs.korge.view.SContainer
import korlibs.korge.view.sprite
import korlibs.korge.view.text
import korlibs.korge.view.position
import korlibs.korge.view.solidRect
import korlibs.korge.view.scale
import korlibs.korge.input.onClick
import korlibs.io.file.VfsFile
import korlibs.image.format.readBitmap
import korlibs.io.file.std.resourcesVfs
import korlibs.io.file.std.localVfs
import com.yourgame.enemy.EnemyAI
import com.yourgame.combat.CombatManager
import com.yourgame.combat.Round
import com.yourgame.combat.EnemySpawn
import com.yourgame.player.PlayerController
import com.yourgame.data.EnemyData
import com.yourgame.data.EnemyRepository
import com.yourgame.data.SkillData

class CombatScene : Scene() {
    override suspend fun SContainer.sceneMain() {
        val bgView = try {
            val bgBmp = localVfs("D:/Mobile-Game-Project/C# Unity Build/Dragon_Raid/Assets/Sprites/cave(white).png").readBitmap()
            sprite(bgBmp) { position(0, 0); scale(1024.0 / bgBmp.width, 576.0 / bgBmp.height) }
        } catch (_: Throwable) {
            try {
                val bgBmp = resourcesVfs["backgrounds/slime_cave.png"].readBitmap()
                sprite(bgBmp) { position(0, 0); }
            } catch (_: Throwable) {
                solidRect(1024, 576, color = korlibs.image.color.Colors["#2b2b2b"]).apply { position(0, 0) }
            }
        }
        val enemyRepo = EnemyRepository(resourcesVfs)
        enemyRepo.loadAll()

        val goblinData = enemyRepo.getEnemy("goblin")!!
        suspend fun trySprite(path: String, x: Double, y: Double, scaleFactor: Double = 1.0): korlibs.korge.view.View {
            return try {
                val bmp = resourcesVfs[path].readBitmap()
                sprite(bmp) { position(x, y); scale(scaleFactor) }
            } catch (e: Throwable) {
                solidRect(120, 160, color = korlibs.image.color.Colors.DARKGRAY).apply { position(x, y) }
            }
        }
        val playerView = trySprite("player/hero.png", 120.0, 360.0, 1.0)
        val enemySpritePath = goblinData.spritePath.ifEmpty { "enemies/goblin.png" }
        val enemyView = trySprite(enemySpritePath, 820.0, 260.0, 1.0)
        val combatManager = com.yourgame.combat.CombatManager(this)
        val playerController = com.yourgame.player.PlayerController(combatManager)
        val playerHP = text("") { position(playerView.x + 0.0, playerView.y - 40.0) }
        val playerAP = text("") { position(playerView.x + 0.0, playerView.y - 15.0) }
        playerController.healthText = playerHP
        playerController.apText = playerAP
        playerController.init()
        combatManager.playerHealthText = playerHP
        combatManager.playerApText = playerAP
        val enemyAI = EnemyAI()
        enemyAI.setup(goblinData, combatManager, playerController)
        enemyAI.attachView(enemyView)
        val enemyHP = text("") { position(enemyView.x, enemyView.y - 40.0) }
        enemyAI.healthText = enemyHP
        combatManager.enemyHealthText = enemyHP

        val actionPanelBg = solidRect(240, 60, color = korlibs.image.color.Colors["#1f1f1f"]).apply { position(24, 492) }
        val attackLabel = text("Attack") { position(32, 504) }
        attackLabel.onClick {
            enemyAI.takeDamage(playerController.attack, playerController.luck, playerController.attack)
        }
        val cycleLabel = text("Cycle Target") { position(120, 504) }
        cycleLabel.onClick {
            combatManager.cycleTarget()
        }
        text("Player") { position(playerView.x, playerView.y - 60.0) }
        text("Enemy") { position(enemyView.x, enemyView.y - 60.0) }

        combatManager.playerSpawnPoint = korlibs.math.geom.Point(playerView.x, playerView.y)
        combatManager.enemySpawnPoints.clear()
        combatManager.enemySpawnPoints.add(korlibs.math.geom.Point(enemyView.x, enemyView.y))
        combatManager.enemySpawnPoints.add(korlibs.math.geom.Point(enemyView.x - 140.0, enemyView.y))
        combatManager.enemySpawnPoints.add(korlibs.math.geom.Point(enemyView.x - 280.0, enemyView.y))
        combatManager.allRounds = listOf(
            Round(
                roundName = "Round 1",
                enemiesInThisRound = listOf(
                    EnemySpawn(enemyViewClass = "Goblin", enemyData = goblinData)
                )
            ),
            Round(
                roundName = "Round 2",
                enemiesInThisRound = listOf(
                    EnemySpawn(enemyViewClass = "Slime", enemyData = enemyRepo.getEnemy("slime")!!),
                    EnemySpawn(enemyViewClass = "Slime", enemyData = enemyRepo.getEnemy("slime")!!)
                )
            ),
            Round(
                roundName = "Round 3",
                enemiesInThisRound = listOf(
                    EnemySpawn(enemyViewClass = "Ooze", enemyData = enemyRepo.getEnemy("ooze")!!)
                )
            )
        )

        val skillPanelBg = solidRect(420, 60, color = korlibs.image.color.Colors["#1f1f1f"]).apply { position(280, 492) }
        val btnBasic = text("Basic Attack") { position(292, 504) }
        btnBasic.onClick {
            val skill = SkillData(skillName = "Basic Attack", power = playerController.attack, apCost = 5)
            combatManager.onPlayerSkill(skill, playerController.attack, playerController.luck)
        }
        val btnHeal = text("Heal") { position(420, 504) }
        btnHeal.onClick {
            val skill = SkillData(skillName = "Heal", power = 0, apCost = 10)
            combatManager.onPlayerSkill(skill, playerController.attack, playerController.luck)
        }
        val btnEnd = text("End Turn") { position(500, 504) }
        btnEnd.onClick {
            combatManager.endTurn()
        }
    }
}
