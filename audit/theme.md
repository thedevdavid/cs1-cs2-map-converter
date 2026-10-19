# Dakota Falls: theme and visual settings audit (CS1 → CS2)

Written 2026-10-19. Findings only; no other file was changed.

## Sources

| Source | What it gave |
|---|---|
| `D:\SteamLibrary\steamapps\workshop\content\255710\2972881605\ThemeMix.xml` + `UsedAssets.txt` ("Dakota Falls - Theme mix", Theme Mixer 2.5) | which theme supplies each texture/colour/sky/weather slot, tiling overrides, LUT name |
| `...\2972879554\Dakota Falls.crp`, asset `Dakota Falls` (`MapMetaData`) | environment, map theme reference, mods list |
| The six theme `.crp` files (`MapThemeMetaData` + texture assets) | theme environment, colours, sky, weather, texture sizes and average colours |
| Workshop page of 2972879554 (in-app browser; WebFetch was rate-limited) | author's "VISUALS" section, 30 screenshots, one YouTube video `t5K4YOyWoko` (not watched) |
| Workshop page of 2972881605 | the author's Relight settings screenshot |
| `Cities Skylines II\...\Content\Game\Locale.cok`, `...\Maps\*.cok` (MapMetadata JSON) | CS2 climate category keys; vanilla map theme, temperature range, lat/long |
| PDX mod cache `163650_1\TemperateGreatBasinDesertClimate.cok`, `Player.log` | real CS2 `ClimatePrefab` names and fields |
| `cs1-playset/AUDIT.md`, `cs2mcp/MAP_SETTINGS.md`, `cs2mcp/SURFACES.md`, `cs2-playset/ASSET_PACKS.md`, `net-converter/surface-mapping.tsv`, `next-run/RUNBOOK.md` | existing decisions and prefab names |

Scripts used (scratchpad, not in the repo): `cs1-playset/crp.mjs` to extract assets, a field-by-field reader for CS1 `MapMetaData`/`MapThemeMetaData` blobs, and a DDS (DXT5) block-endpoint averager for texture colours.

## 1. CS1 theme facts

### The map has no single theme

`MapMetaData` of Dakota Falls: `environment = "Sunny"` and the `mapThemeRef` field is **null**. So the map loads on the vanilla Sunny (temperate) base, and its look comes entirely from the **Theme Mixer 2.5 mix "Dakota Falls"** (2972881605), which picks each slot from one of seven themes. The map's own mods list also names Relight (1209581656), Render It! (1794015399), Daylight Classic (530871278) and the removed theme 2954236385.

Author's "VISUALS" section, in short: LUT **Relight2Natural** (from More Relight LUTs 1539181199), the custom theme mix, Render It! settings "in the theme mix collection", Relight settings in a screenshot on the mix page, Hide It! to hide the shore effect, Daylight Classic optional ("all to classic").

### Theme mix: which theme supplies what

| Slot | Source theme (id, environment) | Tiling in mix | Avg. texture colour* |
|---|---|---|---|
| Grass | Seychelles (1230768185, Sunny) | 0.01 (≈100 m repeat) | `#262c18` dark olive |
| Cliff | Springwood (921636136, Sunny) | 0.0045 (≈220 m repeat, very large) | `#403931` warm grey-brown |
| Sand | Owl's Albedo (1367973971, Tropical) | 0.006 | `#453c32` brown, not yellow |
| Gravel | Seychelles | 0.02 | `#322b26` dark brown |
| Ruined | Springwood | 0.012 | `#3e362c` brown dirt |
| Pavement | Owl's Albedo | 0.0235 | `#383838` neutral grey |
| Oil | Springwood | 0.003 | `#55413f` |
| Ore | r69 Coastal (1594406264, Sunny) | 0.005 | `#0b180b` (dark; channel packing suspected, ignore) |
| Cliff/sand normal | Owl's Albedo | 0.0015 | |
| Grass tints | field: Seychelles `(-0.002,-0.006,-0.001)`; fertility: Owl's `(0.0189, 0.0128, -0.0007)` = warmer/yellower on fertile land; forest: Los Cabos `(-0.0092,-0.0056,-0.0104)` = darker under forest; pollution: Owl's `(0,0,0)` | | |
| Grass / fertile / rocks 3D detail | Owl's Albedo: **all off** (flat ground, no detail meshes) | | |
| Water clean / dirty / under | **CO-Temperate-Theme (2954236385) — removed from Workshop, values unrecoverable**. By its name it is a copy of the vanilla Temperate water colours. | | |
| Water foam | r69 Coastal | | |
| Water normal | Owl's Albedo | | |
| Sky: lat/long, sun size, anisotropy, Rayleigh, Mie, exposure, moon size, stars | Australia Coast (1305157510, Sunny), with mix overrides: **latitude −26, longitude 39**, sun size 0.82, anisotropy 0.651, Rayleigh 1.013, Mie 2.16, exposure 1.0 | | |
| Sky tint | Owl's Albedo `(0.52, 0.49, 0.48)` = desaturated warm-grey sky | | |
| Night colours | Springwood / r69 Coastal | | |
| Weather (temperature, rain, fog, aurora) | Springwood: day 15–30 °C, night 1–16 °C, rain 15 % day / 20 % night, fog 15 % / 20 %, no northern lights, never below 0 °C → **no snow** | | |
| Roads, building floors/bases, light palette | Seychelles | | |

\* Mean of the DXT5 block endpoints of the top mip, sRGB as stored. Raw albedo values before the CS1 terrain shader and the LUT; useful to compare themes, not as final on-screen colour.

Theme texture sizes: 2048² in Seychelles/Springwood; 4096² for Owl's Albedo grass/gravel/cliff/sand. All six theme files are present locally; 2954236385 is not.

### LUT and lighting mods

- LUT: **Relight2Natural** (More Relight LUTs 1539181199, not installed locally).
- Relight (read from the author's 1080p screenshot, values approximate): brightness/gamma/contrast at default, temperature +0.05, sun temperature −0.25, sky temperature −0.15, moon temperature −0.75, moon light 0.25, other tints 0; "Enable sky tonemapping" on. Net effect: slightly cool sun and sky, neutral overall.
- Render It! profile: said to be "in the theme mix collection"; not found on the mix page itself (its description only says to subscribe to the collection). Not recovered.

## 2. Intended look (from the 30 Workshop screenshots)

- **Season / time:** full summer, midday to afternoon, soft high cloud; no snow anywhere, peaks are bare grey-brown rock.
- **Vegetation:** very dense, dark green conifer forest (pine/spruce) covering the hills and river banks; lighter deciduous clumps on the plains. Grass is a mid green with olive/yellow patches on the eastern plains.
- **Rock and cliffs:** warm grey to tan-brown rock faces (the "Flatirons" shot), brown boulders on the falls and slopes, eroded tan bare hillsides in the foothills (shot 19 "dry" slope). The brown surface rocks are props (Prop Painter colours), not theme texture.
- **Fields:** centre-pivot circles in tan, orange-brown and green on the eastern plains.
- **Sand / banks:** pale tan-grey sand bars and gravelly shores along the river, rocky river bed at the falls.
- **Water:** grey-blue to slate, slightly green-teal in the shallows, low contrast; white foam at the falls and dam.
- **Atmosphere:** noticeable distance haze (whitish-blue horizon), fairly flat, desaturated "natural" grade (the LUT name agrees).

## 3. CS2 recommendation

### Settings table

| Setting | Value | How it is set |
|---|---|---|
| Theme | **North American** (US Midwest/Colorado; the author's tags say "AMERICAN"; already in `RUNBOOK.md` step 9) | bridge `theme` (`cs2_map_settings`) or editor Map Settings → Theme |
| Climate | **the climate prefab used by the vanilla map "River Valley"** (North American, lat 38.75 / long −108.07 = western Colorado; −5.4…35.4 °C, cloudiness 0.16, precipitation 0.037). Its prefab name is not readable offline (map data is compressed); read it in the editor (Climate Settings → Selected climate, then `cs2_map_settings` GET returns `climate`). Alternative: "Corral Riches" climate (Texas panhandle plains, 3.2…33.7 °C, very dry), or the PDX mod climate `TemperateGreatBasinDesertClimate` (163650, already installed; dry, lat 41.37) | bridge `climate` (exact prefab name) or editor Climate Settings → Selected climate |
| Latitude / longitude | **40.0 / −105.3** (Boulder, CO: Front Range with the Flatirons, plains to the east). Ignore CS1's −26 / 39 (inherited sky slot, southern hemisphere). | bridge `latitude`/`longitude`, **after** the climate (the climate panel overwrites them) |
| Sea level | run A (Height Scale 2048): **97.5 m**; run B (4096): **195 m** (decided) | bridge `seaLevel` or Water tab → Sea Level → Apply |
| Starting month | **June or July** (CS1 is perpetual summer; CS2 snows in winter with a Colorado climate) | bridge `startingMonth` or Map Settings → Starting month |
| Starting time | ~0.5 (midday, as in the screenshots) | bridge `startingTime` |
| Natural disasters | off (CS1 map has nothing scripted; owner's call) | bridge `naturalDisasters` |
| LUT / colour grade, Relight values | not part of a CS2 map; nothing to set. Players' own photo-mode / graphics settings. | — |
| Water colour, foam | not a map field in CS2 (water look follows the game's renderer). | — |
| Fog / haze | comes from the climate's fog curve; only changeable by editing the climate (Climate Settings → edit curves), not by the bridge | editor only |
| Terrain base textures (grass, dirt, cliff) | fixed per theme/climate in vanilla CS2. Map Texture Replacer (installed: `MapTextureReplacer.coc` in the CS2 user folder) can swap them, but it is a player-side mod, not stored in the map | — |

### CS1 texture class → CS2 surface prefab

| CS1 class | Look to match | CS2 surface (first choice → alternative) | Painted |
|---|---|---|---|
| Cliff (slope texture) | warm grey-brown rock | CS2 terrain already draws rock on steep slopes; add `PDO23 Stone Surface 01` only where the CS2 rock band is visibly missing → `gaga TTP - Pebbles 1L dark` for scree | **auto, optional**: cells whose **CS1 (true-height) slope ≥ 40°** in the western range; compute the slope on the true heights (run A data) so runs A and B paint the same cells, because run B's doubled heights double every gradient. Check one screenshot first and skip if the vanilla rock band already reads right. |
| Sand (shore/bed, `m_sand` in the export) | brown sand (Owl's), grey gravel bars | `PDO23 Dirty Sand Surface 01` → `Sand Surface 01`; submerged bed at the falls `gaga TTP - Riverbed 1L` | **auto** from the exporter's `terrainSurfaces` sand cells (CS1 shore sand) |
| Gravel (building/road brush) | dark brown gravel | `gaga TTP - Pebbles 1M dark` → `Sand Surface 02` (current `surface-mapping.tsv` choice) | **auto** from `terrainSurfaces` gravel cells |
| Ruined (dirt brush) | brown dirt | `gaga TTP - Ground 1A` or `PDO23 Dirty Surface 01` → `Sand Surface 01` (current choice, likely too light) | **auto** from `terrainSurfaces` ruined cells |
| Pavement | neutral grey | `Pavement Surface 01` → `PDO23 Asphalt Surface 01` | **auto** from `terrainSurfaces` pavement cells |
| Field (farm/fertile) | centre-pivot circles, tan/green | `Agriculture Surface 01` (67 polygons already in `import-surfaces-fields.json`) | **auto** |
| Grass base | mid green, olive on plains | none (base terrain); `gaga TTP - Wild Grass 1L` / `Dry Grass 1L` for accents | **by hand** |
| Dry eroded foothills (shot 19) | tan bare slopes | `gaga TTP - Dry Grass 1L` / `gaga TTP - Ground 2A` | **by hand** |
| Forest floor | dark, under conifers | `gaga TTP - Forest Ground 1A` | **by hand** (an auto pass from tree density would be thousands of polygons; not worth it) |
| Oil / ore tints | resource tint only | none: CS2 shows resources in the overlay, imported via `cs2_import_resources` | — |

The suggested replacements for ruined and gravel differ from `net-converter/surface-mapping.tsv`; that file was not changed (open question 4).

## 4. Terrain facts the theme implies

- **Water level:** CS1 themes carry no water level. The map's sea level (CS1 default 40 m, read as 97.5 m true / 195 m doubled per the earlier decision) stays the reference. The removed CO-Temperate water colours mean the CS1 look itself needs Theme Mixer to fall back to the base water; the screenshots are the only colour reference.
- **Snow line:** none. Springwood weather never goes below 1 °C and the Sunny environment has no snow, and the screenshots show bare peaks. A Colorado CS2 climate will snow over the whole map in winter (CS2 snow follows temperature, not altitude), so start in summer and accept winter snow, or pick a milder climate.
- **Cliff angle:** the theme has no cliff threshold; CS1's cliff texture is set by the terrain shader from slope. The mix stretches the cliff texture to ≈220 m per repeat and the sand/cliff normal to ≈670 m, so CS1 cliffs read as large smooth rock faces. The 40° figure above is a starting value; confirm it against the CS1 surface export (cliff cells vs slope histogram) before an auto pass.
- **Grass detail off:** the CS1 ground had no 3D grass meshes. The 3 076 imported grass plants already add more than the original had; do not add more by default.
- **Shore effect hidden:** the author hides the shore effect (dead trees at the waterline). CS2 has no equivalent, but expect trees on wet banks to show; check the imported trees within a few metres of water.

## Open questions for the owner

1. Climate: use River Valley's climate (Colorado, cold winters) or the dry Corral Riches / Great Basin one? Its exact prefab name has to be read in the editor once.
2. Snow: is winter snow acceptable, or should the map use a milder climate so it stays close to CS1's endless summer?
3. Lat/long 40.0 / −105.3 (Boulder) fine, or a fictional Midwest point east of the range?
4. Change `surface-mapping.tsv` for ruined → `gaga TTP - Ground 1A` and gravel → `gaga TTP - Pebbles 1M dark` (both packs are in the playset), or keep vanilla-only surfaces so the map has fewer PDX Mods dependencies?
5. Run an automatic rock-surface pass on steep slopes, or rely on CS2's own slope rock and the imported rock props?
6. Note: the Great Basin climate mod stores `m_Longitude = 112.332` (positive, i.e. east); if it is chosen, longitude must be set by the bridge afterwards anyway.
