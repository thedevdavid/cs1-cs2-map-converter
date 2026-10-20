# Plan: Procedural Object structures (bridge pieces, tunnel blocks, paved blocks)

Scope: the 185 PO objects in `cs1-offline/out/po-offline.json` with class `structure`:
- 54 `bridge/pillar`
- 46 `tunnel/shed`
- 85 `paved block`

Companion to [PLAN.md](PLAN.md) (bridges as nets). Read-only research; no pipeline file changed. **[V]** = seen in a file, **[U]** = unverified.
Per-object data: `bridges/data/po-structures-proposal.json`, made by `bridges/data/po-structures.js` (Node). Coordinates: CS1 metres. x and z are the same in CS2; y doubles in the x2 set (`y2` = 2 × bottom of the PO mesh bounds).

## What the pieces really are

All three classes are mostly **bridge structure**, built by the author from re-purposed meshes. The class names come from the base prefab, not from the role. I checked each group against the dump: nearby segments, CS1 tunnel portal nodes, group roots and PO colour.

| group | objects | real role | evidence |
|---|---:|---|---|
| `Detroit Bridge` meshes (16 × 32–48 × 15 m) | 38 | **piers** of the BR001 Detroit approach viaducts: two rows of 14 at x 1976…2304 (one per carriageway), 10 at x 846…1080 | on the `USRP 3L Interstate E` lines; Network Skins removed CS1 pillars there (cs1-bridges.md) |
| `[US] 3-Lane Highway Pillar` | 12 | hammerhead **piers** of the BR003 interchange viaduct | double row under `USRP 2L Interstate B` |
| `artdeco-truss01` (Hyperion's Gate PO pack) | 4 | the **green truss arch** of BR009 (four stacked copies of one 100 × 19 × 69 m piece) | same position ±12 m, same yaw |
| `R69 Tunnel Entrance 1`, red tint (0.90/0.59/0.59) | 2 (27, 274) | the **red deck arch** of BR006, 190 m long, 62 m high | under `USRP 2-Lane National Road B` 10906/15108 at 8 m; confirms the BR006 inference in cs1-bridges.md |
| `R69 Tunnel Entrance 1`, blue tint (0.45/0.88/0.91) | 2 (502, 503) | a **blue arch** under the BR001 south approach (1008, −4190), 190 m | 8 m from `USRP 3L Interstate E` 14574/35648; not listed in cs1-bridges.md before |
| `R69 Tunnel Entrance Block 1/2`, white | 20 (ids 1–20) | the **dam body and crest** (BR004 walkway runs on top) | x 960…1439, z 4623…4725, the dam line |
| `R69 Tunnel Entrance Block 1` | 3 (619–621) | a 20 m **frame on two 17 m posts over the 2L interstate** at (608, −4721): a sign gantry or portal frame [U] | beam 6 m above the deck of `USRP 2L Interstate E` 21099 |
| `R69 Tunnel Entrance Block 2`, Sidewalk-007 texture | 8 (pairs) | **abutment blocks** at the ends of four short interstate viaducts (BR018, BR029, BR031, BR037). Two of these viaducts start 60–80 m from CS1 tunnel mouths (nodes 19082/12983 and 3614/23990) | each pair sits 7–10 m from both carriageways of a `USRP 2L Interstate E` piece |
| `rock_c` (2077702761) | 11 | the **JP Rock shed**: a 430 m rock-faced gallery along the `USRP 4-Lane National Road` at x −6453…−6245, z 677…1069, beside the gorge falls | pieces 13–17 m from the road axis, y 236–258 around a road at y 251; `audit/web.md` img 22 caption |
| `Cube`, group 27 (Cobble-003) | 68 | **BR006**: 34 thin spandrel columns (3 × 18–31 × 3 m, heights falling toward the crown) and 34 piers (8–16 × 39 × 9–14 m) along the viaduct | same group root as the red arch (27) |
| `Cube`, group 25 | 3 | **BR022** arch footing pads (35 × 8 × 38 m at y 139, river level) | under the monorail arch bridge |
| `Cube`, group 288 | 6 | **BR005 Hernando-Desoto** bearings: 2 caissons 44 × 17–21 × 43 m and 4 columns 7 × 23–27 × 7 m | under the arch |
| other `Cube` | 8 | abutment / end blocks of BR009, BR025, BR044 and one interchange | next to bridge ends |

**None of the 85 paved blocks loads** in the current CS1 playset; they are decoded offline. **There are no real road tunnel portals among the PO objects.** The 46 "tunnel/shed" objects are the dam, an arch pair, a gantry, abutments and the rock shed.

## (c) Paved blocks: the current handling needs a correction

The converter (Round 9, `Round9.LoadPavedBlocks` / `PavedStructures`) turns every paved block into a pavement polygon on its top, plus `QuaySmall01` edges along the long sides of blocks taller than 4 m [V: `net-converter/src/NetConverter/Round9.cs`]. That fits the waterfront blocks, but **77 of the 85 are bridge parts**:
- **BR006 (68):** paints cobble pavement in a dry valley and raises quay walls 39 m tall around the piers' footprints.
- **BR022 pads (3):** acceptable as is.
- **BR005 (6):** pavement and quays inside the river under the bridge.

Recommendation: exclude group roots 27 and 288 from `PavedStructures`, keep group 25 and the 8 end blocks as they are, and handle BR006 and BR005 as bridges (PLAN.md and the table below).

## CS2 options per group

### What CS2 does on its own [V: decompiled Game.dll; PLAN.md]
- Roads and tracks that go underground get their **tunnel portal automatically** at the slope-to-tunnel transition. There is no selectable portal style and no standalone vanilla portal object. Vanilla Locale.cok has no portal, retaining-wall or rock-shed object names; only barriers, `Sound Barrier` and the camera portals of traffic cameras.
- Roads 4–8 m above ground get **retaining walls** automatically (1–2 × the elevation limit). Higher roads become elevated with generic pillars. Quays (`QuaySmall01` …) are walls with a path or road on top. CS2MCP's `wall` edge option forces a wall on raised sides [V: IMPORT_SCHEMA.md].
- So for the PO "portals": **leave the tunnel entries to the game.** None of the 46 objects is a portal at a CS1 tunnel mouth. The nearest, the abutment pairs 262/263 and 264/265, stand 60–80 m from the mouths at the far ends of short viaducts. CS2 builds its own portal at each mouth and its own abutment where the viaduct lands.

### Object kits available locally [V: prefab names read from the `.cok` packs in the PDX mod cache]

| mod (id) | playsets | pieces relevant here |
|---|---|---|
| **ConcreteSet01** (154132) | lessgo only | `TunnelEntrance18`, `TunnelEntrance18Deep32`, `TunnelEntrance18Deep64` (concrete portal faces), `Wall16x16`, `Wall32x16`, `Wall32x32`, `Wall62x32` (concrete walls), `Pillar1x8`, `Pillar1x16`, `Pillar2x8`, `Pillar2x16`, `Pillar4x8`, `Pillar4x16`, `BasePlateConcrete16/32/62`, stairs and ramps. Names suggest sizes in metres; real sizes [U] until `/prefabs/info` |
| **DIY German Bridges, Viaducts & Walls** (136487) | lessgo + clone | `GermWallTunNarrow`, `GermWallTunWide`, `GermWallTunWideMid` (portal walls), `GermWallArch`, `GermWallFlat`, `GermWallAlt`, `GermWallTop`, `GermWallPillar`, `GermWallOutcrop`, `GermanBridgeSpan(Short/Long)`, `GermanRailPillar` |
| **Island-Type Bridge Pier** (140978) | lessgo + clone | `ITBP_Pillar Piers 01`, `ITBP_Pillar Median 01…04`, `ITBP_Single-Sided Beam 01…05` |
| **DIY Artdeco Bridge Kit** (136168) | lessgo + clone | `OlympicBridge`, `4thStBridgePillar`, `4thStBridgePillarDeck` |
| **DIY Stone Arch kit** (135944) | lessgo + clone | `StoneArchRailBridge` |
| **Generic Arched Truss Bridge** (142603) | lessgo only | `PittTrussBridge`, `TrussBridgeHollow` |
| **Classic Rail Truss kit** (143426) | lessgo + clone | `RailBridge1`, `RailBridge2`, `RailBridge3`, `RailBridgePillar` |
| **Truss Gantry Prop Pack** (132188) | lessgo + clone | `HWY Truss Gantry 1L…7L  A`, `RAMP Truss Gantry …`, `ROAD Truss Gantry A–D`, `HWY Truss Mast B` (note the double spaces in some names) |
| **Monotube Gantry Pack** (127756) | lessgo + clone | `NA_Gantry…Arm…` monotube gantries |
| **Cliffs and Rocks Pack** (137334) | **in DakotaFalls-E2C** | `CliffSideTall`, `CliffFreeStanding`, `CliffSlim` (bounds in cs2mcp/PREFAB_NAMES.md) |
| **Tigons Rail Infrastructure Pack** (133250) | lessgo + clone | nets `Stone Pillar Bridge - Train`, `Truss Arch Bridge - Train`, `Tied Arch Bridge 4 Track - Train` (+ `Plain`). **New for PLAN.md:** a stone-pillar rail viaduct net for BR007 Ourense / BR010 Pedrido approaches |
| CS KR Stream Park (142259) | lessgo + clone | `CSKRStoneWall8m`, `CSKRStoneWallBridge24m` (small stone walls) |
| Culverts 1 (156959) | lessgo only | `MacLukCulvert001…007` (small culvert mouths) |
| Art Deco Walls, Tunnels and Bridges Set (157964) | **not downloaded**, in no playset | WEB_157964 |

No local mod has a rock shed or avalanche gallery. WEB_OTHER

### How the pipeline would place them
- **Objects:** `/import/objects` with `prefab`, `x`, `z`, `y` (absolute; a y above the terrain becomes the object's elevation) and `rotation` (yaw) [V: IMPORT_SCHEMA.md]. There is no pitch or roll, so pieces under sloped decks need a Move It pass. Where a kit prefab's pivot sits (base or centre) is [U]; measure bounds with `/prefabs/info` and set `y = y2 − bounds.min.y`. The game validates overlaps, so pieces touching decks or water need Anarchy (74604, already in E2C) or `bypassValidation`.
- **Nets:** BR009's green arch and BR001's blue arch are better done as bridge-prefab swaps on the deck edges (PLAN.md rules: straight edges, 0.6–1.4 × segment length).
- **Pillar suppression:** the pier objects only make sense if the CS2 deck drops its generic pillars on those edges. Use `noPillars` (CS2MCP feature, PLAN.md step 9) or Pillar Spacing (162558). Without it, skip the piers and leave CS2's own.

## Per-object mapping proposal

One row per run of objects with the same proposal; every object id is listed. "y bottom" is the PO mesh's lowest point. Sizes are the PO mesh bounds (x × y × z, m). Exact per-object x, z, yaw and y for every id are in `bridges/data/po-structures-proposal.json`.

| PO ids | CS1 piece | role | proposed CS2 | mod | x, z (CS1 = CS2) | yaw | y bottom CS1 → x2 | size m |
|---|---|---|---|---|---|---:|---|---|
| 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 | R69 Tunnel Entrance Block 2_Data ×20 | dam body / crest (R69 blocks) | no equivalent needed: the hand-placed HydroelectricPowerPlant01 + dam deck replace it |  | 960…1439, 4623…4725 | 171.2 | 129…169 → ×2 | 134×8×36 |
| 260, 261, 240, 242, 262, 263, 264, 265 | R69 Tunnel Entrance Block 2_Data ×8 | abutment block at a short interstate viaduct next to a tunnel mouth | leave to the game (CS2 builds its own abutment walls); optional Wall32x16 | 154132 | -7215…-4984, 175…1841 | 5.1 | 174…295 → ×2 | 39×18×46 |
| 22, 23, 24, 215, 216, 217, 218, 219, 220, 221, 222 | rock_c_Data ×11 | JP Rock shed piece over the 4-lane national road | no equivalent (no CS2 rock shed found); optional CliffSideTall overhang | 137334 (in E2C) | -6453…-6245, 677…1069 | 144.6 | 235…257 → ×2 | 58×23×60 |
| 278, 289, 291, 293, 286, 294 | Cube_Data ×6 | BR005 Hernando-Desoto bearing / column | no object (the truss-arch net brings its own supports) |  | 287…578, 145…248 | 344.5 | 115…119 → ×2 | 7×23×7 |
| 266, 25, 26 | Cube_Data ×3 | BR022 arch footing pad | BasePlateConcrete32 (dimensions [U]) | 154132 | 950…1343, 4075…4277 | 242.8 | 139…139 → ×2 | 35×8×38 |
| 27 | R69 Tunnel Entrance 1_Data | BR006 red deck arch | StoneArchRailBridge or OlympicBridge (owner choice, PLAN.md Q3) | 135944 / 136168 | -3534.6, -3180.2 | 324 | 199.3 → 398.6 | 139×62×190 |
| 268, 269, 270, 271, 273 | Cube_Data ×5 | BR006 viaduct pier | Pillar4x16 (dimensions [U]) | 154132 | -3587…-3359, -3409…-3100 | 324 | 228…240 → ×2 | 8×39×9 |
| 274 | R69 Tunnel Entrance 1_Data | BR006 red arch (twin of 27) | no object (covered by 27) |  | -3522.4, -3171.3 | 324 | 199.3 → 398.6 | 139×62×190 |
| 275 | Cube_Data | BR006 viaduct pier | Pillar4x16 (dimensions [U]) | 154132 | -3481.9, -3252.9 | 324 | 235.1 → 470.2 | 8×39×9 |
| 276, 277, 285, 290, 295, 297, 299, 301, 304, 415, 418, 420, 422, 429, 430, 432, 434, 272, 284, 287, 292, 296, 298, 300, 302, 303, 349, 417, 419, 421, 431, 433, 435, 436 | Cube_Data ×34 | BR006 spandrel column | Pillar1x16 (dimensions [U]) | 154132 | -3582…-3475, -3246…-3107 | 324 | 244…258 → ×2 | 3×30×3 |
| 437, 439, 440, 441, 442, 443, 444, 445, 446, 447, 448, 449, 450, 452, 451, 438, 453, 454, 455, 456, 457, 458, 459, 460, 461, 462, 463, 464 | Cube_Data ×28 | BR006 viaduct pier | Pillar4x16 (dimensions [U]) | 154132 | -3696…-3367, -3398…-2945 | 324 | 228…244 → ×2 | 8×39×9 |
| 465, 467, 469, 471, 473, 475, 477, 479, 481, 483, 485, 487, 489, 491, 267, 466, 468, 470, 472, 474, 476, 478, 480, 482, 484, 486, 488, 490, 492, 493, 494, 495, 496, 497, 498, 499, 500, 501 | Detroit Bridge_Data ×38 | BR001 approach pier | ITBP_Pillar Piers 01 | 140978 | 846…2304, -4400…-2485 | 37.2 | 90…109 → ×2 | 16×32×15 |
| 503 | R69 Tunnel Entrance 1_Data | BR001 blue arch under the 3L approach | BXP Truss Arch Bridge - Highway Oneway - 3 Lanes (net on that approach edge); object fallback PittTrussBridge | 92245 / 142603 | 1017.1, -4197.6 | 37.2 | 83.5 → 167.1 | 146×57×190 |
| 502 | R69 Tunnel Entrance 1_Data | BR001 blue arch (twin of 503) | no object (covered by 503) |  | 998.2, -4183.2 | 37.2 | 83.5 → 167.1 | 146×57×190 |
| 504, 505 | Cube_Data ×2 | bridge abutment / end block | keep current handling (pavement top) or leave to the game |  | -4157…-4115, 51…121 | 301 | 137…137 → ×2 | 27×17×31 |
| 507, 506, 508, 509, 510, 511, 512, 513, 514, 515, 516, 517 | [US] 3-Lane Highway Pillar_Data ×12 | BR003 hammerhead pier | ITBP_Pillar Piers 01 | 140978 | -1651…-1520, -1696…-1543 | 40 | 95…95 → ×2 | 16×30×15 |
| 620 | R69 Tunnel Entrance Block 1_Data | sign-gantry-like frame over the 2L interstate | HWY Truss Gantry 2L  A | 132188 | 608.3, -4721.4 | 0 | 124.7 → 249.4 | 20×3×3 |
| 619, 621 | R69 Tunnel Entrance Block 1_Data ×2 | gantry post (part of 620) | no object (covered by 620) |  | 598…618, -4721…-4721 | 0 | 111…111 → ×2 | 3×17×3 |
| 646, 647 | Cube_Data ×2 | bridge abutment / end block | keep current handling (pavement top) or leave to the game |  | 3453…3696, 2036…2187 | 139.5 | 159…174 → ×2 | 51×20×61 |
| 690 | artdeco-truss01_Data | BR009 green truss arch | BXP Truss Arch Bridge - Highway Oneway - 2 Lanes (net, per PLAN.md); object fallback PittTrussBridge | 92245 / 142603 | -4490.7, 71.9 | 303.9 | 153.7 → 307.3 | 100×19×69 |
| 281, 691, 692 | artdeco-truss01_Data ×3 | BR009 truss (stacked copy of 690) | no object (covered by 690) |  | -4485…-4476, 81…94 | 303.9 | 154…154 → ×2 | 100×19×69 |
| 693, 694, 695, 696 | Cube_Data ×4 | bridge abutment / end block | keep current handling (pavement top) or leave to the game |  | -4545…3806, 36…2924 | 303.9 | 151…182 → ×2 | 62×16×55 |

Totals:
- **With a proposed object:** 123 CS2 objects, one per PO object: BR006 68 columns and piers plus 1 arch (the arch twin 274 is dropped), BR001 38 piers, BR003 12 piers, 1 gantry (its posts fold into it), 3 BR022 pads. BR006 counts only if the object route is chosen for it (PLAN.md Q3), and the piers only with `noPillars`.
- **Net swap instead of objects:** the green arch (4 objects) and the blue arch (2).
- **No equivalent needed / leave to the game:** 20 dam blocks, 8 abutment blocks, 6 Hernando-Desoto bearings, 8 end blocks (42), plus 7 objects covered by another row (arch twins 274 and 502, truss copies 281/691/692, gantry posts 619/621).
- **No equivalent:** the 11 rock-shed pieces (optional cliff overhang).

## Recommendations

1. **Tunnel portals:** leave all tunnel entries to the game. None of the PO objects is a portal at a CS1 tunnel mouth. Optionally put `TunnelEntrance18` (154132) faces at the two interstate tunnel mouths (nodes 19082/12983 at (−5339, 168…183) and 3614/23990 at (−5076, 173…189)) if the vanilla portal looks too plain. That is a taste choice, not a port of PO data.
2. **Rock shed (11):** no CS2 equivalent. Either (a) leave the national road open (simplest), or (b) place `CliffSideTall` / `CliffFreeStanding` (137334, already in E2C) on the hill side at the 11 positions, yaw + 90° toward the road, as an overhang look with Anarchy. A true gallery (road under a roof) would need the road built as a shallow tunnel, and CS2 tunnels need terrain above them, so no.
3. **Piers (BR001 38, BR003 12):** worth it only together with `noPillars`. Use `ITBP_Pillar Piers 01` (140978) at the PO positions with the PO yaw (37.2° and 40°). Otherwise drop them; CS2's pillars stand roughly where CS1's did.
4. **BR006 (red deck arch, 68 cubes + 2 arch meshes):** one arch object (`StoneArchRailBridge` or `OlympicBridge`, owner choice) at the arch centre (−3528, −3176), yaw 324, plus `Pillar1x16` columns and `Pillar4x16` piers at the 68 cube positions. All of it must come out of the pavement/quay path.
5. **Green and blue arches:** net swaps (`BXP Truss Arch Bridge - Highway Oneway - 2 Lanes` on BR009, `… - 3 Lanes` on the BR001 south approach at (1008, −4190)). Add the blue arch to PLAN.md as BR001-S.
6. **Gantry (619–621):** `HWY Truss Gantry 2L  A` (132188) at (608, −4721), y = deck. Optional; it is probably just a sign gantry.
7. **Dam blocks (20):** nothing; the CS2 hydro dam replaces them.

## Mods to add to DakotaFalls-E2C for this part

| id | mod | for | state |
|---|---|---|---|
| 154132 | ConcreteSet01 | BR006 columns/piers, BR022 pads, optional portal faces and walls | downloaded (lessgo) |
| 140978 | Island-Type Bridge Pier | BR001/BR003 piers (only with noPillars) | downloaded |
| 135944 or 136168 | Stone Arch kit / DIY Artdeco kit | BR006 arch (already in PLAN.md) | downloaded |
| 132188 | Truss Gantry Prop Pack | optional gantry | downloaded |
| 74324, 77240 | Move It, Find It | sloped pieces by hand | downloaded |
| 137334 | Cliffs and Rocks Pack | optional rock-shed overhang | **already in E2C** |
| 133250 | Tigons Rail Infrastructure | optional `Stone Pillar Bridge - Train` for the rail viaducts (PLAN.md) | downloaded |
| 157964 | Art Deco Walls, Tunnels and Bridges | WEB_157964_ADD | not downloaded |

## Converter and CS2MCP work

| # | step | effort |
|---|---|---|
| 1 | Measure every kit prefab above with `/prefabs/info` (bounds, pivot) once they are in the playset | 0.5 h in-game |
| 2 | Converter: exclude PO group roots 27 and 288 from `PavedStructures`; add an `import-po-structure-objects.json` writer driven by `bridges/data/po-structures-proposal.json` (+ owner choices), y = y2 − bounds.min.y | 0.5 d |
| 3 | Pier objects only after `noPillars` works (PLAN.md step 9) | – |
| 4 | Move It pass for BR006 and the arch pieces | 0.5 d by hand |
