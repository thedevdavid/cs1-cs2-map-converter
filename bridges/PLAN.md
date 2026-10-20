# Plan: replacing the Dakota Falls bridges with CS2 bridge assets

Inputs: [cs1-bridges.md](cs1-bridges.md) (53 CS1 bridges, ranked; segment ids in `data/cs1-bridges.json`) and [cs2-bridge-assets.md](cs2-bridge-assets.md) (what this user has).
Nothing in the pipeline was changed. **[U]** = unverified (a name not yet confirmed in-game, or a mechanism not seen in code).

## Ground rules from the research

1. **A bridge look = a net prefab per edge.** CS2 has no per-segment bridge style. The converter swaps the edge prefab on the bridge's deck edges and keeps the base road/track on the approaches. Deck and approach meet at ordinary nodes: CS2 joins a bridge prefab to the road of the same lane layout, so no special transition piece is needed [U: confirm with a one-way 2-lane pilot].
2. **Bridge prefabs have hard geometry rules** (`Game.Prefabs.Bridge`, decompiled):
   - straight edges unless `m_CanCurve`;
   - each edge 0.6–1.4 × `m_SegmentLength`;
   - symmetrical edges and smooth slopes;
   - a minimum height over water (`m_ElevationOnWater`, 10 m for the road bridges);
   - for fixed-span bridges (`m_FixedSegments`), an exact sequence of spans whose towers sit at given nodes.

   The converter's current bridge output breaks most of these: it splits elevated courses longer than 75 m, smooths decks and follows CS1 curves. Bridge edges need their own re-segmentation.
3. **Heights double in CS2** (height factor 2; x and z unchanged). Spans keep their CS1 length. Decks stand twice as high, e.g. Detroit at about 124 m over the river. Tower bridges were modelled for 20–60 m decks, so how their pylon bases extend downward is [U] (`PillarObject.m_VerticalPillarOffsetRange`).
4. **Fixed-span bridges are not buildable through CS2MCP today.** It builds courses with `m_FixedIndex = -1`. Prefabs without fixed spans (all BXP "Truss Arch / Tied Arch / Extradosed / Suspension … Highway Oneway" items, seg 128–320) only need the right edge lengths.

## Mapping table

"seg" = `m_SegmentLength` read from the mod's prefab file; an edge must be 0.6–1.4 × seg. Lane counts: USRP nL Interstate → `Highway Oneway - n lanes` (current mapping.tsv).

| CS1 bridge (rank) | CS1 facts | first choice | fallback | reason / fit | converter must emit |
|---|---|---|---|---|---|
| **BR001 Detroit** river crossing (1) | twin 3L interstate, blue suspension, towers 723 m apart, deck 62 m over water (CS1), ~1.4 km of `B` per carriageway | `BXP SuspensionBridge03 - Six Lane Highway` (124160; 6-lane **two-way** highway; 4 fixed spans 168 + 320 + 320 + 168 m, towers 640 m apart) | per carriageway: vanilla `Suspension Bridge - Highway Oneway - 3 lanes` (lane-exact, no merge; span rules [U]); or `BXP Suspension Bridge - Highway Oneway - 6 Lanes` (seg 320, no fixed spans, but 6 lanes) | the closest landmark suspension; tower spacing 640 vs 723 m (−11 %); 3 + 3 lanes = two 3-lane carriageways | merge the two carriageways into one centreline over 976 m, centred on the CS1 towers' midpoint (1532, −3498), on the line through both tower positions; 4 straight edges with `fixedIndex` 0–3; Y-split nodes to the two `Highway Oneway - 3 lanes` at both ends; the rest of the `B` part and all `E` approaches stay plain elevated highway. Colour is the prefab's (blue not available [U]) |
| **BR022 + BR002-B** "vanilla asset bridge" below the dam (2) | twin 2L interstate, ~450 m per carriageway, deck 37 m (CS1), white multi-arch from 192 monorail segments | deck: plain elevated `Highway Oneway - 2 lanes`; arches: object pieces from **DIY Artdeco Bridge Kit** (136168: `OlympicBridge`, `4thStBridgeComplete`, `4thStBridgePillar`) placed under the deck | `BXP Arc Pedestrian Bridge Revised` (92245; curvable, seg 32, **no fixed spans**) along each CS1 monorail arch line as decor (replaces the current `Arc Pedestrian Bridge`, which has fixed spans and was dropped in Round 21) | no CS2 net reproduces open-spandrel white arches under a highway; the art-deco kit is white concrete arches. `BXP Art Deco Viaduct` would fit but needs San Francisco content (not installed) | object placements: one piece per CS1 arch span (from the monorail arch apexes and feet), y = deck − piece height, yaw along the deck; pieces need dimensions from `/prefabs/info` first. Then a Move It pass by hand |
| **BR005 Hernando-Desoto** (3) | twin 2L interstate, 305 m per carriageway, twin through-arch "M" | `BXP Truss Arch Bridge - Highway Oneway - 2 Lanes` (92245; seg 128 → edges 77–179 m; curves allowed; no fixed spans) per carriageway, **2 edges of ~150 m = two arches** | vanilla `Truss Arch Bridge - Highway Twoway - 2 Lanes` (two-way, needs a merge, 1 + 1 lanes) | lane-exact, one-way, two arches in a row give the "M" | swap the prefab on the river part of the 8 `B` segments, re-segmented into 2 straight edges per carriageway, deck kept level; drop the Hernando-Desoto building (already MANUAL). **Pilot bridge**: no fixed spans, no merge |
| **BR006 PO red deck arch** (4) | 2-lane national road, 564 m, 49 m above a dry valley, no pillars | deck: plain elevated `Small Road` with long spans (Pillar Spacing 162558) + arch object(s) from 135944 `StoneArchRailBridge` or 136168 `OlympicBridge` under the centre | vanilla `Truss Arch Bridge - Small Road - 2 Lanes` over the centre span (a through arch, not a deck arch; lane-exact) | CS2 has no deck-arch net; the PO arch itself is lost (its cubes do not load) | either object placement (first) or a prefab swap on the centre 1–2 edges (fallback); identification of this bridge as the captioned "PO BRIDGE" is an inference [U] |
| **BR009 green truss arch** (5) | twin 2L interstate, 190 m per carriageway over a gorge (76 m of water), PO `artdeco-truss01` ×4 | `BXP Truss Arch Bridge - Highway Oneway - 2 Lanes`, **one edge per carriageway** over the gorge (≤ 179 m; approaches plain) | plain deck + `PittTrussBridge` / `TrussBridgeHollow` (142603) objects | same prefab as BR005; colour cannot be green [U] | prefab swap + 1 straight edge per carriageway |
| **BR010 Pedrido** rail (7) | double track, 534 m, concrete bow-string arch (`B`, 74 m segments) + arched viaduct (`E`) over the river fork | `TrussArchBridge01` (vanilla Double Train Track Truss Arch, Quays & Piers) over the `B` part | `BXP Extradosed Bridge - Train - 2 Tracks` (seg 128, curves) | double track and an arch; `TrussArchBridge01` probably has 2 fixed spans of 128 m (as its BXP copies) [U] | prefab swap on the `B` edges; if fixed spans, needs CS2MCP `fixedIndex`; `E` part plain elevated `Double Train Track` |
| **BR007 Ourense** rail viaduct (8) | double track, 526 m, three large concrete parabolic arches | plain elevated `Double Train Track` + `StoneArchRailBridge` (135944) objects under the three main spans | `TrussArchBridge01` over the main span | no concrete deck-arch rail net exists | object placements (as BR022) |
| **BR003 PO-pillar viaduct** (9) | interstate + ramps, 603 m, 12 PO hammerhead piers | unchanged plain elevated highway | add `ITBP_Pillar Piers 01` (140978) objects at the 12 PO pier positions | look already close (concrete viaduct) | optional object layer from `po-offline.json` positions |
| **BR002 rest, BR008, BR014, BR016, BR021 … interchange viaducts and flyovers** (10, 12 …) | USRP `E`/`B` concrete viaducts | unchanged: plain elevated `Highway Oneway - n lanes` | `BXP Truss Viaduct - Highway Oneway - 3 lanes` for a few showcase flyovers (taste; Anarchy) | concrete viaducts are CS2's default look | nothing (Pillar Spacing optional) |
| **BR004 dam crest walkway** (11) | `Pedestrian Elevated`, 799 m on the PO dam | unchanged (`Pavement Path` elevated / dam deck) | – | – | nothing |
| **BR013, BR027 USRP `B` overpasses** | 3L/2L interstate over roads | unchanged | – | – | nothing |
| **R2USA `B0` rail bridges** BR011, 012, 015, 017, 028 | double track, 140–400 m, 7–23 m high, `nesque-pillar2` piers | unchanged: plain elevated `Double Train Track` | `BXP Truss Viaduct - Train` (seg 64, Anarchy) | CS1 look is a plain deck girder [U] | nothing |
| **R2 TRH through girder** BR023, 026, 033, 035, 038, 042 | single-span double track, 37–96 m, black through plate girder | `BXP Truss Viaduct - Train` (92245; seg 64 → 38–90 m: one edge per span; Anarchy) | plain track + `RailBridge1/2/3` objects (143426) | a through truss is the nearest CS2 look to a through girder | prefab swap, one straight edge per span; BR023/026 (96 m) need 2 edges |
| **Gravel-road bridges** BR019, 020, 044, 045, 049, 050, 053 | gravel road on wooden trestles, 63–172 m, low | vanilla `Wooden Covered Bridge - 2 lanes` (taste) | plain elevated road of the gravel mapping (whether CS2 `Gravel Road` can be elevated is [U]) | – | prefab swap |
| **National-road bridges** BR025, BR043 | `USRP 3-Lane National Road E`, `2-Lane … B` | unchanged plain elevated | – | – | nothing |

## What the converter must emit (net-converter)

1. **Bridge override table** `net-converter/bridges.tsv` (new input, not yet written): bridge id → CS1 segment ids (from `bridges/data/cs1-bridges.json`) → CS2 prefab, edge plan (`fixed:168,320,320,168`, `count:2`, `span:≤179`), and an optional `merge` (BR001). It overrides `mapping.tsv` only for those segments.
2. **Bridge re-segmentation** for override edges: replace the CS1 chain by straight edges of legal length (0.6–1.4 × seg, or the fixed lengths) between the chain's end nodes. Keep a constant grade (`SmoothSlopes`); the deck height is the CS1 deck ×2 at both ends. Skip the 75 m span split (`MaxElevatedSpan`), deck smoothing and chord pinning for these edges. Carry a `fixedIndex` per edge for fixed-span prefabs.
3. **Carriageway merge (BR001 only):** one centreline over the bridge's fixed length, plus two split nodes per end where the two one-way approaches meet the two-way bridge. The split must lie on the deck at least one approach-edge length from the tower spans. Check clearance over what passes under BR001 (the R2USA main line, the ship path, a 2L interstate).
4. **Object layer** `import-bridge-objects.json`: kit pieces for BR022, BR006, BR007 and optionally ITBP piers for BR003, with absolute y and yaw. Pitch is not supported by `/import/objects`, so sloped pieces need Move It by hand.
5. **Report:** `bridges` section in `stats.md`, listing per bridge the prefab, edge count and lengths, and any rule violation (length, curvature, height over water).

## What CS2MCP must support that it does not yet

1. **`/prefabs/info` bridge data:** report the `Bridge` component (`m_SegmentLength`, `m_CanCurve`, `m_AllowMinimalLength`, `m_ElevationOnWater`, `m_BuildStyle`, fixed span lengths) and the prefab class. This is needed to confirm the vanilla names and rules, which cannot be read offline.
2. **Fixed spans:** an edge field `fixedIndex` written to the course (`NetCourse` fixed index, as the net tool sets it for `FixedNetElement` prefabs). Needed for BR001 (SuspensionBridge03), probably BR010 (`TrussArchBridge01`), and to bring back the dropped `Arc Pedestrian Bridge` and the dam deck. How the net tool sets the index is [U]; read `NetToolSystem` before coding.
3. **Pre-validation of bridge edges:** length range, straightness and height over water, with errors that name the rule, before the game rejects or silently merges the edge.
4. **`noPillars` per edge** (already written by the converter, ignored by the bridge): delete or suppress the generic `Game.Objects.Pillar` sub-objects of chosen elevated edges (BR006, BR001 approaches, BR005). Whether the game regenerates them on the next update is [U]. Pillar Spacing (162558) is the global alternative.
5. Nothing new for objects: `/import/objects` with `y` and `rotation` exists (0.9.19+).

## Mods to add to the DakotaFalls-E2C playset

The active playset has 24 mods and **no bridge mod**. Everything below except 162558 is already downloaded (lessgo playsets).

| id | mod | why | required? |
|---|---|---|---|
| 92245 | Bridge Expansion Pack | truss arch / truss viaduct / Arc pedestrian decor (BR005, BR009, BR022 fallback, rail girders) | yes. Sep 2026 comments say it broke after the last game update; test first [U] |
| 124160 | Bridge Expansion Pack: B&P | `BXP SuspensionBridge03 - Six Lane Highway` (BR001) | yes (needs Bridges & Ports, installed) |
| 136168 | DIY Artdeco Bridge Kit / LA River Bridges | white arches (BR022, BR006) | yes if the object route is chosen |
| 135944 | DIY Stone Arch Rail/Road Bridge Kit | Ourense arches (BR007), BR006 option | taste |
| 142603 | Generic Arched Truss Bridge | BR009 fallback | optional |
| 143426 | Classic Rail Truss Bridge Kit | TRH girder fallback | optional |
| 140978 | ITBP pillars | BR003 piers | optional |
| 74324, 77240 | Move It, Find It | placing kit pieces by hand | yes for any object route |
| 162558 | Pillar Spacing | long open spans (BR006, Detroit approaches) | optional; **not downloaded** |
| 160320 | Bridge Builder | only if a needed combination is missing (e.g. a one-way 3-lane truss arch) | optional |

Not needed: Functioning Swing/Draw Bridge (153832), Clark Street Bascule (155250), and Art Deco Walls, Tunnels and Bridges (157964). Dakota Falls has no movable bridge, and 157964 duplicates 136168. EAI is not required by any bridge pack.

## Open questions for the owner (taste)

1. **Detroit:** accept a two-way 6-lane suspension with 640 m tower spacing in the B&P paint colour instead of the blue twin-carriageway look? Or two one-way vanilla 3-lane suspensions side by side?
2. **White arch below the dam:** object kit pieces placed by hand (most faithful, about a day of Move It work), or the automatic `BXP Arc Pedestrian Bridge Revised` decor arches?
3. **Red deck arch (BR006) and Ourense (BR007):** object arches under a plain deck, or a CS2 through arch (truss arch) that reads clearly as "arch bridge" but is a different type?
4. **Rail girders:** keep CS2's plain elevated track, or use BXP truss viaducts (black/grey truss, needs Anarchy) on the TRH spans and the R2USA bridges?
5. **Gravel-road bridges:** wooden covered bridge, or plain?
6. **x2 heights:** is a 124 m-high Detroit deck acceptable? Otherwise the deck could be lowered locally (terrain or deck), which departs from CS1.

## Ordered implementation list

| # | step | where | effort |
|---|---|---|---|
| 1 | Owner answers the 6 questions above | owner | – |
| 2 | Add 92245, 124160 (+ 136168, Move It, Find It as chosen) to DakotaFalls-E2C; start the editor; `cs2_find_prefabs category=net query=Bridge` and `query=BXP`; confirm every name in this plan, check that BXP works on the current game build, and check the `TrussArchBridge02` name clash (do not enable 155730) | game, owner | 0.5 d |
| 3 | CS2MCP: bridge data in `/prefabs/info`; read all vanilla bridge rules into `bridges/cs2-bridge-rules.json` | cs2mcp | 0.5 d |
| 4 | Converter: `bridges.tsv` + re-segmentation for non-fixed bridge prefabs; pilot **BR005 Hernando-Desoto** in-game (two arches per carriageway), then BR009 | net-converter, game | 1.5 d |
| 5 | Converter: TRH girder spans → `BXP Truss Viaduct - Train` (if chosen) | net-converter | 0.5 d |
| 6 | CS2MCP: `fixedIndex` on courses; test on one `BXP SuspensionBridge03` over open water, then the `Arc Pedestrian Bridge` and the dam deck | cs2mcp, game | 1–2 d |
| 7 | Converter: BR001 carriageway merge + fixed-span suspension; in-game check of the split nodes, clearance and tower heights at x2 | net-converter, game | 1.5 d |
| 8 | Converter: object layer for kit arches (BR022, BR006, BR007) after measuring piece sizes with `/prefabs/info`; import, then a Move It pass | net-converter, owner | 1 d + 1 d by hand |
| 9 | Optional: `noPillars` support in CS2MCP or Pillar Spacing; ITBP piers for BR003 | cs2mcp | 1 d |
| 10 | Rail arches BR010/BR007 per answers (needs step 6 if `TrussArchBridge01` has fixed spans) | net-converter | 0.5 d |

About 7–9 working days in all, plus owner hand placement. Steps 4–5 need no new CS2MCP features and can ship first.
