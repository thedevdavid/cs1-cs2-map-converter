# Pre-export audit: consolidated findings (2026-10-19)

Sources: `audit/web.md` (Workshop and web), `audit/local.md` (packages, saves, dependencies), `audit/theme.md`, `audit/export-coverage.md` (exporter vs CS1 data), `audit/cs2-side.md` (import plan vs CS2). Five Opus 5.5 sessions, read-only.

## What changes before the export

1. **Procedural Objects are the map's signature features, and 86 of 690 do not load in CS1.** Mountains above the 1024 m ceiling, the Flatirons, the falls steps, the dam and two bridges are PO meshes. v0.3.0/v0.4.0 export placement and (v0.4.0) shapes, but only of objects that load; 86 cube-based objects reference `1094334744.Cube_Data` from the original PO item and fail with the successor mod. **Decision: read all 690 offline from the `.crp`** (proven by `audit/tools/po-summary.cs`), producing world-space meshes and a max-height raster; the in-game export stays as a cross-check. Do **not** subscribe to the original PO item as an asset source (two PO dlls share an assembly name; untested). **Never save the Workshop map from the CS1 editor**: the autosave already dropped the 86 objects.
2. **Source of truth confirmed:** the 2026-10-18 dump came from the Workshop map loaded in the map editor. The user's saves `dktanew` and `dktareference` are city games with less map content; export from the Workshop map again.
3. **Exporter v0.4.1 (last):** bugs B1–B6 from `export-coverage.md` §3; tree y/flags/scale/id in the tree sibling (≈70 600 scaled trees); Network Skins per segment (pillar/light overrides on the USRP roads); RawHeights/RawHeights2/BlockHeights PNGs; skipped buildings flagged + `NetNode.m_building` (separates generated from hand-placed pillars); raw custom-data dump as insurance; sand/shore layers; failed PO containers written regardless. TM:PE, Adaptive Networks, Node Controller, Theme Mixer, ACME data: skipped (low CS2 value; the `.crp` keeps them).
4. **CS1 playset is ready:** Procedural Objects 3071492847, Effects Module, Tree Control, theme mix, Surface Painter enabled; Tree Anarchy absent. Prop Painter and Surface Painter hold no data in the Workshop map, so the `terrainSurfaces` section will be null (expected).

## What changes before the CS2 run

5. **Trees:** tree `age` is ignored in the editor (≈5 % would spawn dead) → bridge makes it deterministic. ≈10 k trees stand in water and ≈3.6 k on nets → converter masks them. 508 k trees is within reach for the editor; save before the tree import (undo is in memory only).
6. **Ship connection** sits ≈1 km from the seaway's border end → moved to (4718.4, −7168). Whether a seaway border node spawns its own marker is a game-time check.
7. **Walls layer:** 21 flat quay edges need a forced wall → bridge edge option (`Upgraded` side flags, `Side.Raised`); canals as `Medium Boatway` only where water exists; flood walls as `Sound Barrier` are a stand-in; the 7 `QuayMedium01` may be roads.
8. **Water sources:** `natural` is the right type; drop 8, clamp 3 inward; the lower river is the sea-level plane.
9. **Height Scale 4096 (run B):** about 70 steep edges and 176 high bridge nodes cannot be re-edited in game without errors; slope and height rules are absolute. Both runs stay, per the owner; B is the riskier one.
10. **Surfaces:** union overlapping polygons per prefab (276 decal pairs, 22 field pairs); Agriculture Surface needs no industry area.
11. **Theme:** no single CS1 theme; a Theme Mixer blend of 7 themes plus the Relight2Natural LUT. CS2: North American theme, latitude 40.0 / longitude −105.3, **both climates to be tried** (Colorado with snow vs a milder no-snow one), surfaces from the texture packs, automatic rock pass (151 polygons at 40°, judge in game).
12. **Monorail segments (192)** are the arches of the vanilla-asset bridge below the dam → decorative layer, not dropped. Centre-pivot fields: check the 67 polygons are circles. Rail connections: 4 in the dump vs 3 described → one to look at.
13. **Lost for good unless hand-built:** IMT markings (chevrons, guardrails: offline decode to GeoJSON is being produced for the barrier lines), Effects Module foam (positions exported in v0.4.0 if readable), Castle Wall fort, Prop Painter tints (none in the map anyway).

## Work in flight from this audit

| Piece | Session | Output |
|---|---|---|
| Offline PO + IMT extraction | audit-local | `cs1-offline/out/po-offline.json`, `po-offline-heights.png`, `po-classes.md`, `imt-markings.geojson` |
| Exporter v0.4.1 | exporter | installed dll, SCHEMA.md |
| PO height stamping tool | hm-rebuild | `hm-rebuild stamp`, new time-prefixed playable |
| Bridge 0.9.10 | spike | tree age, prop alias, forced quay walls, seaway checks |
| Converter round 7 | converter | tree mask, ship/rail fixes, walls, water rule, surface unions, monorail decor, fields check |

## Export gate

Run the CS1 export when: exporter v0.4.1 is installed **and** the offline PO extraction has reported (it does not depend on the export, but its result tells us whether the in-game PO section matters). Load the Workshop map in the map editor, dismiss PO's loading-failure popup without deleting, export, do not save.
