# IMT data kept for a later CS2 mod

Owner decision (2026-10-19): the Intersection Marking Tool data of Dakota Falls is kept, because an IMT-like mod may be built for CS2 later. Nothing in the pipeline should drop it.

## Where the data is

- Raw: `out/payloads/NodeMarkup.unz.bin` (IMT 1.14.1.1 XML, 1.3 MB) and `out/payloads/NodeMarkupUsedPrefabs.bin`. Both are regenerated from the map .crp by `node cs1-offline/run.mjs`.
- Decoded: `out/imt-markings.geojson` (986 guardrail lines = 81.6 km, 1 014 painted lines = 32.7 km, 87 fillers; CS1 metres `[x, z, y]`) and `out/imt-summary.md`.
- Prefabs used: `1889660581.Single Steel Barrier - raisable_Data` (network lines), `2889685220.Sidewalk-003 Decal_Data` (filler texture).

## XML format (decoded for `src/markings.mjs`)

- `<M Id=node>` = junction marking: its entrances are segments.
- `<S Id=segment>` = segment marking: its entrances are the two end nodes.
- Point id = `enterId | index << 16 | type << 24`, where index counts from 1, left to right looking from the node into the segment; type 0 = enter.
- Line id = `pointA << 32 | pointB`.
- `<L T=256>` regular line with rules `<R>`. Each rule has 2 supports `<E T=0 P=point/>` or `<E T=1 L=line/>` (cut at the intersection with another line) and a style `<S T=…>`.
- Stored style code = `StyleType − 257` for lines (0 Solid, 1 Dashed, 2 DoubleSolid, 160 Network) and `StyleType − 513` for fillers (512 Stripe, 515 Chevron, 641 Grass, 645 Texture).
- `<F>` filler: contour vertices `<V T=0 P=point L=line/>` / `<V T=1 L1 L2/>` (line intersection).

## Geometry rules (from the decompiled IMT 1.15, `Entrance`, `NetInfoPointSource`, `SegmentMarking`)

- Entrance position = midpoint of the two segment corners; CornerDir = left corner → right corner.
- Enter points sit at drive-lane edges (two points when the gap between lanes ≥ the mean lane half-width, else one), each shifted along CornerDir by the point's `O` offset / sin(corner angle).
- Segment-marking lines follow a Bézier between the two entrance positions (their normal directions); junction lines are Béziers with tangents into the node.
- The decompiled sources were produced with `ilspycmd` from `D:\SteamLibrary\steamapps\workshop\content\255710\2140418403\IntersectionMarkingTool.dll`; regenerate them when needed.
