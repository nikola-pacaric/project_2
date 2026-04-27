# PLANNING.md — 2D Platformer (Project 2)

## U toku
- [ ] Polish: ground level design
- [ ] Polish: cherry collectables
- [ ] Sistemi koji fale (see below)
- [ ] Arc_2 ending cinematic (see below)

---

## Završeno
- [x] Player movement (coyote time, ground/air speed, New Input System)
- [x] Combat sistem (stomp + melee fire slash, 0.5s cooldown)
- [x] Health sistem (3 srca × 4 segmenta, knockback, death zone)
- [x] Checkpoint sistem (Bonfire prefab)
- [x] Collectables (dijamanti + Gem_Life)
- [x] Cinemachine kamera + parallax scrolling
- [x] Arc_1 — Introduction arc (tunel, dijamant, bonfire, jedan neprijatelj)
- [x] EnemyFrogAI (patrol, jump, taunt)
- [x] Game Over screen

---

## Arc_2 — Main Arc (Surface only — scope for this dev cycle)

### Surface (on the ground)
- [x] Level design — surface deo
- [x] Enemy AI: snake, lizard, opossum
- [x] Score / diamond counter UI

### Puzzle A — Surface (Ground Level)

**Mehanika:** Pressure Plates + Moving Platforms

- [x] `PressurePlate.cs` — aktivira se kad igrač ili box stane na nju, šalje `UnityEvent`
- [x] `MovingPlatform.cs` — kreće se između 2 tačke, može biti vezana za PressurePlate/Switch
- [x] `PushableBox.cs` — igrač ga gura levo/desno, reaguje na gravitaciju, aktivira PressurePlate
- [x] Igrač stane na pressure plate → platforma se pomeri i otvori put
- [x] Napreduje do: plate na jednom mestu, platforma na drugom → moraš brzo da stigneš
- [x] Finalni korak: gurni PushableBox na plate da zaključaš platformu u poziciji trajno

---

## Arc_2 ending cinematic — close-out before boss-room wall

**Trigger:** invisible trigger placed *before* the unbreakable wall, so the player visibly stops short of it and the camera frames the wall.

**Flow (high level):**
1. Player enters trigger → cinematic kicks in, controls cut.
2. Camera detaches from player, slowly pans + zooms to wall while screen partially darkens.
3. Music fades to silence.
4. "Congratulations! You finished the game... TO BE CONTINUED!" text pops over the darkened scene (wall still visible behind).
5. Hold a few seconds, then normal `GameOverUI` panel takes over (auto-submit + name input).

### Implementation checklist

#### Script: `Assets/Scripts/EndingCinematic.cs`
- [ ] `[RequireComponent(typeof(Collider2D))]` — trigger collider, `Reset()` forces `isTrigger = true`
- [ ] Inspector slots:
  - `playerTag` (default `"Player"`)
  - `playerScriptsToDisable: MonoBehaviour[]` — wire `PlayerController` + `PlayerCombat`
  - `vcam: CinemachineCamera`
  - `cameraFocus: Transform` — empty GO placed at desired camera end position
  - `cameraMoveDuration` (~2.5s), `zoomTargetSize` (~3)
  - `dimGroup: CanvasGroup` — full-screen semi-transparent black image
  - `dimTargetAlpha` (~0.65), `dimFadeDuration` (matches camera move)
  - `messageGroup: CanvasGroup` — "Congratulations! ... TO BE CONTINUED!" text
  - `messageFadeDuration` (~0.4s), `messageHoldDuration` (~3s)
  - `musicFadeDuration` (~2s)
  - `gameOverUI: GameOverUI`
- [ ] `triggered` bool guard (fires once)
- [ ] On player enter trigger:
  1. `RunTimer.Instance?.Pause()` — timer stops *now*, before cinematic time accrues
  2. Disable each script in `playerScriptsToDisable` (their `OnDisable` cuts input actions)
  3. Zero `Rigidbody2D.linearVelocity`; set Animator `Speed = 0f`
  4. `vcam.Follow = null; vcam.LookAt = null`
  5. Concurrent coroutines (use `Time.unscaledDeltaTime` everywhere):
     - Camera move: lerp `vcam.transform.position` → `cameraFocus.position` (preserve z) + lerp `Lens.OrthographicSize` → `zoomTargetSize`, smoothstep ease
     - Dim fade: `dimGroup.alpha` 0 → `dimTargetAlpha`
     - Music fade: `AudioManager.Instance.FadeOutMusic(musicFadeDuration)` (see below)
  6. After all three finish: fade `messageGroup.alpha` 0 → 1
  7. `WaitForSecondsRealtime(messageHoldDuration)`
  8. `gameOverUI.Show(GameManager.Instance?.Score ?? 0)`
     - Note: `GameOverUI.Show` already sets `Time.timeScale = 0` and pauses the timer (no-op since we already paused). Cinematic itself does NOT touch `timeScale` — uses unscaled time so it keeps running at normal speed.

#### Audio support
- [ ] Add `AudioManager.FadeOutMusic(float duration)` — coroutine that lerps `musicSource.volume` to 0 over duration. Local fade only, does NOT touch mixer params (preserves user's volume preference). Restored next time `PlayMusic` is called (it sets `volume` from library).

#### Scene work (Arc_2.unity)
- [ ] Add invisible trigger GameObject in front of the wall:
  - `BoxCollider2D` (isTrigger), sized so player walks into it before reaching the wall sprite
  - `EndingCinematic` component
- [ ] Empty `CameraFocus` GameObject placed where camera should end up (centered on wall, at desired zoom framing)
- [ ] Cinematic UI Canvas (Screen Space Overlay, sortingOrder ≥ 33000 so it's above `ScreenFader`'s 32000):
  - Child 1: `DimGroup` — full-stretch `Image` (black, alpha 1) + `CanvasGroup` (start alpha 0)
  - Child 2: `MessageGroup` — TMP text "Congratulations! You finished the game... TO BE CONTINUED!" + `CanvasGroup` (start alpha 0)
- [ ] Wire all Inspector references on `EndingCinematic` (vcam, focus, dim group, message group, GameOverUI, player scripts)

#### Verification
- [ ] Player approaches wall → stops short → no input accepted (movement, jump, attack)
- [ ] Camera smoothly pans + zooms; screen darkens to ~65% (wall still readable behind)
- [ ] Music fades out cleanly, no abrupt cut
- [ ] Message appears after camera settles, holds, then GameOverUI takes over
- [ ] Run timer value frozen at cinematic start (not at GameOver start) — visible on GameOver panel
- [ ] Restart from GameOver returns to fresh Arc_1 with full state reset (existing flow, just verify)

---

## Sistemi koji fale (today's focus)

### 1. Audio — AudioManager singleton

**Pristup:** wire system first sa placeholder ID-jevima, import audio files-ova kasnije. Gameplay code zove po ID-u — kad fajlovi stignu samo popunimo Inspector slotove, bez izmene koda.

- [x] `AudioManager.cs` singleton u `Scripts/Audio/` (DontDestroyOnLoad)
- [x] `SoundLibrary.cs` ScriptableObject — mapa `SfxId` enum → `AudioClip`, i `MusicId` enum → `AudioClip`
- [x] Definisati enume:
  - `SfxId { Jump, Slash, Stomp, Hit, PlayerDeath, DiamondPickup, CherryPickup, Checkpoint, EnemyFrogJump, EnemyFrogTaunt, EnemyMushroomGas, EnemyEagleAttack, EnemyDeath }`
  - `MusicId { Gameplay }`
- [x] AudioMixer asset (`MainMixer.mixer`) sa grupama: Music, SFX, UI + exposed params `MusicVolume`, `SfxVolume`, `UiVolume`
- [x] API:
  - `AudioManager.Instance.PlaySFX(SfxId id)`
  - `AudioManager.Instance.PlayMusic(MusicId id)`
  - `AudioManager.Instance.StopMusic()`
  - `AudioManager.Instance.SetVolume(AudioChannel channel, float 0–1)`
- [x] SFX pool (nekoliko AudioSource-a za preklapanje zvukova) umesto jednog source-a
- [x] Null-safe: ako clip nije upisan u SoundLibrary, log warning ali ne baca exception (omogućava rad sa praznim slotovima)
- [x] `MainSoundLibrary` asset popunjen sa svih 13 SFX entries + 1 Music entry
- [x] `AudioManager` kao prefab instanciran u `Arc_1.unity` i `Arc_2.unity` (Library, Mixer, 3 grupe wired up)
- [x] On-screen gate — `PlaySFXAt(id, worldPos)` varijanta, enemy sounds gate-ovani kroz `Camera.main.WorldToViewportPoint`. Van ekrana → tišina.

### 2. SFX hookup — completed

**Player (always audible)**
- [x] `SfxId.Jump` — `PlayerController` (normal jump + ladder jump)
- [x] `SfxId.Slash` — `PlayerCombat.MeleeAttack`
- [x] `SfxId.Stomp` — `PlayerController.OnTriggerEnter2D` (stomp trigger)
- [x] `SfxId.Hit` — `PlayerHealth.TakeEnemyDamage` + `TakeSpikeDamage`
- [x] `SfxId.PlayerDeath` — `PlayerHealth.GameOver`

**Pickups (always audible — player-triggered)**
- [x] `SfxId.DiamondPickup` — `DiamondCollectable` (Gem_Life je isti objekat = DiamondCollectable)
- [x] `SfxId.CherryPickup` — `CherryCollectable`
- [x] `SfxId.Checkpoint` — `Checkpoint` (guard: fires jednom per bonfire, ne na svakom walk-through)

**Enemies (on-screen gated via `PlaySFXAt`)**
- [x] `SfxId.EnemyFrogJump` — `EnemyFrogAI.JumpSequence`
- [x] `SfxId.EnemyFrogTaunt` — `EnemyFrogAI.TauntSequence`
- [x] `SfxId.EnemyMushroomGas` — `EnemyMushroomAI.FireGas`
- [x] `SfxId.EnemyEagleAttack` — `EnemyEagleAI.AttackSequence` (spotted telegraph)
- [x] `SfxId.EnemyDeath` — `EnemyHealth.Die` (shared svi enemies)

### 3. Muzika
- [x] Gameplay loop — ground level tema (trigger na Start Screen → gameplay scene transition)

### 4. Menu sistem
- [x] **Start Screen (scene)** — 2 dugmeta: **Play** i **Leaderboard**
  - Play → load gameplay scene, start session (v. 5e)
  - Leaderboard → otvori leaderboard panel (isti scene ili overlay) sa top-N listom
  - Fire leaderboard warmup ping on scene load (v. 5e)
- [x] **Pause Menu** — `Time.timeScale = 0`, Resume / Restart / Quit to Menu
  - Disable input actions selectively dok je pauzirano
- [x] Game Over Screen (already done)
- [x] Proširiti Game Over Screen: name input field + Submit/Skip dugmad (v. 5e)

### 5. Leaderboard — Unity Gaming Services (UGS)

**Stack:** UGS Authentication (anonymous sign-in) + UGS Leaderboards. Unity-native, first-party, free tier (50k MAU). Nema custom backend, nema deploy koraka, nema CORS setup-a — Unity SDK hendluje sve preko `UnityServices` API-ja.

⚠️ Originalno planirano na MongoDB Atlas App Services — **deprecated 2025-09-30**, nije dostupan za nove naloge. Pivot na UGS jer je standard za Unity projekte i ne zahteva custom backend hosting.

#### 5a. Dashboard provisioning (korisnik radi sam na cloud.unity.com — Claude ne može login)
- [x] Signup/login na **cloud.unity.com** (isti Unity ID kao Editor)
- [x] Kreirati ili link-ovati Unity Cloud Project sa lokalnim Unity Editor projektom
- [x] Enable **Authentication** servis + turn on **Anonymous** identity provider
- [x] Enable **Leaderboards** servis
- [x] Create leaderboard:
  - ID: `main_scores`
  - Sort order: **Descending** (higher score = better)
  - Update type: **Keep Best** (overwrite samo ako novi score > postojeći)
  - Reset: **None** (all-time leaderboard)
- [x] Verifikovati u Editor-u: Edit > Project Settings > Services → pokazuje linked Project ID + Environment

#### 5b. Data model (per-player-per-leaderboard entry)
UGS Leaderboards auto-genericka šema:
```
{
  playerId:   string   // auto-assigned od Authentication servisa
  playerName: string   // set preko UpdatePlayerNameAsync; null/"" → UI prikazuje "Player"
  score:      double   // u našem slučaju int score
  metadata:   string   // arbitrary JSON — koristimo za { "timePlayed": seconds }
  rank:       int      // server-computed, read-only
  updatedAt:  DateTime // server-set
}
```

#### 5c. Unity package setup (korisnik radi kroz Package Manager)
- [x] Window > Package Manager → Unity Registry → install:
  - `com.unity.services.core`
  - `com.unity.services.authentication`
  - `com.unity.services.leaderboards`
- [x] Edit > Project Settings > Services → link na Unity Cloud project iz 5a

#### 5d. Unity client scripts
- [x] `LeaderboardConfig.cs` ScriptableObject — drži leaderboard ID (`main_scores`) i default fetch limit (50)
- [x] `LeaderboardClient.cs` — async wrappers (rade UniTask/Task):
  - `InitializeAsync()` — `UnityServices.InitializeAsync()` + `AuthenticationService.Instance.SignInAnonymouslyAsync()`
  - `SubmitScoreAsync(score, timePlayed)` — `LeaderboardsService.Instance.AddPlayerScoreAsync(id, score, new AddPlayerScoreOptions { Metadata = "{\"timePlayed\":X}" })`
  - `SubmitNameAsync(name)` — `AuthenticationService.Instance.UpdatePlayerNameAsync(name)` (globalno per-player ime, vidljivo u svim leaderboardima)
  - `FetchTopNAsync(limit)` — `LeaderboardsService.Instance.GetScoresAsync(id, new GetScoresOptions { Limit = limit })`
- [x] `SessionTracker.cs` — trackuje `timePlayed` preko `Time.unscaledTime` delta (pauza ne broji). Nema više GUID-a (UGS auto-hendluje identitet).
- [x] Name display fallback: empty/null `playerName` → render kao "Player"
- [x] Time format helper: seconds → `mm:ss`

#### 5e. Integration points
- [x] Start Screen `Awake()` → `LeaderboardClient.InitializeAsync()` (zamenjuje stari Warmup ping — ovo je stvarna UGS inicijalizacija + anonimni sign-in)
- [x] Start Screen Leaderboard dugme → `FetchTopNAsync(50)` → populate UI listu (playerName, score, timePlayed mm:ss)
- [x] Play dugme → `SessionTracker.StartSession()` (reset timer) → load gameplay scene
- [x] `GameOverUI.Show()` → `SubmitScoreAsync(score, timePlayed)` fire-and-forget. KeepBest server-side → ne treba client-side poređenje.
- [x] Name input Submit → `SubmitNameAsync(name)`; Skip → zatvori (ime ostaje prazno → UI prikazuje "Player")

#### 5f. WebGL compatibility
- [x] UGS podržava WebGL out-of-the-box; nema CORS setup (SDK priča sa `*.services.api.unity.com`, Unity hendluje)
- [x] `InitializeAsync` okružiti try/catch — failed init (offline) ne sme da sruši Start Screen; Leaderboard dugme u tom slučaju prikaže "Offline"

### 6. Deploy — WebGL + GitHub Pages
- [ ] Unity WebGL build settings: Brotli compression, default ili minimalni custom template
- [ ] Build output → GitHub Pages (odvojen repo ili `gh-pages` branch ovog repo-a — odluka pri deploy-u)
- [ ] Configure GitHub Pages source (branch + folder)
- [ ] Verifikovati: UGS Authentication + Leaderboards rade iz WebGL build-a (nema CORS config potrebnog — SDK hendluje)
- [ ] Smoke test: open page → init fires (anonimni sign-in) → klik Leaderboard (prazan na početku) → Play → završi game → submit name → refresh Leaderboard → vidiš entry

---

## Combat refactor — Hurtbox / Hitbox system (pre-DLC prep)

**Why:** Current combat splits responsibility across tag-based triggers (`Stomp`, `Enemy`), `OnCollisionEnter2D` on `PlayerController`, and the slide-hit path in `FixedUpdate`. Two independent systems can decide the same physical contact, which required velocity/normal/position heuristics and an `isStomping` guard to disambiguate. That fragility will multiply with every new enemy in DLC 1–3 (slimer, mushroom, bat, boss dragon). Replace it with a standard Hurtbox/Hitbox model before adding more combat surface area.

**Scope gate:** Do NOT start until Arc_2 is shipped and WebGL deploy (§6) is green. This refactor touches every enemy prefab + player combat scripts — too much regression risk pre-ship.

### Design

- `DamageInfo` struct — `{ int amount, Vector2 knockbackDir, GameObject source, DamageType type }` (`DamageType` enum: `Stomp`, `Slash`, `Contact`, `Projectile`, `Spike`)
- `Hitbox.cs` — trigger collider, emits `DamageInfo` on overlap. Exposes `SetActive(bool)` for time-windowed attacks. Lives on: player feet (stomp), player slash arc, enemy bodies (contact), future projectiles.
- `Hurtbox.cs` — trigger collider, receives `DamageInfo`. Forwards to a `Health` component via `ReceiveHit(DamageInfo)`. Always on.
- `Health.cs` — unified replacement for `PlayerHealth` + `EnemyHealth`. Handles HP, i-frames, death event. Exposes `UnityEvent<DamageInfo> OnDamaged` so VFX/SFX/camera shake/score UI subscribe instead of being wired directly into health code.
- Hitbox↔Hurtbox overlap resolved in a single place: `Hurtbox.OnTriggerEnter2D(hitbox) → health.ReceiveHit(hitbox.BuildDamageInfo())`. No tags, no contact normals, no `isStomping` bool.

### Migration steps (incremental — one enemy at a time, old + new coexist during transition)

- [x] Create `Assets/Scripts/Combat/` — `DamageInfo.cs`, `Hitbox.cs`, `Hurtbox.cs`, `Health.cs`
- [x] Migrate `PlayerHealth` → `Health` on Player. Port `TakeEnemyDamage` / `TakeSpikeDamage` callers to fire through a player-contact Hitbox instead.
- [x] Replace player's Stomp behaviour: add child `Hitbox` at feet, active while `rb.linearVelocity.y < 0`. Delete the `OnTriggerEnter2D("Stomp")` path in `PlayerController`.
- [x] Replace player's Slash: `PlayerCombat` activates a child `Hitbox` during the slash anim window, deactivates after. Delete current slash damage dispatch.
- [x] Delete the `OnCollisionEnter2D` contact-damage block in `PlayerController` and the slide-hit damage path in `FixedUpdate`. Player damage is now ONLY Hurtbox-driven.
- [x] Migrate enemies one prefab at a time: `Enemy_Frog` → `Enemy_Eagle` → `Mushroom_Enemy` → snake/lizard/opossum. Each gets Hurtbox (body) + contact Hitbox (body, disabled during hitstun). Remove `Stomp_Box` child trigger — redundant.
- [x] `EnemyHealth` → `Health`. Move the 0.15s i-frame logic into `Health` so player and enemies share the same invulnerability model.
- [x] Subscribe existing systems to `Health.OnDamaged`: AudioManager SFX (`SfxId.Hit`, `SfxId.Stomp`, `SfxId.EnemyDeath`), knockback (`LockMovement`), GameManager score on death, flash VFX.
- [x] Remove now-unused tags (`Stomp`, possibly `Enemy` if no other systems check it). Audit `CompareTag` callsites.
- [x] Layer audit: use Physics2D layer collision matrix so Hitbox-vs-Hurtbox only fires between valid pairs (player-hitbox ↔ enemy-hurtbox, enemy-hitbox ↔ player-hurtbox). Prevents enemy-vs-enemy friendly fire accidentally.

### Success criteria

- Mutual damage (player + enemy both taking a hit from the same contact) is structurally impossible, not heuristically avoided.
- Adding a new enemy requires zero changes to player scripts.
- Adding a new attack type (projectile, AoE, boss breath) is: spawn a GameObject with a Hitbox + lifetime, done.

---

## Future updates (DLC — out of current scope)

### DLC 1 — Underground Tunnel
- [ ] Level design — underground deo
- [ ] Enemy AI: slimer, mushroom (gas attack)
- [ ] Mini-boss fight → daje **Double Jump**
- [ ] Double Jump implementacija u PlayerController
- [ ] Puzzle B — PushableBox u mraku → Pressure Plates
  - 2-3 PushableBox-a raspoređena po tunelu
  - Igrač ih gura na odgovarajuće pressure plate-ove da otvori put do Boss Arene
  - Mračnija atmosfera — vizuelno ograničen prostor čini puzzle težim za čitanje

### DLC 2 — Cloud Level
- [ ] Level design — cloud platforme
- [ ] Enemy AI: bat (leteći, drugačiji patrol pattern)
- [ ] Mini-boss fight → daje **Ultimate Fireball** (long range)
- [ ] Ultimate Fireball implementacija
- [ ] Nove puzzle skripte:
  - `TimedSwitch.cs` — aktivira se slash attack-om (`TakeDamage`), ostaje aktivan X sekundi
  - `TimedGate.cs` — otvara/zatvara se na signal od Switch-a, sa vizuelnim tajmerom
  - `CrumblingPlatform.cs` — raspada se N sekundi nakon što igrač stane, ne može se resetovati
- [ ] Puzzle C — Timed Switches + Crumbling Platforms — vertikalni uspon
  - Igrač udari switch mačem → otvori se serija platformi na ograničeno vreme (3-5s)
  - Crumbling platforms se raspadaju čim igrač krene — nema povratka
  - Finalni korak: dva switch-a u pravom redosledu + precizno skakanje kroz nestajuće platforme

### DLC 3 — Boss Arena
- [ ] Level design — boss arena
- [ ] Boss Dragon fight (breath attack, faze, death)
- [ ] Boss tema muzika
- [ ] Victory / end level state

### Maybe — Offline leaderboard fallback

**Status:** deferred. Considered during main-menu polish, postponed. Add only if players actually report losing offline runs.

**Idea:** If UGS init fails at Main Menu Awake, route leaderboard ops to a local JSON file instead of erroring out. No sync, no merge — strictly a fallback so offline players still see their own scores.

**Design outline (when/if we build it):**
- One online/offline decision per session, fixed at Main Menu Awake (`LeaderboardClient.IsOnline`)
- `LocalLeaderboardStore.cs` — reads/writes `Application.persistentDataPath/offline_leaderboard.json`, top-50 cap, sorted desc by score
- `LeaderboardClient.SubmitScoreAsync` / `FetchTopNAsync` internally route on `IsOnline` — callers unchanged
- Local name source: existing `PLAYER_NAME_PREF_KEY` in PlayerPrefs, fallback `"Player"`
- Optional UI hint: small "Offline mode" label on LeaderboardPanel when `!IsOnline`

**Caveats that made us defer:**
- WebGL persistentDataPath = IndexedDB per browser per origin → different browsers see different local lists; incognito wipes on close
- Adds a second code path to test + maintain for an edge case (player opens GitHub Pages build with no internet)
- Global UGS leaderboard already works on WebGL over HTTPS for the 99% of players with internet

---

## Kako ažurirati
- Završen task → `- [ ]` u `- [x]`
- Počet task → dodaj u **"U toku"**
- Blocker → `⚠️ BLOCKER:` ispod relevantnog taska
