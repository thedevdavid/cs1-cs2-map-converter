# Water check

No CS1 water surface in this dump (v3 adds it); only the source rules are applied.

## Source changes (audit/cs2-side.md finding 6; sea level 97.5 × 1)

65 CS1 sources → 42 in import-water.json.

- 1: dropped, 1392 m outside the map (backdrop only)
- 2: dropped, 1429 m outside the map (backdrop only)
- 3: dropped, 1414 m outside the map (backdrop only)
- 13: moved inward from (-7517, 1808)
- 46: dropped, level 97.56 m ≤ sea level 97.5 + 1 m (× 1)
- 47: dropped, level 92.14 m ≤ sea level 97.5 + 1 m (× 1)
- 51: dropped, level 94.98 m ≤ sea level 97.5 + 1 m (× 1)
- 59: dropped, level 96.56 m ≤ sea level 97.5 + 1 m (× 1)
- 61: moved inward from (7337, 3091); level raised 136.31 → 139.43 (depth ≥ 2 m)
- 62: moved inward from (1467, 7583); level raised 171.75 → 178.41 (depth ≥ 2 m)
- 65: dropped, level 89.42 m ≤ sea level 97.5 + 1 m (× 1)
- 10+9+7+11+12+6: merged 6 canal feeds into one source at (1283, 4579), radius 141 m, level 139.90
- 28+27+26+5+29+25+31+30+34+32+33: merged 11 canal feeds into one source at (1248, 4521), radius 150 m, level 142.27
