# PLANNING.md — 2D Platformer (Project 2)

## U toku
- [ ] ...

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

---

## Arc_2 — Main Arc

### Surface (on the ground)
- [x] Level design — surface deo
- [x] Enemy AI: snake, lizard, opossum
- [ ] Score / diamond counter UI

### Underground tunnel
- [ ] Level design — underground deo
- [ ] Enemy AI: slimer, mushroom (gas attack)
- [ ] Mini-boss fight → daje **Double Jump**
- [ ] Double Jump implementacija u PlayerController

### Above (platformer in clouds)
- [ ] Level design — cloud platforme
- [ ] Enemy AI: bat (leteći, drugačiji patrol pattern)
- [ ] Mini-boss fight → daje **Ultimate Fireball** (long range)
- [ ] Ultimate Fireball implementacija

---

## Arc_2 — Puzzle Mehanike

> Svaki od 3 sloja u Arc_2 ima jednu puzzle sekciju. Svaki puzzle koristi iste skripte, samo u drugom kontekstu.

### Skripte koje treba napraviti

- [x] `PressurePlate.cs` — aktivira se kad igrač ili box stane na nju, šalje `UnityEvent`
- [x] `MovingPlatform.cs` — kreće se između 2 tačke, može biti vezana za PressurePlate/Switch
- [x] `PushableBox.cs` — igrač ga gura levo/desno, reaguje na gravitaciju, aktivira PressurePlate
- [ ] `TimedSwitch.cs` — aktivira se slash attack-om (`TakeDamage`), ostaje aktivan X sekundi
- [ ] `TimedGate.cs` — otvara/zatvara se na signal od Switch-a, sa vizuelnim tajmerom
- [ ] `CrumblingPlatform.cs` — raspada se N sekundi nakon što igrač stane, ne može se resetovati

### Puzzle A — Surface (Ground Level)

**Mehanika:** Pressure Plates + Moving Platforms

- [x] Igrač stane na pressure plate → platforma se pomeri i otvori put
- [x] Napreduje do: plate na jednom mestu, platforma na drugom → moraš brzo da stigneš
- [x] Finalni korak: gurni PushableBox na plate da zaključaš platformu u poziciji trajno

### Puzzle B — Underground Tunnels

**Mehanika:** PushableBox u mraku → Pressure Plates

- [ ] 2-3 PushableBox-a raspoređena po tunelu
- [ ] Igrač ih gura na odgovarajuće pressure plate-ove da otvori put do Boss Arene
- [ ] Mračnija atmosfera — vizuelno ograničen prostor čini puzzle težim za čitanje

### Puzzle C — Cloud Level (Gornji sloj)

**Mehanika:** Timed Switches + Crumbling Platforms — vertikalni uspon

- [ ] Igrač udari switch mačem → otvori se serija platformi na ograničeno vreme (3-5s)
- [ ] Crumbling platforms se raspadaju čim igrač krene — nema povratka
- [ ] Finalni korak: dva switch-a u pravom redosledu + precizno skakanje kroz nestajuće platforme

---

## Arc_3 — Boss Arena
- [ ] Level design — boss arena
- [ ] Boss Dragon fight (breath attack, faze, death)
- [ ] Victory / end level state

---

## Sistemi koji fale
- [ ] Audio — AudioManager singleton (Music, SFX, UI mixer grupe)
- [ ] SFX: jump, attack, hit, death, pickup, checkpoint
- [ ] Muzika: gameplay loop, boss tema
- [ ] Menu sistem: Start Screen, Pause Menu, Game Over Screen
- [ ] WebGL build + GitHub Pages deploy

---

## Kako ažurirati
- Završen task → `- [ ]` u `- [x]`
- Počet task → dodaj u **"U toku"**
- Blocker → `⚠️ BLOCKER:` ispod relevantnog taska
