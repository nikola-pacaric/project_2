# Project 2 - 2D Platformer
Project 2 is a Unity 6 2D platformer built as a portfolio project and as the next step after my first Unity game, Project 1. The player controls a salamander character through hand-built platforming levels with combat, collectables, checkpoints, puzzle objects, audio, menus, timer tracking, and an online leaderboard.

Compared with Project 1, this project shows a more Unity-native approach to architecture. Project 1 used a separate Node.js / Express / MongoDB backend for leaderboard persistence. Project 2 moves the leaderboard into Unity Gaming Services, using Unity Authentication and Unity Leaderboards directly from the Unity client.

## Related Repositories
- [Project 2 Source Repository](https://github.com/nikola-pacaric/project_2) - this Unity source project.
- [Project 2 Web Build](https://github.com/nikola-pacaric/project_2_Web_Build) - exported Unity WebGL build repository.

## Engine and Packages
The project is built with **Unity 6 (6000.3.7f1)**.

The active scenes are:
- `MainMenu`
- `Arc_1`
- `Arc_2`

## Gameplay Overview
The game is a side-scrolling 2D platformer. The player moves through level arcs, avoids hazards, fights enemies, collects score items, activates checkpoints, solves simple environmental puzzles, and reaches the end state of the current build.

Core gameplay loop:
1. Start from the main menu.
2. Enter the platforming level.
3. Move, jump, climb, and use coyote-time movement.
4. Defeat enemies with stomp and melee slash attacks.
5. Collect diamonds, cherries, and other pickups.
6. Activate bonfire checkpoints.
7. Solve pressure plate, moving platform, and pushable box puzzles.
8. Finish the run and submit the score to the online leaderboard.
9. Enter a player name and compare results from the leaderboard menu.

## Implemented Systems

### Player Movement
`PlayerController.cs` handles the core platforming controls.

Implemented movement features:
- New Unity Input System controls
- Ground and air acceleration
- Coyote time
- Jump buffering behavior through queued jump input
- Ladder climbing
- Pushable box interaction
- Animator parameter updates
- Movement locking for damage and cinematic sequences

### Combat
Combat has been refactored toward a hitbox / hurtbox structure.

Implemented combat scripts include:
- `DamageInfo.cs`
- `Hitbox.cs`
- `Hurtbox.cs`
- `Health.cs`
- `PlayerCombat.cs`
- `SlashAttack.cs`

The player can defeat enemies with:
- Stomp attacks
- Melee slash attacks

The newer combat model reduces dependence on tag-only collision checks and makes future enemy or boss attacks easier to add.

### Health, Damage, and Checkpoints
The project includes a heart-based player health system with segmented hearts, damage feedback, respawning, and checkpoint state.

### Enemies
The project includes multiple enemy behaviors, including:
- Frog enemy AI
- Mushroom enemy AI
- Eagle enemy AI

Enemy behavior includes patrol, jumps, attacks, gas/projectile-style hazards, score rewards, death VFX, and audio hooks.

### Collectables and Score
Collectables and score are tracked through `GameManager.cs` and UI scripts.

Implemented collectables include:
- Diamonds
- Cherries
- Acorns
- Life gems / health pickups

The score updates during the run and is submitted at game over.

### Puzzle Objects
Arc 2 introduces environmental puzzle mechanics:
- `PressurePlate.cs`
- `MovingPlatform.cs`
- `PushableBox.cs`

The player can activate plates directly or push boxes onto them to move platforms and unlock traversal routes.

### Menus and UI
The UI is built with Unity Canvas / uGUI and TextMesh Pro.

### Run Timer
`RunTimer.cs` tracks run duration across scenes using unscaled time. The timer starts when gameplay begins, pauses on game over, and is included as metadata in leaderboard submissions.

### Audio
Audio is handled by an `AudioManager` singleton and a `SoundLibrary` ScriptableObject.

### Camera and Presentation
The project uses Cinemachine and 2D presentation systems:
- Cinemachine camera follow
- Camera zoom helper
- Parallax layers
- Screen fade transitions
- Ending cinematic flow

## Leaderboard System
Project 2 uses **Unity Gaming Services** for online leaderboard support.

The leaderboard flow:
1. Main menu initializes Unity Services.
2. The player is signed in anonymously through Unity Authentication.
3. Starting a run creates a fresh anonymous run identity.
4. Game over submits the score and elapsed time.
5. The player can enter a display name after the score is saved.
6. The leaderboard menu fetches the top scores and displays rank, name, score, and run time.

The configured leaderboard ID is:
```text
main_scores
```

Leaderboard metadata includes:
```json
{
  "timePlayed": 123.45
}
```

## Project 1 vs Project 2 Leaderboard Architecture

| Area | Project 1 | Project 2 |
|---|---|---|
| Game type | 2D survival shooter | 2D platformer |
| Leaderboard storage | MongoDB Atlas | Unity Gaming Services Leaderboards |
| Backend | Custom Node.js / Express API | No custom backend required |
| Unity communication | `UnityWebRequest` to REST endpoints | Unity Services SDK |
| Player identity | Custom run GUID + display name update | Unity Authentication anonymous player identity |
| Deployment pieces | Unity source repo, WebGL repo, backend repo | Unity source repo and WebGL repo |
| Main learning focus | Connecting Unity to an external REST backend | Using a first-party Unity service stack |

Project 2 demonstrates the next step: choosing a platform-native service when it fits the game better. The leaderboard is handled through Unity Authentication and Unity Leaderboards, which reduces deployment complexity and keeps the online score system closer to the Unity project.

## Current Status
Implemented:
- Platformer movement
- Combat and hitbox / hurtbox refactor
- Health and checkpoints
- Arc 1 and Arc 2 gameplay flow
- Collectables and score
- Puzzle mechanics
- Audio manager and SFX hooks
- Main menu, pause menu, settings, game over, and leaderboard UI
- Unity Gaming Services leaderboard integration

In progress / planned:
- Final WebGL deployment polish
- Arc 2 ending cinematic scene wiring
- Additional level arcs and boss content as future updates
- Optional offline leaderboard fallback if needed

## How to Open the Project
1. Clone this repository.
2. Open the folder in Unity Hub.
3. Use Unity **6000.3.7f1**.
4. Open `Assets/Scenes/MainMenu.unity`.
5. Press Play from the Main Menu scene.

For leaderboard testing, the Unity project must be linked to a Unity Cloud project with Authentication and Leaderboards enabled.

Required Unity Cloud setup:
- Authentication enabled
- Anonymous sign-in enabled
- Leaderboards enabled
- Leaderboard created with ID `main_scores`
- Sort order: descending
- Update type: keep best

## What This Project Demonstrates

This project demonstrates:

- Unity 6 2D gameplay scripting
- Platformer movement with coyote time and climbing
- Scene-based game flow
- Tilemap level construction
- Cinemachine camera usage
- Player combat systems
- Hitbox / hurtbox combat architecture
- Enemy AI behaviors
- Collectables, score, timer, and checkpoints
- Puzzle mechanics with pressure plates and moving platforms
- Canvas-based UI with TextMesh Pro
- AudioManager and ScriptableObject-based sound library
- Unity Gaming Services Authentication
- Unity Gaming Services Leaderboards
- WebGL-oriented project structure

The result is a more complete Unity portfolio project that builds on Project 1 while showing a cleaner, more Unity-native approach to online leaderboard implementation.
