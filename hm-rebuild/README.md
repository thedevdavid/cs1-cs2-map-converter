# hm-rebuild

Converts a Cities: Skylines 1 map-editor heightmap export (1081×1081, 16-bit PNG) into the CS2 **1:1 playable heightmap** (4096×4096, 16-bit grayscale) for **Height Scale 2048 m**.

## Run it on the real export

1. In the CS1 map editor, export the heightmap. It lands in
   `%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\MapEditor\Heightmaps\<name>.png`.
2. From `D:\work\mods\csl2-editor\hm-rebuild`:

```powershell
dotnet run -c Release -- "$env:LOCALAPPDATA\Colossal Order\Cities_Skylines\Addons\MapEditor\Heightmaps\DakotaFalls.png" --out-dir out
```

(Replace `DakotaFalls.png` with the exported file name.) Outputs in `out\`:

| File | Content |
|---|---|
| `<stem>-1to1-4096.png` | CS2 playable heightmap, 4096×4096 L16. Import with Height Scale **2048**. |
| `<stem>-fullextent-1081-x0.5.png` | Untouched 1081 grid, values ×0.5 (reference for converter/overlay work). |
| `<stem>-1to1-4096_preview.png` | 1024 px 8-bit preview, min–max normalized. |

Options: `--value-scale 0.5` (default), `--height-scale-out 2048` (logging label only), `--kernel bicubic|lanczos3` (default bicubic), `--out-dir DIR` (default current dir).

The tool prints min / max / mean / p1 / p50 / p99 and the share of pixels at the ceiling (65535 in, 32768 out = clipped CS1 peaks) for the full input, for the input samples inside the playable window, and for the output, in u16 and in metres. The last line checks that output p50 matches the input-window p50 within 0.1 m.

Input rules: exactly 1081×1081, 16-bit; grayscale (or 16-bit RGB, red channel used). Anything else is rejected with exit code 1.

## Tests

```powershell
dotnet run -c Release -- selftest
```

Builds a synthetic 1081 input from an analytic surface (tilted plane + broad Gaussian hill + a narrow hill that exceeds 1024 m and clips at 65535), runs the full PNG→PNG conversion, and compares every output cell to the analytic value at its world position ×64×0.5. Cells whose kernel support touches a clipped sample are excluded. Current result: **max error 0.81 u16** (bicubic). Also checks the geometry constants, the full-extent ×0.5 file, red-channel RGB input, rejection of 1080² and 8-bit input, and that a 0/65535 checkerboard (worst-case overshoot) never exceeds 32768. `selftest --keep DIR` keeps the synthetic files.

## Geometry convention

- CS1 source sample `i` (column; same for rows/z) is a **node** at world `x = -8640 + 16·i`, i = 0..1080 (17.28 km, origin at map centre).
- CS2 target pixel `k` is a **cell centre** at `x = -7168 + 3.5·(k + 0.5)`, k = 0..4095 (14.336 km).
- So pixel k samples source index `u = 92 + (3.5k + 1.75)/16`: k=0 → 92.109, k=4095 → 987.891. The 896-sample window is not an integer crop; the half-pixel offset is handled by continuous resampling.
- Row 0 stays row 0 (no flip); x and z use the same mapping.

**Alternative (node-centred):** treat CS2 pixel k as the node at `x = -7168 + 3.5·k`. Every sample is then taken **1.75 m** (half a CS2 pixel, 0.11 of a CS1 sample) further towards −x/−z. If CS2 turns out to be node-centred, the visible error is that 1.75 m shift; change `SrcCoord` in `Program.cs` to `(DstOrigin + DstStep * k - SrcOrigin) / SrcStep`.

## Kernel

**Bicubic, Catmull-Rom (Keys, a = −0.5)**, separable, 4×4 taps, weights normalized. It reproduces planes and quadratics exactly, which keeps the error under 1 u16 on steep slopes. Lanczos-3 (`--kernel lanczos3`) is available but misses by up to ~20 u16 next to the steep narrow hill in the self-test, so it is not the default. Before scaling, the interpolated value is clamped to [0, 65535], so overshoot near clipped peaks can never push the output above round(65535 × 0.5) = 32768.

## Values and metres

- Output = round(clamp(resampled, 0, 65535) × value-scale), half away from zero.
- Metres in: `v / 64` (CS1, 65535 ≈ 1024 m). Metres out: `v × 2048 / 65535`. The two conventions differ by 0.0015 % (0.006 m at 400 m).
- The p50 check compares the input samples in the window [92..988] against the resampled output, so a few cm of difference come from the different sample sets, not from a conversion error (synthetic test: +0.08 m).

## Build

.NET 10 SDK, `dotnet build -c Release`. Dependency: SixLabors.ImageSharp 3.1.x (NuGet reports advisory GHSA-wmxv-xphr-5c9g, moderate; irrelevant for local trusted files).

## `peaks`: rebuild the clipped summits (v2)

```powershell
$H = "$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Heightmaps"
dotnet run -c Release -- peaks "$H\dktflls-1to1-4096.png" "$H\dktflls-1to1-4096_worldmap.png" --out-dir out\v2
```

Writes `<stem>-v2.png`, `<stem>-v2_worldmap.png`, `stats.md` and before/after hillshade previews to `out\v2`. It does not install anything; copy the two PNGs into the Heightmaps folder yourself.

- **Components:** 8-connected pixels at 32768 (the 1024 m CS1 clamp after ×0.5).
- **Rim slope s:** mean gradient on the ring 12–30 px outside. Closer in, the slope is flattened by the bicubic support of the clipped samples (≤ 9 px) and by CS1's own clamp. On Dakota Falls it measures 0.02 at 1–4 px and 0.19 at 20–30 px.
- **Dome:** `h = 1024 m + R·s·g(u)`, with `u` = inner distance / inscribed radius R, `g(u) = c0·u + a·u² + b·u³`, `g'(0) = c0 = s_edge/s` (the measured slope 1–4 px outside, so there is no crease along the old outline), `g(1) = 1.5`, `g'(1) = 0`. The map edge counts as a dome boundary, so a plateau that runs off the map returns to about 1024 m there and matches the world-map exterior.
- **Cap:** `min(1024 + 2.5·R·s, 1900 m)`.
- **Smoothing and noise:** margin ≤ 12 px outside, where pixels are only ever raised towards the continued slope line. σ = 3 px Gaussian smoothing, faded out by 11 px. Value-noise fBm ≤ 4 m, scaled by the local dome slope and tapered to 0 at the rim.
- **Guarantee:** pixels farther than 12 px from every component are bit-identical. This is asserted on the real run, and `selftest` asserts it on a synthetic map.
- **World map:** the existing inner is checked to equal hmap-ext's `box_down(4)` of v1 (`(sum + 8) / 16`, at offset 1536). Then box_down(4) of v2 is spliced in the same way, and the exterior is untouched. The seam ring (first exterior ring vs last interior ring) is reported before and after.

### Variant v2b (taller summit, lift starts outside the plateau)

```powershell
dotnet run -c Release -- peaks "$H\10181339-dktflls-1to1.png" "$H\10181509-dktflls-1to1_worldmap.png" --out-dir out\<MMddHHmm> --time-prefix --world-tag .placeholder-hm-rebuild --peak-factor 4 --cap-factor 5 --start-outside 12 --min-variant-pixels 1000
```

**Naming (owner convention since 2026-10-19):** files in the CS2 Heightmaps folder are named `<MMddHHmm>-dktflls-1to1.png` and `<MMddHHmm>-dktflls-1to1_worldmap.placeholder-hm-rebuild.png`, where `<MMddHHmm>` is the production time. `--time-prefix` produces these names (preview files too), so you can copy them into the folder as is. v2b was produced at `10190449`.

- **Which components:** only those with at least 1000 px. Smaller ones keep the v2 dome.
- **Lift:** `h = source + (peak − 1024 m)·smoothstep(dd / (R + 12 px))`, where `dd` is the distance from the line 12 px outside the plateau. The lift has zero slope where it starts and at the summit, so the rim slope is raised smoothly and the top is concave.
- **Map edge:** the lift tapers to 0 over the last 12 px before the map edge.
- **Peak and cap:** peak = 1024 m + 4·R·s, capped at min(1024 m + 5·R·s, 1900 m).
- **Defaults:** without these flags the tool reproduces v2 byte for byte.

## `stamp`: raise terrain to the tops of terrain-like Procedural Objects

```powershell
$H = "$env:USERPROFILE\AppData\LocalLow\Colossal Order\Cities Skylines II\Heightmaps"
$PO = "D:\work\mods\csl2-editor\cs1-offline\out"
dotnet run -c Release -- stamp "$H\<MMddHHmm>-dktflls-1to1.png" "$H\<MMddHHmm>-dktflls-1to1_worldmap.placeholder-hm-rebuild.png" --po-json "$PO\po-offline-heights.json" --po-raster "$PO\po-offline-heights.png" --ids terrain-ids.txt
```

Writes `out\<now>\<now>-dktflls-1to1.png`, `<now>-dktflls-1to1_worldmap.placeholder-hm-rebuild.png`, `<now>-dktflls-1to1_stamp<k>_before-after.png` (one per stamped region, largest first, `--max-previews 12`) and `stats.md`. Copy the two PNGs into the Heightmaps folder as they are.

- **PO source:** the JSON (`[index, maxY, minY, objectId]` rows, as a top-level array or the first array property) is preferred. It carries object ids and heights above 1024 m. Without it, the raster is read as one object `all` with metres = value / `--po-units-per-m` (default 64). Values of 65535 saturate at 1024 m, and stats.md warns when that happens.
- **Object filter:** `--ids FILE` takes one id per line (`#` comments allowed); without it, all objects are used.
- **Resample:** the same geometry as the 1:1 rebuild (CS1 cell i at x = −8640 + 16·i → 4096 cell centres). Lanczos-3 normalized convolution: height = conv(m·h) / conv(m), footprint = conv(m) ≥ 0.5. Heights are clamped to the covered cells of the central 4×4 support, so there is no ringing at footprint edges.
- **Stamp:** where the PO top is more than 0.5 m above the terrain (`--min-rise`), out = max(terrain, top). Outside that, a 6 px feather (`--feather`) uses the highest stamped top within 6 px, weighted from 1 down to 0.
- **Slope limit (`--max-slope 1.2`, 0 = off):** a lower envelope `min_q surf(q) + k·|p − q|` (two-pass 8-neighbour chamfer), never below the terrain. Wall tops erode into ridges with sides of slope k. In the fixture, a 32 m thick, 80 m tall wall ends up 42 m tall.
- **Guarantee:** pixels outside the stamped footprints plus the 6 px feather are bit-identical. This is asserted on every run and in `selftest`.
- **World map:** the input world map's inner is checked against `box_down4(input)`, then replaced with `box_down4(output)`. Seam ring numbers before and after go in stats.md.

### `stamp` on the real data (2026-10-19)

```powershell
$PO = "D:\work\mods\csl2-editor\cs1-offline\out"
dotnet run -c Release -- stamp "$H\10190442-dktflls-1to1.png" "$H\10190442-dktflls-1to1_worldmap.placeholder-hm-rebuild.png" --po-json "$PO\po-offline-heights.json" --po-raster "$PO\po-offline-heights.png" --ids inputs\terrain-like-ids.txt --max-slope 1.2 --preview-at 526,697,falls-central,1100 --preview-at -6605,1162,falls-west,700 --preview-at 542,597,dam-central,500 --preview-at -6553,1063,dam-west,400
```

- **Id list:** `inputs\terrain-like-ids.txt` holds the 399 ids whose `class` is `terrain-like` in `po-offline.json` (falls steps, mountain, rock work).
- **Orientation:** the PO raster and the CS1 heightmap both have row 0 = north. Checked on 5,582 road nodes of the DakotaExport dump: heightmap vs node terrain differs by 0.06 m with row 0 = north, 54 m flipped.
- **Variants:** `10190604` is unlimited (`--max-slope 0`); `10190607` is limited at 1.2 m/m (5×5 chamfer).
- **Extra options:** `--preview-at x,z,label[,sizeM]` adds crops at world positions. `--name BASE` sets the base name after the time prefix. A run refuses to overwrite an existing `<prefix>` file, which happens when two runs fall in the same minute.

### `stamp` with the extractor's v2 layers (current)

```powershell
$V2 = "D:\work\mods\csl2-editor\cs1-offline\out\v2"
# default: mountain + rock work, no slope limit, installed with its time prefix
dotnet run -c Release -- stamp "$H\10190442-dktflls-1to1.png" "$H\10190442-dktflls-1to1_worldmap.placeholder-hm-rebuild.png" --po-dir $V2 --install $H
# optional falls layer on top of that result, under its own name
dotnet run -c Release -- stamp "$H\<prefix>-dktflls-1to1.png" "$H\<prefix>-dktflls-1to1_worldmap.placeholder-hm-rebuild.png" --po-dir $V2 --layer falls --name dktflls-1to1-falls --install $H
```

- **Finding layers:** `--layers` (default `mountain,rock work`) matches file names in `--po-dir` by letters and digits only. A 4096 playable-frame file (`*-cs2-4096.png`, metres × 32) is used as is; a 1081 file is the fallback and is resampled (pass `--po-units-per-m 32` for v2 encoding).
- **Falloff (16 px, `--falloff`):** a smoothstep from the nearest stamped top down to the terrain. It is then smoothed with σ = 2 px over the margin only, which stops the nearest-pixel streaks without a lip at the rim.
- **Step smoothing:** σ = 1.5 px (`--smooth-sigma`) is applied only within 3σ of PO pixels whose neighbour differs by more than 6 m (`--step`). Terraces flatten out (in the fixture, the largest step drops from 10 m to 2.8 m) while gentle relief and ridgelines stay as they are.
- **Slope limit:** off by default; `--max-slope` is still available.
- **`--install DIR`:** copies the playable map and its placeholder world map and refuses to overwrite an existing file.
- **Stats file:** a run with `--name` writes `<prefix>-<name>_stats.md`, so two runs in the same minute don't overwrite each other's stats.

### Falls steps capped at the CS1 water line (`--water-cap`)

```powershell
$W = "$env:LOCALAPPDATA\DakotaExport\2972879554_Dakota_Falls-20261019-063123-water.png"
dotnet run -c Release -- stamp "$H\10190615-dktflls-1to1.png" out\10190615\10190615-dktflls-1to1_worldmap.placeholder-hm-rebuild.png --po-dir $V2 --layer falls --water-cap $W --po-mesh ..\cs1-offline\out\po-offline.json --ids $V2\ids-falls-steps.txt --name dktflls-1to1 --install $H
```

- **Heights:** taken from the v2 falls-steps layer (full-resolution mesh).
- **Object ownership:** each layer pixel belongs to the object whose decimated mesh from `po-offline.json` is highest there; pixels no mesh covers take the nearest owned pixel within 4 px.
- **Per-object cap:** top' = min(top, CS1 water − 0.5 m) on wet pixels. A pixel is wet if its nearest CS1 cell is wet; its water height is bilinear over the wet cells of the surrounding 2×2. Objects whose largest rise after the cap is under 1 m (`--min-add`) are skipped.
- **Final surface cap:** after the falloff and smoothing, wet pixels are held at max(terrain, water − 0.5 m), so skirts from bank rocks can't build a wall across the channel.
- **Stats:** `<prefix>-<name>_stats.md` lists per object the layer, wet and capped pixels, the capped CS1 cells, and the maximum added height raw and capped.
