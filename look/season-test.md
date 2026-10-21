# Season test: every season, with screenshots

Goal: confirm that `climate.json` behaves as designed (audit/theme-remake.md §5) in every part of the year:
- foliage stays green, since all four seasons use `SeasonSummer`;
- light snow comes and goes in the cold season;
- temperatures sit in the designed ranges;
- evening fog appears in summer.

## What can and cannot be automated today

- **Can:** `cs2_set_camera` / `POST /camera/set` and `cs2_screenshot` / `GET /screenshot`. Climate and look read-back via `GET /map/settings?detail=1`.
- **Cannot (bridge gap):** setting the in-game date and time of day. The bridge has no date/time endpoint. The editor's bottom bar has the date and time sliders (calendar and clock), which work by hand.
- **Needed for an unattended run:** a bridge call `POST /time {month, day, hour, paused}` that sets the date and time of day (`TimeSystem` / `TimeData`) and reports the current temperature, precipitation, cloudiness, fog and snow cover (`ClimateSystem` values). Then a runner loop over the table below.

## Shots

Camera spots (same three every time):
- **A** the main river and the falls from the east bank, about 300 m up, looking west to the range;
- **B** a foothill slope with deciduous and conifer trees, about 150 m up (foliage check);
- **C** the highest peak from 1 km away (snow check).

| # | Date | Time | Expected (run A) |
|---|---|---|---|
| 1 | 15 January | 12:00 | about 2 °C at the start area. Foliage green. Plains snow only on the day after a cold-season precipitation event; peaks may carry light snow |
| 2 | 15 January | 06:00 | about −5 °C. If there was precipitation, a dusting on the plains |
| 3 | 1 March | 12:00 | about 6–13 °C, breezy (turbulence 0.25), some cloud; peaks light snow possible |
| 4 | 1 May | 12:00 | about 17–20 °C, green, a few showers |
| 5 | 15 July | 12:00 | about 25–28 °C, clear to broken cumulus |
| 6 | 15 July | 20:00 | about 18–20 °C. Evening fog possible (16 °C day–night swing) |
| 7 | 15 September | 12:00 | about 24–26 °C, dry |
| 8 | 15 November | 12:00 | about 10–13 °C, clear (autumn cloud chance 30) |
| 9 | 15 December | 12:00 | about 5–8 °C. The winter season starts on 1 December, but foliage stays green (SeasonSummer) |

Record per shot: the temperature on the bottom bar, the weather icon, snow (yes/no; plains, peaks), foliage colour, fog.

## Pass / fail

- Foliage must look the same in all nine shots. If it does not, the season prefab references did not apply: check `look.climate.seasons[*].season` in the read-back.
- **No snow at all** in shots 4–8 on the plains.
- Shots 1, 2 and 9: snow, if any, gone again within a day or two of game time (let the clock run).
- Temperatures within ±5 °C of the table (each month draws a random spread). If **all** of them read 5–10 °C too hot, the edited climate copy is not the current one. The A-v7 screenshot showing 31 °C at 17:56 in June points that way: re-post `climate.json`, save, reload, read back.

## A-v8 finding: the readout was stale (2026-10-20)

All A-v8 shots S1–S9 read 31 °C and showed no snow. The cause is in the decompiled `Game.dll`:
- the HUD binds `ClimateSystem.temperature`, and nothing overrides it;
- that value, plus weather, season and fog, is written only by `ClimateSystem.OnUpdate`, which runs in the EditorSimulation phase;
- `SimulationSystem` runs that phase **only while the clock runs**. `SnowSystem` also needs running steps.

The shots were taken paused after `POST /map/time`, so every one showed the last value computed before the date change. The test procedure is therefore:
1. Set the date and hour (`POST /map/time`).
2. **Run the clock** at speed 1–3 for about 10–20 s of real time; for snow (shots 1, 2, 9), a few game hours.
3. Pause, then read the values from `GET /map/time` once the bridge returns the live and directly sampled climate values (requested from the bridge session). Do not read the HUD.
4. Take the screenshot.

A paused clock also freezes snow build-up and melt, so a snow check without running time says nothing.
