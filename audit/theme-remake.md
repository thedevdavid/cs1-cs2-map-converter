# Dakota Falls: remaking the CS1 theme mix with vanilla CS2 options

Written 2026-10-20. Findings only. Owner constraints: **vanilla CS2 only** (no texture packs, no decals), the CS1 look remade in the editor. This supersedes the pack-based surface choices in [theme.md](theme.md) §3; the CS1 facts there still hold.

Sources (all read offline; the bridge was not running):
- `D:\SteamLibrary\steamapps\workshop\content\255710\2972881605\ThemeMix.xml` (copy in `cs1-playset/theme/DakotaFalls-ThemeMix.xml`) and the six source theme `.crp` files (their `MapThemeMetaData` fields, texture assets).
- CS2 `Content/Game/Maps/*.cok` MapMetadata (theme, temperature range, cloudiness, precipitation, lat/long) and `Locale.cok` `Maps.MAP_TITLE[...]` (file name → in-game map name).
- Climate and render-settings prefabs I could decode: `TemperateArchipelagoHavenClimate (copy)`, `BarrierIslandWaterRenderSettings`, `DefaultTerrainRenderSettings` (embedded in the user's saves of 14–15 October), `Twin Dams_WaterRenderSettings` (a recovery save), and the PDX mod prefabs `GrandTetonsClimateFinal` / `GrandTetonsWaterRenderSettingsFinal` (148529) and `TemperateGreatBasinDesertClimate` (163650).
- `audit/cs2-side.md` (snow lapse rate), `cs2mcp/NOTES.md` and `MAP_SETTINGS.md` (lat/long reset by the climate), `net-converter/out/surfaces-summary.md` (CS1 surface classes present), `cs1-playset/theme/LUMINA-MAPPING.md`.

## 1. The CS1 theme mix, every setting

Base: map environment **Sunny** (vanilla temperate), no map theme; the mix is applied over it. A slot without a value in the XML takes the source theme's value, listed here from that theme's metadata.

### Terrain

| Slot | Source theme | Tiling (mix) | Value / texture | Texture average colour* |
|---|---|---|---|---|
| Grass diffuse | Seychelles | 0.01 (≈100 m repeat) | 2048² DXT5 | `#262c18` dark olive |
| Cliff diffuse | Springwood | 0.0045 (≈220 m) | 2048² | `#403931` warm grey-brown |
| Sand diffuse | Owl's Albedo | 0.006 (≈170 m) | 4096² | `#453c32` brown |
| Gravel diffuse | Seychelles | 0.02 (50 m) | 2048² | `#322b26` dark brown |
| Ruined diffuse | Springwood | 0.012 (≈85 m) | 2048² | `#3e362c` brown dirt |
| Pavement diffuse | Owl's Albedo | 0.0235 (≈43 m) | 2048² | `#383838` neutral grey |
| Oil diffuse | Springwood | 0.003 | 2048² | `#55413f` |
| Ore diffuse | r69 Coastal | 0.005 | 2048² | `#0b180b` (channel-packed, not a colour) |
| Cliff/sand normal | Owl's Albedo | 0.0015 (≈670 m) | 2048² | — |
| Grass field colour offset | Seychelles | — | (−0.002, −0.006, −0.001): fields slightly darker/less green | |
| Grass fertility colour offset | Owl's Albedo (mix value) | — | (+0.0189, +0.0128, −0.0007): fertile land **warmer, yellower** | |
| Grass forest colour offset | Los Cabos (mix value) | — | (−0.0092, −0.0056, −0.0104): ground under forest **darker** | |
| Grass pollution colour offset | Owl's Albedo | — | (0, 0, 0): no pollution tint | |
| Grass / fertile / rocks 3D detail | Owl's Albedo | — | all **off** | |

\* Mean of the DXT5 block endpoints of the top mip, as stored (before the shader and LUT). Compare between themes only.

### Water

| Slot | Source | Value |
|---|---|---|
| Water clean / dirty / under colour | CO-Temperate-Theme (2954236385) | **lost** (removed from the Workshop). By name a copy of CS1's vanilla Temperate water. The screenshots show slate grey-blue, slightly teal in the shallows. |
| Water foam texture | r69 Coastal | texture only |
| Water normal texture | Owl's Albedo | texture only |

For reference, the other source themes' water colours (RGBA, linear): Springwood clean (0, 0.008, 0.051) / under (0.408, 0.522, 0.431); Seychelles clean (0.012, 0, 0.008) / under (0.212, 0.733, 0.957); Australia clean (0.180, 0.191, 0.195) / under (0.358, 0.45, 0.378); r69 Coastal clean (0, 0.071, 0.016) / under (0.471, 0.588, 0.490).

### Sky, sun, moon

| Slot | Source | Value |
|---|---|---|
| Latitude / longitude | Australia Coast (mix values) | **−26 / 39** (southern hemisphere: the sun crosses the northern sky) |
| Sun size / anisotropy | Australia Coast (mix) | 0.82 / 0.651 |
| Rayleigh / Mie | Australia Coast (mix) | 1.013 / 2.16 (hazy halo) |
| Exposure | Australia Coast | 1.0 |
| Sky tint | Owl's Albedo | (0.52, 0.488, 0.478): warm grey, desaturated |
| Moon texture | Seychelles | texture |
| Moon size | Australia Coast (mix) | 0.07 |
| Moon inner / outer corona | r69 Coastal | (1, 1, 1, 0.498) / (0.247, 0.388, 0.498, 0.498) |
| Night horizon colour | r69 Coastal | (0.427, 0.467, 0.498) |
| Early / late night zenith | Springwood | (0.125, 0.145, 0.243) / (0.208, 0.239, 0.259) |
| Stars / outer space intensity | Australia Coast (mix) | 2.208 / 1.263 |
| LUT | mix | **Relight2Natural** (More Relight LUTs 1539181199) |

### Weather (all Springwood)

Day 15–30 °C, night 1–16 °C, rain 5–15 °C, fog 5–15 °C; rain chance 15 % day / 20 % night; fog chance 15 % / 20 %; northern lights 0. Never below 0 °C and the Sunny environment never snows, so CS1 is an **endless summer**.

### Structures (Seychelles)

Upward/downward road diffuse, building floor/base/base normal/burnt/abandoned diffuse, light colour palette: textures only.

## 2. Mapping onto vanilla CS2 knobs

### Knob table

| CS1 setting | CS2 knob | Recommendation | How it is set | Fidelity |
|---|---|---|---|---|
| Base environment Sunny + building/road textures | **Theme prefab** | `North American`. In CS2 the theme selects building and road styles and signage; it does not change terrain or water textures. | bridge `theme`; Map Settings → Theme | style only |
| Weather, seasons, temperature | **Climate prefab** | `TemperateWaterwayPassClimate` as the base, every field edited (§5) | bridge `climate`; Climate Settings → Selected climate | ≈ |
| Sun path (lat −26 / long 39) | **comes from the climate prefab** (`m_Latitude`/`m_Longitude`); `ClimateSystem.PostDeserialize` resets it on every load. An edited climate copy is saved in the map, so `climateEdit {latitude, longitude}` (bridge 0.9.23) makes it stick | 40.0 / −105.3 on the edited copy (§5). CS1's −26 / 39 is possible the same way | `climateEdit` | ✅ |
| Water colour, clarity, foam, waves | **WaterRenderSettingsPrefab** stored with the map (fields: `m_WaterColor`, `m_WaterScatteringColor`, `m_AbsorptionDistance`, `m_MaxRefractionDistance`, `m_FoamAmount`, `m_FoamFadeRange`, `m_WavesMultiplier`, `m_LakeWavesMultiplier`, `m_CausticsIntensity`, `m_RipplesWindSpeed`, smoothness, scattering terms) | see the water table below; pick by the target values | bridge (being added); editor | ≈ colour; textures ✗ |
| Grass/cliff/sand textures and tiling | **TerrainRenderSettingsPrefab** stored with the map (fields: `m_RockDiffuse/Normal`, `m_DirtDiffuse/Normal`, `m_GrassDiffuse/Normal` as texture asset ids, `m_Extra1–4` slots, tiling near/mid/far per layer, splat and triplanar blending, depth scale) | `DefaultTerrainRenderSettings` unless the bridge listing shows a vanilla set with a browner rock layer. It can only reference textures that already exist as assets: **the Springwood/Seychelles/Owl's textures cannot be used** | bridge (being added); editor | ✗ textures, ≈ tiling |
| Cliff texture by slope | CS2 terrain draws its rock layer on steep slopes by itself | nothing to paint; no vanilla rock surface exists | automatic | ≈ (different texture) |
| Grass colour offsets (fertility yellower, forest darker, fields darker) | no tint knob; approximate with `Grass Surface 01/02` areas | §4 rules G1–G3 | `cs2_import_surfaces` | ≈ coarse |
| Sand, gravel, ruined, pavement, field brushes | vanilla surface areas | §4 | `cs2_import_surfaces` | ≈ |
| Oil / ore tint | none (CS2 shows resources only in the overlay) | do not paint `Oil Surface 01`/`Ore Surface 01`, they look like industry yards | — | n/a |
| LUT Relight2Natural | not a map setting | Lumina External mode with the converted `.cube` (`cs1-playset/theme/LUMINA-MAPPING.md`) | per player | ✅ with Lumina only |
| Sky tint, Rayleigh/Mie, sun size, moon, night colours, stars | not exposed in vanilla CS2 or Lumina | — | — | ✗ |
| Fog/haze (15–20 % chance, hazy horizon) | climate `m_Fog` curve, **derived** from temperature swing, cloud and rain (no field) | summer day–night swing 16 °C (§5) | `climateEdit` seasons | ≈ |
| 3D grass detail off | n/a (CS2 has no theme detail meshes) | — | — | matches |

### Water render settings: values found and the target

| Prefab | Origin | Water colour (RGB) | Scattering colour | Absorption m | Refraction m | Foam | Waves / lake waves | Caustics |
|---|---|---|---|---|---|---|---|---|
| `BarrierIslandWaterRenderSettings` | vanilla (Barrier Island map) | 0.366, 0.581, 0.600 | 0.189, 0.437, 0.450 | 16 | 3 | 0.544 | 1.3 / 0.05 | 1.16 |
| `Twin Dams_WaterRenderSettings` | a custom map in a recovery save; identical to the `WaterRenderSettingsPrefab` code defaults | 0.14, 0.62, 0.62 | 0.19, 0.33, 0.39 | 8 | 3 | 0.30 | 1.0 / 1.0 | 1.0 |
| `GrandTetonsWaterRenderSettingsFinal` | PDX mod 148529, not vanilla | 0.152, 0.430, 0.516 | 0.070, 0.258, 0.324 | 8.6 | 2.7 | 0.709 | 0.76 / 0.38 | 0.41 |
| **target for Dakota Falls** | | ≈ 0.15–0.25, 0.35–0.45, 0.42–0.50 (slate blue, little green) | dark, ≈ 0.07–0.10, 0.25, 0.30 | **6–10** (river murk; CS1 hides the bed after a few metres) | ≤ 3 | 0.5–0.7 (white foam at the falls) | ≤ 1 / ≤ 0.4 (inland, no swell) | ≤ 0.6 |

Barrier Island water is tropical turquoise and clear (16 m): wrong for this river. Since `waterEdit` can set every field on an editable copy that is saved in the map, the base set no longer matters: §6 gives a value for each of the 22 fields. The target row above is superseded by §6.

### Cannot be reproduced without mods

- The CS1 terrain textures themselves (Seychelles grass, Springwood cliff/ruined, Owl's sand/pavement), their colours and the very large cliff repeat. Map Texture Replacer could, but it is a mod.
- The LUT and colour grade: Lumina only, per player.
- Sky tint, scattering, sun size, moon texture/corona, night colours, star intensity.
- Grass colour offsets as a continuous tint (only coarse surface patches).
- Water foam and normal textures; the exact lost CO-Temperate colours.
- A water pollution tint (no prefab field; the shader takes pollution from the simulation) and different water settings per water body (one prefab per map, §6).

## 3. Climates

Map file → in-game name from `Locale.cok`. Meta values: CS2 MapMetadata (temperature range °C, mean cloudiness, mean precipitation, lat/long), the same values the climate drives. Curve rows: decoded from the prefab where a copy was available. Snow: CS2 snows below the climate's freezing temperature (0 °C), lowered 1 °C per 100 m above the start-tile mean height (`audit/cs2-side.md`). Dakota's peaks sit ≈ 770 m above the start area in run A (≈ −7.7 °C) and ≈ 1 540 m in run B (≈ −15 °C).

| Climate prefab | Map (in game) | Theme of that map | Temp range | Cloud | Precip | Lat / long | Look and seasons | Snow on Dakota (A / B) |
|---|---|---|---|---|---|---|---|---|
| `TemperateWaterwayPassClimate` | River Valley (Waterway Pass) | North American | −5.4 … 35.4 | 0.161 | 0.037 | 38.75 / −108.07 (western Colorado) | hot dry summers, cold winters; real Colorado sun | whole map in winter; peaks most of the cold half (A and B) |
| `ContinentalCorralRichesClimate` | Corral Riches | North American | 3.2 … 33.7 | **0.063** | **0.003** | 35.20 / −101.89 (Texas panhandle) | the sunniest, driest vanilla climate; mild winters | plains none; peaks in winter (A), most of the year (B) |
| `TemperateBarrierIslandClimate` | Barrier Island | North American | 10.4 … 30.6 | 0.136 | 0.027 | **−27.30** / 153.46 (Queensland) | subtropical, green all year; southern-hemisphere sun like CS1's −26° | none (A); peaks rarely (B) |
| `TemperateSunshinePeninsulaClimate` | Sunshine Peninsula | European | 11.1 … 26.8 | **0.016** | **0.000** | 0 / 0 (equator) | cloudless, rainless endless summer; sun near the zenith, short flat shadows | none (A); peaks rarely (B) |
| `TemperateArchipelagoHavenClimate` | Archipelago (Archipelago Haven) | European | 7.7 … 30.1 (curve 7.8 … 35.5, mean 19.8) | 0.240 (curve mean 0.22) | 0.097 (curve: rain Dec–Mar, dry May–Sep) | 37.22 / 25.03 (Aegean) | Mediterranean: dry summers, mild wet winters, fog in winter | peaks marginal in winter (A); peaks in winter (B) |
| `TemperateSanFranciscoClimate` | San Francisco | North American | 5.5 … 27.8 | 0.167 | 0.098 | 37.77 / −122.42 | mild, damp, cloudy coast | peaks in winter |
| `TemperateRiverDeltaClimate` | Delta (River Delta) | European | −5.3 … 33.3 | 0.187 | 0.071 | 31.54 / 121.39 (Shanghai) | humid subtropical, hot wet summers | winter, map-wide |
| `ContinentalMountainVillageClimate` | Mountain Village | European | −11.6 … 27.0 | 0.221 | 0.096 | 47.26 / 11.41 (Tyrol) | alpine, wet, long snowy winters | long, map-wide |
| `ContinentalGreatHighlandsClimate` | Highlands (Great Highlands) | European | −2.9 … 19.0 | 0.163 | 0.067 | 57.23 / −4.55 (Scotland) | cool, grey, never warm | peaks most of the year |
| `ContinentalLakelandClimate` | Lakeland | European | −14.9 … 25.8 | 0.215 | 0.074 | 62.04 / 28.51 (Finland) | boreal, long snowy winters | long, map-wide |
| `ContinentalTampereClimate` | Tampere | European | −13.3 … 23.6 | 0.107 | 0.033 | 61.50 / 23.76 | boreal, drier | long, map-wide |
| `PolarSweepingPlainsClimate` | Plains (Sweeping Plains) | North American | −4.2 … 23.4 | 0.131 | 0.025 | −43.90 / 171.74 (Canterbury NZ) | cool dry plains | peaks most of the year |
| `PolarWindyFjordsClimate` | Fjords (Windy Fjords) | North American | −6.0 … 18.2 | 0.130 | 0.055 | −51.67 / −72.65 (Patagonia) | cold, windy | most of the year |
| `PolarTwinMountainClimate` | Coast (Twin Mountains) | European | −3.7 … 14.8 | 0.190 | 0.064 | 64.70 / −22.32 (Iceland) | sub-arctic | most of the year |
| `GrandTetonsClimateFinal` | **mod** "The Tetons \| Wyoming" (148529) | — | curve −15.8 … 26.9, mean 8.1 | curve mean 0.34 (wet Dec–Apr) | curve mean 0.15 (snow Dec–Mar) | 43.8 / −110 | Rocky Mountain, long snowy winter, max sun elevation 70° | long, map-wide. Not vanilla. |

Also installed but not in the game's list: `TemperateGreatBasinDesertClimate` (mod 163650; −14.2 … 31.3, very dry; its longitude is stored as +112.332, a sign error). Not vanilla.

### Ranking for Dakota Falls

Criteria: CS1 is an endless green summer in a Colorado setting with dry plains; snow must not cover the map for long; vanilla only.

1. **`TemperateWaterwayPassClimate`**: the only vanilla climate set in Colorado, North American theme, dry, hot summers. Real seasons and winter snow. Best "place" match.
2. **`TemperateBarrierIslandClimate`**: best "CS1 look" match. Green all year, no snow in run A, and its −27.3° latitude reproduces the CS1 sun path (the mix used −26°). Too humid in name only (precipitation 0.027 is as dry as Waterway Pass).
3. `ContinentalCorralRichesClimate`: the sunniest and driest, American plains; snow only on the peaks in winter.
4. `TemperateSunshinePeninsulaClimate`: the literal endless summer, but an equatorial sun (flat noon light all day) and a European map.
5. `TemperateArchipelagoHavenClimate`: warm, dry summers; European, Mediterranean.
6. `TemperateSanFranciscoClimate`, 7. `TemperateRiverDeltaClimate`, 8. `PolarSweepingPlainsClimate`, then the continental and polar ones (snowy most of the year).

**Try first: Waterway Pass and Barrier Island.** Run B (doubled heights) favours Barrier Island, because every other climate keeps the peaks white for months.

### Per-map overrides

Superseded by §5. Every editable climate field gets a deliberate value there, so the base climate only supplies its season prefabs and default weathers. **Base: `TemperateWaterwayPassClimate`** (North American, four-season set). With §5 applied, the Barrier Island alternative is no longer needed for a snow-free look.

## 4. Paint plan with vanilla surfaces (converter rules, no hand work)

**Revised 2026-10-20 after the live swatch test and the owner's A-v5 review.** The owner's rules:
- paint surfaces **only where CS2's own terrain shading cannot do the job**;
- **no rectangles:** every shape organic, as a brush with blending would leave it.

What the CS1 data contains: the v3 export's `terrainSurfaces` has **only the Sand class** (85 890 cells, 22.0 km², 16.8 km² inside ±7168 m). No pavement, gravel, ruined or field cells were painted. The rest of the CS1 look came from the slope cliff texture, the grass tints, 128 dirt decals, cornfield props and buildings. On this map CS1 "sand" is Owl's brown sand: banks, sand bars and some bare hillsides.

### Swatch test result (dakota-A5-2048, at (5500, 2500))

Front row, west to east: Grass 01, Grass 02, Sand 01, Sand 02, Agriculture 01. Back row: Landfill 01, Pavement 01/02, Concrete 01/02. Judged from the three oblique screenshots and the owner's review:

| Prefab | Reads as | Verdict |
|---|---|---|
| `Grass Surface 01` | mown lawn, dark green, faint mowing stripes | **do not use** for natural ground (a park/garden lawn) |
| `Grass Surface 02` | mown lawn, a little lighter, with **visible mowing stripes** (the owner's "striped, mown-lawn texture") | **do not use.** The icon-based "dry grass" guess was wrong |
| `Sand Surface 01` | pale tan-beige sand with faint raking lines | **bank sand and sand bars** (CS1's pale shores) |
| `Sand Surface 02` | grey-brown ground with fine parallel furrows | grey, not CS1's warm brown, and the furrows give a ploughed look; at most small patches, not large hillsides |
| `Agriculture Surface 01` | ploughed field | **field circles** |
| `Landfill Surface 01` | rubble heap texture | do not use |
| `Pavement Surface 01/02` | dark / lighter grey asphalt | **01 under stand-in buildings** |
| `Concrete Surface 01/02` | near-white concrete | dam crest and plant pad only |

**Dry grass:** no vanilla surface reads as dry prairie grass. **Paint nothing.** CS2's terrain grass layer and the season/climate shading already give the plains their tone, and the yellow-olive CS1 tint is not worth a lawn texture. The CS1 fertility and forest tints (G1–G3) are dropped.

### Rules

| Rule | Where (from data) | Surface | Notes |
|---|---|---|---|
| S1 bank sand | CS1 Sand cells within 25 m of the water edge, or ≤ 2 m above the water surface (`water.png`), and not under water deeper than 1.5 m | `Sand Surface 01` | CS1 sand bars and shores; CS2 has no automatic shore sand |
| S3 bare earth | other CS1 Sand cells **with slope < 20°** | `Sand Surface 02`, only patches ≤ 0.05 km² after smoothing; larger ones are skipped | on steeper slopes CS2's own rock/dirt shading already shows bare ground. Large Sand 02 areas read as ploughed grey fields |
| A1 fields | the 67 field polygons (`import-surfaces-fields.json`) | `Agriculture Surface 01` | centre-pivot circles stay circles (real fields have crisp edges); other fields get the organic treatment |
| P1 pavement | only under imported stand-in buildings without a lot of their own, and under Procedural Object platforms flagged flat and paved | `Pavement Surface 01`; `Concrete Surface 01` at the dam crest and the power-station pad | footprint plus a 3–6 m apron; the only rule where straight edges are right (it sits under a building) |
| dropped | grass G1/G2/G3, dirt decals S4, rock, oil, ore, forestry, tiles, landfill | — | CS2 terrain shading does it, or no vanilla surface looks right. S4: 128 small dirt decals as Sand 02 would be 128 grey blotches |

### Organic shapes (all rules except the centre-pivot circles and P1)

1. **Mask:** rasterise the rule at 4 m (resample the 16 m CS1 grid with bilinear interpolation, not nearest neighbour, so no 16 m staircase).
2. **Smooth:** Gaussian blur σ = 12 m (S1) or 20 m (S3), then threshold at 0.5. This rounds corners and removes the grid.
3. **Edge noise:** displace the contour along its normal by 2-octave value noise, amplitude ±6 m (S1) or ±10 m (S3), wavelengths 40 m and 15 m, seeded per patch. No straight run longer than 25 m should survive; check by measuring the longest segment after simplification.
4. **Feathered look without alpha:** CS2 surfaces have hard edges (visible in the swatch). Fake a brush falloff by breaking each patch edge into a fringe: along the outer 10–15 m, drop 30–50 % of the area as irregular bites (the noise threshold rising toward the edge), so the edge reads as dabs.
5. **Clean-up:** remove patches under 400 m² (S1) or 1 500 m² (S3); fill holes under 400 m² (areas cannot have holes; split larger holes into hole-free pieces per `cs2mcp/SURFACES.md`).
6. **Simplify:** Douglas-Peucker at 1.5 m (not 4 m, which re-straightens the noise), rings ≤ 500 nodes, no self-intersections.
7. **Priority** where rules overlap: A1 > P1 > S1 > S3.

The previous rectangular/grid-aligned outputs (`import-surfaces-grass*.json`, the decal squares) should not be imported again.

## 5. Climate: every editable field, with a value

### How the game uses these fields (decompiled `Game.dll` 1.6.2, `ClimatePrefab`, `ClimateSystem`, `SnowSystem`)

- **The curves are not edited directly.** `m_Temperature`, `m_Precipitation`, `m_Cloudiness`, `m_Aurora` and `m_Fog` are `[HideInEditor]` and are rebuilt by `RebuildCurves()` from `m_Seasons` and `m_RandomSeed`. The editor and the bridge (`climateEdit`, which runs `RebuildCurves` afterwards) edit the **seasons**.
- **Seasons:** each `SeasonInfo` value is keyed at its season's **mid-time**, and the curves interpolate between seasons (looped over the 12-month year; time 0 = 1 January). `m_StartTime` (0–1 of the year) also decides when each season prefab (summer/winter foliage) is active.
- **Temperature:** the night and day mean ± deviation give min/max curves. Each month draws a Gaussian (σ = deviation × 0.78), and each hour blends night to day on a cosine (coldest at 00:00, warmest at 12:00).
- **Clouds and precipitation:** amount ± deviation (in %) gives the monthly level. `chance` gates how often it happens. `turbulence` adds noise. **Precipitation is zeroed when cloudiness < 0.2.**
- **Fog has no field.** It is derived: fog appears when cloudiness > 0.15, temperature is −1…25 °C, precipitation < 0.5, **and the temperature has been falling more than 2 °C per hour** (evening cooling). The hourly cosine gives a peak cooling rate of 0.131 × (day − night) °C/h, so **fog needs a day–night swing above ≈ 15 °C**.
- **Snow:** `SnowSystem` lays snow where precipitation ≥ 0.1 and the local temperature, temperature − 0.01 × (height − start-tile mean height), is ≤ **0 °C**. This is hard-coded; `m_FreezingTemperature` only decides whether the weather counts as rain or snow (`isRaining`/`isSnowing`). **No precipitation means no snow, whatever the temperature.**
- **Sun:** `m_Latitude`/`m_Longitude` place the sun. `m_MaxSunElevationAngle` caps its noon height (45–90°), and `m_SunElevationClampStart` is where the cap starts to bend the real angle.
- Peaks vs start area (`audit/cs2-side.md`): ≈ −7.7 °C in run A and ≈ −15.4 °C in run B.

### Design

The reference is the Colorado Front Range at 40° N: semi-arid, sunny and dry in winter. Precipitation peaks with spring storms and July–August thunderstorms. Days are warm with cool nights (a 15–17 °C swing), and the plains are windier and drier. CS1 is Sunny with Springwood weather: days 15–30 °C, nights 1–16 °C, fog 15–20 %, no snow ever, endless green summer.

The compromise:
- **Real Front Range temperatures and sun, but a long summer and a short, nearly dry cold season.**
- **Long summer:** summer runs April–October (7 months), the warm months; with all four seasons on the summer prefab the foliage stays green all year.
- **Nearly dry cold season (owner decision 2026-10-20):** most precipitation falls in the warm months. The cold season keeps about a third of Colorado's precipitation days, light, so snow comes as occasional dustings that melt next day, never as white months. Winter nights can stay real (−5 °C), because snow needs precipitation.
- **Evening fog from the dry-air swing:** the summer day–night swing is 16 °C, which gives CS1-like evening fog through the game's own rule.

### Climate fields (apply to the edited copy of `TemperateWaterwayPassClimate`)

| Field (bridge key) | Value | Reasoning |
|---|---|---|
| base climate (`climate`) | `TemperateWaterwayPassClimate` | North American, four seasons; supplies the season prefabs and default weathers, which the bridge cannot set |
| `latitude` | **40.0** | Front Range (Boulder). The sun is due south at noon; the range to the west backlights the afternoon. Persists because the copy is saved in the map |
| `longitude` | **−105.3** | Boulder. Affects sunrise/sunset timing only |
| `maxSunElevationAngle` | **73.5** | the real summer-solstice noon height at 40° N (90 − 40 + 23.44 = 73.44). A lower cap would fake longer shadows; not needed |
| `sunElevationClampStart` | **60** | inactive in practice (the real sun never exceeds the cap); set so that a later cap reduction bends smoothly |
| `freezingTemperature` | **0** | keep physical. Snow accumulation uses a hard-coded 0 °C, so lowering this would only show "rain" at −5 °C while snow still forms. The low cold-season precipitation, not this field, limits snow |
| `randomSeed` | keep the base value | changes only the day-to-day draw. If the first in-game summer looks stormy, try another seed (any int) and compare |
| `defaultWeather`, `defaultWeathers` | keep the base | prefab references; the bridge cannot set references by name |
| `seasons[*].prefab` | **`SeasonSummer`** for all four | the bridge sets prefab references by name (0.9.23): foliage green all year, CS1's endless summer. The name is inferred from `Climate.SEASON[SeasonSummer]`; if wrong, the bridge reply lists the real names |

Seasons (four entries; `index` = position in the base's `seasons` array. Read the order with `GET /prefabs?category=climate&detail=1&query=WaterwayPass` and map by the `season` name):

| Field (bridge key) | Spring | Summer | Autumn | Winter | Reasoning |
|---|---|---|---|---|---|
| `startTime` (0–1) | **0.0833** (1 Feb) | **0.25** (1 Apr) | **0.8333** (1 Nov) | **0.9167** (1 Dec) | 7-month summer (CS1 endless-summer look); one month of autumn colour; two short cold seasons. Mid-times (where the values below are keyed): Spring 1 Mar, Summer mid-Jul, Autumn mid-Nov, Winter 1 Jan |
| `tempNightDay` °C | **[0, 13]** | **[12, 28]** | **[0, 13]** | **[−5, 8]** | Denver normals: Jan −8/7, Mar −3/12, Jul 16/31, Nov −4/11. Summer is kept inside CS1's 15–30 °C day range, with nights a little above CS1's mean; winter is a little milder than Denver. The summer swing of 16 °C (> 15.3) gives evening fog |
| `tempDeviationNightDay` °C | **[4, 5]** | **[3, 4]** | **[4, 5]** | **[4, 5]** | Front Range weather swings (chinooks, cold fronts) are larger in the cold half; summer is steadier |
| `cloudChance` % | **45** | **40** | **30** | **35** | sunny region; spring is the cloudiest, autumn the clearest |
| `cloudAmount` % | **35** | **25** | **25** | **30** | broken cloud, never overcast for long; summer afternoon cumulus |
| `cloudAmountDeviation` % | **25** | **25** | **20** | **20** | summer convection varies the most |
| `precipitationChance` % | **9** | **25** | **5** | **6** | owner decision 2026-10-20: a short cold season with **occasional light snow**, at about a third of Colorado's cold-season precipitation days (spring ≈ 27 %, winter ≈ 17 %, November ≈ 15 %). Summer thunderstorms stay at full strength |
| `precipitationAmount` % | **20** | **35** | **15** | **15** | light: around the game's 0.1 snow threshold, so many cold-season events are flurries that lay nothing. Summer thunderstorms are short and moderate |
| `precipitationAmountDeviation` % | **15** | **30** | **10** | **10** | summer storms vary a lot; cold-season events stay light |
| `turbulence` (0–1) | **0.25** | **0.5** | **0.15** | **0.15** | convective summer weather changes hour to hour; the cold season is calm |
| `auroraAmount` (0–1) | **0** | **0** | **0** | **0** | 40° N: practically never; CS1 northern lights 0 |
| `auroraChance` % | **0** | **0** | **0** | **0** | as above |

What this produces, checked against the game's rules:
- **Snow, run A:** on the plains, an occasional light dusting on cold-season nights (nights 0 to −5 °C, about one event in 6–11 days with precipitation chance 5–9 %). It melts on the next day (days 8–13 °C), so there are no white months. The summits are ≈ 7.7 °C colder (winter days ≈ 0 °C), so they can keep a light cover through December–January: snow-capped peaks, realistic. Rare light snow on the summits on the coldest summer storm nights.
- **Snow, run B:** summits ≈ 15 °C colder, so summer storm nights will whiten the highest peaks for a day at a time. This is acceptable as "snow-capped"; if not, lower the summer `precipitationChance` to 10.
- **Fog:** summer evenings with cloud > 0.15 and the 16 °C swing: occasional evening/night fog, CS1's 15–20 % feel. None in the cold season (swing 13 °C).
- **Foliage:** green all year, because all four seasons use the summer prefab (assuming, as in vanilla, that the season prefabs drive foliage).

Map-level fields (not in the climate) that go with it: `startingMonth` **6** (June: early summer, everything green), `startingTime` **0.5** (midday, as in the screenshots), `theme` **North American**, `naturalDisasters` **false**.

Bridge request: [`look/climate.json`](../look/climate.json) (apply order in [`look/README.md`](../look/README.md)).

## 6. Water rendering: every field, per water body and the single compromise

### What exists (`Game.Prefabs.Water.WaterRenderSettingsPrefab`, 22 fields) and what can vary per water body

There is **one prefab per map**: no field is per water body. The only per-body differences come from the shader itself:
- **Depth.** Absorption, refraction, caustics and the wave threshold all act on depth. A 0.5 m mountain creek and a 6 m river channel look different under the same values.
- **Sea vs lake.** `m_WavesMultiplier` applies to sea-level water; `m_LakeWavesMultiplier` (0–1) scales waves on lake water. On this map the lower river is the sea-level plane, so it takes the sea value; the reservoir and the source-fed lakes most likely take the lake value (the shader's lake mask; not verified in game).

Not available at all: a pollution tint (the shader takes pollution from the simulation), a per-body colour, a shoreline-specific setting, and river-flow speed (flow comes from the simulation; `m_RipplesChaos` only roughens the surface).

### Real-world targets per water body

| Field | Cold mountain river (west; snowmelt, clear, blue-green, visible bed) | Wide lowland river and reservoir (Mississippi-type; silty, greener, low clarity, gentle) | Small lakes (plains/foothill ponds) |
|---|---|---|---|
| water colour (refraction tint) | blue-green, light (0.30, 0.62, 0.58) | olive-green (0.30, 0.45, 0.30) | green (0.28, 0.50, 0.40) |
| scattering colour (body colour from above) | (0.08, 0.26, 0.28) | grey-green-brown (0.20, 0.25, 0.18) | (0.14, 0.24, 0.20) |
| absorption distance | 15–25 m (bed visible) | 2–4 m (opaque) | 4–6 m |
| max refraction distance | 3 | 1 | 2 |
| foam | high at rapids/falls | low except the dam spillway | none |
| waves / lake waves | none (small channels) | gentle chop | calm |
| caustics | strong (1.0–1.5) | none | weak |
| ripples wind speed / chaos | 3 / 1.0 (turbulent flow) | 5 / 0.5 (wind on a wide surface) | 2 / 0.3 |
| ambient scattering | low 0.2 (clear water) | high 0.5–0.6 (suspended silt glows) | 0.35 |
| smoothness | 0.92 / 0.80 | 0.90 / 0.75 (wind-roughened) | 0.95 / 0.88 (mirror-like) |

### Selection rule for the single prefab

1. **Weight by what the player sees.** The wide river, its falls and the reservoir behind the dam fill most of the visible water and every CS1 hero screenshot. The mountain creeks are narrow and shallow, and the lakes are small. So colour, scattering, smoothness and ripples follow the **lowland river and reservoir**, moved toward CS1's slate grey-blue (the author's look) rather than the full Mississippi brown.
2. **Let depth separate the bodies.** Choose the depth-driven fields so that shallow water comes out like the mountain river and deep water like the lowland river. With 5 m absorption, a 0.5 m creek transmits about 90 % (bed clearly visible, tinted blue-green by the refraction colour), while a 6 m channel transmits about 30 % and reads as opaque body colour. Caustics are limited to the top 3 m, so only creeks and shallows show them.
3. **Use the one sea/lake split for waves.** The lower river (sea plane) gets small waves; the reservoir and lakes get calmer ones.
4. **Foam follows the falls,** the map's namesake. The simulation makes foam where flow is fast, so a higher amount shows at the falls, rapids and spillway and stays invisible on still water.

### Water fields (apply as `waterEdit` on any base; the copy is saved in the map)

| Field (bridge key) | Range | Default | **Value** | Reasoning |
|---|---|---|---|---|
| `waterColor` | colour | (0.14, 0.62, 0.62) | **[0.28, 0.48, 0.36]** | refraction tint, owner blend 60 % lowland river (0.30, 0.45, 0.30) + 40 % CS1-slate compromise (0.26, 0.52, 0.46): olive-green through shallow water, still green-blue in clear creeks |
| `waterScatteringColor` (field `m_WaterScatteringColor`) | colour | (0.19, 0.33, 0.39) | **[0.18, 0.25, 0.20]** | deep-water body colour, 60 % silty Mississippi (0.20, 0.25, 0.18) + 40 % CS1 slate (0.16, 0.25, 0.23): grey-green with a hint of brown |
| `absorptionDistance` m | 0–100 | 8 | **3.8** | 60 % real (3 m) + 40 % CS1 compromise (5 m). The depth rule still holds: a 0.5 m creek transmits ≈ 88 % (bed visible), a 6 m channel ≈ 21 % (opaque). Real upper Mississippi Secchi depth is about 0.5–1.5 m |
| `maxRefractionDistance` m | 0–4 | 3 | **2** | less bed distortion; silty water refracts less visibly; creeks still shimmer |
| `ambientScattering` | 0–1 | 0.2 | **0.5** | more suspended silt (60 % toward the real river's 0.55) lights the body of the wide river; the shallow clear creeks show little of it anyway |
| `heightScattering` | 0–1 | 0.2 | **0.2** | wave-crest brightening; waves are small, so the default |
| `displacementScattering` | 0–1 | 0.1 | **0.1** | as above |
| `directLightTipScattering` | 0–1 | 0.6 | **0.4** | less sunlit glow in crests: an inland river, not surf |
| `directLightBodyScattering` | 0–1 | 0.4 | **0.35** | slightly less sun glow through the body: turbid water |
| `foamAmount` | 0–2 | 0.3 | **0.9** | rule 4: white water at the falls, rapids and spillway |
| `foamFadeRange` [start, distance] m (camera height) | 0–1000 / 500–1500 | [500, 1500] | **[600, 1500]** | keeps the falls' foam visible from the usual overview height |
| `wavesMultiplier` | 0–2 | 1 | **0.3** | the sea-level plane is a river here: small wind chop, no swell |
| `lakeWavesMultiplier` | 0–1 | 1 | **0.2** | reservoir and lakes calm; a reservoir chop shows only in deep open water |
| `minWaterAmountForWaves` m | 5–200 | 10 | **15** | no waves in water shallower than 15 m: creeks, shallows and most of the river surface stay flat; only the deep reservoir and main channel move |
| `seaWindAffectWavesDirection` | bool | false | **false** | inland map, no sea wind |
| `causticsPlaneDistance` m | 0–10 | 4 | **3** | caustics only in the top 3 m: creeks and shallows, never on the deep silty river |
| `causticsIntensity` | 0–3 | 1 | **0.6** | visible on clear creek beds, slightly weaker for the siltier water |
| `ripplesWindSpeed` | 0–15 | 5 | **4** | the plains are windy, but CS1's water reads calm; a moderate value |
| `ripplesChaos` | 0–1 | 1 | **0.6** | flowing water, less random than open sea |
| `smoothnessClose` | 0–0.99 | 0.95 | **0.92** | slightly softer highlights (CS1's soft look) |
| `smoothnessDistant` | 0–0.99 | 0.85 | **0.78** | wind-roughened distance, matches the hazy horizon |
| `smoothnessRange` [start, distance] m | 0–1000 / 500–1500 | [100, 500] | **[150, 600]** | the blend to distant roughness starts a little further out |

Bridge request: [`look/water.json`](../look/water.json).

Check after applying (one screenshot each): the falls (foam), the reservoir from the dam (calm, green-grey, opaque), a western creek (bed visible, blue-green), the lower river at overview height (slate, small chop). Expected adjustments: `absorptionDistance` ±2 if the creeks look muddy or the river looks glassy; `scatteringColor` toward blue if it reads too swampy against CS1.

## Owner decisions (2026-10-20)

1. Cold season: in between. Occasional light snow at about a third of Colorado's cold-season precipitation chance, no persistent white months (§5).
2. All four seasons use the summer season prefab (the bridge supports references by name).
3. Water: in between, closer to real: 60 % silty Mississippi, 40 % CS1 slate, keeping the depth rule for clear creeks (§6).
4. Lumina may be recommended in the map description as an optional companion.
5. Surfaces: after the live swatch test, only bank sand, small bare-earth patches, agriculture and pavement under stand-ins, all with organic outlines; no grass surfaces (§4).

All of these are encoded in [`look/`](../look/README.md).
