# DakotaExport (CS1 mod)

Read-only Cities: Skylines 1 mod that dumps the loaded map's vector data — network nodes and
segments with Bézier control points, node elevation and flags, the network prefab table, water
sources, plopped buildings and outside connections, tree and prop density, natural resources, map
settings, the measured water surface, terrain surface classes and Procedural Objects (placement,
decimated world-space geometry, a height raster, Effects Module effects) — to JSON,
with every tree as a point in a second JSON, two PNG rasters and an SVG quick look. It is the CS1 end of the Dakota Falls → CS2 pipeline (see
`..\dakota-falls-migration-analysis.md`); the format is in [SCHEMA.md](SCHEMA.md).

The mod never modifies game state: it only reads manager buffers (the water source list is
copied under the simulation's own lock).

## Build and install

```powershell
dotnet build .\cs1-exporter\DakotaExport\DakotaExport.csproj
```

The build references `ICities`, `Assembly-CSharp`, `ColossalManaged` and `UnityEngine` from
`D:\SteamLibrary\steamapps\common\Cities_Skylines\Cities_Data\Managed` (`Private=false`, nothing
is copied) and then copies only `DakotaExport.dll` to

```text
%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\Mods\DakotaExport\DakotaExport.dll
```

Options: `-p:CitiesSkylinesManagedDir=<...\Cities_Data\Managed>` for another Steam library,
`-p:InstallMod=false` to skip the copy, `-p:ModInstallDir=<dir>` to copy elsewhere.

## Run the export

1. Start CS1, open **Content Manager > Mods**, enable **Dakota Export**.
2. Subscribe to / enable everything Dakota Falls needs (its required Workshop collection), or the
   network prefabs will load as missing and their segments will not exist.
3. Open **Editors > Map Editor > Load**, choose **Dakota Falls**.
4. About ten frames after the map has loaded the export runs once. It can take a few seconds
   (≈740 000 trees are binned). Watch for this line in
   `%LOCALAPPDATA%\Colossal Order\Cities_Skylines\output_log.txt` (or in the export's own `.log`):

   ```text
   [DakotaExport] export complete: C:\Users\<you>\AppData\Local\DakotaExport\<map>-<timestamp>.json
   ```

5. Open the `.svg` next to the JSON to check the result.

The export also runs for **New Map**, **Load Game** and **New Game**, so the `dktanew` city can
be exported the same way. Every load writes a new timestamped set; nothing is overwritten.

Each section is isolated: if one throws, the log has `section <name> FAILED` with the exception,
the section's keys are `null` in the JSON, `sectionErrors` names it, and the other sections still
run. The last log lines are `total time: … failed sections: none` and `export complete: …`. A
failure outside the sections is logged as `[DakotaExport] export FAILED:`.

`<map>` comes from `%LOCALAPPDATA%\DakotaExport\mapname.txt` (contains `dakota-falls`) because the
editor leaves the map name empty. Every file is written as `.tmp` and renamed when complete; a
leftover `.tmp` means that file did not finish.

Effects Module data (waterfall and spillway foam) is exported per PO object and for failed objects,
with world position, direction and effect name; counts per effect are in `proceduralObjects.byEffect`.

Keep **Procedural Objects**, **Effects Module**, **Tree Control**, **Network Skins** and
**Theme Mixer** enabled in the playset used for the export (Surface Painter is not used by this map);
otherwise the JSON says why their data is missing (`proceduralObjectsReason`, `terrainSurfaces.notes`).

## Output

```text
%LOCALAPPDATA%\DakotaExport\
  <map>-<yyyyMMdd-HHmmss>.json          data (SCHEMA.md, dakota-export/cs1/v5)
  <map>-<yyyyMMdd-HHmmss>-trees.json    every tree: x, z, prefab, id, y, flags, Tree Control scale (trees/v2)
  <map>-<yyyyMMdd-HHmmss>-water.png     water surface, 1081², 16-bit, row 0 = north, surface*64 where wet
  <map>-<yyyyMMdd-HHmmss>-surfaces.png  terrain surface classes, 1080², 8-bit grey, row 0 = north
  <map>-<yyyyMMdd-HHmmss>-po.json       Procedural Objects with geometry, effects and failed objects (po/v3), only if PO is active
  <map>-<yyyyMMdd-HHmmss>-po-meshes.bin    full edited PO meshes + failed-container vertices (binary, see SCHEMA.md)
  <map>-<yyyyMMdd-HHmmss>-heights-raw.png / -raw2.png / -block.png   RawHeights, RawHeights2, BlockHeights
  <map>-<yyyyMMdd-HHmmss>-sdata/        every mod-data blob in the map + index.json
  <map>-<yyyyMMdd-HHmmss>-po-heights.png   max PO mesh height, 1081², 16-bit, row 0 = north, maxY*64
  <map>-<yyyyMMdd-HHmmss>-po-heights.json  the same as sparse [index, maxY, minY, objectId]
  <map>-<yyyyMMdd-HHmmss>.svg           segments coloured by service, water sources, outside connections
  <map>-<yyyyMMdd-HHmmss>.log           counts, file sizes, time per section, errors, "export complete"
```

Tuning constants are in `DakotaExport\MapExporter.cs`: `RasterSize` (270 per-prefab tree/prop
grid; set to 1081 for 16 m cells at the cost of a much larger file), `TotalTreeRasterSize` (1081),
`PropPointLimit` (50 000).

## Notes and limits

- The ±8640 m extent is assumed (vanilla 81 tiles), not read from the game.
- Procedural Objects (Dakota Falls' peaks above 1024 m, waterfall steps, the dam, bridges) are
  exported by reflection when the Procedural Objects mod is active: placement, modules and effects,
  a decimated world-space mesh per object (≤ 2 000 vertices) and a full-resolution height raster of
  all PO triangles. Without the mod enabled the JSON says why they are missing.
- Network-owned and sub-buildings are skipped; outside-connection buildings are always kept.
- v1 was run once in the map editor (5 582 nodes, 65 water sources, 740 967 trees, 11.2 MB JSON).
  v2–v5 are verified by compiling against the installed assemblies; the JSON splicing, the PNG
  encoder (deflate and the stored fallback), the PO triangle rasterizer, the decimation, the
  temp-file commit, the mesh binary layout and the generic modifier dump were tested offline.
- Expected total for Dakota Falls (v5, estimates): about 100–150 MB. Main JSON 20–30 MB, trees
  file about 30 MB, `-sdata/` about 37 MB (the 22 blobs in the map), `-po-meshes.bin` 5–30 MB,
  three height PNGs 1–2 MB each. Earlier per-file estimates: main JSON 15–22 MB, trees file about 12 MB,
  water PNG 0.2–1 MB (2.3 MB if zlib is unavailable and PNGs are stored uncompressed),
  surfaces PNG 0.05–0.3 MB (1.2 MB stored), `-po.json` about 0.7 KB per object plus up to ~50 KB per large mesh (2 000 vertices),
  PO heights PNG 0.1–1 MB, PO heights JSON 1–5 MB depending on PO coverage.
- The terrain *look* (which texture, tiling and colour offset each class uses: grass, cliff, sand,
  ore, oil, pavement, gravel, ruined) comes from the Theme Mixer 2 mix "Dakota Falls", Workshop
  2972881605, `ThemeMix.xml`. It mixes textures from themes 1230768185 (Seychelles), 921636136
  (Springwood), 1367973971 (Owl's Albedo) and 1594406264 (r69-coastal), with LUT Relight2Natural.
  The mix sets how each class looks, not where it is; `naturalResources` and `terrainSurfaces` hold
  the where.
- Terrain surface classes come from runtime data rebuilt from objects plus the persisted sand and
  Surface Painter layers. CS1's own texture blending by slope and height is not a stored
  classification and is not exported.

## Source layout

| File | Role |
|---|---|
| `DakotaExportMod.cs` | `IUserMod` entry |
| `ExportTrigger.cs` | `LoadingExtensionBase` arms on load; `ThreadingExtensionBase` runs the export on the main thread after a short delay |
| `MapExporter.cs` | reads the managers and writes the main JSON (incl. map settings, natural resources) |
| `TreePointsWriter.cs` | streams the per-tree points file |
| `WaterSurfaceExporter.cs` | water surface section and 16-bit PNG |
| `TerrainSurfaceExporter.cs` | terrain surface section, Surface Painter reflection, 8-bit PNG |
| `ProceduralObjectsExporter.cs` | Procedural Objects by reflection, `-po.json` |
| `PoGeometry.cs` | PO mesh reading and decimation, height rasterizer, PO modules and Effects Module |
| `PoFailures.cs` | failed PO containers, `-po-meshes.bin` writer |
| `ExtraExporters.cs` | height PNGs, Network Skins, serializable-data dump, Theme Mixer, generic field dump |
| `SafeFile.cs` | write-to-`.tmp`-then-rename helpers |
| `ModReflection.cs` | side-effect-free reflection helpers for optional mods |
| `PngWriter.cs` | greyscale PNG encoder (deflate, verified, with stored fallback) |
| `SvgSink.cs` | SVG quick look |
| `JsonWriter.cs` | streaming JSON writer (net35) |
| `ExportLog.cs` | Unity log + sidecar log file |
