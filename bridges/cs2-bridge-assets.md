# CS2 bridge assets available for the Dakota Falls port

Researched 2026-10-20. Markers: **[V: source]** = seen in that file or page; **[U]** = unverified (inferred, from memory, or not readable offline).
The game was not running, so no name has been confirmed with `cs2_find_prefabs` yet. That is step 1 of the plan.

## 1. How CS2 builds bridges [V: decompiled Game.dll, `NetInitializeSystem`, `NetCompositionHelpers`]

- **Ordinary roads and tracks** (`Highway Oneway - 3 lanes`, `Small Road`, `Double Train Track`, `Pavement Path`, ...) become elevated when the edge and both nodes are at least 2 × the prefab's elevation limit above the ground: 8 m for roads and tracks, 4 m for paths. Between 1× and 2× they get side walls; at −2× or deeper they become tunnels. The elevated deck spawns a pillar placeholder (`Pillar Placeholder`, `Train Pillar Placeholder`, `Pillar Pedestrian Placeholder`). The game then picks the `Vertical Pillar` / `Horizontal Pillar NN` / `Pillar Top NN` variant that best fits the height and deck width. This is the "generic bridge" look the converter produces today.
- **There is no per-segment bridge style.** `CompositionFlags` and `NetPieceRequirements` have no bridge-style value (`Elevated`, `Raised`, `Tunnel`, `StyleBreak`, ... only). A different look needs a **different net prefab on that edge**, which `/import/network` already accepts (`cs2mcp/NOTES.md` round 16 reached the same conclusion).
- **Named bridges** are separate network prefabs with a `Game.Prefabs.Bridge` component. At load the game gives them:
  - always elevated (`RequireElevated`), straight edges unless `m_CanCurve`, straight ends, symmetrical edges, smooth slopes;
  - an edge length of **0.6–1.4 × `m_SegmentLength`**; the minimum is dropped when `m_AllowMinimalLength` is set;
  - a minimum height over water, `m_ElevationOnWater` → `PlaceableNetData.m_MinWaterElevation`;
  - a build style of Elevated, Raised or Quay (Quay adds `ShoreLine` placement);
  - optional `m_FixedSegments` (`FixedNetSegmentInfo[]`), a fixed sequence of spans with set lengths. The game numbers edges with `m_FixedIndex` and places towers and anchors from them. Golden Gate, Grand Bridge, the Bridges & Ports suspensions and the truss arches use them, and so do the draw and lift bridges.
  - Their towers and pylons are `PillarObject` statics (e.g. `4LaneSuspensionBridgePillar`, `GoldenGateBridgePylon01`) placed by the net, not by the importer.
- **CS2MCP today** builds plain courses with `m_FixedIndex = -1` [V: `cs2mcp/NOTES.md` line ~386 per the local scan; `BridgeToolSystem.cs` builds `NetCourse` without fixed indices]. Fixed-span prefabs are untested, and Round 21 dropped the 87 `Arc Pedestrian Bridge` edges and the dam deck from the runner.
- **"Hydroelectric_Power_Plant_01 Dam"** is the hydro plant's dam deck, a road-like net (NOTES.md: RoadPrefab, 34 m wide) drawn across the river, not a bridge. `Quay*` nets are waterfront walls with a road or path on top [V: locale descriptions].

## 2. Vanilla base game [V names: `Cities2_Data\Content\Game\Locale.cok`, `Assets.NAME[...]`; prefab class and span rules U]

| prefab name | display name | fits CS1 |
|---|---|---|
| `Suspension Bridge - Highway Oneway - 2 Lanes` / `- 3 lanes` / `- 4 Lanes` / `- 5 Lanes` | n-Lane Highway One-Way Suspension Bridge | interstate carriageways (one-way, lane-exact) |
| `Suspension Bridge - Medium Road` | Four-Lane Suspension Bridge | national road |
| `Cable-Stayed Bridge - Highway Oneway - 3 Lanes`, `Cable-Stayed Bridge - Small Road - 2 lanes`, `Cable Stayed Bridge - 4 lanes`, `Cable-stayed Bridge - XL Road Divided - 8 Lanes` | cable-stayed | – (no CS1 cable-stayed bridge) |
| `Truss Arch Bridge - Highway Twoway - 2 Lanes`, `Truss Arch Bridge - Small Road - 2 Lanes`, `Truss Arch Bridge - 4 lanes` | truss arch | national road; two-way only for highways |
| `Tied Arch Bridge - 4 lanes` | tied arch | – |
| `Extradosed Bridge - Highway Oneway - 3 Lanes`, `Extradosed Bridge - 4 lanes`, `... widemedian`, `Extradosed Bridge - Large Road Divided - 6 Lanes` | extradosed | concrete-viaduct alternative |
| `Wooden Covered Bridge - 2 lanes` | wooden covered road bridge | gravel-road bridges (taste) |
| `Grand Bridge`, `Golden Gate Bridge` | landmark spans | – |
| `TrussArchBridge01` | Double Train Track Truss Arch Bridge (Quays & Piers) | rail arches (Pedrido, Ourense) |
| `ExtradosedBridge01` | Four-Lane Highway Double Train Track Double-Decked Extradosed | – |
| `Arc Pedestrian Bridge`, `Covered Pedestrian Bridge`, `Cable Stayed Pedestrian Bridge` (+ `... Bike Bridge`) | path bridges | dam walkway, footbridges; `Arc Pedestrian Bridge` is the converter's current monorail-arch decor |
| `Hydroelectric_Power_Plant_01 Dam` | dam deck | dam crest (already handled) |

Correction to `cs2mcp/NOTES.md` round 16: `TrussArchBridge01Net` is "Net piece for TrussArchBridge01" (a sub-net), and `TrussArchBridge01` is a **train** bridge, not a road truss arch [V: locale description].

## 3. Bridges & Ports DLC: installed [V: `Cities2_Data\Content\BridgesAndPorts\` (Blob.cok 1.26 GB, Locale.cok, 5 maps, UI icons)]

Released 2025-10-29 and adds 20 bridges, all drawable networks. Movable spans animate only over a Narrow Seaway; elsewhere they are fixed [V: Paradox dev diary https://www.paradoxinteractive.com/games/cities-skylines-ii/news/bridges-and-ports-dev-diary-bridges].

| prefab name [V: BridgesAndPorts/Locale.cok] | display name | geometry (read from the BXP copy of the same bridge, see §4) |
|---|---|---|
| `SuspensionBridge01` | Two-Lane Suspension Bridge | [U] |
| `SuspensionBridge02` | Three-Lane Highway Double-Decked Suspension Bridge | BXP copy: 4 fixed spans of 100 m (towers 200 m apart) |
| `SuspensionBridge03` | Six-Lane Suspension Bridge | BXP copy `BXP SuspensionBridge03 - Six Lane Highway`: fixed spans **168 + 320 + 320 + 168 m = 976 m, towers 640 m apart** |
| `SuspensionBridge04` | Six-Lane One-Way Suspension Bridge | [U], probably like 03 |
| `TrussArchBridge02` | Four-Lane Truss Arch Bridge | [U] (BXP B&P `TrussArchBridge03 - Highway` copy: 2 fixed spans of 128 m) |
| `TrussArchBridge03` | Double Subway Track Truss Arch Bridge | as above |
| `ExtradosedBridge02` / `03` / `04` | 2+4-lane double-deck / 4-lane / 4-lane one-way extradosed | BXP copy of 02: 256 m segments, no curves |
| `DrawBridge01/02/03` | 2 / 4 / 6-lane bascule | 144–152 m segments |
| `LiftBridge01`…`05` | train, double train, highway + train, subway, tram lift bridges | [U] |
| `PedestrianBridgeCoveredWood01`, `PedestrianDrawBridge01/02` | wooden covered path bridge, path drawbridges | BXP copy: 64 m segments |
| `QuayMedium02/03`, `QuayLarge02/03`, `QuaySmall03`, `QuaySmallWide03`, `HarborQuay01-03` | quays | – |

The class of each vanilla and DLC name (RoadPrefab, TrackPrefab, ...) and the vanilla `m_SegmentLength` values are not readable offline. The definitions live in `resources.assets`, not in the `.cok` blobs [V: zip directories].

## 4. PDX Mods

Mod cache: `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\.cache\Mods\pdx_mods\<id>_<ver>`. The `mods_subscribed` folder named in the brief does not exist. Playsets come from `playset_config.json`: `lessgo-withEAI` (702 mods), `Clone - lessgo-withEAI` (495), and **`DakotaFalls-E2C` (24, active)**. **No bridge mod is in DakotaFalls-E2C**; it has Anarchy (74604) but neither Move It nor Find It [V].

### 4.1 Network bridge packs (draw like a road; the converter can emit them as the edge prefab)

Span data is read from the `.Prefab` files in the mods' `.cok` packs (binary prefab serialisation, `Game.Prefabs.Bridge` fields) [V: `BridgeExpansionAssetPack.cok`, `BridgeExpansionAssetPack2.cok`]. "seg" = `m_SegmentLength` (an edge must be 0.6–1.4 × seg); "fixed" = number of fixed spans.

| mod (id, cached ver) | playsets | EAI | how applied | bridges relevant here (exact prefab names) |
|---|---|---|---|---|
| **Bridge Expansion Pack** (92245, v36 in cache; page v1.5.0 for game 1.5.*) by brianrocha | lessgo + clone | no; code mod + asset pack. San Francisco content needed for Golden Gate/Art Deco items, **not installed** (no `SanFrancisco` folder in Content); Anarchy needed for the Truss Viaducts [web] | separate network prefabs, menu search "BXP" | `BXP Truss Arch Bridge - Highway Oneway - 2 Lanes` (seg 128, curves allowed, no fixed spans); `BXP Truss Arch Bridge - Highway Oneway - 3 Lanes` (seg 128); `BXP Tied Arch Bridge - Highway Oneway - 4 Lanes` (seg 128); `BXP Suspension Bridge - Highway Oneway - 6 Lanes`, `... Highway Twoway - 4/6 Lanes` (seg 320); `BXP Extradosed Bridge - Highway Twoway - 4/6 Lanes`, `- Medium Road Divided - 4 Lanes` (seg 128); `BXP Truss Viaduct - Highway Oneway - 3 lanes`, `- Highway Twoway - 2 lanes`, `- Train`, `- Subway` (seg 64, Anarchy); `BXP TrussArchBridge01 - Highway Twoway - 2/6 Lanes` (2 fixed × 128 m, no curves); `BXP Suspension Bridge - Train - 2 Tracks` (seg 320); `BXP Extradosed Bridge - Train - 2 Tracks` (seg 128); `BXP Tied Arch Bridge - Train - 4 Tracks`; `BXP Art Deco Viaduct - 6 Lanes` (seg 60) and `- Train - 4 Tracks` (seg 50); `BXP Grand Bridge Viaduct` (seg 60); `BXP Golden Gate Bridge Train/Subway` (4 fixed: 350 + 640 + 640 + 350 m); `BXP Arc/Covered/Cable Stayed Pedestrian Bridge Revised` (seg 32/32/64, min 5 m over water); 13 pillar objects (`BXP BridgeTrussHighClearance`, `BXP Art Deco Pylon`, ...). Comments from Sep 2026 report breakage after the latest game update [web, U] |
| **Bridge Expansion Pack: B&P** (124160, v3) | lessgo + clone | no; needs Bridges & Ports | separate network prefabs | `BXP SuspensionBridge03 - Six Lane Highway` (6-lane two-way highway, 4 fixed: 168 + 320 + 320 + 168 m); `BXP SuspensionBridge02 - Four-Lane` (4 fixed × 100 m); `BXP ExtradosedBridge03 - Highway` (seg 320); `BXP TrussArchBridge03 - Highway / - Train / - Tram` (2 fixed × 128 m); `BXP ExtradosedBridge02 - Train/Tram/Subway` (+ `Track`, seg 256); `BXP DrawBridge01/02/03 One-Way`, `BXP DrawBridgeAlley`; `BXP PedestrianBridgeCoveredWood01 Revised` (seg 64) |
| **Bridge Builder** (160320, v17; updated 2026-10-13) by cxhe | lessgo only | no; code mod (Road Builder, BXP, Node Controller optional) | generates **new** bridge prefabs at runtime from any road/track + a style (suspension, cable-stayed, truss arch, tied arch, covered wood), double-deck possible | names are created in-game, so none on disk [V]. The only way to get e.g. a one-way 3-lane highway truss arch, or a Small Road tied arch, if no pack has it. Generated names must be read back in-game before the converter can use them [U] |
| **Train Arch Bridge** (155730, v1) by ilPanzo | lessgo only | no | network | `TrussArchBridge02` as a **track**. Same name as the DLC's 4-lane road `TrussArchBridge02`; how the clash resolves is [U] (risk: one shadows the other) |
| **Truss Arch Bridge - Highway Oneway - 2 / 3 Lanes** (142796 / 142798) by hhillcomposer | not cached | no | network | names not read [U]; BXP already contains the same two bridges |
| **Pedestrian Bridges (NEW)** (151418, v4) | lessgo only | no | network (paths) | `W7WoodPedBridge875`, `W7ArcPath875`, `W7CoveredPath875`, `W7CablePath875`, `W7PavementPath875`, `W7ElevatorPath875` |
| **Extra Bridges** (115063) by likelyrandom | not cached | needs the retired Asset Packs Manager; game 1.3 | network | not usable [web] |
| **Functioning Swing Draw Bridge** (153832) by joergeske | **not downloaded, in no playset** (listed only in `pdx_mods_cache.json`) | needs Bridges & Ports | animated rail swing span, net prefab; small shipways only | names unread [U]; no CS1 counterpart (no movable bridge in Dakota Falls) |
| **Clark Street Bascule Draw Bridge Chicago** (155250) by joergeske | not downloaded | B&P [U] | animated road bascule net | no CS1 counterpart |

### 4.2 Object and building bridge kits (placed with Move It / Find It; a plain road is drawn on top with its pillars hidden)

| mod (id, ver) | playsets | prefab names [V: `.cok` contents] | look [web] | use here |
|---|---|---|---|---|
| DIY Artdeco Bridge Kit / LA River Bridges (136168, v1) | lessgo + clone | `4thStBridgeComplete`, `4thStBridgePillar`, `OlympicBridge` | white/grey concrete art-deco multi-arch (4th Street, Olympic Blvd bridges) | **white multi-arch** below the dam |
| DIY Stone Arch Rail/Road Bridge Kit (135944, v1) | lessgo + clone | `StoneArchRailBridge` | stone arches | alternative for the white arch |
| DIY German Bridges, Viaducts & Walls (136487, v8) | lessgo + clone | `GermanBridgeSpan`, `GermanBridgeSpanShort`, `GermanBridgeSpanLong`, `GermanRailPillar`, 10 `GermWal…` | Hohenzollern-style arched truss | green truss arch alternative |
| Generic Arched Truss Bridge (142603, v3) | lessgo only | `PittTrussBridge`, `TrussBridgeHollow` | scaled-down Sydney-style arch truss | **green truss arch** (BR009), PO-like |
| Classic Rail Truss Bridge Kit (143426, v1) | lessgo + clone | `RailBridge1`, `RailBridge2`, `RailBridge3`, `RailBridgePillar` | steel rail truss spans + pier | R2 through-girder spans (taste) |
| Peterson Bridge (144730, v2) | lessgo + clone | `Peterson Bridge` | [U] | – |
| Sydney Harbor Bridge (136938, v1) | lessgo + clone | `SydneyHarborBridge` (building) | through-arch landmark | not a match |
| ITBP pillars (140978, v1) | lessgo + clone | `ITBP_Pillar Piers 01`, `ITBP_Pillar Median 01-04`, `ITBP_Single-Sided Beam 01-05` | interstate pier bents | hand-placed piers under USRP-style viaducts |
| Pillar1x8…4x16 (154132, v1) | lessgo only | `Pillar1x8` … `Pillar4x16` | plain concrete columns | generic piers |
| DIY Truss Bridge Kit (136135), Parisian/European Arch Kit (135754), Art Deco Walls, Tunnels and Bridges Set (157964) | **not cached** (157964 is requested but not downloaded) | names unread [U] | Burrard St truss; stone arches; 13 concrete art-deco "Lego" parts | optional, for the white arch / PO pieces |
| Free Mode Vanilla Bridge Piers (141766) | not cached | [U] | vanilla piers as props; needs Anarchy, Better Bulldozer, Find It, Move It | – |
| Expressway Pillars (158752) | not cached | [U] | decorative Japanese pillars | – |

### 4.3 Pillar and placement tools

| mod | id | state | relevance |
|---|---|---|---|
| Pillar Spacing (Ethan327) | 162558 | not cached | max pillar spacing 40–1000 m for ordinary roads/tracks/paths (game 1.6); no effect on tower bridges. Lets plain viaducts copy CS1 pier spacing, and gives the long free spans of BR006 and BR001's Network-Skins segments. Do not combine with 77597/132381 |
| ConfigurableElevatedRoad / BridgePillarAdjuster | 77597 / 132381 | not cached | outdated (game 1.1 / 1.5) |
| Anarchy | 74604 | in DakotaFalls-E2C | needed to overlap object bridges with roads, water and terrain, and for BXP Truss Viaducts |
| Move It | 74324 | lessgo only | placing and aligning kit pieces |
| Find It | 77240 | lessgo only | finding kit pieces |
| ExtraAssetsImporter (EAI) | 80529 | lessgo + clone | **no bridge pack depends on it**; its 81 packs hold no bridge net lanes (only the `LowBridgeAhead` decal) [V] |
| Road Builder | 87190 | – | its FAQ says styled bridges are not supported; plain roads only [web] |

## 5. CS1 source assets (for the look)

- Detroit Bridge [Ploppable] 2931939156: a **building**, a hand-made blue suspension bridge loosely based on the Ambassador Bridge; roads are drawn over it. https://steamcommunity.com/sharedfiles/filedetails/?id=2931939156
- Hernando de Soto Bridge 1310328404: a **building** of the I-40 twin through-arch "M Bridge"; needs Move It. https://steamcommunity.com/sharedfiles/filedetails/?id=1310328404
- R2 Ourense Viaduct 2836620152: concrete, three large parabolic arches (the real bridge is 415 m long and 46 m high), double track.
- R2 Pedrido Bridge Complex 2932555116: elevated = concrete arched viaduct (32 m segments); bridge = concrete **bow-string arch** (74 m segments).
- R2 Through Girder Bridge Pack 1 2926036331: black steel **through plate girder**, 1 and 2 tracks, with its own pier.
- R2 American Wooden 2610252544: B0/E0 look not described [U]; probably a steel deck girder on concrete piers.
- USRP Interstate 2452038713: B/E look not described [U]; concrete girder with jersey barriers on pier bents.
- `artdeco-truss01` = Hyperion's Gate PO Elements Pack 2376025671: art-deco truss-arch pieces (full arch, half arch, rivet face).

Raw notes (full locale key list, every prefab in the bridge mod packs, both research write-ups) are in the shared side-session notes folder `bridges-research/`, outside the repo; the facts used are copied above.
