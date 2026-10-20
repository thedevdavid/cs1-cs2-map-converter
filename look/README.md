# Map look: climate, water and terrain rendering

Ready request bodies for `POST /map/settings` (bridge 0.9.23 or later; MCP `cs2_map_settings` with the same body as `set`). The values and their reasoning are in [audit/theme-remake.md](../audit/theme-remake.md) §5 (climate) and §6 (water). The field reference is in [cs2mcp/MAP_SETTINGS.md](../cs2mcp/MAP_SETTINGS.md), "Look".

| File | What it sets |
|---|---|
| [climate.json](climate.json) | Base climate `TemperateWaterwayPassClimate`, then an edited copy (saved in the map): sun at 40.0 / −105.3, the four seasons, plus theme, starting month/time and natural disasters |
| [water.json](water.json) | All 22 `WaterRenderSettingsPrefab` fields on an edited copy of whatever water settings the map has (= water-v1) |
| [water-v1.json](water-v1.json), [water-v2a.json](water-v2a.json), [water-v2b.json](water-v2b.json) | Water A/B after the A-v7 review (below): v1 as applied, v2a clearly siltier, v2b the midpoint. Each has all 22 fields plus a `_rationale` key per changed field, which the bridge ignores |
| [terrain.json](terrain.json) | Selects `DefaultTerrainRenderSettings`, no edits (superseded by terrain-cs1.json) |
| [terrain-cs1.json](terrain-cs1.json) | Edited copy of `DefaultTerrainRenderSettings` for the **CS1 theme-mix textures**: legacy texture format on, CS1 tiling. The textures themselves are set in the panel (below) |
| [swatch.mjs](swatch.mjs) | Writes the surface swatch-test body |
| [season-test.md](season-test.md) | Every season with screenshots: dates, camera spots, expected values, pass/fail |
| [tools/export-theme.mjs](tools/export-theme.mjs) | Extracts the CS1 theme-mix textures from the theme `.crp` files into `textures/` (regenerable, gitignored; `textures/manifest.json` is kept) |

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
| `Grass Surface 02` | mown-lawn grass, a little lighter, faint stripes | **not used (owner policy 2026-10-20: paint with the editor brushes, not area surfaces).** Dry grass comes from the **Dry** brush with the CS1 texture |
| `Grass Surface 01` | darker lawn | not used |
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

**terrain.json vs terrain-cs1.json:** terrain.json only picks the vanilla set. terrain-cs1.json makes an editable copy for the CS1 textures (next section). The texture fields cannot be written from code (asset references), so they are assigned once by hand in the panel; the numeric fields come from the file.

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

**Camera spot for every water A/B shot** (run A, x1 frame = CS2 metres; `POST /camera/set` / `cs2_set_camera`):

| Shot | x | z | angleX (yaw) | angleY (pitch) | zoom | Time of day |
|---|---|---|---|---|---|---|
| W1 main channel, looking upstream | **600** | **−2750** | **0** | **35** | **700** | 17:56 (as `22-fort-ruins.png`) |
| W2 same spot | 600 | −2750 | 0 | 35 | 700 | 12:00 |
| W3 close look at the shallows | 600 | −2750 | 0 | 55 | 250 | 12:00 |

- **Where:** the wide main channel 600 m east of the fort keep (OldCastle01 at 5, −2619). From the CS1 water raster the channel there is about 400–800 m wide and runs north–south, so yaw 0 looks straight upstream.
- **Height:** `y` is left out (the bridge samples the terrain).
- **Yaw convention:** check it once. If W1 does not look along the channel, read `GET /camera` after framing it by hand and replace the numbers here.
- **Repeatable conditions:** pause the clock, keep the same date (June), and take all three shots for v1, v2b and v2a in one session, so sun and weather match.

**How to A/B:** post v2b, then v2a (each edits the same map copy, no new clone). Screenshot the same camera and time as `22-fort-ruins.png` (17:56, June) plus one noon shot. If v2a is too muddy, the next step is v2b's scattering colour with v2a's ambient 0.85, since brightness, not hue, was the main problem. Keep the winner as `water.json`.

## CS1 theme-mix textures in the terrain render settings

The textures the CS1 map really used (theme mix `2972881605`, audit/theme-remake.md §1), extracted and decoded from the theme `.crp` files:

```bash
node look/tools/export-theme.mjs
```

This writes `look/textures/raw/` (as stored in CS1) and `look/textures/gain16/` (RGB ×1.6). CS1's terrain shader brightens its textures, so stored as-is they are dark; try gain16 first. Each set has 14 PNGs named like a Map Texture Replacer pack (`<Slot>_BaseColor.png`, `<Slot>_Normal.png`).

| Slot (panel field) | Editor brush | CS1 source | Normal |
|---|---|---|---|
| Grass (`m_GrassDiffuse` / `m_GrassNormal`) | base ground | Seychelles grass (mix grass) | generated from luminance |
| Cliff → Rock (`m_RockDiffuse` / `m_RockNormal`) | slopes (automatic) | Springwood cliff (mix cliff) | Owl's `CliffSandNormal` RG (the mix's cliff normal) |
| Dirt (`m_DirtDiffuse` / `m_DirtNormal`) | base dirt | Springwood ruined (mix ruined) | generated |
| Extra1 | **Dry** | r69 Coastal grass (olive-brown; the driest grass among the mix's themes) | generated |
| Extra2 | **Lush** | Springwood grass | generated |
| Extra3 | **Dirt** | Seychelles gravel (mix gravel) | generated |
| Extra4 | **Sand** | Owl's Albedo sand (mix sand) | Owl's `CliffSandNormal` BA (the mix's sand normal) |

The brush ↔ slot mapping is from the game's locale (`Assets.DESCRIPTION[Terrain Material Extra 1..4]`: Dry, Lush, Dirt, Sand) and `TerrainMaterialType` Extra1–4.

**A-v8 file list** (brightened set first; the raw set is the fallback if gain16 looks too light). Folder: `D:\work\mods\csl2-editor\look\textures\gain16\` (raw: `...\textures\raw\`).

| # | Panel field (Terrain Rendering Settings) | File | Size | Tiling (terrain-cs1.json: far / mid / near) |
|---|---|---|---|---|
| 1 | Rock Diffuse | `Cliff_BaseColor.png` | 2048² | rock 40 / 65 / 260 |
| 2 | Rock Normal | `Cliff_Normal.png` | 2048² | (rock) |
| 3 | Dirt Diffuse | `Dirt_BaseColor.png` | 2048² | dirt 40 / 172 / 688 |
| 4 | Dirt Normal | `Dirt_Normal.png` | 2048² | (dirt) |
| 5 | Grass Diffuse | `Grass_BaseColor.png` | 2048² | grass 40 / 143 / 572 |
| 6 | Grass Normal | `Grass_Normal.png` | 2048² | (grass) |
| 7 | Extra 1 Diffuse (brush **Dry**) | `Extra1_BaseColor.png` | 4096² | uses the grass tiling |
| 8 | Extra 1 Normal | `Extra1_Normal.png` | 4096² | |
| 9 | Extra 2 Diffuse (brush **Lush**) | `Extra2_BaseColor.png` | 2048² | grass tiling |
| 10 | Extra 2 Normal | `Extra2_Normal.png` | 2048² | |
| 11 | Extra 3 Diffuse (brush **Dirt**) | `Extra3_BaseColor.png` | 2048² | grass tiling |
| 12 | Extra 3 Normal | `Extra3_Normal.png` | 2048² | |
| 13 | Extra 4 Diffuse (brush **Sand**) | `Extra4_BaseColor.png` | 4096² | grass tiling |
| 14 | Extra 4 Normal | `Extra4_Normal.png` | 4096² | |

The files are regenerated by `node look/tools/export-theme.mjs` (≈ 25 s) and are gitignored. They exist now on this machine.

**Can the picker reach them?** Yes (decompiled `DualImagePickerPopupField`, `FilePickerPopup`, `DirectoryBrowserPanel`):
- The folder button ("Select from disk") opens a `DirectoryBrowserPanel` with **no root limit**: it lists every ready drive (`DriveInfo.GetDrives()`), so `D:\work\...` is reachable.
- It opens at the last folder used for that field (`SharedSettings.editor.lastPickerDirectory`, else `lastTextureAssetDirectory`). After the first file, the rest are one click away.
- The import (`DefaultTextureImporter`, BC7) is saved into the **user asset database** (`AssetDatabase.user.AddAsset`) and the field gets that asset's guid. So on every later fresh map the 14 textures are already in the picker's **asset list** (the left part of the field), with no disk browsing. And the map embeds them on save.

**For the bridge:** the same path works from code. Run `DefaultTextureImporter.Import` with the field's `TextureImportAttribute` settings, then `AssetDatabase.user.AddAsset` + `Save`, then set the field to `new AssetReference<TextureAsset>(guid)`, then `TerrainMaterialSystem.ApplyRenderSettings()`. On the second and later maps, assigning by the guid of the already-imported asset avoids re-importing.

**Apply (in the editor, once per map):**
1. `POST /map/settings` with **terrain-cs1.json**. This creates the editable copy "DefaultTerrainRenderSettings (copy)", sets `m_UseLegacyTextureFormat` and the CS1 tiling. The legacy format means plain diffuse PNGs and standard normal maps for grass/dirt/rock; it is what Map Texture Replacer sets for user PNGs.
2. Editor → Environment → Terrain Rendering Settings. For each of the 14 texture fields, use the image picker and choose the matching file from `look/textures/gain16/`. The import is BC7-compressed and embedded in the map, so no mod is needed.
3. Save, reload, check: cliffs warm grey-brown, grass dark olive, the brushes show CS1 textures. Paint one stroke of each brush to see it.
4. If a brush looks wrongly lit (the Extra slots' normals may expect the new packed format), clear that slot's normal; the game then uses its flat fallback normal.
5. Alternative without touching the map: Map Texture Replacer (installed) can load the same folder as a pack, but only per player.

**Tiling in terrain-cs1.json** (repeats per layer; starting values, judge in game):
- **mid** = 14 336 m ÷ the CS1 repeat (grass 100 m → 143; dirt 83 m → 172; rock 222 m → 65);
- **near** = 4 × mid (CS1's textures hold 5 cm/px, enough up close);
- **far** = the default 40.

The Extra slots reuse the grass tiling (`TerrainMaterialSystem` binds `_TerrainExtraTextureTiling` from `m_TerrainGrassTiling`). If the playable area is not the tiling unit, all three scale together.

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
