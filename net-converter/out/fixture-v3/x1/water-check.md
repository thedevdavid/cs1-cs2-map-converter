# Water check (CS1 water surface vs import-water.json)

Source: waterSurface in the dump; 1,274 wet cells (depth > 0.05 m) of 15.985 m. Heights in CS1 metres; the import file uses × 1.

## Sources

| id | x | z | target | surface at source | ring 100 m median (cells) | Δ target − ring | action |
|---:|---:|---:|---:|---:|---:|---:|---|
| 1 | -1000 | 3000 | 200.00 | 203.40 | 203.40 (25) | -3.40 | height → 203.40 (×1 = 203.40) |
| 2 | 0 | 10 | 97.50 | 97.60 | 97.60 (39) | -0.10 | keep |

Sources changed: 1.

## Wet bodies

Largest body (the lower river): 1,200 cells (0.31 km²), median surface 97.60 m (× 1 = 97.60) vs chosen sea level 97.50 m (× 1 = 97.50): Δ +0.10 m.

| body | cells | km² | median surface | min | max | centre x | centre z |
|---:|---:|---:|---:|---:|---:|---:|---:|
| 1 | 1,200 | 0.307 | 97.60 | 97.60 | 97.60 | -647 | 0 |
| 2 | 49 | 0.013 | 150.00 | 150.00 | 150.00 | 1998 | 1998 |
| 3 | 25 | 0.006 | 203.40 | 203.40 | 203.40 | -1007 | 3005 |

## Wet cells above sea level that no source within 400 m explains

49 cells in 1 patches (largest 25 listed). Each may need its own source.

| patch | cells | km² | median surface (×factor) | centre x | centre z | inside ±7168 |
|---:|---:|---:|---:|---:|---:|---|
| 1 | 49 | 0.013 | 150.00 (150.00) | 1998 | 1998 | yes |
