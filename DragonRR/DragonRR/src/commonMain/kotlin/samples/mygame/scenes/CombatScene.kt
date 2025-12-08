package samples.scenes

class CombatScene : Scene() {
    override suspend fun SContainer.sceneMain() {
        val enemyRepo = EnemyRepository(resourcesVfs)
        enemyRepo.loadAll()

        val goblinData = enemyRepo.getEnemy("goblin")!!
        val enemyView = sprite(goblinSprite) { /* position etc */ }
        val enemyAI = EnemyAI(views)
        enemyAI.setup(goblinData, combatManager, playerController)
        enemyAI.healthText = text("") { position(200, 10) }

        //store reference for removal when enemy dies
        enemyView.userData = enemyAI
    }
}
