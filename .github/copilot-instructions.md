# Copilot Instructions — 2D Platformer (Unity 6)

## Project

Unity 6 (6000.3.7f1) 2D platformer with a salamander character. Player navigates platforming levels with combat, collectables, and checkpoints.

## Unity Requirements

- **Unity 6000.3.7f1** — use only Unity 6 APIs
- **New Input System only** — never use `Input.GetKey()` / `Input.GetAxis()`
  - Input defined in `Assets/InputSystem_Actions.inputactions`
  - Generated class: `Assets/Input Actions/PlayerControls.cs`
- **Cinemachine** — use Unity 6 API (`CinemachineCamera`, not deprecated legacy)
- **2D Physics** — `Rigidbody2D`, `Collider2D`, `Physics2D` only
- **Tilemaps** — `UnityEngine.Tilemaps`
- **URP** — settings in `Assets/Settings/`

## Code Style

### Serialization
```csharp
// ✅ CORRECT
[SerializeField] private float moveSpeed = 5f;

// ❌ WRONG
public float moveSpeed = 5f;
```

Use `[SerializeField] private` for Inspector fields. Public properties with private setters for external access:
```csharp
public int Health { get; private set; }
```

### Naming
- Classes/Methods: `PascalCase` 
- Private fields: `camelCase`
- Constants: `UPPER_SNAKE`

### Script Structure
1. Serialized fields (use `[Header("...")]`)
2. Private fields
3. Public properties
4. Unity lifecycle (`Awake`, `Start`, `Update`, etc.)
5. Public methods
6. Private methods
7. Gizmos

See `Assets/Scripts/Puzzles/PressurePlate.cs` for reference.

### General
- Cache components in `Awake()`
- Use coroutines for time-based sequences
- No magic numbers — serialize or const
- `TryGetComponent<>()` for null checking

## Architecture

### Enemy AI
Follow `EnemyFrogAI.cs` pattern:
- Enum state machine
- `[SerializeField]` for all tunable values
- Separate method per state
- All enemies implement `TakeDamage(int amount)`
- Reuse `enemy_death1_0.prefab` on death

### Health System
- 3 hearts × 4 segments = 12 health
- Don't rewrite — extend if needed
- Use existing `TakeDamage()` / `Heal()` interface
- `PlayerHealth.cs` + `HeartsUI.cs` for player
- `EnemyHealth.cs` for enemies

### Combat
- Melee: Fire slash (0.5s cooldown) via `PlayerCombat.cs`
- Stomp: Jump on enemies via `PlayerController.cs`
- All attacks call `TakeDamage(int amount)`

### Input System
Subscribe in `Awake()`, enable/disable in `OnEnable()`/`OnDisable()`:
```csharp
private void Awake()
{
    controls = new PlayerControls();
    controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
}
private void OnEnable() => controls.Player.Enable();
private void OnDisable() => controls.Player.Disable();
```

### Scenes
Each scene needs:
- Player, Cinemachine camera, Tilemap (Grid), Background parallax, Bonfire checkpoint, Canvas, EventSystem
- Add to Build Settings

### Puzzles
Scripts in `Assets/Scripts/Puzzles/`:
- `PressurePlate.cs` — activates on player/box contact
- `PushableBox.cs` — pushable, activates plates
- `MovingPlatform.cs` — moves between points

Use `UnityEvent` fields to wire up in Inspector.

## File Organization

```
Assets/Scripts/
├── Player (PlayerController.cs, PlayerHealth.cs, PlayerCombat.cs)
├── Enemies (EnemyFrogAI.cs, EnemyHealth.cs)
├── Environment (DeathZone.cs, Checkpoint.cs)
├── UI (HeartsUI.cs)
├── Debug (HealthTester.cs, JumpTest.cs) — dev only
└── Puzzles/
```

**Never create scripts in root `Assets/` folder** — use `Assets/Scripts/` or subfolders.

## Avoid

- Refactoring working systems unless asked
- Legacy Input API
- Deprecated Cinemachine classes
- Public fields (use `[SerializeField] private`)
- `AudioSource.PlayClipAtPoint` (wait for AudioManager)

## Key Scripts

| System | Script | Location |
|--------|--------|----------|
| Player | `PlayerController.cs`, `PlayerHealth.cs`, `PlayerCombat.cs` | `Assets/Scripts/` |
| Enemy (Frog) | `EnemyFrogAI.cs`, `EnemyHealth.cs` | `Assets/Scripts/` |
| Checkpoints | `Checkpoint.cs` | `Assets/Scripts/` |
| UI | `HeartsUI.cs` | `Assets/Scripts/` |
| Puzzles | `PressurePlate.cs`, `PushableBox.cs`, `MovingPlatform.cs` | `Assets/Scripts/Puzzles/` |

See `PLANNING.md` for roadmap and `CLAUDE.md` for extended documentation.
