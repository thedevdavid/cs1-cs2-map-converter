# Runbook: Dakota Falls roads through Earth2Cities

Goal: find out, in about an hour of game time, whether Earth2Cities (E2C) is good enough for the Dakota Falls road skeleton. Background and the reasons behind each step are in [E2C_CONTRACT.md](E2C_CONTRACT.md).

What to expect before you start:

- **Roads only, flat on the terrain.** E2C ignores elevation: bridges, viaducts and the 38 or so tunnel segments are laid on the ground. Interchanges will be rough.
- **Only vanilla prefabs.** One-way 1- and 2-lane interstates cannot become 1- or 2-lane highways from a file. The converter writes them as **Highway Oneway - 3 lanes**, and ramps as **Small Road Oneway - 1 lane**. Change this with `--motorway-min-lanes` and `--link-lanes`.
- **The outer 600 m become highways.** E2C swaps every edge within 600 m of the map border for a Highway prefab. With the 1:1 rebuild (scale 1.0, clipped at ±7168 m) 392 edges fall in that ring.
- **No undo button.** E2C's Undo Import is hidden in v0.0.17. Undo means reloading a save made before the import.

## 0. Build the converter (once)

```bash
dotnet build D:/work/mods/csl2-editor/e2c-experiment/src/Cslmap2E2c -c Release
```

```bash
dotnet test D:/work/mods/csl2-editor/e2c-experiment/e2c-experiment.sln
```

The executable is `src\Cslmap2E2c\bin\Release\net10.0\cslmap2e2c.exe`; below it is called `cslmap2e2c`. Run `cslmap2e2c --help` for every option and `cslmap2e2c --print-mapping` for the prefab rules.

## 1. Export Dakota Falls from CS1 (CSL Map View)

1. Start CS1 with CSL Map View enabled (Content Manager → Mods). Its required collection is not needed for the export if the map loads.
2. Map Editor → Load Map → **Dakota Falls**. If the mod's export button is missing in the map editor, start a new game on the Dakota Falls map instead (the export reads networks, and a fresh city has exactly the map's networks).
3. Open CSL Map View (Options → CSL Map View, or its in-game button) → **Export**.
4. The file lands in `D:\SteamLibrary\steamapps\common\Cities_Skylines\CSLMapView\` as `<city>-<yyyyMMdd-HHmmss>.cslmap.gz` (the gzip file is fine as input). Note the name.

An older export is already there: `test-20251217-054007.cslmap.gz` (city "test", 5 095 segments, USRP and R2USA networks). Whether it is Dakota Falls I cannot tell; the converter handles it (3 652 road ways, 472 rail lines).

## 2. Convert

Calibration file (no CS1 input):

```bash
D:/work/mods/csl2-editor/e2c-experiment/src/Cslmap2E2c/bin/Release/net10.0/cslmap2e2c.exe --calibration -o D:/work/mods/csl2-editor/e2c-experiment/out/calibration
```

Full map (replace the file name with yours):

```bash
D:/work/mods/csl2-editor/e2c-experiment/src/Cslmap2E2c/bin/Release/net10.0/cslmap2e2c.exe "D:/SteamLibrary/steamapps/common/Cities_Skylines/CSLMapView/Unknown-20261018-133824.cslmap.gz" -o D:/work/mods/csl2-editor/e2c-experiment/out/dakota --scale 1.0 --clip --stats
```

Each run writes:

| File | Use |
|---|---|
| `overpass_roads.osm` | The file E2C reads. |
| `e2c-settings.txt` | The four bbox values to type into E2C (centre 0, 0). |
| `stats.txt` | Segments per class, skipped prefabs, predicted E2C prefab per group, expected edge count, border/snap risks. Read it before importing. |
| `roads.json` | E2C's intermediate format, for inspection only (E2C regenerates its own). |
| `rail.geojson` | Rail in CS2 metres, for the later importer. Not used by E2C. |

The converter prints a round-trip check through E2C's projection; it should be a few millimetres.

Useful knobs: `--drop-tunnels` (keep tunnels off the surface), `--mapping my-rules.tsv` (lines of `regex<TAB>class`, matched against `icls|prefab base name`, tried before the defaults), `--reverse-oneways` (if one-way arrows come out backwards, see §6), `--min-spacing` / `--simplify` (fewer, longer straight pieces).

## 3. Put the file where E2C reads it

E2C reads `overpass_roads.osm` from its own mod folder, nowhere else:

```text
%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\.cache\Mods\pdx_mods\140919_13\
```

```bash
cp D:/work/mods/csl2-editor/e2c-experiment/out/calibration/overpass_roads.osm "$USERPROFILE/AppData/LocalLow/Colossal Order/Cities Skylines II/.cache/Mods/pdx_mods/140919_13/"
```

If E2C updates, the folder suffix changes (`_14`, ...); the E2C log line `Mod install folder:` gives the path in use. Every "Import Roads" click overwrites `roads.json` in that folder; that is expected.

## 4. Editor playset

In the launcher, create a playset **"e2c-test"** with only:

- Earth2Cities (140919)
- Anarchy
- Image Overlay
- Scene Explorer

Nothing else, so a crash or a stray road points at E2C. Start the game with this playset.

In Options → Earth2Cities:

- Main: set **Overpass Top / Right / Bottom / Left** to `0.100000 / 0.100000 / -0.100000 / -0.100000` (also in `e2c-settings.txt`). Only their centre counts and it must be (0, 0).
- **Do not click Step 1, 2 or 3.** Step 1 resets the four fields to the Atlantic default; Steps 2 and 3 overwrite your terrain or the OSM file from the internet.
- Debug: switch **Enable Debug Logging** on (more log lines; it takes effect after a restart).

## 5. Calibration import (flat map first)

1. Editor → New Map (flat default terrain). Save it as `e2c-calibration`.
2. Copy the calibration `overpass_roads.osm` into the E2C folder (§3).
3. Options → Earth2Cities → **Step 4: Import Roads**. Wait for Operation Status "Import complete: queued N segments" and give it a few seconds.
4. Measure and write down:

| Check | Expected | How |
|---|---|---|
| Edge count | Log line `Queued 60 road segments`; `Terrain sampling: N hits, 0 misses` | `Logs\OSMImportMod.Mod.log` in the CS2 user folder |
| Scale | X arm ends at x = ±2500, Z arm at z = ±2500, diagonal end at (3535.5, 3535.5) | Scene Explorer → click the end node → `Node.m_Position` |
| Orientation | Small Road along x, Medium Road along z, diagonal in the +x/+z quadrant | Look at the map; compare with the editor's heightmap preview to learn which way +z points on screen |
| Direction | Diagonal traffic arrows point away from the centre | Zoom in on the diagonal |
| Junction | One shared node at (0, 0) joining all three roads | Scene Explorer on the centre node (count connected edges) |
| Prefabs | Small Road, Medium Road, Highway Oneway - 3 lanes | Hover / Scene Explorer `PrefabRef` |
| Height | Roads lie on the terrain, nothing at +500 m | Look from the side |
| Move tag | Move Imported Offset X = 100 → Move Imported Roads moves exactly these roads | Then move back with −100 |

If scale is off by a factor, the contract is wrong somewhere: stop and report the numbers. If only the orientation differs from what the CS1 map needs, note the needed rotation or flip; the converter gets a flag for it.

5. Undo: Load `e2c-calibration` again without saving.

## 6. Full import on the Dakota map

1. Open `dktflls`, check Height Scale is **1024 m** (re-import the heightmap at 1024 m if it is still 4096 m; roads take their heights from the terrain at import time).
2. **Save as `dktflls-e2c-test`**. This save is your undo.
3. Copy the full `overpass_roads.osm` into the E2C folder (§3). Bbox fields as in §4.
4. Step 4: Import Roads. Expect a pause of seconds to minutes; thousands of edges are applied in one batch.
5. Record:

| What | Where | Compare with |
|---|---|---|
| Segments queued, skipped short/duplicate/no-prefab | log `Queued N road segments ... (skipped ...)` | `stats.txt` "straight pieces E2C should queue" |
| Edges and nodes created | log `Import batch tagging complete. Tagged edges: N, tagged nodes: M` | queued count (big gaps mean failed placements) |
| Terrain misses | log `Terrain sampling: hits, misses` | 0 misses |
| Prefab mix | log `[RoadSelect]` lines (first 25) and a visual pass | `stats.txt` predicted prefabs |
| Interchange quality | 3–4 interchanges: welded carriageways (6 m snap), missing ramp joins, overlaps | screenshots |
| Elevation | former bridges/viaducts now on the ground; tunnels on the surface | `stats.txt` structures |
| One-way direction | a known interstate: arrows match CS1 driving direction (CS1 drives on the right) | if all are backwards, rerun with `--reverse-oneways` |
| Border connections | the 7 CS1 road connections reach the map edge; roads in the outer 600 m became highways | `stats.txt` "edges within 600 m of border" |
| Stability | editor responsive, save and reload OK, roads still there after reload | |

6. Save as `dktflls-e2c-result` only if you want to keep it; otherwise reload `dktflls-e2c-test`.

Move/Rotate Imported Roads work only in the same session as the import (the import tag is most likely not saved), so align before saving.

## 7. Undo paths

| Situation | Do |
|---|---|
| Any time | Reload the save made before importing (`e2c-calibration`, `dktflls-e2c-test`). |
| Whole import offset or rotated, same session | Move / Rotate Imported Roads with the offset sliders. |
| A few bad roads | Bulldozer (Anarchy helps with overlaps). |
| Do not | Edit E2C's settings file to reach the hidden Undo Import: when it finds no tagged roads (e.g. after a reload) it deletes **every road in the map**. |

## 8. Report back

Send the calibration table, the measurements from §6 step 5, two or three interchange screenshots, and `stats.txt`. That decides whether E2C carries the road skeleton or whether we go straight to the CS2MCP batch importer.
