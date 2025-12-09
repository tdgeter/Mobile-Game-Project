package com.yourgame.ui
import korlibs.korge.scene.Scene
import korlibs.korge.view.*
import korlibs.korge.ui.*

class MenuManager(private val container: SContainer) {

    private var mainMenuPanel: Container? = null
    private var zoneSelectPanel: Container? = null

    suspend fun init() {
        mainMenuPanel?.visible = true
        zoneSelectPanel?.visible = false
    }

    fun showZoneSelect() {

        mainMenuPanel?.visible = false

        zoneSelectPanel?.visible = true
    }

    fun showMainMenu() {
        mainMenuPanel?.visible = true
        zoneSelectPanel?.visible = false
    }

    fun loadZone(zoneName: String) {
        println("Loading zone: $zoneName")
        //korGE scene container to change scenes:
        //sceneContainer.changeTo<Zone1Scene>()
        //use a scene registry/router pattern
    }

    fun quitGame() {
        println("QUITTING GAME...")

    }
}
