# Korge Dragon Raid Demo

You can find the KorGE documentation here: [https://docs.korge.org/korge/](https://docs.korge.org/korge/)



\# Objectives

* \*\*Recreate the core turn-based combat and player/enemy interactions from the original Unity version using Kotlin and KorGE.\*\*
* \*\*Maintain the original gameplay feel (stats, skills, combat flow) while simplifying the architecture into clean Kotlin modules.\*\*
* \*\*Target multiple platforms (desktop, web, mobile) through KorGE’s multiplatform tooling with a single shared codebase.​\*\*



\# Features

* \*\*Turn-based combat system with player actions, skills, AP management, and enemy AI using shared damage formulas.
* \*\*Data-driven stats for players, enemies, and skills using Kotlin data classes and resource files (YAML/JSON) instead of Unity ScriptableObjects.​
* \*\*Simple text-based HP/AP bars and UI panels implemented with KorGE views and scenes, replacing Unity canvases and TextMesh Pro.​



\# Technical Stack

* \*\*Language: Kotlin (multiplatform, coroutines, data classes).\*\*
* \*\*Engine: KorGE – modern 2D game engine for Kotlin with hot reload and multi-target Gradle plugin.​\*\*
* \*\*Project Layout: Multi-module setup with a samples module containing the game, and shared resources for assets and configuration.\*\*
* \*\*Resources: KorGE virtual file system for loading sprites, button assets (ASE/PNG), level configs, and balance data from resources/ommon/....​\*\*
* \*\*Build Tools: Gradle with the KorGE plugin for building and running on JVM (desktop), JS (web), and other supported targets.​\*\*



\# Getting Started

\*\* Prerequisites\*\*



* \*\*Install IntelliJ IDEA (Community or Ultimate) with the Kotlin plugin.\*\*
* \*\*Install the KorGE plugin or clone a KorGE starter project as a base.​\*\*



\*\*Clone the repository\*\*



bash

git clone https://github.com/your-user/your-korge-game.git

cd your-korge-game

Run the game (desktop/JVM)



In IntelliJ, open the project and wait for Gradle sync.



Locate the main game module (for example, :samples:mygame) and run the runJvm Gradle task, or run the Main.kt entry point configured for KorGE.​



\# Project structure 



samples/mygame/

&nbsp; src/commonMain/kotlin/

&nbsp;   /scenes/      // KorGE Scene classes (CombatScene, MenuScene, etc.)

&nbsp;   /player/      // PlayerController Kotlin port

&nbsp;   /enemy/       // EnemyAI, EnemyData, repositories

&nbsp;   /combat/      // CombatManager, damage formulas

&nbsp; /resources/

&nbsp;   common/

&nbsp;     assets/                 // buttons, UI sprites

&nbsp;     world\_1/

&nbsp;       assets/              // enemy and skill data files

&nbsp;       level\_1/             // level-specific configuration
