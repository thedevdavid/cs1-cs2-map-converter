# Conversion outputs (2026-10-19)

Dump: `fixtures/v3/map-v3.json`. Import order: network → power → water → connections → objects → trees → resources; each file is its own import.
out/fixture-v3/ = 2× heights (Height Scale 4096), out/fixture-v3/x1/ = true heights (Height Scale 2048); same nodes and courses in both.

| order | file | content | count | size |
|---:|---|---|---|---:|
| 1 | `out\fixture-v3\import-all.json` | network (2×) | 2 edges, 3 nodes | 0.0 MB |
| 1 | `out\fixture-v3\x1\import-all.json` | network (1×) | 2 edges, 3 nodes | 0.0 MB |
| 3 | `out\fixture-v3\import-water.json` | water (2×), seaLevel 195 | 2 sources, 1 refined | 0.0 MB |
| 3 | `out\fixture-v3\x1\import-water.json` | water (1×), seaLevel 97.5 | 2 sources, 1 refined | 0.0 MB |
| 4 | `out\fixture-v3\import-connections.json` | outside connections | 0 | 0.0 MB |
| 4 | `out\fixture-v3\x1\import-connections.json` | outside connections | 0 | 0.0 MB |
| 5 | `out\fixture-v3\import-objects.json` | objects (2×) | 2 | 0.0 MB |
| 5 | `out\fixture-v3\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (2×) | 1 | 0.0 MB |
| 5 | `out\fixture-v3\x1\import-objects.json` | objects (1×) | 2 | 0.0 MB |
| 5 | `out\fixture-v3\x1\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (1×) | 1 | 0.0 MB |
| 6 | `out\fixture-v3\trees/import-trees-01.json` … `import-trees-01.json` | trees (both scales) | 3 in 1 files | |
| 7 | `out\fixture-v3\import-resources.json` | natural resources | fertility, ore, oil | 0.0 MB |
| 7 | `out\fixture-v3\x1\import-resources.json` | natural resources | fertility, ore, oil | 0.0 MB |

Reports (both folders): `water-check.md`, `surfaces-summary.md`, `surfaces.geojson`, `manual-objects.md`, `connections-check.md`, `stats.md`.
