# CLAUDE.md — 2D Platformer (Unity 6)

## Project Overview

A 2D platformer built in **Unity 6 (6000.3.7f1)**. The player controls a salamander character through platforming levels with combat, collectables, and checkpoint systems.

## Unity & Engine Rules

- **Unity version: 6000.3.7f1** — use only APIs available in Unity 6. Do not use deprecated Unity 5/2020/2021 patterns.
- **Input System: Always use the new Unity Input System** (`UnityEngine.InputSystem`). Never use the legacy `Input.GetKey()` / `Input.GetAxis()` API.
- **Cinemachine** is already in use — follow its Unity 6 API (e.g., `CinemachineCamera`, not deprecated legacy classes).
- **2D Physics** — use `Rigidbody2D`, `Collider2D`, `Physics2D`. Never use 3D physics components.
- **Tilemaps** — use `UnityEngine.Tilemaps` for level construction.

---

## C# Coding Conventions

Follow **senior Unity C# standards** throughout. When in doubt, match the style of existing scripts in the project.

### Fields & Serialization
```csharp
// ✅ CORRECT — private field exposed to Inspector
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private Rigidbody2D rb;

// ❌ WRONG — public field
public float moveSpeed = 5f;
```

- Use `[SerializeField] private` for all Inspector-exposed fields.
- Use `public` **only** when another object/script genuinely needs to read or write the value at runtime.
- Prefer `public` **properties with private setters** over raw public fields when external read access is needed:
```csharp
public int Health { get; private set; }
```

### Naming
| Element | Convention | Example |
|---|---|---|
| Classes | PascalCase | `EnemyFrogAI` |
| Methods | PascalCase | `TakeDamage()` |
| Private fields | camelCase | `isGrounded` |
| Public properties | PascalCase | `IsAlive` |
| Constants | UPPER_SNAKE | `MAX_HEALTH` |
| SerializeField | camelCase | `[SerializeField] private float jumpForce` |

### General Rules
- Use `[Header("Section Name")]` to keep the Inspector readable.
- Cache component references in `Awake()`, not in `Update()`.
- Prefer **events and delegates** (`System.Action`, `UnityEvent`) over direct cross-script method calls where appropriate — keeps coupling low.
- Use **coroutines** for time-based sequences (cooldowns, flash effects, spawn delays).
- No magic numbers — extract to `[SerializeField] private` or `private const`.
- Null-check with `TryGetComponent<>()` instead of `GetComponent<>()` + null check.

### Script Structure (standard order)
Serialized fields → private fields → public properties → Unity lifecycle (Awake, Start, Update, FixedUpdate) → public methods → private methods → Gizmos.

---

## Architecture Guidelines

### Enemy AI Pattern
All enemy AI should follow the same base pattern (matching the existing `EnemyFrogAI` style):
- State machine with an `enum` for states (e.g., `Idle`, `Patrol`, `Chase`, `Attack`, `Dead`)
- `[SerializeField]` for all tunable values (speed, detection range, attack range, damage)
- Separate methods per state, called from `Update()`
- Reuse the existing enemy death VFX prefab on death
- All enemies must implement a `TakeDamage(int amount)` public method (for stomp + slash compatibility)

### Health System
- Do not rewrite the heart/segment health system — extend it if needed.
- Always go through the existing `TakeDamage()` / `Heal()` interface.

### Scene/Level Structure
- New levels = new Scenes added to Build Settings.
- Each scene should have: Player prefab, Cinemachine camera, Tilemap, background parallax layers, at least one Bonfire checkpoint.

### Audio (when implementing)
- Use an **AudioManager singleton** with separate mixer groups for: Music, SFX, UI.
- All SFX triggered via `AudioManager.Instance.PlaySFX(clipName)` — no direct `AudioSource.PlayClipAtPoint` in gameplay scripts.

### UI / Menus
- Use **Canvas-based UI** (uGUI) for all menus and HUD.
- Menu scenes are separate from gameplay scenes.
- Pause menu should use `Time.timeScale = 0` + disable input actions selectively.

---

## What Claude Should NOT Do

- Do not refactor working systems (movement, health, checkpoints) unless explicitly asked.
- Do not add placeholder audio with `AudioSource.PlayClipAtPoint` — use the AudioManager pattern.
- Do not create new scripts in the root `Assets/` folder — always place in the correct `Scripts/` subfolder.

---

## Debug / Test Scripts

`HealthTester.cs` and `JumpTest.cs` are development-only. They should remain isolated and not be referenced by gameplay scripts. Remove before shipping.
