# Map look: climate, water and terrain rendering

Ready request bodies for `POST /map/settings` (bridge 0.9.23 or later; MCP `cs2_map_settings` with the same body as `set`). The values and their reasoning are in [audit/theme-remake.md](../audit/theme-remake.md) §5 (climate) and §6 (water). The field reference is in [cs2mcp/MAP_SETTINGS.md](../cs2mcp/MAP_SETTINGS.md), "Look".

| File | What it sets |
|---|---|
| [climate.json](climate.json) | Base climate `TemperateWaterwayPassClimate`, then an edited copy (saved in the map): sun at 40.0 / −105.3, the four seasons, plus theme, starting month/time and natural disasters |
| [water.json](water.json) | All 22 `WaterRenderSettingsPrefab` fields on an edited copy of whatever water settings the map has |
| [terrain.json](terrain.json) | Selects `DefaultTerrainRenderSettings`, no edits (see below) |
| [swatch.mjs](swatch.mjs) | Writes the surface swatch-test body |

## Owner decisions (2026-10-20), encoded in the files

| Question (theme-remake.md) | Decision | In the files |
|---|---|---|
| 1. Cold season | **In between:** a short cold season with occasional light snow, at about a third of Colorado's cold-season precipitation chance. No persistent white months | climate.json precipitation (chance / amount / deviation, %): spring **9 / 20 / 15**, autumn **5 / 15 / 10**, winter **6 / 15 / 10**; summer unchanged at 25 / 35 / 30 |
| 2. All four seasons on the summer season prefab | **Yes, since the bridge supports it:** every season entry has `"prefab": "SeasonSummer"`. Foliage stays green all year; snow can still fall | delete the four `"prefab"` keys to keep the base's own seasons |
| 3. Water | **In between, closer to real:** 60 % silty Mississippi, 40 % CS1 slate. The depth rule still keeps the shallow mountain creeks clear | water.json: `waterColor` **[0.28, 0.48, 0.36]**, `scatteringColor` **[0.18, 0.25, 0.20]**, `absorptionDistance` **3.8**, `ambientScattering` **0.5**, `causticsIntensity` **0.6**; the other 17 fields are unchanged |
| 4. Lumina in the map description | **Yes,** recommended as optional (text below) | set the description by hand |
| 5. Sand/grass swatch | **Decided offline** from the game's surface icons (below). The in-game swatch test stays as a later confirmation | — |

**Sand and grass, from the game's own surface icons** (`Cities2_Data/Content/Game/UI/Media/Game/Icons/Surface*.svg`; the swatch colour of each icon, with `#606060` as the frame):

| Prefab | Icon colour | Reads as | Use (theme-remake.md §4) |
|---|---|---|---|
| `Sand Surface 01` | `#7f745f` | light warm tan | **S1 bank sand** (sand bars, shores) |
| `Sand Surface 02` | `#5e5a50` | darker grey-brown | **S3 bare earth** on hillsides, **S4** dirt patches |
| `Grass Surface 01` | `#353f1f` | dark lush olive | not painted (the base terrain is already dark grass) |
| `Grass Surface 02` | `#424c25` / `#394221` | lighter, slightly yellower | **G1/G2** fertile land and dry plains |

The icons are UI art, not the textures themselves. The sand pair differs clearly (light tan vs darker grey-brown); the grass pair only slightly (Grass 02 is about 25 % lighter and a little yellower). Sand is therefore reliable, grass is a **likely but marginal** call: confirm both with the swatch test.

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
