# Plan: Scoring System + Cherry Collectable

## Context

The game has no scoring system yet. The player needs a persistent score that tracks across arcs (scenes), resets only on Game Over (0 HP), and gets stored for a leaderboard. Cherries are the common collectable (like coins in Mario). Score sources: cherries, enemy kills, diamonds, and future enemy drops.

---

## Design Decisions (Agreed)

- **Score lives in GameManager** — extends the existing singleton, since score needs to persist across arcs/scenes
- **Score sources:** Cherries (common, 10pts), enemy kills (varies by enemy), diamonds (rare, 200pts), enemy drops (future)
- **Score UI:** cherry sprite icon + TextMeshPro number on HUD (top-right area)
- **Score persists until Game Over** — dying/respawning at checkpoint does NOT reset score. Only 0 HP Game Over resets it.
- **Leaderboard:** final score saved to persistent storage (PlayerPrefs) on Game Over. Top 10 high scores displayed.
- **Cherry behavior:** idle bobbing animation (sine-wave), collect animation (like diamond), sound effects later
- **HUD is fully event-driven** — both HeartsUI and ScoreUI subscribe to events from their data source instead of polling in `Update()`. Establishes a consistent architecture and avoids per-frame cost.

---

## Implementation Phases

Work proceeds in four phases. Each phase is independently testable before moving on.

### Phase A0 — Migrate HeartsUI to event-driven (prerequisite)

Before building ScoreUI event-driven, migrate the existing HeartsUI off its `Update()` polling so the HUD architecture is consistent from day one. This also validates the event pattern on known-working code.

**File: `Assets/Scripts/PlayerHealth.cs`**
- Add `public static event System.Action OnHealthChanged;` (parameterless — subscribers read state from `PlayerHealth` directly, matching the way HeartsUI already reads `currentSegment` / `maxHearts`)
- Fire `OnHealthChanged?.Invoke()` at every health-state mutation: after any damage path (`TakeEnemyDamage`, `TakeSpikeDamage`, `RespawnAfterFall`, `RespawnAfterEnvironmentDamage`), after `Heal` / `GainHeart`, after segment changes, after max-heart changes (diamond pickup), after checkpoint state restore on scene load
- Also fire once from `Start()` so freshly-subscribed UI gets the initial state without waiting for a change

**File: `Assets/Scripts/HeartsUI.cs`**
- Remove the `Update()` polling entirely
- Subscribe in `OnEnable`: `PlayerHealth.OnHealthChanged += UpdateHearts;`
- Unsubscribe in `OnDisable`: `PlayerHealth.OnHealthChanged -= UpdateHearts;`
- Keep `UpdateHearts()` logic as-is — we're only changing *when* it runs
- Call `UpdateHearts()` from `Start()` as a safety net if subscription timing misses the initial fire

**Checklist:**
- [x] `OnHealthChanged` event declared on `PlayerHealth`
- [x] Event fired at every health-state mutation site (damage, heal, GainHeart, segment change, checkpoint restore)
- [x] Event fired once from `PlayerHealth.Start()`
- [x] `HeartsUI.Update()` removed
- [x] `HeartsUI.OnEnable` / `OnDisable` subscribe + unsubscribe
- [x] `HeartsUI.Start()` does a one-shot `UpdateHearts()`
- [x] Play Arc_1: hearts display correctly at start, segments decrease on damage, diamond pickup increases max hearts, checkpoint respawn restores hearts — no regression vs. polling version

---

### Phase A — Score foundation + visible slice

Minimum vertical slice: GameManager tracks score, enemy kills award points, HUD shows the number. This proves the whole chain before layering cherries or Game Over UI.

#### A1. Extend GameManager

**File: `Assets/Scripts/GameManager.cs`**
- Add `public int Score { get; private set; }` (property, not raw field — per CLAUDE.md conventions)
- Add `public static event System.Action<int> OnScoreChanged;`
- Add `public void AddScore(int points)` — adds to `Score`, fires event
- Add `public void ResetScore()` — zeroes `Score`, fires event (called on Game Over only — actual Game Over wiring is Phase C)
- Do NOT touch `SavePlayerState` / `OnSceneLoaded` — score is session-wide, not tied to checkpoints

**Checklist:**
- [x] `Score` property with private setter
- [x] `OnScoreChanged` static event
- [x] `AddScore(int)` method fires event
- [x] `ResetScore()` method fires event
- [x] Score not touched by scene-load / checkpoint-restore paths

#### A2. Enemy kill scoring

**File: `Assets/Scripts/EnemyHealth.cs`**
- Add `[SerializeField] private int scoreValue = 50`
- In `Die()`, before `Destroy(gameObject)`: call `GameManager.Instance.AddScore(scoreValue)`

**Checklist:**
- [x] `scoreValue` serialized field added
- [x] `AddScore` called in `Die()` before destroy
- [x] Per-enemy values configured in prefabs (Frog 50, Mushroom 75, Eagle 100)

#### A3. Create ScoreUI

**New file: `Assets/Scripts/ScoreUI.cs`**
- `[SerializeField] private TMP_Text scoreText`
- Subscribe to `GameManager.OnScoreChanged` in `OnEnable`, unsubscribe in `OnDisable`
- Update display on event: `scoreText.text = score.ToString()`
- Cherry icon is a static UI Image next to the text (placed in Inspector, not in code)

**Checklist:**
- [x] `ScoreUI.cs` created with TMP_Text reference
- [x] Subscribes / unsubscribes via `OnEnable` / `OnDisable`
- [x] TMP Essentials imported (first-time Unity prompt)
- [x] Canvas in Arc_1 HUD has ScoreUI GameObject with cherry icon + TMP text
- [x] Play Arc_1: HUD shows "0", killing a Frog jumps display to 50
- [x] Transition Arc_1 → Arc_2: score persists on HUD

---

### Phase B — Collectables

#### B1. Diamond scoring

**File: `Assets/Scripts/DiamondCollectable.cs`**
- Add `[SerializeField] private int scoreValue = 200`
- In `OnTriggerEnter2D`, after `GainHeart()`: `GameManager.Instance.AddScore(scoreValue)`

**Checklist:**
- [ ] `scoreValue` serialized field added
- [ ] `AddScore` called after `GainHeart()`
- [ ] Play: diamond pickup adds 200 AND grants heart

#### B2. Cherry collectable

**New file: `Assets/Scripts/CherryCollectable.cs`** — mirrors `DiamondCollectable.cs`:
- `[SerializeField] private int scoreValue = 10`
- `[SerializeField] private float bobSpeed = 2f`, `[SerializeField] private float bobAmplitude = 0.1f`
- Cache `Animator` in `Awake()`
- `OnTriggerEnter2D`: check "Player" tag → `AddScore` → trigger "Collected" animator → disable collider
- `DestroyCherry()` called by animation event → `Destroy(gameObject)`
- In `Update()`, sine-wave bob: `transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;`

**Unity-editor work:**
- Cherry sprite imported
- Cherry prefab with: `SpriteRenderer`, `Collider2D` (is trigger), `Animator` with idle + "Collected" states, "Collected" trigger parameter, animation event at end of "Collected" clip calling `DestroyCherry()`

**Checklist:**
- [ ] `CherryCollectable.cs` created
- [ ] Sine-wave bob implemented
- [ ] Cherry prefab with Animator + Collected animation event
- [ ] Play: cherry bobs idly, collect adds 10 and plays collect anim before destruction

---

### Phase C — Game Over + Leaderboard

#### C1. Leaderboard methods on GameManager

**File: `Assets/Scripts/GameManager.cs`**
- `public void SaveHighScore()` — inserts `Score` into PlayerPrefs-backed top 10, sorted descending
- `public List<int> GetHighScores()` — reads top 10 from PlayerPrefs

**Checklist:**
- [ ] `SaveHighScore()` writes to PlayerPrefs keys (e.g. `HighScore_0` … `HighScore_9`)
- [ ] `GetHighScores()` returns sorted descending list
- [ ] Verified via test call: score 500 appears in leaderboard after `SaveHighScore`

#### C2. GameOverUI

**New file: `Assets/Scripts/GameOverUI.cs`**
- Canvas panel with: "GAME OVER" text, final score display, top 10 leaderboard list, restart button
- `public void Show(int finalScore, List<int> highScores)`
- On show: `Time.timeScale = 0`
- Restart button: `Time.timeScale = 1`, load Arc_1, call `GameManager.Instance.ResetScore()`

**Checklist:**
- [ ] `GameOverUI.cs` created with `Show(...)` method
- [ ] Canvas panel with final score + top-10 list + restart button
- [ ] `Time.timeScale = 0` on show, restored on restart
- [ ] Restart reloads Arc_1 and calls `ResetScore()`

#### C3. Wire GameOver

**File: `Assets/Scripts/PlayerHealth.cs`**
- Replace the stub body of `GameOver()` with:
  - `GameManager.Instance.SaveHighScore()`
  - `gameOverUI.Show(GameManager.Instance.Score, GameManager.Instance.GetHighScores())`

**Checklist:**
- [ ] `GameOver()` calls `SaveHighScore` then `gameOverUI.Show`
- [ ] `Debug.Log` stub removed
- [ ] `startingPossPoint` teleport removed (Game Over UI replaces it)
- [ ] Play: lose all HP → Game Over panel with final score + leaderboard
- [ ] Click Restart → score resets to 0, Arc_1 reloads, leaderboard persists
- [ ] Close + reopen Play mode: leaderboard still there (PlayerPrefs persists)

---

## File Summary

| Phase | Action | File |
|-------|--------|------|
| A0.1 | Modify | `Assets/Scripts/PlayerHealth.cs` (event + fire sites) |
| A0.2 | Modify | `Assets/Scripts/HeartsUI.cs` (subscribe, drop polling) |
| A1   | Modify | `Assets/Scripts/GameManager.cs` (score + event) |
| A2   | Modify | `Assets/Scripts/EnemyHealth.cs` (scoreValue + AddScore) |
| A3   | Create | `Assets/Scripts/ScoreUI.cs` |
| B1   | Modify | `Assets/Scripts/DiamondCollectable.cs` (scoreValue + AddScore) |
| B2   | Create | `Assets/Scripts/CherryCollectable.cs` |
| C1   | Modify | `Assets/Scripts/GameManager.cs` (SaveHighScore + GetHighScores) |
| C2   | Create | `Assets/Scripts/GameOverUI.cs` |
| C3   | Modify | `Assets/Scripts/PlayerHealth.cs` (GameOver wiring) |

---

## End-to-End Verification (after all phases complete)

1. Enter Play mode in Arc_1 — score UI shows "0" on HUD
2. Collect a cherry — score increases by 10, cherry bobs idle then plays collect animation and disappears
3. Kill an enemy — score increases by enemy's scoreValue
4. Collect a diamond — score increases by 200 and heart is gained
5. Die and respawn at checkpoint — score remains unchanged
6. Transition from Arc_1 to Arc_2 — score carries over
7. Lose all HP (Game Over) — Game Over screen shows final score + leaderboard, score saves to high scores
8. Restart — score resets to 0, leaderboard persists across sessions
9. HeartsUI reacts instantly to damage/heal/diamond without any `Update()` polling (check Profiler)
