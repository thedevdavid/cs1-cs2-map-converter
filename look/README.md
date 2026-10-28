# Map look: climate, water and terrain rendering

Ready request bodies for `POST /map/settings` (bridge 0.9.23 or later; MCP `cs2_map_settings` with the same body as `set`). The values and their reasoning are in [audit/theme-remake.md](../audit/theme-remake.md) §5 (climate) and §6 (water). The field reference is in [cs2mcp/MAP_SETTINGS.md](../cs2mcp/MAP_SETTINGS.md), "Look".

| File | What it sets |
|---|---|
| [climate.json](climate.json) | Base climate `TemperateWaterwayPassClimate`, then an edited copy (saved in the map): sun at 40.0 / −105.3, the four seasons, plus theme, starting month/time and natural disasters |
| [water.json](water.json) | All 22 `WaterRenderSettingsPrefab` fields on an edited copy of whatever water settings the map has. **The chosen water set** (owner after A-v8); no variants unless the owner asks |
| [terrain.json](terrain.json) | Selects `DefaultTerrainRenderSettings`, no edits (superseded by terrain-cs1.json) |
| [terrain-cs1.json](terrain-cs1.json) | Edited copy of `DefaultTerrainRenderSettings` for the **CS1 theme-mix textures**: legacy texture format on, CS1 tiling, and the Extra3/Extra4 splat/blur values (`terrainEdit`). The runner posts it in the post-save phase, before the textures |
| [swatch.mjs](swatch.mjs) | Writes the surface swatch-test body |
| [season-test.md](season-test.md) | Every season with screenshots: dates, camera spots, expected values, pass/fail |
| `textures/current/` | The CS1 theme-mix export (14 PNGs, gitignored except `manifest-current.json`); the base of every later set |
| `textures/v3/*.u1.png` | **The terrain texture set in use** (gitignored except `manifest-v3.json`): CS1 gloss alpha, graded Extra2, 4K Cliff/Dirt/Extra3. Rebuild path below |
| [tools/](tools/) | The texture pipeline: `export-theme.mjs`, `export-current.mjs`, `make-v3.mjs`, `make-extras.mjs`, `make-up4k-set.mjs` and helpers |

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

## Climate: verified (A-v9b, 2026-10-21)

Measured with `look/season-test.mjs` on dakota-A9c-2048, bridge 0.9.41, mod overrides cleared (that report was removed in the 2026-10-28 cleanup; the latest one is `next-run/shots-seasons/a24/report.md`, PASS 9/9):
- **All nine shots PASS:** live = sampled curve value.
- **Noon °C per shot:** Jan 11.8, Jan 06:00 4.8, Mar 14.1, May 25.7, Jul 28.6, Jul 20:00 17.0, Sep 23.3, Nov 14.3, Dec 10.5.
- **Other checks:** every month's noon value is within ±7 °C of the design; no plains snow is possible April–October.
- **Curve keys:** 289 is correct (288 hourly keys plus the closing key `LoopCurve` appends at t = 12); the check now expects 289.

**Temperatures: no change.** The shoulder months read 2–4 °C above the design targets (May +3.7, Sep +2.8, Jan +2.5); summer is on target (+0.6).
- That is the game's own smoothing (`SmoothTangents` on the season keys) plus one fixed Gaussian draw per month (`randomSeed` 1, σ ≈ 3 °C), not a wrong setting.
- Against the real Front Range: Denver highs Jan 7 / May 21 / Sep 25 °C. Mild winter days and a warm May suit the CS1 endless-summer brief.
- If the owner wants the shoulders cooler, lower the spring and autumn day value by 2 (`tempNightDay` [0, 11]) rather than changing the seed.

**Snow: changed after A-v9b.** No snow in any shot, not even on the summits, and that was **not** the design intent ("occasional light snow").
- **Cause:** the curves multiply precipitation by cloud cover (× cloud/0.7, plus × cloud/0.4 below 0.4) and zero it below 0.2 cloud. With the cold seasons at 25–35 % cloud and 15–20 % amount, an event peaked around 0.05, under the 0.1 snow threshold, so it could never snow.
- **Fix in climate.json:**

  | Season | cloudAmount | precipitationAmount |
  |---|---|---|
  | Spring | 35 → **45** | 20 → **45** |
  | Autumn | 25 → **40** | 15 → **35** |
  | Winter | 30 → **45** | 15 → **40** |

  The precipitation **chances are unchanged** (9 / 5 / 6 %), so snow stays occasional; events now reach about 0.2–0.3.
- **What to expect** (run A, summits about 7.7 °C colder): the summits get snow on cold-season nights when an event happens (Jan midnight −9.6 °C, noon +4 °C, so mostly melting by day). The plains get a rare night-time dusting (Jan midnight −1.9 °C) that melts by noon.
- **Re-check after re-posting climate.json:** the test now runs a **full-year hourly snow scan** from the curves (24 samples per month, no running clock). It checks: no plains snow hours April–October; at least some summit snow hours December–March; plains snow at most 10 % of cold-season hours. The scan table is in the report. Screenshots alone cannot prove snow, because one 120 s settle rarely coincides with an event.

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

## Water: water.json is the chosen set

**Decision (owner, A-v8):** keep the first version. `water.json` is the set the runner posts; the darker/siltier A/B variants were rejected ("ugly") and their files were deleted on 2026-10-28. If the river ever needs another pass: deep-channel brightness comes from `waterScatteringColor` × the ambient/sun scattering terms (ambient 0.5 here), so raise those rather than shifting the hue; `absorptionDistance` ±2 for muddy creeks or glassy river.

**Camera spot for water shots** (run A, x1 frame = CS2 metres; `POST /camera/set` / `cs2_set_camera`; shot W1 of the review scripts):

| Shot | x | z | angleX (yaw) | angleY (pitch) | zoom | Time of day |
|---|---|---|---|---|---|---|
| W1 main channel, looking upstream | **600** | **−2750** | **0** | **35** | **700** | 17:56 |
| W2 same spot | 600 | −2750 | 0 | 35 | 700 | 12:00 |
| W3 close look at the shallows | 600 | −2750 | 0 | 55 | 250 | 12:00 |

The wide main channel 600 m east of the fort keep runs north–south there, so yaw 0 looks straight upstream; `y` is left out (the bridge samples the terrain). Pause the clock and keep the same date for comparable shots.

## CS1 theme-mix textures in the terrain render settings

The textures the CS1 map really used (theme mix `2972881605`, audit/theme-remake.md §1), extracted and decoded from the theme `.crp` files. Each set has 14 PNGs named like a Map Texture Replacer pack (`<Slot>_BaseColor.png`, `<Slot>_Normal.png`).

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

**Current state (2026-10-28):**
- `look/textures/current/` is the CS1 export: brightened ×1.6 for every slot (CS1's terrain shader brightens its textures), Dirt and Extra3 one step lighter (×1.9, 10 % less saturated; owner after A-v8). 2048² except Extra1 and Extra4 (4096²). It is the runner's default `--textures-dir`, but no longer the set in use.
- `look/textures/v3/*.u1.png` is **the set in use** (owner, 2026-10-22; final on `Dakota Falls.cok` and in every run since). It adds three things to `current/`: the CS1 per-pixel alpha on the 7 BaseColor files (alpha = smoothness; scaled so the brightest pixel is 66, Grass/Extra2 uniform 60), Extra2 graded away from the base grass so the Lush brush shows, and Real-ESRGAN 4K Cliff/Dirt/Extra3 blended with a wrap-aware Lanczos 2× (Grass and Extra2 stay 2048, their ESRGAN output came out painterly).
- The intermediate sets were deleted on 2026-10-28; only `current/` and the `.u1` files remain.

**Rebuild path** (from the CS1 theme `.crp` files; every output is gitignored):
1. `node look/tools/export-theme.mjs` then `node look/tools/export-current.mjs` → `look/textures/current/`.
2. `node look/tools/make-v3.mjs --max 66 --tag .s66` → `v3/*.s66.png` (gloss alpha).
3. `node look/tools/make-extras.mjs --from .s66 --tag .e2 --cs1` → `v3/*.e2.png` (Extra2 graded).
4. Real-ESRGAN x4plus (ncnn portable in `tools-bin/realesrgan`) on Cliff, Dirt and Extra3 BaseColor with 64 px wrap padding (`look/tools/wrap-pad.mjs` before, `crop-down.mjs` after) → `look/textures/up4k/`, plus the Lanczos 2× reference (`lanczos2x.mjs`) in `up4k/lanczos/`. Exact, reproducible commands per file `<f>` (verified 2026-10-28, byte-identical rebuild): `node look/tools/wrap-pad.mjs current/<f> 64 up4k/pad/<f>`; `tools-bin/realesrgan/realesrgan-ncnn-vulkan.exe -i up4k/pad/<f> -o up4k/x4/<f> -n realesrgan-x4plus -s 4 -t 256 -m <models dir>` (**`-t 256` is required**: the automatic tile size is not deterministic and changes the output); `node look/tools/crop-down.mjs up4k/x4/<f> 256 2 up4k/<f>`; `node look/tools/lanczos2x.mjs current/<f> up4k/lanczos/<f>`. The intermediates (`v3/*.s66.png`, `*.e2.png`, `up4k/pad|x4|lanczos`) are kept on disk until the next cleanup; a full rebuild takes about 2 minutes.
5. `node look/tools/make-up4k-set.mjs --tag .u1 --w 0.6` → `v3/*.u1.png` (60/40 blend, colour-matched; RGB means exact, no seams; compare strips in `up4k/compare/`).

**Apply:** the runner does it on a saved and reloaded map (terrain render edits are refused on never-saved maps): `node tools/import-run.mjs --scale x1 --phase post-save --only textures --textures-dir D:/work/mods/csl2-editor/look/textures/v3 --textures-tag .u1` from `net-converter/`. That posts terrain-cs1.json (tiling, legacy format, Extra3/4 blend values), then the 14 files through `POST /map/terrain-textures`; save afterwards. The imports are BC7, stored in the user asset database and embedded in the map on save, so no mod is needed to play it. Bridge 0.9.63 names each texture asset by a hash of the file bytes, so a changed file is always re-imported (0.9.62 reused assets by name and silently kept old pixels; that is why the sets carry tags).

**By hand instead** (Editor → Environment → Terrain Rendering Settings): the image picker's "Select from disk" browser has no root limit, so `D:\work\...` is reachable; pick the 14 files from `look/textures/v3/` after posting terrain-cs1.json. If a brush looks wrongly lit, clear that slot's normal (the game falls back to a flat normal).

**Tiling in terrain-cs1.json** (repeats per layer):
- **mid** = 14 336 m ÷ the CS1 repeat (grass 100 m → 143; dirt 83 m → 172; rock 222 m → 65);
- **near** = 4 × mid (CS1's textures hold 5 cm/px, enough up close);
- **far** = the default 40.

The Extra slots reuse the grass tiling (`TerrainMaterialSystem` binds `_TerrainExtraTextureTiling` from `m_TerrainGrassTiling`). Extra3/Extra4 use the grass brush's blend (`m_Extra3/4SplatRange` 0.35, `m_Extra3/4BlurDepth` 0.22; owner, 2026-10-22: the defaults painted "too rectangular" edges).

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
