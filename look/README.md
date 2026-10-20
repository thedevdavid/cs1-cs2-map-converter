# Map look: climate, water and terrain rendering

Ready request bodies for `POST /map/settings` (bridge 0.9.23 or later; MCP `cs2_map_settings` with the same body as `set`). The values and their reasoning are in [audit/theme-remake.md](../audit/theme-remake.md) §5 (climate) and §6 (water). The field reference is in [cs2mcp/MAP_SETTINGS.md](../cs2mcp/MAP_SETTINGS.md), "Look".

| File | What it sets |
|---|---|
| [climate.json](climate.json) | Base climate `TemperateWaterwayPassClimate`, then an edited copy (saved in the map): sun at 40.0 / −105.3, the four seasons, plus theme, starting month/time and natural disasters |
| [water.json](water.json) | All 22 `WaterRenderSettingsPrefab` fields on an edited copy of whatever water settings the map has |
| [terrain.json](terrain.json) | Selects `DefaultTerrainRenderSettings`, no edits (see below) |
| [swatch.mjs](swatch.mjs) | Writes the surface swatch-test body |

## Defaults pending the owner's answers

These are encoded now. Each can be changed in one place.

| Open question (theme-remake.md) | Default in the files | To change |
|---|---|---|
| 1. Dry cold season or Colorado spring snow | **Dry cold season, no snow (CS1 endless summer wins).** Spring, autumn and winter have `precipitationChance` 0, so nothing falls when it is cold | climate.json, the entry with `startTime` 0.0833: `precipitationChance` 25, `precipitationAmount` 30, `precipitationAmountDeviation` 25 |
| 2. All four seasons on the summer season prefab | **Yes:** every season entry has `"prefab": "SeasonSummer"` (the bridge sets prefab references by name since 0.9.23). Foliage stays green all year | delete the four `"prefab"` keys to keep the base's own seasons |
| 3. Water: CS1 slate or real Mississippi brown-green | **CS1 slate** (`scatteringColor` [0.16, 0.25, 0.23], `absorptionDistance` 5) | Mississippi: `scatteringColor` [0.20, 0.25, 0.18], `absorptionDistance` 3 |
| 4. Lumina in the map description | **Mentioned as optional** (text below) | edit the description by hand |

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

   Expected tweaks: `absorptionDistance` ±2 if the creeks look muddy or the river looks glassy, and `scatteringColor` toward blue if it reads swampy.

**Why terrain.json makes no edits:** the terrain textures cannot be written from code (asset references; the panel's image picker only), so the CS1 textures cannot come back. The numeric fields (tiling, blur, splat, triplanar) have no CS1 counterpart worth guessing blind. The only candidate is the rock tiling: CS1 stretched its cliff texture to a ≈220 m repeat, while CS2's near rock tiling is 1600 (≈9 m repeat on a 14 km map). If the cliffs look busy in the screenshots, try `{"terrainEdit": {"terrainRockTiling": {"nearTiling": 800}}}`, judge, and undo by posting 1600.

## Swatch test (surfaces, before the paint plan)

Purpose: decide which vanilla surface reads as brown earth, light bank sand and dry grass (theme-remake.md §4, rules S1, S3, G1, G2).

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
