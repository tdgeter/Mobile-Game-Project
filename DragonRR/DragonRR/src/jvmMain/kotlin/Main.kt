import korlibs.korge.Korge
import korlibs.korge.scene.sceneContainer
import korlibs.image.color.Colors
import korlibs.math.geom.Size
import samples.scenes.CombatScene

suspend fun main() = Korge(
	virtualSize = Size(1024, 576),
	windowSize = Size(1280, 720),
	backgroundColor = Colors["#2b2b2b"]
) {
	val sc = sceneContainer()
	sc.changeTo { CombatScene() }
}
