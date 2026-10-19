# CS2-side audit of the Dakota Falls import plan

Date: 2026-10-19. Read-only audit; no pipeline file was changed.

**Sources:**
- An ILSpy decompile of the installed `Game.dll` (dated 2026-09-20, decompiled 2026-10-19).
- `Locale.cok` (`Assets.NAME[...]` keys, 7 114 names).
- `cs2-playset/ASSET_PACKS.md`.
- The bridge source (`cs2mcp/CS2MCP.Bridge`).
- `net-converter/out` and `out/x1`.
- The playable heightmap `10190442-dktflls-1to1.png`.
- The 14 vanilla maps in `Cities2_Data/Content/Game/Maps/*.cok`, decoded. Each `.MapData` is a series of `[u32 raw][u32 packed][zstd frame]` blocks followed by an archetype table. The decode script is `arch3.js` in this session's scratchpad.

**Limits of this audit:**
- Per-asset values (slope limit per road, pylon spacing per line, sub-objects of a seaway) live in the compressed prefab bundles, not in `Game.dll`. They are marked **asset value, unknown**.
- Nothing here was run in the game.

Paths like `Game.Net/ValidationHelpers` refer to the decompile, by namespace folder and class.

---

## Findings, by impact

### 1. Tree `age` is ignored in the editor; about 5 % of the 508 k trees will spawn dead (high)

**What the code does:**
- The bridge's `CreateDefinitions.cs` (copied from `Game.Tools.ObjectToolBaseSystem`, line 1198 there) uses `m_Age = random.NextFloat(1f)` whenever `m_EditorMode` is true. The `m_AgeMask` built from our `age` field is only used outside the editor, through `ToolUtils.GetRandomAge`.
- `GenerateObjectsSystem` then calls `ObjectUtils.InitializeTreeState(m_Age)`:

  | age range | state | share of trees |
  |---|---|---|
  | < 0.10 | child | 10 % |
  | < 0.25 | teen | 15 % |
  | < 0.60 | adult | 35 % |
  | < 0.95 | elderly | 35 % |
  | ≥ 0.95 | **dead** | 5 % |

**Effect on our files:**
- The converter's age mix (seedling 20 k, sapling 93 k, adult 286 k, elderly 109 k) is discarded.
- About 25 000 dead trees appear.

**Other notes:**
- `TreeGrowthSystem` runs only in `SystemUpdatePhase.GameSimulation` (`SystemOrder.cs:470`), so nothing changes while in the editor. In a city, the trees then age normally.
- The grass, fern and flower pack items are `PlantObject` without `TreeData`, so `age` means nothing for them.

**Fix (bridge):** in editor mode, map the requested `AgeMask` to a fixed age inside the right band, for example sapling 0.05, young 0.17, mature 0.42, elderly 0.75. Alternatively, call `GetRandomAge` in editor mode too; it never yields dead.

### 2. About 10 k trees stand in water, and about 3.6 k trees and 32 objects sit on nets (high for looks, low for function)

**Trees in the river:**
- Trees are rebuilt from the 16 m and 64 m rasters with jitter (`Objects.cs` `FromRasters`). There is no water or net mask.
- Sampled against the playable heightmap, **10 194 trees stand on terrain below sea level**: 97.5 m at 2048, and the same points at 195 m at 4096. Those are in the river.
- Trees inside lakes above sea level (fed by sources) are not counted in that figure.
- The orientation used is row 0 = north. It was checked against the network: the mean |y − terrain| is 4.0 m with that orientation and 47 m when flipped.
- Object validation flags `InWater`, but `ToolSystem.ignoreErrors` only bypasses the gate in `ToolBaseSystem.GetAllowApply()`. The trees are created standing in water.

**Trees and objects on nets:**
- 3 570 trees lie within half the net width + 2 m of a ground-level net, and 32 objects lie on a net corridor.
- `Game.Objects/OverrideSystem` collides every `GeometryFlags.Overridable` object (trees and props, set in `ObjectInitializeSystem:610`) against nets, objects and areas whenever any of them update. A colliding object gets `Overridden` and is **hidden, not deleted**, and this happens in either import order.
- Non-overridable objects, such as the 7 buildings, stay overlapping.

**Fix (converter):**
- Drop tree points whose terrain is below sea level, or below the CS1 water surface once the v3 `waterSurface` grid exists.
- Drop tree points inside net corridors.

### 3. The ship outside connection is placed about 1 km from the seaway's border end (high for shipping)

- `import-connections.json` puts `Ship Outside Connection - Twoway` at (5736.3, −7168). That is the CS1 building's x projected onto the border.
- The only seaway border node is at (4718.4, −7168).
- A standalone outside-connection object routes through its own `SubLane` buffer (`WaypointConnectionSystem.GetLaneContainer`). A marker that is not on the seaway almost certainly gives ships no path in or out.
- The code does not decide whether a seaway border node spawns its own marker. `GenerateNodesSystem` tags *every* node beyond the bounds with `Game.Net.OutsideConnection`, but a marker sub-object exists only if the seaway asset lists one with `RequireOutsideConnection` (asset value, unknown).

**Fix (converter):** place the ship object at the seaway end (4718.4, −7168), facing outward (rotation 180 ± the course angle). If the in-game check (NOTES.md item 6) shows that the seaway spawns its own marker, drop the object.

### 4. Canals as Medium Boatway and flood walls as Sound Barrier don't work like CS1; quays work if the flat ones are forced (medium)

**Medium Boatway (14 edges)** is a `WaterwayPrefab`, a ship *lane*, not a channel:
- `NetInitializeSystem` (~2054–2066) gives it the flags `OnWater | FlattenTerrain`, `PlacementFlags.Floating`, edge length ≤ 1000 m, and `CourseSplitSystem.CalculateElevation` returns 0 for it.
- `ValidationHelpers.CheckSurface` raises `NoWater` when the water depth is < 0.2 m (bypassed by `ignoreErrors`).
- Terrain under a waterway can only be pushed *down* to the hull bottom (`TerrainSystem`: max height = `m_HeightRange.min`). It never adds water.
- All 21 boatway nodes are on terrain above sea level, at y ≈ terrain.
- So the canals are dry unless sources fill them. The 21 small 26 m sources around (1140–1400, 4485–4595) at 279–286 m look like CS1's canal feeds. With a source, the boatway is just a lane for boat lines.
- The CS1 channel cut is already in the heightmap (it was exported from CS1 terrain).
- Vanilla CS2 has **no canal net**: no `Assets.NAME` contains Canal or Channel.
- Recommendation: keep the boatways only where the canal has water. Otherwise skip them; the terrain and sources make the canal.

**Quays.** `QuaySmall01` (90) and `QuayMedium01` (7) are *road or path nets* with `Bridge.m_BuildStyle = Quay`:
- Their flags are `ElevatedIsRaised`, `PlacementFlags.ShoreLine` and `m_MinWaterElevation = m_ElevationOnWater` (default 10).
- `CourseSplitSystem.CalculateElevation` (~3605) raises a side only where that side is water deeper than `m_ElevationLimit`, or is lower. That raised side is where the wall forms.
- Nets are not required to be next to water (`NotOnShoreline` is an object-only error).

**Quays can be forced, and most of ours form a wall by themselves.** *(Corrected 2026-10-19 after the user's note; an earlier version said most would become plain paths.)* There are two ways a wall forms:
- **Automatically**, from `CourseSplitSystem.CalculateElevation` for `ShoreLine` nets. A side whose terrain lies ≥ 0.1·`m_ElevationLimit` (0.4 m at the default limit of 4) below the deck, or in water deeper than the limit, gets elevation max(limit, drop). That side composes as `Side.Raised`, i.e. a wall.
- **Forced**, by giving the course elevation. In `NetCompositionHelpers.GetElevationFlags`, elevation ≥ 2·`m_ElevationLimit` with `ElevatedIsRaised` gives `Raised` on both sides, and ≥ `m_ElevationLimit` gives `Raised` on that side (quays lack `RaisedIsElevated`, so they never become bridges). In game, this is the quay tool's elevation step. Through the bridge, it is node `elevation` > 0, or `y` above terrain with `heightMode: absolute`.

Cross-slope at each quay edge midpoint, measured on the playable heightmap (largest terrain difference between the two sides at 4–8 m):

| | flat (< 0.4 m) | 0.4–4 m | 4–8 m | ≥ 8 m |
|---|---:|---:|---:|---:|
| QuaySmall01, x1 (2048) | 21 | 57 | 10 | 2 |
| QuaySmall01, 2× (4096) | 8 | 63 | 7 | 12 |
| QuayMedium01, x1 | 0 | 2 | 1 | 4 |
| QuayMedium01, 2× | 0 | 0 | 2 | 5 |

So at 2048, 69 of the 90 small quays and all 7 medium ones form a wall automatically on the lower side. The 21 flat ones need forcing.

- **Forced at any height, through the upgrade flags** (the user's note; this is how mods such as Extended Road Upgrades do it). `CompositionSelectSystem.GetEdgeFlags` builds the composition as `upgradeFlags | subObjectFlags | elevationFlags`, where `upgradeFlags` comes from the edge's `Upgraded.m_Flags` (line ~430). So an edge carrying `Upgraded` with `m_Left`/`m_Right |= CompositionFlags.Side.Raised` composes as a wall whatever its elevation. If the net has an asset composition for raised sides, this works for other road nets too, which is how mods add "retaining wall" upgrades to roads. (`Side.SoundBarrier` is also an upgrade flag, so sound barriers can be a road-side upgrade instead of a separate fence net.)
  - Through the bridge, this needs an import option that sets per-side upgrade flags on the created edges, the way the bridge's `cs2_upgrade_road` writes upgrade definitions. A mod in the playset can also do it by hand afterwards.

What stays true:
- Quays are path or road nets. QuaySmall01 adds a pedestrian path along the wall top. `QuayMedium01` is a **two-lane road with sidewalks** (`Assets.DESCRIPTION`); if it connects to the road graph it carries traffic and blocks zoning.

Recommendation:
- Keep the quays.
- For the 21 flat QuaySmall01 edges (x1), write node elevation = the CS1 wall height (≥ 4 m for a one-sided wall, ≥ 8 m for both sides) instead of 0, with `bypassValidation` on.
- The real `m_ElevationLimit` of the quay assets is unknown (the code default is 4). Check one forced edge in game first.
- Check whether the 7 CS1 "Quay" pieces (round 10, marked UNVERIFIED) really were quays and not decorative walls.

**Sound Barrier (2)** is a fence net: `Layer.Fence` or `NetFence`, `FlattenTerrain`, `OnGround`. In CS2 only terrain holds water; no net blocks the water simulation. Any land a CS1 flood wall protected will flood if it lies below the water level. The wall is visual only.

### 5. Height Scale 4096 with doubled heights: what breaks (medium; run A is safer)

| Rule (evidence) | 2048, true heights (`out/x1`) | 4096, ×2 heights (`out/`) |
|---|---|---|
| **Net slope.** `Game.Net/ValidationHelpers:1457–1488`: `SteepSlope` when an edge grade exceeds `m_MaxSlopeSteepness` (+0.0005; half-edges at ×2). The code default is `NetGeometryPrefab.m_MaxSlopeSteepness = 0.2`; real values are per asset. `GeometrySystem.LimitMiddleHeights` and `CourseSplitSystem` also clamp mid-heights to this slope. | Edges > 20 %: highway 5, rail 1, others 0. Max: highway 27 %, rail 20 %. | Edges > 20 %: highway 34, rail 9, Medium Road 13, gravel 3, path 4. Max: highway 54 %, rail 41 %. **Bypassed at import, but the game reshapes mid-spans, and any later edit by a player fails `SteepSlope`.** |
| **Elevation range.** `PlaceableNet.m_ElevationRange` defaults to (−50, 50). It is clamped only by the tools (`NetToolSystem:1942/3771`, `CourseSplitSystem:3835`); there is no validation error. Pillar height limit: none found. | Elevated nodes > 50 m above terrain: 49, max 97 m. Tunnels: min −26 m. | > 50 m: 176, of which 49 are > 100 m (max 194 m). Tunnels: 2 below −50 m. **Imports keep these, but upgrading or replacing such a segment in-game clamps it to 50 m.** |
| **Composition thresholds.** `m_ElevationLimit` defaults to 4: elevated at ≥ 8 m, tunnel at ≤ −8 m (min ≤ −12 m), `NetCompositionHelpers:1717–1729`. These are absolute metres, not scaled. | Same as CS1. | Low CS1 overpasses (4–8 m) become ≥ 8 m at ×2 and so become true bridges. This is harmless; the converter already uses ≥ 8 m. |
| **Objects and trees on slopes.** There is no slope check anywhere in `Game.Objects` or `Game.Buildings` validation. | Trees on 45–60° ground: 229. | Trees on 45–60° ground: 35 043; on 60–75°: 775. Accepted, but they look odd on near-cliffs. |
| **Snow line.** `SnowSystem.SnowTransfer` uses temperature − 0.01·(height − `temperatureBaseHeight`), i.e. 1 °C per 100 m (`ClimateSystem.snowTemperatureHeightScale`). The base height is the mean height of the starting tiles. | Peaks ≈ 1 069 m, about 8 °C colder than a ~300 m start area. | Peaks ≈ 2 138 m, about 16 °C colder. **Snow covers far more of the map for most of the year.** Pick a warmer climate or accept it. |
| **Cliff and rock texture by slope.** Not in C#; it is in the terrain shaders (unknown). | Matches the CS1 look. | Steeper terrain shows more cliff texture (probable, not verified). |
| **Water.** Source depth is clamped to 250 m (`WaterSystem.kMaxSourceHeight`). The sea level cap is 2000. `MaxHeightDownscaled` is `R16_SFloat`, with 0.5 m steps above 512 m. | Source depth above terrain: median 9.9 m, max 26.9 m. Sea level 97.5. | Median 19.8 m, max 53.8 m. Sea level 195. Upland sources above 512 m get 0.5 m height precision. Nothing is out of range. |
| **Buildings.** `BuildingTerraformData` flattens terrain; no cap was found. | Fine. | The 7 buildings flatten more on steeper ground. |

**Verdict:**
- Run A (2048, `out/x1`) passes every rule the code exposes.
- Run B (4096) depends on `bypassValidation` for about 70 network edges. It leaves steep and high geometry that players cannot edit without errors, and it changes the snow and probably the cliff look.
- Run B is fine as an artistic choice, but it is the one that risks follow-up failures.

### 6. Water sources: `natural` is the right type; drop or adjust 11 of the 65 (medium)

**How the new water system works:**
- `WaterSystem.SetDefaults` gives new maps `m_UseLegacyWaterSources = false` and `kDefaultSeaLevel = 511.7`.
- `WaterSimulation.SourceStep` (new system) ignores both the source *type* and `m_ConstantDepth` / `m_Multiplier`:
  - If `m_Polluted > 0`, the source is a flow source of `0.3165952·steps·m_Height`.
  - Otherwise it is a constant-level source with target `position.y + m_Height` within `m_Radius·m_Modifier`.
  - The type ints (0 stream, 1 lake, 2 river, 3 sea) matter only in `WaterSimulationLegacy`.
- **There is no sea source.** Sea level is the global `WaterSystem.SeaLevel` plus "Apply sea level" (`ResetToSealevel`).
- Editor tool defaults are radius 30 and height 10. The ranges are radius 1–2500 and height 1–250 (`WaterToolSystem`, `WaterPanelSystem`).

**What the bridge does:** `RequestHandlers.Water.cs` maps `natural` → kind 1, samples ground, and writes `m_Height = height − ground`. It rejects ≤ 0 and clamps at 250. That is the vanilla constant-level source, so `natural` (a lake-type level) is right for every source, including river inflows. Stream or river types would not change behaviour in the new system.

**Risks:**
- Never send `polluted > 0`: the depth would then be read as a flow.
- Never send a negative height: it drains water.
- Sources outside ±7168 are skipped by the playable simulation (`if (!MathUtils.Intersect(bounds.xz, …)) continue;`). They only matter for the backdrop simulation (`simulateBackdrop`).

**Vanilla counts** (decoded `WaterSourceData` archetypes): 3–47 per map, median about 5. Tampere has 47, San Francisco 21, Lakeland 18.

**Per-source rule** (S = sea level: 195 for `out/`, 97.5 for `x1`):

| Rule | IDs | n |
|---|---|---:|
| `height ≤ S`: drop (the sea level covers them) | 47, 51, 59, 65 | 4 |
| `height ≤ S + 1 m`: drop | 46 | 1 |
| Outside the map by ≤ 500 m: move inward to ±(7168 − radius − 14), keep as a river inflow (`natural`), with `depth = max(height − ground, 2)` as a fallback | 13, 61, 62 | 3 |
| Outside by > 500 m: drop, unless backdrop water is wanted | 1, 2, 3 | 3 |
| Everything else: keep `natural`, height = CS1 target level, ignore `rate` | — | 54 |

That leaves 57 sources, above Tampere's 47.

**Optional merge:** the 21 sources of 26 m radius at (1140–1400, 4485–4595), 279–286 m (IDs 5–7, 9–12, 25–34), can become 2–3 sources of about 150 m radius, grouped where levels differ by < 1 m.

**Ordering:** ship paths are imported before water, and seaway and boatway validation (`NoWater`) samples water at apply time. That is harmless with `bypassValidation`, but apply the sea level (runbook step 2) before any import.

### 7. Seaway courses are written at riverbed height, and waterways push terrain down (medium; verify in game)

- All 101 seaway nodes have y = terrain ± 0.2 m, i.e. the riverbed, not the water surface. 10 of them lie on terrain above sea level 195, which is upstream and wet only if sources fill it.
- Waterways carry `FlattenTerrain`, which becomes `LaneFlags.ShiftTerrain`. The terrain under them is lowered to course y + `m_HeightRange.min`, the hull depth (asset value).
- With `heightMode: absolute` and the course on the riverbed, the river bed may be **dug a further hull-depth down** along the whole ship path.
- Check in game:
  1. Import one seaway edge.
  2. Compare `cs2_terrain` along it before and after.
- If it digs, write seaway y = the water surface (S, or the CS1 water level), or let the bridge sample it (no y).

### 8. Outside connections: tolerance, counts and requirements (low; the plan is right)

**Tagging:** `GenerateNodesSystem.UpdateNodesJob` (~1379) tags a node when it lies outside `TerrainBounds` shrunk by 0.1 m:
- `TerrainUtils.GetBounds` gives ±7166.25 m for a 4096 heightmap, so the node is tagged at |x| or |z| ≥ ≈ 7166.15.
- No angle or direction is required, and the node does not even have to be an end node.
- All 26 network border nodes, the power end and the seaway end are exactly at ±7168, with degree 1. Their last-segment angles from the border normal are 0–42°.
- One rail end (7168, 2299.7) has a **4 m** last segment, which may raise `ShortDistance` (bypassed). It is better to merge it into the previous edge.

**After tagging:**
- `NodeAlignSystem` turns the node to face inward.
- `GeometrySystem` clips the edge to the bounds.
- The marker comes from the net asset's sub-object with `RequireOutsideConnection` (`SubObjectSystem:1623`).

**Limits and requirements:**
- There are **no count limits**. `OutsideConnectionData` holds only the type and remoteness.
- **Map checklist** (`Game.UI.Editor/MapRequirementSystem`):
  - Required: starting tiles; water in a starting tile (surface depth > 0 or groundwater); a road outside connection; a map name.
  - Optional: train, air (`AirplaneStop` + `Objects.OutsideConnection`), electricity, and the oil, ore, forest and fertile resources. Ship is not listed.
  - Saving is never blocked; it only shows a warning. Sharing is disabled when not playable (`EditorHierarchyUISystem:1390`).
- **Note:** 8 of the 19 road connections are Gravel Roads on the east border, at 512 m spacing (z 530–4064). Each becomes a real road outside connection with through traffic. Consider ending the ones that were only field roads 1 m short of the border.

### 9. Power line spans are 207–242 m; the game will add pylons (low)

- `PowerLinePrefab.m_MaxPylonDistance` defaults to 120 m (asset value unknown). `m_EdgeLengthRange.max` and `m_ElevatedLength` are set to it.
- Validation allows ×1.1 before `LongDistance`.
- `CourseSplitSystem` (~3518) splits every course longer than `m_EdgeLengthRange.max` into equal pieces, so most of the 60 courses gain one mid-pylon. No failure; only the pylon rhythm differs from CS1.
- Overhead lines get `StraightEdges`, so the 4 slightly curved courses (≤ 2.4 m off straight) are straightened.
- Nodes on the ground are fine (`OnGround`; elevation = y − terrain).
- The east-border end makes an electricity outside connection if the asset carries the marker. That satisfies the optional checklist item.

### 10. Surfaces: overlaps, node limits and agriculture (low)

- **Node limits:** none found in `AreaToolSystem`, `Game.Areas/ValidationHelpers` or `AreaGeometryData`. Minimum node spacing is 0.375 m; snapping is 0.75 m (`AreaUtils.GetMinNodeDistance`). Our maximum is 330 nodes (fields).
- **Overlap:** in the editor, unowned surfaces get `PhysicalGeometry` (`AreaUtils.SetCollisionFlags`). Surface vs surface, surface vs lot and surface vs net all raise `OverlapExisting`; trees and props are ignored. With `bypassValidation` they are still created.
- **Our files:**
  - 22 field–field pairs overlap.
  - **276 decal–decal pairs** overlap (128 squares of 16 m or more).
  - 2 grass–grass pairs overlap.
  - There is no cross-file overlap.
  - Overlaps draw with undefined order (z-fighting) and leave error icons on later edits.
  - **Fix:** union overlapping polygons per prefab in the converter.
- **Agriculture Surface 01:** `SurfaceData` is an empty tag and `SurfacePrefab` has no lot or extractor requirement, so the surface can stand alone. It is probably the spawnable variant behind "Agriculture Surface Placeholder". It is purely visual and does **not** set fertility; the synthetic resources import does that.

### 11. Tree count, performance, import time and file size (low; 508 k is within reach)

**Vanilla maps** (decoded archetype counts):

| | trees | objects with `Transform` | decompressed MapData | `.cok` |
|---|---|---|---|---|
| range | 65 685 (San Francisco) – 338 891 (Delta) | 235 k – 850 k | 167–222 MB | 26–55 MB |
| others | Lakeland 322 k, Tampere 306 k, Sunshine Peninsula 276 k | | | |

- Our 508 k trees are **1.5× the densest vanilla map**. Our total of about 515 k objects is inside the vanilla range (Sunshine Peninsula has 850 k).
- There is no hard object limit in code; trees are instanced, and growth only runs in a city.
- Expect the file to grow by roughly 10–20 MB compressed (estimate; about 30–40 B per tree after zstd).
- Expect editor FPS somewhat below that of the densest vanilla maps.

**Import time:**
- `BridgeObjectToolSystem` handles one batch per Define → Settle (`settleFrames`, default 1) → Apply → Resolve cycle, about 4 frames.
- At `batchSize` 500 that is 1 017 batches, or about 4 100 frames. At 10–30 fps with a dense map, that is **≈ 2.5–7 min in total**, or 15–40 s per 50 k file.
- `/import/*` waits up to 15 min per request, so the margin is ample.
- After the 2 000-tree timing test, raise `batchSize` to 1 000–2 000 (the bridge maximum is 5 000) if a batch applies in under 2 frames.

**Thinning rule (recommended, mostly for quality):**
1. Drop points below sea level or in CS1 water: about 10 k.
2. Drop points within net corridors: about 3.6 k (the game would hide them anyway, but they still cost).
3. Only if the editor drops below about 20 fps after the import: thin the densest cells. Keep at most N trees per 16 m cell (N ≈ 20, i.e. 3 m spacing), and drop bushes under tree canopy, i.e. bushes in cells with ≥ 10 trees (41 k bushes in total).

   This reaches a Delta-like 350–400 k. Decide from the timing test, not up front.

### 12. Minor

- **Kind hints:** the converter writes `kind: "prop"`, but the bridge's `KindOf` returns `"object"` for non-tree, non-plant, non-building prefabs. That gives about 3 659 "kind given … is a object" warnings, capped at 1 000 in the report. Write `kind: "object"`, or let the bridge accept `prop`.
- **Age names:** the bridge maps `seedling` to `AgeMask.Sapling` and `sapling` to `AgeMask.Young`. Keep this in mind for the fix in finding 1.
- **Undo history:** undo records are kept in memory, 50 imports at most (`kMaxRecords`). Eleven tree files plus the other layers fit, but a game restart loses them. Save before the tree import.
- **Air connections:** the 4 airplane outside connections face roughly outward (182°, 77°, 340°, 336°). The bridge snaps them onto the border (`ObjectToolSystem.HandleWorldSize`); `NotOnBorder` is raised only inside bounds −0.1 m.

---

## Prefab names: check result

- There are **117 distinct prefab names** across every `import-*.json` in `out/`, `out/x1/` and `out/trees/`, plus the surfaces files.
- **86 are vanilla** and match `Assets.NAME[...]` in `Locale.cok` exactly, case included. That covers nets, trees and bushes, rocks, props, buildings, outside connections, `Agriculture Surface 01` and `Grass Surface 01`.
- **31 are pack prefabs** and match a backticked prefab name in `cs2-playset/ASSET_PACKS.md` exactly:
  - RZ Rocks: `rzrock1–5,7,8`
  - Arid Rocks: `RZaridrock1–5`
  - Cliffs and Rocks: `CliffRock3/5/6`, `CliffStack`
  - Styx: `Grass01–06 (Styx's)`
  - Ferns: `gaga - Fern 1/2`
  - Leafy Undergrowth: `ReikUndergrowth01/02Trimmed`
  - `pd Long Grass`, `pd Bloom`, `PD-YellowFlower`
  - Dirty Surfaces: `PDO23 Dirty Surface 01`, `PDO23 Soil Surface 01`

**Names not found anywhere: none.**

Caveats:
- Pack names resolve only when the 14 packs are loaded in the playset. The dry run is the final check.
- `pd Long Grass` is used, not the `pd Long Grass Prop ` variant with a trailing space; the inventory lists both.

**Do the prefab choices make sense?**

| Choice | Verdict |
|---|---|
| `Medium Seaway` for CS1 ship paths | **Right.** There is no "Ship Path" or "Shipping lane" prefab. Seaways are the placeable ship lanes ("Requires sufficient clearance under bridges"). See findings 3 and 7. |
| `Medium Boatway` for canals | **Wrong concept.** It is a boat lane needing water, not a channel, and vanilla has no canal net (finding 4). |
| `QuaySmall01` / `QuayMedium01` for quays and retaining walls | They are nets (path / two-lane road with a `Quay` build style), not objects. A wall forms automatically where one side is ≥ 0.4 m lower or in water (69 of 90 small quays at 2048), and can be forced with elevation elsewhere. **Right, with the 21 flat ones forced** (finding 4). |
| `Sound Barrier` for flood walls | Visual only; nets do not block water (finding 4). |
| `High-voltage Line` nodes on the ground | Right; the game adds pylons at ≤ 120 m (finding 9). |
| Grass plants with `age` | `age` is ignored for `PlantObject` and randomised for trees in the editor (finding 1). |

---

## Before the run: converter and bridge changes

**Bridge:**
1. Make tree age deterministic in editor mode (finding 1).
2. Accept `kind: "prop"` as an alias of `object`, or have the converter write `object` (finding 12).

**Converter, trees:**

3. Mask the trees: drop points on terrain below sea level, or in the CS1 water surface once v3 exists, and points inside ground-net corridors (finding 2). This is about 13.8 k fewer trees.

**Converter, connections:**

4. Move the ship outside connection to the seaway end (4718.4, −7168), or drop it if the seaway spawns its own marker (finding 3).
5. Fold the 4 m last rail segment at (7168, 2299.7) into its neighbour. Review the 8 east-border Gravel Road connections (finding 8).

**Converter, walls and channels:**

6. Walls layer (finding 4):
   - Keep the boatways only where the canal holds water.
   - Keep all quays. Force a wall on the 21 flat QuaySmall01 edges (x1; 8 at 2×) in either of two ways:
     - Node elevation = the wall height (≥ 4 m one side, ≥ 8 m both sides).
     - Better, since it doesn't depend on height: a bridge import option that sets `Upgraded` side flags (`Side.Raised` on the wall side), as mods do. A mod can also apply it by hand afterwards.

     Test one forced edge first.
   - Confirm the 7 `QuayMedium01` really are quays; they are roads.

**Converter, water:**

7. Apply the per-source rule (finding 6): drop 8 sources, clamp 3 inward, keep `natural`, never write `polluted` or a negative height. Optionally merge the 21 canal-area sources.
8. Seaways: after the one-edge terrain test, write y = the water surface instead of the riverbed if the seaway digs (finding 7).

**Converter, surfaces:**

9. Union overlapping polygons per prefab: 276 decal pairs and 22 field pairs (finding 10).

**Runbook:**

10. **Decided (user, 2026-10-19):** Run A (2048, `x1`) is the main deliverable. Run B (4096) is still tried as an experiment. For Run B, choose the climate with the snow line in mind, and note that about 70 steep edges and 176 high bridge nodes cannot be re-edited in game without errors (finding 5).
11. Order: set the sea level and apply it before any import. Save before the trees, because undo lives in memory only. Run the timing test, then set `batchSize` (finding 11).
12. Add to NOTES.md item 6: inspect whether the seaway border node at (4718.4, −7168) carries a `Game.Objects.OutsideConnection` sub-object.
