# Round 4 converter passes

- Crossings between layers without a shared node: 36. Upper course raised at the crossing (≥ 8 m, CS1 deck height): 25 nodes inserted, 93 ramp nodes adjusted. Already separated: 11. At grade in CS1: 0.
- Bridge profiles: 93 chains of CS1 bridge/elevated pieces, 14 changed, 33 nodes moved (max 4.3 m); grade flips above 1 % removed; single humps kept.
- Twisted courses (control point behind its end or self-intersection): 0; 0 fixed by swapping the control points, 0 made straight.
- Tiny diverge stubs (< 10 m, junction to dead end) collapsed: 0.
- Highway ramp tails snapped onto the course beside them: 0.
- Transitions (no node in (−8, 0) m, portal over one course; no deck node in (0, 2) m beside a raised node): 30 portal nodes set to 0, 0 tunnel nodes set to −12 m, 50 low nodes set to 0, 47 set to 2 m, 25 tunnel nodes capped so depth grows at most 15 % from the portal.
- Heights: CS2 = 2 × CS1 (Height Scale 4096 with the 2048 m heightmap). import files use heightMode "absolute": every node has y; ground nodes also carry elevation 0.

## Crossings

| upper | prefab | lower | prefab | x | z | CS1 Δy m | CS2 Δelev before | upper elev after | action |
|---|---|---|---|---:|---:|---:|---:|---:|---|
| e15_0 | Highway Oneway - 2 lanes | e24921_0 | Small Road | -1523 | -1533 | 18.9 | 18.1 | 18.1 | separated |
| e20664_0 | Highway Oneway - 2 lanes | e24921_0 | Small Road | -1512 | -1544 | 18.6 | 17.7 | 17.7 | separated |
| e27153_0 | Double Train Track | e11658_0 | Highway Oneway - 2 lanes | -359 | -2952 | 24.3 | 19.5 | 19.5 | separated |
| e10855_0 | Double Train Track | e23716_0 | Highway Oneway - 2 lanes | -351 | -2900 | 29.6 | 28.7 | 28.7 | separated |
| e28282_0 | Highway Oneway - 3 lanes | e27902_0 | Double Train Track | 2005 | -2863 | 56.6 | 56.5 | 56.5 | separated |
| e26537_0 | Highway Oneway - 3 lanes | e28879_0 | Double Train Track | 2018 | -2872 | 56.0 | 55.9 | 55.9 | separated |
| e27946_0 | Highway Oneway - 3 lanes | e29051_0 | Small Road | 6586 | 2244 | 21.0 | 20.7 | 20.7 | separated |
| e9294_0 | Double Train Track | e1430_0 | Highway Oneway - 2 lanes | -109 | -539 | 20.8 | 25.8 | 0.0 | separated (lower course sunk) |
| e9294_0 | Double Train Track | e24715_0 | Highway Oneway - 2 lanes | -101 | -564 | 19.3 | 25.0 | 0.0 | separated (lower course sunk) |
| e22913_0 | Gravel Road | e28255_0 | Double Train Track | 3838 | -4225 | 43.3 | 40.2 | 0.0 | separated (lower course sunk) |
| e3336_0 | Gravel Road | e23907_0 | Double Train Track | 3921 | -4280 | 42.7 | 42.4 | 0.0 | separated (lower course sunk) |
| e568_0 | Highway Oneway - 2 lanes | e9848_0 | Medium Road | -4239 | -85 | 13.5 | 3.3 | 8.0 | upper raised at the crossing |
| e6254_0 | Highway Oneway - 2 lanes | e9848_0 | Medium Road | -4231 | -72 | 13.5 | 2.6 | 8.0 | upper raised at the crossing |
| e3118_0 | Highway Oneway - 1 lane | e29315_0 | Medium Road | -4224 | -60 | 13.5 | 4.3 | 8.0 | upper raised at the crossing |
| e12830_0 | Gravel Road | e10505_0 | Highway Oneway - 3 lanes | -1555 | 4402 | 21.9 | 19.5 | 20.7 | upper raised at the crossing |
| e26236_0 | Gravel Road | e36046_0 | Highway Oneway - 3 lanes | -1550 | 4376 | 22.3 | 18.4 | 21.0 | upper raised at the crossing |
| e30706_0 | Double Train Track | e25337_0 | Highway Oneway - 3 lanes | -1441 | 4331 | 24.3 | 3.5 | 14.1 | upper raised at the crossing |
| e30706_0 | Double Train Track | e19619_0 | Highway Oneway - 3 lanes | -1429 | 4302 | 24.2 | 3.3 | 14.0 | upper raised at the crossing |
| e25462_0 | Highway Oneway - 1 lane | e970_0 | Medium Road | -572 | -1317 | 13.7 | 9.5 | 12.1 | upper raised at the crossing |
| e23601_0 | Highway Oneway - 2 lanes | e970_0 | Medium Road | -566 | -1328 | 14.0 | 9.5 | 12.4 | upper raised at the crossing |
| e9348_0 | Highway Oneway - 2 lanes | e13504_0 | Medium Road | -558 | -1341 | 13.9 | 9.4 | 12.5 | upper raised at the crossing |
| e4316_0 | Highway Oneway - 1 lane | e13504_0 | Medium Road | -552 | -1352 | 13.5 | 9.3 | 12.1 | upper raised at the crossing |
| e14932_0 | Double Train Track | e7388_0 | Highway Oneway - 2 lanes | 217 | 2768 | 22.2 | 5.1 | 17.8 | upper raised at the crossing |
| e14932_0 | Double Train Track | e12489_0 | Highway Oneway - 2 lanes | 232 | 2765 | 21.6 | 4.9 | 17.7 | upper raised at the crossing |
| e4315_0 | Highway Oneway - 2 lanes | e10312_0 | Gravel Road | 1455 | 4344 | 20.7 | 19.6 | 20.3 | upper raised at the crossing |
| e36304_0 | Highway Oneway - 2 lanes | e10312_0 | Gravel Road | 1463 | 4330 | 20.7 | 19.5 | 20.4 | upper raised at the crossing |
| e22671_0 | Highway Oneway - 2 lanes | e28339_0 | Double Train Track | 2500 | 977 | 23.4 | 8.5 | 13.5 | upper raised at the crossing |
| e5111_0 | Highway Oneway - 2 lanes | e28339_0 | Double Train Track | 2502 | 962 | 23.9 | 8.9 | 14.0 | upper raised at the crossing |
| e7805_0 | Highway Oneway - 3 lanes | e16419_0 | Double Train Track | 2505 | 344 | 23.1 | 2.5 | 16.3 | upper raised at the crossing |
| e28200_0 | Highway Oneway - 3 lanes | e16419_0 | Double Train Track | 2509 | 372 | 22.3 | 2.2 | 15.4 | upper raised at the crossing |
| e28777_0 | Double Train Track | e2899_0 | Highway Oneway - 3 lanes | 3733 | 2844 | 28.7 | 24.0 | 26.0 | upper raised at the crossing |
| e16183_0 | Double Train Track | e30536_0 | Highway Oneway - 3 lanes | 3753 | 2841 | 28.5 | 24.3 | 26.2 | upper raised at the crossing |
| e21729_0 | Gravel Road | e18318_0 | Highway Oneway - 3 lanes | 3766 | 2920 | 13.4 | 7.0 | 12.2 | upper raised at the crossing |
| e21729_0 | Gravel Road | e9761_0 | Highway Oneway - 3 lanes | 3786 | 2917 | 13.2 | 6.9 | 11.9 | upper raised at the crossing |
| e11716_0 | Highway Oneway - 3 lanes | e29051_0 | Small Road | 6590 | 2270 | 21.0 | 20.3 | 20.9 | upper raised at the crossing |
| e4424_0 | Double Train Track | e12868_0 | Small Road | 6608 | 2389 | 24.7 | 23.0 | 24.2 | upper raised at the crossing |

