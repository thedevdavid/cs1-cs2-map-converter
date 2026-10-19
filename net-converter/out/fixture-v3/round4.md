# Round 4 converter passes

- Crossings between layers without a shared node: 0. Upper course raised at the crossing (≥ 8 m, CS1 deck height): 0 nodes inserted, 0 ramp nodes adjusted. Already separated: 0. At grade in CS1: 0.
- Bridge profiles: 0 chains of CS1 bridge/elevated pieces, 0 changed, 0 nodes moved (max 0.0 m); grade flips above 1 % removed; single humps kept.
- Twisted courses (control point behind its end or self-intersection): 0; 0 fixed by swapping the control points, 0 made straight.
- Tiny diverge stubs (< 10 m, junction to dead end) collapsed: 0.
- Highway ramp tails snapped onto the course beside them: 0.
- Transitions (no node in (−8, 0) m, portal over one course; no deck node in (0, 2) m beside a raised node): 0 portal nodes set to 0, 0 tunnel nodes set to −12 m, 0 low nodes set to 0, 0 set to 2 m, 0 tunnel nodes capped so depth grows at most 15 % from the portal.
- Heights: CS2 = 2 × CS1 (Height Scale 4096 with the 2048 m heightmap). import files use heightMode "absolute": every node has y; ground nodes also carry elevation 0.

## Crossings

| upper | prefab | lower | prefab | x | z | CS1 Δy m | CS2 Δelev before | upper elev after | action |
|---|---|---|---|---:|---:|---:|---:|---:|---|

