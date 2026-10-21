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
| Grass/cliff/sand textures and tiling | **TerrainRenderSettingsPrefab** stored with the map: `m_RockDiffuse/Normal` (cliff), `m_DirtDiffuse/Normal`, `m_GrassDiffuse/Normal`, `m_Extra1–4Diffuse/Normal` (the editor brushes Dry, Lush, Dirt, Sand), tiling per layer, splat/triplanar blending | **The CS1 theme-mix textures themselves**, extracted from the theme `.crp` files (`look/tools/export-theme.mjs`): Seychelles grass, Springwood cliff (with Owl's cliff normal) and ruined dirt; Dry = r69 Coastal grass, Lush = Springwood grass, Dirt = Seychelles gravel, Sand = Owl's sand (with Owl's sand normal). Tiling from the CS1 mix (`look/terrain-cs1.json`) | textures: the editor panel's image picker (embedded in the map; not writable from code); tiling and legacy flag: `terrainEdit` | ✅ textures; tiling ≈ |
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

- ~~The CS1 terrain textures themselves~~: **they can be used.** The terrain render settings slots import any image through the editor's picker, and the textures are embedded in the map (see `look/README.md`, "CS1 theme-mix textures").
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

## 4. Paint plan (converter rules, no hand work)

**Revised 2026-10-20, from the owner's direct feedback:**
- the dry grass in the swatch **looked good**: the A-v5 rejection was of the rectangles, not the texture;
- buildings bring their own lot surfaces in CS2, so no pavement painting under them;
- research the real landscape for more spots worth painting;
- shapes are organic: a brush with blending, not rectangles.

### Two painting mechanisms

| Mechanism | What it gives | Use for |
|---|---|---|
| **Terrain materials** (`/import/terrain-paint`, bridge 0.9.29+; editor brushes Extra1 **Dry**, Extra2 **Lush**, Extra3 **Dirt**, Extra4 **Sand**) | Soft, feathered weights in the map's splatmap (`feather`, `softEdge`); textures come from the terrain render settings slots, which can now carry the **CS1 theme-mix textures** (§2 and `look/README.md`) | **Everything natural:** sand, dry grass, bare earth, scree, green bottoms |
| **Area surfaces** (`/import/surfaces`) | Prefab texture with a hard edge | **Owner policy 2026-10-20: not for ground painting.** Everything natural is painted with the brushes. Only the existing `Agriculture Surface 01` fields remain, pending the owner's call |

**Dropped:**
- pavement/concrete under buildings: CS2 building lots draw their own ground;
- `Sand Surface 02` (furrowed grey), `Grass Surface 01`, Landfill, Pavement, Concrete, Tiles, Ore, Oil, Forestry surfaces: nothing natural to match.

### Already produced by the converter (round 24, keep)

| File | Material | Source |
|---|---|---|
| `import-terrain-paint-sand.json` | Sand | every CS1 Sand cell (400 polygons, 17.0 km²) |
| `import-terrain-paint-dry-grass.json` | Dry | open dry ground: east/south-east plains and south-facing slopes, fertility < 0.5, no tree within 48 m (433 polygons, 9.7 km²) |
| `import-terrain-paint-bare-earth.json` | Dirt | 11 m strokes at the 128 CS1 dirt/mud decals |
| `import-surfaces-fields.json` | Agriculture Surface 01 | 67 CS1 fields |

### New spots from the real landscape (research)

The setting: a Mississippi-type river leaving the Colorado Front Range onto the high plains. Real features of that landscape that the CS1 map implies, each with a data rule the converter can apply.
- **Reference: shortgrass plains:** blue grama/buffalo grass, prairie dog towns that clip the grass to open ground, and more than 2 500 rain-fed playa wetlands in eastern Colorado ([Colorado Birding Trail: grasslands](https://coloradobirdingtrail.com/birding-basics/habitats-of-colorado/grasslands/), [Bird Conservancy: Colorado playas 2007](https://birdconservancy.org/wp-content/uploads/2025/01/Colorado-Playas-2007-Final-Report.pdf)).
- **Reference: the mountain front:** the first foothills are hogbacks capped by resistant Dakota sandstone over soft Morrison mudstone, cut by water gaps ([Dakota Hogback](https://en.wikipedia.org/wiki/Dakota_Hogback)).
- **Reference: the river below the foothills:** the South Platte below Denver has a floodplain of water-filled gravel pits, willow-held sandbars, cottonwood stands and prairie-dog colonies ([CWCB South Platte documents](https://dnrweblink.state.co.us/CWCB/0/doc/55749/Page6.aspx), [UDFCD South Platte](https://alert5.udfcd.org/fhn/fhn97/spr97.htm)).

| # | Real feature | Data rule (all heights and slopes on true heights, run A file) | Material, weight | Shape |
|---|---|---|---|---|
| N1 | **Cottonwood/willow bottoms** (green riparian strip) | within 150 m of the river or a lake, ≤ 4 m above the water surface, not Sand | **Lush**, 0.6 | feather 30 m; the green edge between sand and dry plains |
| N2 | **Gravel-pit ponds on the floodplain** | small lakes (< 0.3 km²) within 1.5 km of the main river, on flat ground (slope < 3° within 100 m) | **Dirt** ring 8–25 m wide around the shore + **Sand** 0–8 m | noisy ring; real pits have bare, terraced banks |
| N3 | **Reservoir drawdown ring** (bathtub ring) | 0–2.5 m above the reservoir level behind the dam, slope < 25° | **Dirt**, 0.7, fading to 0 at +2.5 m | follows the contour, feather 6 m |
| N4 | **Hogback / foothill sandstone** | mountain-front band (true height between start mean + 60 m and + 450 m), slope 18–38°, east-facing (aspect 45–135°) | **Sand**, 0.5 (tan sandstone tone), in streaks along the contour | strokes elongated along the contour, 20–40 m long |
| N5 | **Talus / scree aprons** | slope 28–40° cells that lie within 60 m downhill of slope ≥ 45° cells (cliffs) | **Dirt**, 0.8 | fan-shaped, widening downhill |
| N6 | **Ponderosa savanna floor** (bare soil between pines) | foothill band as N4, tree count 1–6 per 16 m cell (open forest), slope < 25° | **Dry**, 0.4 + scattered **Dirt** dabs (r 4–8 m) | dabs, not areas |
| N7 | **Prairie dog towns** | flat plains (slope < 4°), no tree within 100 m, not field/road/water, fertility < 0.5; seeded, about 1 town per 12 km² of eligible plains, 3–15 ha each | **Dirt** dabs (r 3–6 m) at 30 % cover + **Dry** 0.8 underneath | roughly round town, speckled interior |
| N8 | **Playas** (dry rain-fed depressions) | closed depressions on the plains from a fill-sinks pass: depth 0.4–3 m, area 0.5–20 ha, no water | **Lush** rim (outer 30 %) + **Dirt** centre (dried mud) | follows the depression contour |
| N9 | **Dirt/gravel county roads and tracks** | CS1 gravel and dirt road segments and paths (`netPrefabs` with gravel/dirt names) | **Dirt**, 0.5, 3–5 m beyond each edge | strokes along the segment, slight jitter |
| N10 | **Rail ballast spill** | railway segments outside towns | **Dirt**, 0.35, 2–3 m beyond each rail edge | strokes along the segment |
| N11 | **CS1 ore/oil ground** (CS1 rendered its ore and oil resources as terrain textures) | CS1 `naturalResources` ore ≥ 0.4 → **Dirt** 0.5; oil ≥ 0.4 → **Dirt** 0.3 (DJ-Basin-style well-pad ground) | as given | follow the resource contour, feather 20 m |
| N12 | **CS1 fertile land** (CS1 tinted it yellower) | fertility ≥ 0.5, not field, not forest | **Dry**, 0.35 | large soft blend |
| N13 | **Bare high ground** (above the trees) | above the highest height where the tree raster still has cover (computed per map), slope < 30° | **Dry**, 0.5 | soft blend |

Keep N1–N13 in separate files (`import-terrain-paint-<feature>.json`), so each can be judged and undone on its own. Order: Sand, Dry (round 24), then N1, N12, N13, N6, N4, N5, N3, N2, N8, N7, N11, N9, N10. Later strokes add weight, so the small, specific features go last.

### Organic shapes (all polygons except the field circles)

- Rasterise at 4 m (bilinear from the 16 m CS1 grid).
- Gaussian blur σ 12–20 m, threshold 0.5.
- Edge noise ±6–10 m (wavelengths 40 m and 15 m).
- No straight run over 25 m.
- Simplify at 1.5 m.
- For terrain paint, use the bridge's `feather` (6–30 m per rule above) instead of hard thresholds.
- Dabs and strokes: radius jitter ±30 %, position jitter ±0.5 r, seeded per feature.
- The old rectangular grass and decal-square files are not imported again.

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
| `cloudAmount` % | **45** | **25** | **40** | **45** | summer: broken afternoon cumulus. Cold seasons raised from 35/25/30 after A-v9: the game scales precipitation by cloud (× cloud/0.7, and again × cloud/0.4 below 0.4), so thin cloud can never reach the 0.1 snow threshold |
| `cloudAmountDeviation` % | **25** | **25** | **20** | **20** | summer convection varies the most |
| `precipitationChance` % | **9** | **25** | **5** | **6** | owner decision 2026-10-20: a short cold season with **occasional light snow**, at about a third of Colorado's cold-season precipitation days (spring ≈ 27 %, winter ≈ 17 %, November ≈ 15 %). Summer thunderstorms stay at full strength |
| `precipitationAmount` % | **45** | **35** | **35** | **40** | cold seasons raised from 20/15/15 after A-v9: with 40–45 % cloud an event now reaches about 0.2–0.3, above the 0.1 snow threshold. The **chance** stays low (5–9 %), so snow stays occasional |
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

Check after applying (one screenshot each): the falls (foam), the reservoir from the dam (calm, green-grey, opaque), a western creek (bed visible, blue-green), the lower river at overview height (slate, small chop). Expected adjustments: `absorptionDistance` ±2 if the creeks look muddy or the river looks glassy; `waterScatteringColor` toward blue if it reads too swampy against CS1.

## Owner decisions (2026-10-20)

1. Cold season: in between. Occasional light snow at about a third of Colorado's cold-season precipitation chance, no persistent white months (§5).
2. All four seasons use the summer season prefab (the bridge supports references by name).
3. Water: in between, closer to real: 60 % silty Mississippi, 40 % CS1 slate, keeping the depth rule for clear creeks (§6).
4. Lumina may be recommended in the map description as an optional companion.
5. Surfaces: terrain-material paint (Dry/Lush/Dirt/Sand with the CS1 textures, feathered) for everything natural; Agriculture circles and Grass Surface 02 dry-grass patches as area surfaces; no pavement under buildings (lots bring their own); research-based extra spots N1–N13 (§4).

All of these are encoded in [`look/`](../look/README.md).
