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
- [ ] Level design — surface deo
- [ ] Enemy AI: snake, lizard, opossum
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
