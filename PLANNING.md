# PLANNING.md — 2D Platformer (Project 2)

## U toku
- [ ] Polish: ground level design
- [ ] Polish: cherry collectables
- [ ] Sistemi koji fale (see below)

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
- [ ] Proširiti Game Over Screen: name input field + Submit/Skip dugmad (v. 5e)

### 5. Leaderboard — MongoDB backend

**Stack:** MongoDB Atlas free tier (M0 cluster) + Atlas App Services HTTPS endpoints. Bez plaćanja, bez zasebnog backend servera. App Services funkcije imaju cold start (~1–3s) → rešavamo warmup ping-om sa Start Screen-a tako da je leaderboard brz kad korisnik klikne dugme.

#### 5a. Provisioning (korisnik radi sam — Claude ne može login u Atlas)
- [ ] Atlas signup + free M0 cluster
- [ ] Create DB user + whitelist IP (0.0.0.0/0 za WebGL public access)
- [ ] Create database `platformer`, collection `sessions`
- [ ] Create Atlas App Services app linked to cluster
- [ ] Create HTTPS endpoints (v. 5c)
- [ ] Copy endpoint base URL → upisati u Unity `LeaderboardConfig` ScriptableObject

#### 5b. Data model
```
sessions {
  sessionId: string         // GUID, client-generated
  name: string | null       // null ili "" → prikazati "Player" u UI
  score: number
  timePlayed: number        // seconds
  createdAt: Date           // server-set on insert
}
```
Index: `{ score: -1, createdAt: -1 }` za brz top-N query.

#### 5c. Atlas App Services HTTPS endpoints
- [ ] `GET /warmup` — vraća 200, no-op. Poziva se sa Start Screen-a za cold-start mitigation.
- [ ] `POST /sessions` — body: `{ sessionId, score, timePlayed }`. Server insert sa `name: null`, `createdAt: now()`. Return 200.
- [ ] `PATCH /sessions/:sessionId/name` — body: `{ name }`. Update record. Idempotent — no-op ako sessionId ne postoji ili name je već upisan.
- [ ] `GET /leaderboard?limit=N` — top-N sortirano po score desc. Default limit 50.
- [ ] CORS: allow origin = GitHub Pages URL (konfiguriše se u App Services)

#### 5d. Unity client
- [ ] `LeaderboardConfig.cs` ScriptableObject — holds endpoint base URL (lako menjati bez rebuild-a koda)
- [ ] `LeaderboardClient.cs` — UnityWebRequest wrappers:
  - `Warmup()` (fire-and-forget)
  - `SubmitSession(sessionId, score, timePlayed)` — POST
  - `SubmitName(sessionId, name)` — PATCH
  - `FetchTopN(limit)` — GET, vraća listu entry-ja
- [ ] `SessionTracker.cs` — generiše `sessionId` (GUID) on game start, trackuje `timePlayed` preko `Time.unscaledTime` delta (pauza ne broji), exposes current values
- [ ] Name display fallback: empty/null name → render as "Player" u leaderboard UI
- [ ] Time format helper: seconds → `mm:ss` za prikaz

#### 5e. Integration points
- [ ] Start Screen `Awake()` → `LeaderboardClient.Warmup()` (fire-and-forget, cold-start mitigation)
- [ ] Start Screen Leaderboard dugme → `FetchTopN(50)` → populate UI listu (name, score, timePlayed mm:ss)
- [ ] Play dugme → `SessionTracker.StartSession()` (reset sessionId + timer) → load gameplay scene
- [ ] `GameOverUI.Show()` → `SubmitSession(...)` fire-and-forget (record se kreira odmah, i ako igrač zatvori tab bez imena ostaje u leaderboard-u)
- [ ] Name input Submit → `SubmitName(...)`; Skip → zatvori (record već postoji sa null name → prikazuje se kao "Player")

### 6. Deploy — WebGL + GitHub Pages
- [ ] Unity WebGL build settings: Brotli compression, default ili minimalni custom template
- [ ] Build output → GitHub Pages (odvojen repo ili `gh-pages` branch ovog repo-a — odluka pri deploy-u)
- [ ] Configure GitHub Pages source (branch + folder)
- [ ] Verifikovati: leaderboard endpointi rade iz WebGL build-a (CORS allow origin u Atlas App Services mora da matchuje GitHub Pages URL)
- [ ] Smoke test: open page → warmup fires → klik Leaderboard (prazan na početku) → Play → završi game → submit name → refresh Leaderboard → vidiš entry

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

---

## Kako ažurirati
- Završen task → `- [ ]` u `- [x]`
- Počet task → dodaj u **"U toku"**
- Blocker → `⚠️ BLOCKER:` ispod relevantnog taska
