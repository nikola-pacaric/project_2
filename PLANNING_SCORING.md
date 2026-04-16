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

---

## Implementation Steps

### Step 1 — Extend GameManager with score tracking

**File:** `Assets/Scripts/GameManager.cs`

Add to the existing singleton:
- `private int score` field
- `public int Score` read-only property
- `public static event System.Action<int> OnScoreChanged` — event so UI reacts without polling
- `public void AddScore(int points)` — adds points, fires event
- `public void ResetScore()` — called on Game Over only
- `public void SaveHighScore()` — saves final score to PlayerPrefs leaderboard
- `public List<int> GetHighScores()` — returns top 10 scores from PlayerPrefs
- Score is NOT reset on scene transitions or checkpoint respawns — only on Game Over
- Score is NOT saved/restored with checkpoint state (it's session-wide)

### Step 2 — Create CherryCollectable.cs

**New file:** `Assets/Scripts/CherryCollectable.cs`

Follows the existing `DiamondCollectable.cs` pattern:
- `[SerializeField] private int scoreValue = 10` — points per cherry
- `Animator` reference cached in `Awake()`
- `OnTriggerEnter2D` — check "Player" tag, call `GameManager.Instance.AddScore(scoreValue)`, trigger "Collected" animation, disable collider
- `DestroyCherry()` — called by animation event, destroys GameObject
- Idle bobbing via sine-wave motion in `Update()` (no Animator needed for the bob — lightweight; Animator handles collect animation only)

### Step 3 — Create ScoreUI.cs

**New file:** `Assets/Scripts/ScoreUI.cs`

Placed on the Canvas alongside HeartsUI:
- `[SerializeField] private TMP_Text scoreText` — TextMeshPro reference
- Subscribes to `GameManager.OnScoreChanged` in `OnEnable`, unsubscribes in `OnDisable`
- Updates display on event: `scoreText.text = score.ToString()`
- Cherry icon is a static UI Image next to the text (set up in Unity Inspector, not in code)

### Step 4 — Enemy kill scoring

**File:** `Assets/Scripts/EnemyHealth.cs`

- Add `[SerializeField] private int scoreValue = 50` — points per enemy kill
- In `Die()`, before `Destroy(gameObject)`: call `GameManager.Instance.AddScore(scoreValue)`
- Different enemies get different point values via the Inspector (Frog=50, Mushroom=75, Eagle=100, etc.)

### Step 5 — Diamond scoring

**File:** `Assets/Scripts/DiamondCollectable.cs`

- Add `[SerializeField] private int scoreValue = 200` — diamonds are rare, worth more
- In `OnTriggerEnter2D`, after `GainHeart()`: call `GameManager.Instance.AddScore(scoreValue)`

### Step 6 — Game Over screen with score + leaderboard

**File:** `Assets/Scripts/PlayerHealth.cs` — modify `GameOver()`

- Instead of just teleporting, call `GameManager.Instance.SaveHighScore()` then show Game Over UI

**New file:** `Assets/Scripts/GameOverUI.cs`

- Panel with: "GAME OVER" text, final score display, top 10 high score leaderboard, restart button
- `Show(int finalScore, List<int> highScores)` method
- Restart button reloads Arc_1 and calls `GameManager.Instance.ResetScore()`
- Pauses game (`Time.timeScale = 0`) while shown, resumes on restart

---

## File Summary

| Action | File |
|--------|------|
| Modify | `Assets/Scripts/GameManager.cs` |
| Modify | `Assets/Scripts/EnemyHealth.cs` |
| Modify | `Assets/Scripts/DiamondCollectable.cs` |
| Modify | `Assets/Scripts/PlayerHealth.cs` |
| Create | `Assets/Scripts/CherryCollectable.cs` |
| Create | `Assets/Scripts/ScoreUI.cs` |
| Create | `Assets/Scripts/GameOverUI.cs` |

---

## Verification Checklist

1. Enter Play mode in Arc_1 — score UI shows "0" on HUD
2. Collect a cherry — score increases by 10, cherry bobs idle then plays collect animation and disappears
3. Kill an enemy — score increases by enemy's scoreValue
4. Collect a diamond — score increases by 200 and heart is gained
5. Die and respawn at checkpoint — score remains unchanged
6. Transition from Arc_1 to Arc_2 — score carries over
7. Lose all HP (Game Over) — Game Over screen shows final score + leaderboard, score saves to high scores
8. Restart — score resets to 0, leaderboard persists across sessions
