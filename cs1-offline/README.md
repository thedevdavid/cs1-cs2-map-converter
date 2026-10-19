# cs1-offline

Reads map content straight from the Dakota Falls Workshop map (`2972879554\Dakota Falls.crp`), without the game. The point is the 86 Procedural Objects that never load with PO 3071492847, plus PO meshes, IMT markings, Node Controller and TM:PE settings, which the in-game exporter does not carry. IMT and TM:PE data are kept for later CS2 mods: an IMT rebuild (`IMT-FOR-CS2.md`) and TM:PE → Traffic (`TMPE-TO-TRAFFIC.md`).

## Regenerate

```bash
node cs1-offline/run.mjs
```

Takes about 5 s. Needs Node and the .NET 10 SDK (`dotnet run` file-based app; the first run restores the `System.Formats.Nrbf` package). Options, all with working defaults:

| option | default | used for |
|---|---|---|
| `--crp` | `D:/SteamLibrary/steamapps/workshop/content/255710/2972879554/Dakota Falls.crp` | everything |
| `--workshop` | `D:/SteamLibrary/steamapps/workshop/content/255710` | PO base meshes (triangles) from the asset .crp files |
| `--dump` | `%LOCALAPPDATA%/DakotaExport/map-20261018-132303.json` | road geometry for the IMT markings (any DakotaExport main JSON of this map) |
| `--heightmap` | `%LOCALAPPDATA%/Colossal Order/Cities_Skylines/Addons/MapEditor/Heightmaps/DakotaFalls.png` | optional: preview backdrop and above-terrain counts |
| `--out` | `cs1-offline/out` | output folder |

Steps: `src/extract.mjs` (mod-data payloads → `out/payloads/`), `src/po-offline.cs` (Procedural Objects), `src/tmpe-offline.cs` (TM:PE, with the CS1 lane table decoded from NetManager data), `src/markings.mjs` (IMT + Node Controller), then `src/audit-corrections.md` is copied and `out/manifest.json` is written.

## Outputs

Sizes from the 2026-10-19 run:

| file | size | content |
|---|---:|---|
| `po-offline.json` | 18.6 MB | schema `dakota-offline/cs1/po/v1`: all **690** PO objects, one per line |
| `po-offline-heights.png` | 86 KB | 1081² 16-bit grey, row 0 north, value = max PO y × 64 (0 = no PO); same frame as a CS1 heightmap export |
| `po-offline-heights.json` | 1.6 MB | sparse cells `[index, maxY, minY, objectIdOfMaxY]`, index = row*1081 + col, x = −8640 + col*16, z = 8640 − row*16 |
| `po-offline-heights-preview.png` | 255 KB | PO heights (orange → yellow = higher) over the CS1 terrain (grey) |
| `po-classes.md` | 12 KB | classes, counts, bounding boxes, base prefabs, ten largest per class |
| `imt-markings.geojson` | 1.9 MB | schema `dakota-offline/cs1/imt/v1`: 2 087 features: 2 000 LineStrings (986 barrier, 1 014 paint) and 87 filler Polygons (4 of 91 fillers have an unresolved vertex and are skipped), coordinates `[x, z, y]` in CS1 metres |
| `imt-summary.md` | 2 KB | IMT counts and method |
| `nc-summary.md` | 126 KB | Node Controller, one row per node (1 067) |
| `tmpe-offline.json` | 79 KB | schema `dakota-offline/cs1/tmpe/v1`: 68 lane connections and 39 lane arrows, each lane resolved to segment, lane index, node and world end position |
| `tmpe-summary.md` | 5 KB | TM:PE list sizes, connections per node, arrows |
| `audit-corrections.md` | 4 KB | corrections for `cs1-playset/AUDIT.md` (hand-written; source in `src/`) |
| `manifest.json` | 1 KB | inputs and file sizes of the last run |
| `payloads/` | 38 MB | the 22 raw mod-data payloads (gzip ones unzipped), plus `index.json` |

### `po-offline.json`

Line 1 is `{"schema":"dakota-offline/cs1/po/v1",`. Then header keys (`coordinates`, `maxVerticesPerObject`, `decimation`, `layers` (empty: the map defines none), `count`, `notLoadingInPlayset`), then `objects`, one per line:

| key | meaning |
|---|---|
| `id`, `basePrefab`, `baseInfoType` | PO id, base prefab name (`<workshop id>.<asset>_Data`), `PROP` or `BUILDING` |
| `class`, `subClass` | see `po-classes.md` |
| `loadsInPlayset` | false for the 86 objects on `1094334744.Cube_Data`, which PO 3071492847 drops on load |
| `position`, `rotation`, `yawDegrees` | CS1 world metres; quaternion `[x,y,z,w]`; heading of local +z, clockwise from north |
| `boundsMin`, `boundsMax` | world-space bounding box |
| `meshStatus`, `meshEdited`, `meshSource` | 2 = vertices edited in PO; `package.crp:mesh` the triangles came from |
| `vertexCountOriginal`, `triangleCountOriginal`, `decimated` | counts before decimation |
| `vertices`, `triangles` | flat world-space `[x,y,z,…]` (0.01 m) and index triples; at most 2 000 vertices (uniform vertex clustering); `triangles` is null for the 2 vanilla `Castle Ruins` objects (no asset package to read) |
| `textureName`, `color`, `group`, `layer`, `visibility`, `renderDistance`, `flipFaces`, `moduleCount` | PO properties; `group` = `{rootId, isRoot}` |

World vertices = `position + rotation × localVertex`, as PO renders them (`Graphics.DrawMesh(mesh, m_position, m_rotation)`). PO stores no triangles: they come from the base prefab's mesh in its asset .crp, matched by vertex count. 976 831 vertices in, 424 523 out.

### Classes (from `po-classes.md`)

| class | subclass | objects | not loading |
|---|---|---:|---:|
| terrain-like | mountain | 239 | 0 |
| terrain-like | rock work | 92 | 0 |
| terrain-like | falls steps | 68 | 0 |
| structure | bridge/pillar | 54 | 0 |
| structure | tunnel/shed | 46 | 0 |
| structure | paved block (PO cubes, 8–39 m) | 85 | 85 |
| structure | wall/ruin | 2 | 0 |
| decal | paving | 1 | 1 |
| other | water effect (animated waterfalls) | 46 | 0 |
| other | effect emitter (zero size, `Dam Electricity` + Effects module) | 57 | 0 |

There is no PO "dam": the 57 Dam Electricity objects are invisible emitters. The falls' rock steps are in `falls steps`.

### Heights

632 objects stamped (no decals, no zero-size emitters), 58 577 cells. Maximum **1 494.7 m** (object 370, Mossy Grey Rock 22 at −7 986, 5 948: map-edge mountains, where CS1 terrain is capped at 1 024 m). Inside ±7 168 m the maximum is 1 457.5 m (object 224). The highest non-mountain object is 311 m (a tunnel block). 42 313 cells are more than 1 m above the CS1 terrain, 38 163 more than 10 m. The 46 waterfall water meshes are included; drop them by `objectId` if only solid ground should be stamped. Note: this v1 PNG uses value = metres × 64, so it clips above 1 024 m (the JSON is not clipped). Prefer v2.

### Heights v2 (`out/v2/`, written when the CS1 heightmap is available)

- **Layers**: terrain-like objects only, in four layers: mountain, rock work, falls steps, and all three combined (`terrain-like`). Per cell, the maximum over the layer.
- **Frames**: each layer comes in two frames. `*-cs1-1081.png` is the CS1 vertex grid at 16 m. `*-cs2-4096.png` is the CS2 playable frame: 4096² pixels of 3.5 m over ±7 168 m, pixel centres, rasterized straight from the triangles.
- **Encoding**: 16-bit, row 0 north, **value = metres × 32** (2 048 m scale), 0 = none.
- **Filters**:
  - Spike: height added > 2 × √footprint → removed (2 objects).
  - Overhang: underside more than 5 m above terrain on over half the footprint. Removed for rock work and falls steps (3 objects); kept and filled solid for mountains (9).
- **Lists**: tables in `out/v2/README.md`, per-object numbers in `objects-v2.json`, id lists in `ids-*.txt`, falls-steps terrain vs top in `falls-steps.md`.

### TM:PE

- The map uses only lane connections (68, at 26 interstate junctions) and lane arrows (39); every other TM:PE list is empty.
- TM:PE stores CS1 lane ids, so the CS1 lane table is decoded from the map's `NetManager` data (DataSerializer/EncodedArray layout from the decompiled CS1 1.21 code; this map's data version predates `m_eventRoute`, which the parser detects).
- Check: all 5 582 node positions equal the dump's; 4 879 of 4 881 lane chains have the prefab's lane count.
- Reuse in CS2: `TMPE-TO-TRAFFIC.md`.

### IMT

- Point and line geometry follow IMT 1.15 (decompiled).
- Segment corners are rebuilt from neighbouring edges; Node Controller offsets override. Expect a few metres of error at large junctions and ~0 elsewhere.
- 991 junction rules have zero length (both points at the same spot of a two-segment node) and are left out.
- Barrier lines sit 2.8 m and 4.6 m from the interstate centrelines, the drive-lane edges of these one-way prefabs.
