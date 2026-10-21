# Season test 20261021T1346

Bridge 127.0.0.1:8642; settle 20 s (snow shots 120 s) at speed 3; summit offset −7.7 °C; tolerance ±7 °C.

**Overall: PASS** (0 climate checks failed, 0 of 9 shots failed)

## Climate checks

| Check | Result | Detail |
|---|---|---|
| current climate is the edited copy | PASS | TemperateWaterwayPassClimate (copy) |
| daysPerYear is 12 | PASS | 12 |
| temperature curve rebuilt (288 hourly keys + LoopCurve closing key = 289) | PASS | 289 |
| curve noon month 1 | PASS | 11.8 °C (design 9.3 ±7) |
| curve noon month 2 | PASS | 14.4 °C (design 12 ±7) |
| curve noon month 3 | PASS | 14.1 °C (design 14.7 ±7) |
| curve noon month 4 | PASS | 18.0 °C (design 18.3 ±7) |
| curve noon month 5 | PASS | 25.7 °C (design 22 ±7) |
| curve noon month 6 | PASS | 26.4 °C (design 25.6 ±7) |
| curve noon month 7 | PASS | 28.6 °C (design 28 ±7) |
| curve noon month 8 | PASS | 27.3 °C (design 24.2 ±7) |
| curve noon month 9 | PASS | 23.3 °C (design 20.5 ±7) |
| curve noon month 10 | PASS | 17.0 °C (design 16.8 ±7) |
| curve noon month 11 | PASS | 14.3 °C (design 13 ±7) |
| curve noon month 12 | PASS | 10.5 °C (design 9.7 ±7) |
| no plains snow possible in month 4 | PASS | midnight 2.6 °C, precip 0.07 |
| no plains snow possible in month 5 | PASS | midnight 6.8 °C, precip 0.00 |
| no plains snow possible in month 6 | PASS | midnight 12.2 °C, precip 0.00 |
| no plains snow possible in month 7 | PASS | midnight 11.0 °C, precip 0.19 |
| no plains snow possible in month 8 | PASS | midnight 11.3 °C, precip 0.05 |
| no plains snow possible in month 9 | PASS | midnight 10.9 °C, precip 0.00 |
| no plains snow possible in month 10 | PASS | midnight 1.5 °C, precip 0.00 |
| scan: no plains snow hours in month 4 | PASS | 0 h |
| scan: no plains snow hours in month 5 | PASS | 0 h |
| scan: no plains snow hours in month 6 | PASS | 0 h |
| scan: no plains snow hours in month 7 | PASS | 0 h |
| scan: no plains snow hours in month 8 | PASS | 0 h |
| scan: no plains snow hours in month 9 | PASS | 0 h |
| scan: no plains snow hours in month 10 | PASS | 0 h |
| scan: occasional summit snow in Dec–Mar (> 0 h) | PASS | 11 h of 96 |
| scan: plains snow stays light in Dec–Mar (≤ 10 % of hours) | PASS | 7 h |

## Snow outlook from the curves

| Month | Midnight °C (start / summit) | Noon °C | Precipitation | Snow possible: plains / summits |
|---|---|---|---|---|
| 1 | -1.9 / -9.6 | 11.8 | 0.01 | no / no |
| 2 | -3.3 / -11.0 | 14.4 | 0.04 | no / no |
| 3 | -0.1 / -7.8 | 14.1 | 0.00 | no / no |
| 4 | 2.6 / -5.1 | 18.0 | 0.07 | no / no |
| 5 | 6.8 / -0.9 | 25.7 | 0.00 | no / no |
| 6 | 12.2 / 4.5 | 26.4 | 0.00 | no / no |
| 7 | 11.0 / 3.3 | 28.6 | 0.19 | no / no |
| 8 | 11.3 / 3.6 | 27.3 | 0.05 | no / no |
| 9 | 10.9 / 3.2 | 23.3 | 0.00 | no / no |
| 10 | 1.5 / -6.2 | 17.0 | 0.00 | no / no |
| 11 | 5.2 / -2.5 | 14.3 | 0.00 | no / no |
| 12 | -1.9 / -9.6 | 10.5 | 0.00 | no / no |

## Full-year snow scan (hourly curve samples)

| Month | Hours with precipitation ≥ 0.1 | Snow hours: plains / summits | Max precipitation | Min temperature °C |
|---|---|---|---|---|
| 1 | 5 | 4 / 5 | 0.20 | -3.2 |
| 2 | 6 | 0 / 2 | 0.21 | -3.2 |
| 3 | 1 | 0 / 1 | 0.13 | 0.1 |
| 4 | 4 | 0 / 4 | 0.12 | 2.7 |
| 5 | 1 | 0 / 0 | 0.11 | 7.1 |
| 6 | 0 | 0 / 0 | 0.02 | 11.0 |
| 7 | 10 | 0 / 0 | 0.28 | 10.9 |
| 8 | 0 | 0 / 0 | 0.05 | 11.0 |
| 9 | 0 | 0 / 0 | 0.00 | 1.7 |
| 10 | 0 | 0 / 0 | 0.08 | 1.4 |
| 11 | 0 | 0 / 0 | 0.10 | -1.8 |
| 12 | 3 | 3 / 3 | 0.22 | -2.3 |

## Shots

| Shot | Date / hour | Expected °C | Live °C | Sampled °C | Precip. | Cloud | Fog | Rain / snow | Season | Screenshots | Result |
|---|---|---|---|---|---|---|---|---|---|---|---|
| S1 mid-winter noon | 1 / 12:00 | 9.3 | 11.8 | 11.8 | 0.01 | 0.23 | 0.00 | rain / — | SeasonSummer | S1-m1-h12-A.png, S1-m1-h12-C.png | PASS |
| S2 mid-winter dawn | 1 / 6:00 | 2.1 | 4.8 | 4.8 | 0.04 | 0.42 | 0.00 | rain / — | SeasonSummer | S2-m1-h6-A.png, S2-m1-h6-C.png | PASS |
| S3 early spring | 3 / 12:00 | 14.7 | 14.1 | 14.1 | 0.00 | 0.51 | 0.00 | — / — | SeasonSummer | S3-m3-h12-A.png, S3-m3-h12-C.png | PASS |
| S4 late spring | 5 / 12:00 | 22 | 25.7 | 25.7 | 0.00 | 0.17 | 0.00 | — / — | SeasonSummer | S4-m5-h12-A.png, S4-m5-h12-C.png | PASS |
| S5 midsummer noon | 7 / 12:00 | 28 | 28.6 | 28.6 | 0.19 | 0.48 | 0.00 | rain / — | SeasonSummer | S5-m7-h12-A.png, S5-m7-h12-C.png | PASS |
| S6 summer evening (fog possible) | 7 / 20:00 | 14.9 | 17.0 | 17.0 | 0.07 | 0.42 | 0.58 | rain / — | SeasonSummer | S6-m7-h20-A.png, S6-m7-h20-C.png | PASS |
| S7 early autumn | 9 / 12:00 | 20.5 | 23.3 | 23.3 | 0.00 | 0.10 | 0.00 | — / — | SeasonSummer | S7-m9-h12-A.png, S7-m9-h12-C.png | PASS |
| S8 late autumn | 11 / 12:00 | 13 | 14.3 | 14.3 | 0.00 | 0.36 | 0.00 | — / — | SeasonSummer | S8-m11-h12-A.png, S8-m11-h12-C.png | PASS |
| S9 early winter | 12 / 12:00 | 9.7 | 10.5 | 10.5 | 0.00 | 0.36 | 0.00 | — / — | SeasonSummer | S9-m12-h12-A.png, S9-m12-h12-C.png | PASS |

Visual checks to do on the screenshots (not automatable): foliage green in every shot (all seasons use SeasonSummer); summit snow at most a light, temporary cover in S1, S2, S9; no snow on the plains in S4–S8.
