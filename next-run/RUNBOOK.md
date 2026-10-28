# Run A-v25: Dakota Falls at Height Scale 2048 on world map 10270758

A-v25 repeats the accepted A-v24 chain (final `dakota-A24d-2048`) on a fresh map. The only change is the world map: hmap-ext ticket 30 (a divide range and two buttes in the east exterior). History of every earlier run is in `setup-plan.md`.

| Input | A-v25 |
|---|---|
| Playable heightmap | `10211409-dktflls-1to1.png` (accepted with A-v24), Height Scale **2048** |
| World map | **`10270758-dktflls-1to1_worldmap.png`** with `10270758-dktflls-1to1_water.json` (hmap-ext main 6722d2e, inner 10211409: divide range 11.9 km / 158 m between the east river and the main river, two buttes 115–117 m east, seam 46 m) |
| Heightmaps folder | `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Heightmaps` |
| Sea level | **93 m** |
| Bridge | CS2MCP **0.9.63** (deployed 2026-10-28; texture asset names carry a content hash, so re-imports need no tag of their own) |
| Converter | repo HEAD, round 52 (`dffafd6`): the runner strips the ruins' own props at build time; import files in `net-converter/out/x1` + `out/trees` |
| Terrain textures | `look/textures/v3/*.u1.png` (14 files, final set of 2026-10-22) |
| Look JSONs | `look/climate.json`, `look/water.json` (build phase); `look/terrain-cs1.json` (post-save; holds the Extra3/4 blend values in `terrainEdit`) |
| Playset | DakotaFalls-E2C (`cs2-playset/PLAYSET.md`): CS2MCP local, the 14 asset packs, Anarchy, Traffic 80095, Move It, Find It, Lumina, TWA (Main profile) |
| Saves | `dakota-A25a-2048` (build), `-A25b-` (textures), `-A25c-` (override loop + water + TM:PE), `-A25d-` (only if a fix pass is needed) |

## Before the game starts

Run `bash next-run/preflight-a25.sh` from the repo root. It checks the heightmaps, every import file and texture the runner reads, the look JSONs, the TM:PE inputs, the converter commit, and the bridge version and deploy. It must end with `== PASS`. "bridge port 8642 not answering" is information only, expected while the game is closed.

## Editor steps (owner, by hand)

1. Start the game on the DakotaFalls-E2C playset. Editor → **Map → New**.
2. Terrain tab → Import Heightmap: set **Height Scale 2048 first**, then pick `10211409-dktflls-1to1.png`.
3. Terrain tab → Import World Map: `10270758-dktflls-1to1_worldmap.png`.
4. Water tab → Sea Level **93** → Apply (a new map defaults to 511.7 m, which floods everything).
5. Map Name: **Dakota Falls**. Say "A-v25 ready" in the orchestrator session.

## Runner chain (orchestrator)

`B=http://127.0.0.1:8642`. Runner commands from `net-converter/`, the rest from the repo root. Every save goes through `curl -s -X POST "$B/map/save?name=<name>"`; wait until `curl -s $B/map/save/status` says `done` (the runner's own `--save` line can say "not confirmed: HTTP 202" while the save is still running: check the status). Names have no leading or trailing space.

1. `curl -s $B/ping` → version `0.9.63-editor-import`, mode Editor.
2. **Build:** `node tools/import-run.mjs --scale x1 --sea 93 --po-structures --with po-gantry --fresh --save dakota-A25a-2048`
   Expect (A-v24): about 3 204 segments, 3 point-check failures, fort-buildings 89, the `fort-strip` line strips the ruins' props, netGuard 0 on every step.
3. **Reload:** `curl -s -X POST "$B/map/load?name=dakota-A25a-2048"`, poll `curl -s $B/map/load/status` until `loaded`.
4. **Textures:** `node tools/import-run.mjs --scale x1 --phase post-save --only textures --textures-dir D:/work/mods/csl2-editor/look/textures/v3 --textures-tag .u1`
   Posts `look/terrain-cs1.json` (tiling and the Extra3/4 splat/blur values), then the 14 `.u1` files. Expect done 14, failed 0. `--textures-tag .u1` only selects the `.u1` files; the 0.9.63 hash keeps re-imports apart on its own.
5. Save **`dakota-A25b-2048`**.
6. **Override loop:** `curl -s $B/debug/override-loop` → objectsInLoop **0**. ownedInLoop of about 132 is the hydro plant's own props (CPU only, accepted on A-v24).
   Ruin-prop fallback (A-v24 needed a live strip; round 52 should make it unnecessary): `node tools/import-run.mjs --scale x1 --only fort-strip --dry-run` must show matched 0 on every strip line; otherwise run it again without `--dry-run`.
7. **Water fill:** `curl -s -X POST $B/map/time -H 'content-type: application/json' -d '{"settle":300,"speed":3}'`, then wait 300 s until the reservoir and rivers are full.
8. **TM:PE → Traffic:** `node cs2mcp/scripts/import-tmpe.mjs --dump "$LOCALAPPDATA/DakotaExport/2972879554_Dakota_Falls-20261019-063123.json" --body net-converter/out/x1/import-all.json --out net-converter/out/x1/tmpe-bridge/tmpe-import-request.json --report-out net-converter/out/x1/tmpe-bridge/tmpe-import-report.json --dry-run` (lane indices 2/3, no "Traffic validation" skips), then the same command without `--dry-run`. Expect 38 created, 30 notNeeded.
9. Save **`dakota-A25c-2048`**.
10. **Review shots:** `bash next-run/shots-a25.sh` → `next-run/shots-a25/`: the 31 A-v24 cameras plus `E01`–`E09`, the east exterior (divide range, buttes, border). Audit every shot before the owner sees it; fix and re-shoot first.
11. **Season test:** `node look/season-test.mjs --out next-run/shots-seasons/a25` → must report PASS 9/9.
12. **Sheets:** `powershell -File next-run/small.ps1 -Dir next-run/shots-a25`, then `powershell -File next-run/sheets.ps1 -Run a25` → `next-run/sheets-a25/` (CS1 vs A-v25 pairs).
13. **Fidelity:** `node tools/fidelity.mjs` (from `net-converter/`, newest `live-graph-*-end.json`) → expect 18/37 OK and no dead ends, as on A-v24.
14. A fix pass (a strip, a demolish, a re-import of one layer), if any, ends with save **`dakota-A25d-2048`** and a re-shoot of the affected cameras.

## Review checklist (owner)

- Water: lower river at sea level, upper reaches fed by sources, no flooding of the interchanges.
- Power lines follow the CS1 corridor and connect to the dam and substation positions in `manual-objects.md`.
- Every road and rail that reaches the border has an outside connection; 4 air + 1 ship exist.
- Tree density reads like CS1 (dense pine on the western slopes, oaks along the river, open plains east).
- Rocks sit on the slopes they came from; landmarks oriented correctly.
- Resources visible in the editor's resource overlay where CS1 had cornfields.
- Peaks: no flat 1024 m plateaus left; no seam at the playable/world-map edge.
- **New world map:** the divide range and the two buttes are visible east of the border (E05–E07, E09).
- **No seam step at the border** (E01–E04, E08: the playable edge runs into the exterior without a cliff or ledge).
- **Water line held:** no new exterior lakes or dry river beds; nothing below 93 m outside the beds.

## Publishing (only after the owner accepts the run)

Done by hand in the editor's Map Inspector, then "Share map"; nothing here is automated yet.

1. **Description:** paste the full text from `MAP-DESCRIPTION.md` (the in-map field is single-line: `next-run/publish/description-inmap.txt`) — Map Settings → Map Description; add translations only if wanted.
2. **Preview image and thumbnail:** take them in the editor's photo mode (the checklist accepts the defaults, but the defaults are the generic placeholder): preview from the river bend with the Detroit suspension bridge and the western range, thumbnail from the falls; set them in Map Settings → Preview / Thumbnail.
3. **Starting tiles:** Map Settings → Select Starting Tiles; the 9 default tiles are the map centre. Owner's pick: the 3 × 3 block around the Detroit junction (~1400, −3700), water, road and rail connections inside.
4. **Content prerequisites:** the game lists them itself (Bridges & Ports for the BXP/vanilla bridge set; the asset packs and ConcreteSet01 / Stone Arch kit are mods, not DLC — note them in the description instead).
5. **Camera start:** Map Settings → Capture Camera Properties from the preview viewpoint.
6. Final `cs2_map_checklist` must be `playable: true` with every publishing item passing; save once more under the final name (no leading or trailing space), then Share map.

**Publishing requirements (cs2-playset/PLAYSET-AUDIT.md, 2026-10-21):** the published map's PDX Mods dependencies are the Bridges & Ports DLC plus 92245, 124160, 140978, 154132 (ships BasePlateConcrete62; no 153995), 132188 (only while po-gantry is in the import), 134056, 137334, 146553, 140176, 146871, 132403, 134147, 132771, 159384, 159309, and 80095 Traffic (add by hand; the editor does not list it). Recommended in the description only: 94394 Lumina, 161533 Enhanced Graphics and Textures, 88893 Time & Weather Anarchy (Main profile!), and Anarchy / Move It / Find It for editing. Never in the requirements: CS2MCP, Scene Explorer, Image Overlay, Earth2Cities, the icon libraries, I18n Everywhere, ImportedData/TextureAssets. Before publishing: compare the final save's MapMetadata contentPrerequisites with this list.
