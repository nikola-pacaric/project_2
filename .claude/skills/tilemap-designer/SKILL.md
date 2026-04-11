---
name: tilemap-designer
description: "2D Game Level Tilemap Designer — creates clean, grid-based HTML visualizations of game level layouts. Use this skill whenever the user asks for a level design, tilemap layout, map blueprint, dungeon layout, game world grid, room layout, or any visual representation of a 2D game environment. Triggers on: level design, tilemap, map layout, grid map, dungeon map, room design, floor plan for games, platformer layout, top-down map, game world layout, blueprint for a level. Also use when the user says things like 'show me how the level would look', 'design the map', 'draw out the layout', 'make a visual of the level', or 'sketch the level structure'. This skill should be used even if the user doesn't say 'tilemap' explicitly — any request for a 2D game level visualization qualifies."
---

# 2D Game Level Tilemap Designer

You create clean, readable HTML tilemap visualizations for 2D game levels. The output is always a single self-contained HTML file with an interactive canvas-based grid that the user can open in any browser.

## The Visual Style

The signature style is **black tiles on a white background** with colored markers for special elements. Think of it as a blueprint or architectural floor plan for a game level. This style exists because it's immediately readable — anyone can look at it and understand the spatial layout without being distracted by textures or art. It also maps 1:1 to how tile-based levels work in engines like Unity, Godot, or Tiled.

Every tile in the grid represents one game unit (1×1). The grid has faint lines so you can count tiles easily. The overall aesthetic is minimal and technical — like graph paper with blocks drawn on it.

## Color System

Use this fixed palette for tile types. The specific types will vary by game genre, but the color categories stay consistent:

| Color | Hex | Meaning | Use for |
|-------|-----|---------|---------|
| Black | `#111111` | Solid / impassable | Ground, walls, ceilings, solid blocks |
| Dark gray | `#555555` | Semi-solid / one-way | One-way platforms, thin floors, bridges |
| Red | `#ee2222` | Danger / lethal | Spikes, lava, death zones, traps |
| Orange (bright) | `#ff9900` | Checkpoint / save | Bonfires, save points, respawn markers |
| Green | `#22aa22` | Player / key position | Spawn point, exit, key locations |
| Blue | `#4488ff` | Puzzle / interactive | Switches, pressure plates, puzzle elements |
| Purple | `#aa44ff` | Locked / gated | Areas requiring abilities, locked doors |
| Yellow-orange | `#ffaa00` | Enemy / hazard | Enemy spawn positions, patrol markers |
| Cyan | `#00cccc` | Common collectable | Coins, score items, regular pickups — appear many times per level |
| Hot pink | `#ff44aa` | Rare / special item | Health hearts, key items, story-critical rewards — appear once or very rarely per level |
| White | `#ffffff` | Empty / air | Open space the player moves through |

**Common vs Rare collectables must always be visually distinct.** Cyan is for things scattered freely (coins, score gems). Hot pink is reserved for things that meaningfully change the player's state (extra heart, key item, ability unlock). Never use the same color for both — a health heart hidden in a corner is not the same design signal as a coin trail.

If the game type needs additional categories (e.g., water, moving platforms, teleporters), pick distinct colors that don't conflict with the existing palette and add them to the legend.

## HTML File Structure

Every tilemap HTML file follows this structure. This matters because consistency makes the outputs predictable and the user builds familiarity with the format across multiple designs.

### 1. Header
A centered title with the level/area name and a subtitle showing dimensions and scale (e.g., "200×35 tiles · 1 tile = 1 Unity unit").

### 2. Color Legend
A horizontal bar of colored swatches with labels, showing every tile type used in this specific map. Only include types that actually appear — don't show a "Danger" swatch if there are no spikes.

### 3. The Canvas Grid
An HTML `<canvas>` element rendered via JavaScript. The canvas approach (not HTML table or div grid) is important because it handles large maps (200×100+ tiles) without performance issues.

### 4. Zoom Controls
Three buttons fixed to the bottom-right: Zoom +, Zoom −, Reset. The default tile size should be 7px, which gives a good overview of large maps. Zoom range: 3px (zoomed out) to 20px (zoomed in).

### 5. Section Labels
Text labels rendered on the canvas that identify different areas/rooms/sections of the level. These use a lighter gray color so they're visible but don't dominate the visual.

### 6. Annotations
Colored text callouts for important gameplay information — things like "needs double jump", "boss arena", "secret passage". These use the color of the relevant tile type they're describing.

## How to Build the Map Data

The tilemap is stored as a 2D array in JavaScript. Define numeric constants for each tile type at the top of the script:

```javascript
const E = 0;  // Empty
const S = 1;  // Solid
const P = 2;  // Platform
const D = 3;  // Danger
// ... etc
```

Then build the map programmatically using helper functions rather than typing out every single tile. This is critical for large maps — a 200×35 grid has 7000 tiles and manually specifying each one would be unreadable and error-prone.

Essential helpers:
- `fillRect(r1, c1, r2, c2, type)` — fill a rectangular region
- `hline(r, c1, c2, type)` — horizontal line of tiles
- `set(r, c, type)` — single tile

Build the map section by section with clear comments marking each area. The comments serve as documentation so the user can find and modify specific parts.

## Adapting to Game Genre

The core grid system works for any 2D game, but the spatial conventions change:

**Side-scrolling platformer:**
- Gravity goes down, so ground is at the bottom rows
- Platforms are horizontal lines with empty space below
- Vertical space = jump height, horizontal space = run distance
- Typical dimensions: 150-300 wide × 25-50 tall

**Top-down RPG / adventure:**
- No gravity — walls form room boundaries on all sides
- Rooms connected by corridors or doorways
- Interior space is walkable floor (empty tiles)
- Typical dimensions: 50-150 wide × 50-150 tall

**Dungeon crawler / roguelike:**
- Rooms are rectangles of empty space inside solid rock
- Corridors are 1-3 tile wide paths between rooms
- Doors/gates at corridor-room intersections
- Typical dimensions: 60-120 wide × 60-120 tall

**Puzzle game:**
- Tighter grids, smaller overall size
- More interactive elements (blue tiles), fewer enemies
- Clear start and goal positions
- Typical dimensions: 20-60 wide × 15-40 tall

Always ask yourself: what does the user's game genre need? Then adapt tile types, dimensions, and spatial conventions accordingly.

## Render Function Checklist

The `render()` function should draw layers in this order (back to front):

1. White background fill
2. Grid lines (faint, `#e0e0e0`, only when tile size ≥ 5px)
3. All non-empty tiles (filled rectangles in their assigned colors)
4. Section divider lines (dashed vertical/horizontal lines between major areas)
5. Section labels (gray text)
6. Annotations (colored text callouts)

## Section Dividers

Use dashed lines (`ctx.setLineDash([4, 4])`) in light gray (`#ccc`) to visually separate major sections of the level. These help the user understand the pacing and structure at a glance. Place them at the column boundaries between distinct gameplay areas.

## Solid Tiles Represent Real Game Geometry — Not Visual Decoration

**Never place solid (black) tiles purely for aesthetic purposes.** Every `S` tile in the map represents a real impassable block that will exist in the game. If you want to visually separate two sections or give a room a "corridor feel," use the dashed section divider lines — not `vline()` or `fillRect()` walls.

Specifically, do NOT do this:
```javascript
// WRONG — phantom walls that don't exist in the game
vline(155, 10, 29, S);  // "puzzle room wall" — looks wrong on the design
vline(245, 10, 29, S);
```

Do this instead:
```javascript
// CORRECT — dashed divider communicates section boundary without implying geometry
SECTION_DIVIDERS.push(150);  // or add to the array at the bottom
```

The designer is a blueprint that a developer follows. A solid black tile tells them "build a wall here." Only draw it if you actually want a wall there.

## Quality Checklist

Before finalizing the HTML file, verify:

- [ ] Every tile type in the map has a matching entry in the legend
- [ ] Section labels don't overlap with tiles (place them in empty space, usually near the top)
- [ ] Annotations are near what they describe and use matching colors
- [ ] The map tells a spatial story — you can trace the player's path from start to finish
- [ ] Ground/walls form a coherent, enclosed space (no accidental gaps unless intentional)
- [ ] No phantom walls — every solid tile represents geometry that will exist in the game
- [ ] Common collectables (cyan) and rare/special items (hot pink) are never mixed — if the game has both, both colors are used
- [ ] Zoom controls work and the default zoom shows the full map
- [ ] The file is completely self-contained (no external dependencies)
- [ ] All CSS is in a `<style>` tag, all JS is in a `<script>` tag

## When the User Gives Vague Directions

If the user says something like "design a level for my platformer" without specifying dimensions or layout details, make reasonable choices based on the genre and complexity they describe. Err on the side of more interesting layouts — add elevation changes, hidden paths, varied terrain. A flat corridor with evenly spaced enemies is boring; a level with vertical sections, shortcuts, and visual landmarks is memorable.

After generating the map, explain the layout in a brief summary so the user knows what they're looking at and can give targeted feedback.
