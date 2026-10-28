# Agent prompt: CS2 road-marking mod

---

Build a Cities: Skylines II mod that draws custom road markings, in the spirit of CS1's Intersection Marking Tool, but grown from the simplest thing that works. New repo at `D:\work\mods\cs\cs2-road-markings`. Load the skills `reverse-engineer-anything` and `cs2-modding:cs2-modding-setup` / `cs2-modding:cs2-mod-project` / `cs2-modding:cs2-modding` / `cs2-modding:cs2-modding-ui` before anything else. The owner starts and closes the game; ask for a restart when a build needs one.

## Step 1 — REA the game's own markings (no mod code yet)

Open `Game.dll` (CS2 1.6.x, `…\Cities Skylines II\Cities2_Data\Managed\`) with the REA tools and answer, with decompiled evidence:

- Where lane lines, crosswalks, stop lines and arrows come from: the net lane / lane-marking prefabs and components (`Game.Net`, `Game.Prefabs` lane and decal types), the systems that generate their geometry, and the rendering path (`Game.Rendering`, batching, materials) that puts them on the road surface.
- How decal objects (`DecalPrefab`/`Decal`-type `ObjectPrefab`s) are placed, rendered, and saved, and whether a decal can be stretched or repeated along a curve.
- How area/surface entities (`Game.Areas`) are triangulated and rendered on terrain.
- What is serialised for each of the three, so anything we create survives save and load.

Write `docs/REA.md`: one page per mechanism with the type names, the system order, and the smallest set of components an entity needs. End with a ranked list of routes for "a 10 cm white line along an arbitrary curve": easiest first.

## Step 2 — one line on the road

Take the easiest route from Step 1. A mod with a single debug command (chat/console or a hotkey) that draws one solid white line between two points you hard-code on any road, visible in the map editor and in game, surviving save and load. Screenshot as proof. Then the same line dashed, and in a colour. Stop and report.

## Step 3 — a marking tool

Only after Step 2 works: a tool mode that lets the player click two points on a road and get a line, with width, colour and dash pattern settings; delete by clicking the line. Keep the data model tiny: a list of polylines with a style. UI through the game's own tool panel conventions (see the cs2-modding-ui skill).

## Step 4 — grow toward IMT

Add in this order, each only when the previous one is solid and the owner says go: lines snapped to lane edges at a junction (IMT's "points" idea), fillers (chevron, stripe, solid polygon), guardrails along a curve (fence-style props or a net lane object, whichever Step 1 showed), then a JSON import/export of the whole marking set.

## Rules

- Generic mod, no map-specific code or data. Open source.
- Each step ends with a report under 30 lines: what works, what does not, evidence, the next decision. Do not move to the next step without the owner's go.
- No desktop automation; the owner drives the game UI.
