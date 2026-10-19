# Round 5 outputs (2026-10-19)

Import in this order: network → power → water → connections → objects → trees → resources. Each file is its own import, so each can be undone alone.
out/ = 2× heights (Height Scale 4096 map), out/x1/ = true heights (Height Scale 2048 map). Both sets come from one conversion at 2× topology: same nodes and courses, x1 heights = ½.

| order | file | content | count | scale | size |
|---:|---|---|---|---|---:|
| 1 | `out/import-all.json` | network: highways + rail + roads, heightMode absolute | 4025 edges, 3866 nodes | 2× | 2.1 MB |
| 1 | `out/x1/import-all.json` | same network | 4025 edges, 3866 nodes | 1× | 2.1 MB |
| 2 | `out/import-power.json` | power: High-voltage Line, nodes on the ground | 60 edges, 62 nodes | 2× | 0.0 MB |
| 2 | `out/x1/import-power.json` | same | 60 edges | 1× | 0.0 MB |
| 3 | `out/import-water.json` | water sources (seaLevel hint 195) | 65 sources | 2× | 0.0 MB |
| 3 | `out/x1/import-water.json` | water sources (seaLevel hint 97.5) | 65 sources | 1× | 0.0 MB |
| 4 | `out/import-connections.json` | outside connections (no y) | 28 (road 19, rail 4, air 4, ship 1) | both | 0.0 MB |
| 4 | `out/x1/import-connections.json` | identical copy | 28 | both | 0.0 MB |
| 5 | `out/import-objects.json` | buildings, rocks, props (y only when > 2 m off terrain) | 3028 (prop 3015, building 4, tree 9; 2384 with y) | 2× | 0.6 MB |
| 5 | `out/x1/import-objects.json` | same, y halved | 3028 | 1× | 0.6 MB |
| 6 | `out/trees/import-trees-01..11.json` | trees (no y), shared by both scales | 508,458 in 11 files of ≤ 50 000 | both | 46 MB total |
| 7 | `out/import-resources-synthetic.json` | SYNTHETIC fertility from Corn Stalk (no naturalResources in the v1 dump) | 813 non-zero cells of 512² | both | 0.5 MB |
| 7 | `out/x1/import-resources-synthetic.json` | identical copy |  | both | 0.5 MB |

Also in both folders: `connections-check.md` (border nodes ↔ CS1 connections), `manual-objects.md` (11 objects and 5 air/ship connections for hand placement), `stats.md` (per layer), `network.geojson`, `overlay-4096.png`. Trees: `out/trees/README.md`. Retry for the v8 map: `out/import-retry.json` (e15883_0, e32650_0b, ends bound to v8 node entities).

## Open contract fields

- Object/connection prefab names come from Locale.cok Assets.NAME keys; cs2mcp/PREFAB_NAMES.md did not exist yet, so they are not reconciled with it.
- rotation sign (−CS1 angle) is unverified until the bridge's asymmetric test.
- Rock size classes (Rock01–10 small, 11–20 medium, 21–30 large, Boulder01–08) are a guess by number.
- Resources: only a synthetic fertility layer; ore/oil/forest need the v2 export.
