# Map look: climate, water and terrain rendering

Ready request bodies for `POST /map/settings` (bridge 0.9.23 or later; MCP `cs2_map_settings` with the same body as `set`). The values and their reasoning are in [audit/theme-remake.md](../audit/theme-remake.md) §5 (climate) and §6 (water). The field reference is in [cs2mcp/MAP_SETTINGS.md](../cs2mcp/MAP_SETTINGS.md), "Look".

| File | What it sets |
|---|---|
| [climate.json](climate.json) | Base climate `TemperateWaterwayPassClimate`, then an edited copy (saved in the map): sun at 40.0 / −105.3, the four seasons, plus theme, starting month/time and natural disasters |
| [water.json](water.json) | All 22 `WaterRenderSettingsPrefab` fields on an edited copy of whatever water settings the map has (= water-v1) |
| [water-v1.json](water-v1.json), [water-v2a.json](water-v2a.json), [water-v2b.json](water-v2b.json) | Water A/B after the A-v7 review (below): v1 as applied, v2a clearly siltier, v2b the midpoint. Each has all 22 fields plus a `_rationale` key per changed field, which the bridge ignores |
| [terrain.json](terrain.json) | Selects `DefaultTerrainRenderSettings`, no edits (see below) |
| [swatch.mjs](swatch.mjs) | Writes the surface swatch-test body |

## Owner decisions (2026-10-20), encoded in the files

| Question (theme-remake.md) | Decision | In the files |
|---|---|---|
| 1. Cold season | **In between:** a short cold season with occasional light snow, at about a third of Colorado's cold-season precipitation chance. No persistent white months | climate.json precipitation (chance / amount / deviation, %): spring **9 / 20 / 15**, autumn **5 / 15 / 10**, winter **6 / 15 / 10**; summer unchanged at 25 / 35 / 30 |
| 2. All four seasons on the summer season prefab | **Yes, since the bridge supports it:** every season entry has `"prefab": "SeasonSummer"`. Foliage stays green all year; snow can still fall | delete the four `"prefab"` keys to keep the base's own seasons |
| 3. Water | **In between, closer to real:** 60 % silty Mississippi, 40 % CS1 slate. The depth rule still keeps the shallow mountain creeks clear | water.json: `waterColor` **[0.28, 0.48, 0.36]**, `waterScatteringColor` **[0.18, 0.25, 0.20]**, `absorptionDistance` **3.8**, `ambientScattering` **0.5**, `causticsIntensity` **0.6**; the other 17 fields are unchanged |
| 4. Lumina in the map description | **Yes,** recommended as optional (text below) | set the description by hand |
| 5. Sand/grass swatch | **Settled by the live swatch test** (2026-10-20, below). No grass surfaces at all; surfaces only where CS2 terrain shading cannot do the job, with organic outlines (theme-remake.md §4) | — |

**Surface swatch result** (live, dakota-A5-2048 at (5500, 2500); the earlier guess from the UI icons was wrong for grass):

| Prefab | Reads as in game | Use |
|---|---|---|
| `Sand Surface 01` | pale tan-beige sand, faint raking lines | **bank sand and sand bars** (rule S1) |
| `Sand Surface 02` | grey-brown ground with fine furrows | small bare-earth patches on gentle slopes only (rule S3, ≤ 0.05 km²) |
| `Grass Surface 01` / `02` | mown lawn with mowing stripes; 02 slightly lighter | **not used.** No vanilla surface reads as dry prairie grass; the plains keep CS2's own terrain grass |
| `Agriculture Surface 01` | ploughed field | field circles (rule A1) |
| `Pavement Surface 01` | dark grey asphalt | under stand-in buildings (rule P1) |
| `Concrete Surface 01` | near-white | dam crest and plant pad only |
| `Landfill Surface 01`, `Pavement Surface 02`, `Concrete Surface 02` | rubble, light asphalt, white | not used |

## Apply order

Run on the open map in the editor, after terrain, water sources and sea level.

1. **Check the names** (once per game version):
   - `GET /prefabs?category=climate&detail=1&query=WaterwayPass`: it must list `TemperateWaterwayPassClimate`, and its `seasons` should show four entries.
   - `GET /map/look/schema`: `season.m_Prefab` must be `writable`.
2. **`POST /map/settings` with climate.json.** Read the response:
   - `applied` should start with "'TemperateWaterwayPassClimate' is read-only; editing the copy …" and then list each field.
   - If `problems` says `no SeasonPrefab named 'SeasonSummer'; available: …`, put the summer name from that list into the four `prefab` keys and post again.
   - If the base has more or fewer than four seasons, `problems` names the out-of-range index. Then add `{"index": N, "remove": true}` for extras.
   - The season values do not depend on the base's order: every entry sets all its fields, including `startTime`. That holds as long as the prefab keys apply. Without them, map the values to the base's entries by their `season` name: spring 0.0833, summer 0.25, autumn 0.8333, winter 0.9167.
3. **`POST /map/settings` with water.json.** `applied` lists 22 fields on the copy.
4. **`POST /map/settings` with terrain.json.**
5. **Read back** with `GET /map/settings?detail=1`:
   - `look.climate` is a "(copy)" with latitude 40 / longitude −105.3;
   - the water copy has the values above;
   - the terrain is `DefaultTerrainRenderSettings`.
6. **Save the map**, reload it, and check the read-back again. The copies must survive, and so must latitude/longitude (the climate copy restores them on load).
7. **Screenshots** (`cs2_screenshot`):
   - the falls (white foam);
   - the reservoir from the dam (calm, grey-green, opaque);
   - a western creek (bed visible, blue-green);
   - the lower river at overview height (slate, small chop);
   - a summer evening (possible fog).

   Expected tweaks: `absorptionDistance` ±2 if the creeks look muddy or the river looks glassy, and `waterScatteringColor` toward blue if it reads swampy.

**Why terrain.json makes no edits:** the terrain textures cannot be written from code (asset references; the panel's image picker only), so the CS1 textures cannot come back. The numeric fields (tiling, blur, splat, triplanar) have no CS1 counterpart worth guessing blind. The only candidate is the rock tiling: CS1 stretched its cliff texture to a ≈220 m repeat, while CS2's near rock tiling is 1600 (≈9 m repeat on a 14 km map). If the cliffs look busy in the screenshots, try `{"terrainEdit": {"terrainRockTiling": {"nearTiling": 800}}}`, judge, and undo by posting 1600.

## Water A/B (owner review of A-v7, 2026-10-20)

Owner: "water coloring not correct". In `next-run/shots-a7/22-fort-ruins.png` the main river reads dark slate blue, near black in the deep channel.

**Cause:**
- **Body colour too dark.** v1's `waterScatteringColor` (0.18, 0.25, 0.20) is darker than the vanilla default (0.19, 0.33, 0.39). In deep water the body colour comes almost entirely from that scattering colour times the ambient/sun scattering terms (ambient only 0.5), so the channel goes near black.
- **Sky reflection.** The high smoothness reflects the blue sky at the oblique camera angle, which adds the slate-blue cast.
- **Wrong lever.** The "60 % silty" blend moved the hue but not the brightness, so the silt never shows.

| Field | v1 (applied) | v2b (midpoint) | v2a (siltier) |
|---|---|---|---|
| `waterColor` (refraction tint) | 0.28, 0.48, 0.36 | 0.40, 0.475, 0.33 | 0.52, 0.47, 0.30 |
| `waterScatteringColor` | 0.18, 0.25, 0.20 | 0.30, 0.325, 0.23 | **0.42, 0.40, 0.26** |
| `absorptionDistance` m | 3.8 | 3.0 | 2.2 |
| `maxRefractionDistance` m | 2 | 1.75 | 1.5 |
| `ambientScattering` | 0.5 | 0.675 | **0.85** |
| `heightScattering` | 0.2 | 0.25 | 0.3 |
| `displacementScattering` | 0.1 | 0.15 | 0.2 |
| `directLightTipScattering` | 0.4 | 0.45 | 0.5 |
| `directLightBodyScattering` | 0.35 | 0.475 | 0.6 |
| `foamAmount` | 0.9 | 1.05 | 1.2 |
| `causticsIntensity` | 0.6 | 0.45 | 0.3 |
| `smoothnessClose` | 0.92 | 0.9 | 0.88 |
| `smoothnessDistant` | 0.78 | 0.74 | 0.7 |

The other 9 fields are unchanged (waves, lake waves, minimum depth for waves, sea wind, foam fade range, caustics depth, ripples, smoothness range). The clear-creek rule still holds at v2a: a 0.5 m creek transmits about 80 % at 2.2 m absorption.

**How to A/B:** post v2b, then v2a (each edits the same map copy, no new clone). Screenshot the same camera and time as `22-fort-ruins.png` (17:56, June) plus one noon shot. If v2a is too muddy, the next step is v2b's scattering colour with v2a's ambient 0.85, since brightness, not hue, was the main problem. Keep the winner as `water.json`.

## Terrain material slots (Dry / Lush / Dirt / Sand) and "DesertTerrainRenderingSettings"

From decompiled `Game.dll` (`TerrainMaterialSystem`, `TerrainMaterialProperties`, `TerrainMaterialType`, `TerrainRenderSettingsPrefab`):

- **What the brushes paint.** The editor's terrain material brushes are terraforming prefabs with a `TerrainMaterialProperties.m_Type` of `Extra1`…`Extra4`. Painting writes only a weight into one channel of the map's user splatmap. The brush itself carries **no texture**.
- **Where the textures come from.** `TerrainMaterialSystem.ApplyRenderSettings` binds the shader's `colossal_TerrainExtra1…4Diffuse/Normal` from the **current `TerrainRenderSettingsPrefab`'s `m_Extra1…4Diffuse/Normal`**, with its `m_ExtraNDirtOverride`, `SplatRange`, `BlurDepth` and `HeightOffset`. Rock, dirt and grass base textures come from the same prefab.
- **Fallback.** If a slot's texture is missing, the code falls back to the new-map default prefab (`NewMapSettingsData.m_DefaultTerrainRenderSettings`) for that slot, logging "Terrain texture missing. Setting fallback texture."
- **So yes: the selected terrain render settings decide what "Dry" looks like.** If the map's current set is (or was) `DesertTerrainRenderingSettings`, its Extra slots carry desert textures, and the slot behind "Dry" can well be a grey gravel/scree texture.
- **The Packages entry.** The Workspace "Packages" list shows the asset packages the open map uses. `DesertTerrainRenderingSettings` there means the map references or embeds that set (possibly from the map it was created from, or an earlier pick). terrain.json selects `DefaultTerrainRenderSettings`, but the runner's "1 applied" only says the pick ran; whether the package is still the *current* one, or just a leftover dependency, has to be read live.
- **Not possible offline:** I could not read the Extra textures of either set, because the vanilla prefabs sit in compressed bundles and the bridge was offline. A save-embedded `DefaultTerrainRenderSettings` from October 2023 had **all four Extra slots empty** (zero asset ids, an older prefab format). The current game's Default may differ.

**Checks to run live (read-only):**
1. `GET /map/settings?detail=1` → `look.terrainRenderSettings`: which prefab is current, and its `m_Extra1…4Diffuse` asset ids.
2. `GET /prefabs?category=terrainRenderSettings&detail=1`: every terrain set with its Extra slot ids. Compare `DefaultTerrainRenderSettings` and `DesertTerrainRenderingSettings`.
3. In the editor's terrain material tool, note which brush is Extra1–4 (the brush order or tooltip). Then pick the terrain set whose slot behind "Dry" is a dry-grass/earth texture, not scree.

**Choosing a set:**
- If no vanilla set has a good dry slot, the textures can only be changed in the panel's image picker (texture references are not writable from code).
- The numeric slot settings *are* writable through `terrainEdit`, e.g. `extra1DirtOverride` (0–1, how much the slot replaces dirt) and `extra1HeightOffset` (−0.4…0.4, height blend), but they change blending, not the texture.

## Apply status (first live run, 2026-10-20, dakota-A5-2048)

| Body | Runner result | Verdict |
|---|---|---|
| climate.json | 62 fields applied | **clean.** 62 = climate pick + 5 climate fields + 4 seasons × 13 fields (including the four `SeasonSummer` references, so that name resolved) + theme, starting month, starting time, natural disasters |
| water.json | 22 sent, `scatteringColor` skipped ("not in the schema") | **my key was wrong:** the field is `m_WaterScatteringColor`, key `waterScatteringColor`. Fixed in water.json; re-post water.json (all 22 apply to the existing copy, no new clone) |
| terrain.json | 1 applied | **clean** (`DefaultTerrainRenderSettings` selected) |

**To check:** the swatch screenshot shows **31 °C at 08:39 in June**. With the edited seasons, an early-June morning should read about 17–22 °C (night ≈ 9, day ≈ 23 in early June; 08:39 is about 80 % of the way from night to day). Either the edited copy is not the current climate when the clock runs, or the curves were not rebuilt. Check `GET /map/settings?detail=1`: `look.climate` must be the "(copy)" with latitude 40, and its summer season must show `tempNightDay` [12, 28]. Then let the clock run to noon (expected ≈ 23–25 °C in early June). If it still reads the vanilla Waterway Pass temperatures, post climate.json again, save, reload and re-check.

## Swatch test (surfaces, before the paint plan)

Done once (result above); rerun only after a game update. Purpose: decide which vanilla surface reads as brown earth, light bank sand and dry grass (theme-remake.md §4, rules S1, S3, G1, G2).

1. Pick a flat, open, dry spot near the start area and note its south-west corner (x, z).
2. `node look/swatch.mjs <x> <z> out/swatch.json`. This writes ten 60 × 60 m squares, five per row with 10 m gaps: Grass 01/02, Sand 01/02, Agriculture 01, then Landfill 01, Pavement 01/02, Concrete 01/02.
3. `cs2_import_surfaces` with that file (or `POST /import/surfaces`).
4. Take one `cs2_screenshot` from about 300 m up, sun high, and one at a low angle.
5. Record which of `Sand Surface 01/02` is the lighter bank sand and which the browner bare earth, and which of `Grass Surface 01/02` reads drier/yellower. Feed that into the converter's surface mapping.
6. Undo with `POST /import/objects/undo?name=look-swatch` (MCP `cs2_undo_objects`).

## Map description: optional companion

Text for the description, set by hand in Map Settings:

> Optional: for the original Dakota Falls colour grade, use Lumina with the "Dakota Falls" preset (Relight2Natural LUT). The map looks right without it.

The preset and the converted LUT are in [cs1-playset/theme/lumina/](../cs1-playset/theme/lumina/) (see `LUMINA-MAPPING.md`).
