package com.yourgame.ui
import com.soywiz.korge.scene.Scene
import com.soywiz.korge.view.*
import com.soywiz.korge.view.ui.button
import com.soywiz.kmmuix.ui.text.Text

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
