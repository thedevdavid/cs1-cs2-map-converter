# Season test: every season, measured

Goal: confirm that `climate.json` behaves as designed (audit/theme-remake.md §5):
- foliage stays green (all four seasons use `SeasonSummer`);
- light snow comes and goes only in the cold season;
- temperatures follow the designed curve;
- evening fog is possible in summer.

Owner after A-v8: "next run do better". Every shot is now **measured** (bridge values, not the HUD) and graded PASS/FAIL by a script.

## Run (unattended)

Needs bridge **0.9.37+** (`POST /map/time` with `settle`, `GET /map/time` with `live`, `sampleAtDate` and the climate curves). The Dakota map must be open in the editor, with the look step applied (`climate.json`).

```bash
node look/season-test.mjs --out next-run/shots-seasons/a9
```

Options:
- `--settle 20`: seconds of running clock per shot (default 20);
- `--snow-settle 120`: for the cold-season shots S1, S2, S9;
- `--speed 3`;
- `--peak-offset 7.7`: run A; use 15.4 for run B;
- `--no-shots`: measurements only.

Exit code 0 = all pass, 2 = something failed. Output in the `--out` folder:
- `report.md`: climate checks, a snow outlook per month, one row per shot with the measured values and a PASS/FAIL result, and an **Overall** line;
- `report.json`;
- `S<n>-m<month>-h<hour>-<A|C>.png` screenshots.

## What the script does

1. **Curve checks** (`GET /map/time` → `climate`):
   - the current climate is the "(copy)";
   - `daysPerYear` = 12;
   - the temperature curve has 288 keys (rebuilt);
   - each month's noon temperature lies within ±7 °C of the design (below);
   - no month April–October can produce plains snow (midnight ≤ 0 °C together with precipitation ≥ 0.1).
2. **Per shot:**
   - `POST /map/time {date, hour, settle, speed}`. **The hour is encoded in `date`**: a CS2 day is one month unit of the climate curve, so `date = (month − 1 + hour/24) / 12`. The bridge's `{month, day}` form puts the day of the month into the date, which samples the curve's daily cycle at the wrong hour; day 15 is about 11:00 whatever `hour` says.
   - Wait while the clock runs, so `ClimateSystem` and `SnowSystem` update. Paused, they freeze; that is why every A-v8 shot read 31 °C.
   - Re-pin the date, read `GET /map/time`, place the camera at spots A and C, and take the screenshots.
3. **Grading per shot:**
   - the live temperature equals the sampled one (±1.5 °C), which proves the climate actually updated;
   - the sampled temperature is within ±7 °C of the expected value;
   - no snow outside the cold season, and never above 2 °C;
   - the season name is the summer prefab.

## Shots and expected values (start area, run A)

Expected = the designed night/day means interpolated between the season keys (Winter 1 Jan −5/8, Spring 1 Mar 0/13, Summer mid-July 12/28, Autumn mid-November 0/13), then the game's night-to-day cosine for the hour. The tolerance is ±7 °C, because each month draws a random spread (σ ≈ 3–4 °C).

| Shot | Month / hour | Expected °C | Snow allowed | Settle |
|---|---|---|---|---|
| S1 | January 12:00 | 9.3 | yes | 120 s |
| S2 | January 06:00 | 2.1 | yes | 120 s |
| S3 | March 12:00 | 14.7 | yes | 20 s |
| S4 | May 12:00 | 22.0 | no | 20 s |
| S5 | July 12:00 | 28.0 | no | 20 s |
| S6 | July 20:00 | 14.9 (fog possible) | no | 20 s |
| S7 | September 12:00 | 20.5 | no | 20 s |
| S8 | November 12:00 | 13.0 | no | 20 s |
| S9 | December 12:00 | 9.7 | yes | 120 s |

Monthly noon design for the curve check: Jan 9.3, Feb 12.0, Mar 14.7, Apr 18.3, May 22.0, Jun 25.6, Jul 28.0, Aug 24.2, Sep 20.5, Oct 16.8, Nov 13.0, Dec 9.7.

Camera spots (from the A-v8 run):
- **A** = (600, −2750), yaw 270, pitch 20, zoom 900 (the main river);
- **C** = (−6200, 1200), yaw 270, pitch 10, zoom 1500 (a peak).

## Snow: what to expect

- Snow forms only where precipitation is at least 0.1 and the local temperature is at or below 0 °C. The local temperature is the climate value minus 1 °C per 100 m above the start tiles; this threshold is hard-coded and does not use `m_FreezingTemperature`.
- The cold seasons have a low precipitation chance (5–9 %, about a third of Colorado's).
- The **plains**: an occasional dusting at night, melting by day.
- The **summits** (−7.7 °C in run A, −15.4 °C in run B): a light cover in December–February.
- The curve outlook table in the report shows which months allow snow at each height.
- A single settle of 120 s may or may not catch a precipitation event. A missing snow shot is not a failure; snow on the plains in S4–S8 is.

## Visual checks (not automated)

On the screenshots:
- foliage green in all nine shots;
- summit snow, if any, light;
- no white plains outside the cold season;
- for S6, note whether evening fog appears.

## History

A-v8: every shot read 31 °C and showed no snow. The cause was that the shots were taken with the clock paused. The HUD binds `ClimateSystem.temperature`, which only `ClimateSystem.OnUpdate` writes, and that runs only in EditorSimulation steps, i.e. while the clock runs. The test now settles with the clock running and reads the bridge's live and sampled values.
