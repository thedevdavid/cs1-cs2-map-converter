# DakotaFalls-E2C playset audit (2026-10-21)

## Sources
- **Placed prefabs:** [prefab-counts-a10.txt](prefab-counts-a10.txt), the A-v10 import set (converter bf4d1a9/0fe9f35, `out/x1`).
- **Prefab-to-mod mapping:** every installed `pdx_mods` package was scanned with `node scan-assets.mjs --all`, and each prefab name was matched case-insensitively against the `.Prefab` names in the packages' `.cok` bundles. Names that no package contains are vanilla or DLC.
- **Saved map:** `contentPrerequisites` in `Maps\…\dakota-A9c-2048.cok` (`.MapMetadata`), the newest saved map. A-v10 itself was lost to a crash before it was saved.
- **Dependencies:** `pdx_mods_cache.json` `dependencies`.
- **Runner:** `net-converter/tools/import-run.mjs` and the `out/x1/optional/` files (read only).

The map's prerequisites match the prefab mapping exactly: **BridgesAndPorts DLC + 15 mods**, listed in the first table below.

## Per mod

Status key:
- **REQUIRED:** a placed prefab or net comes from the mod, or the saved map lists it.
- **TOOLING:** needed only while building.
- **DEPENDENCY:** needed by a REQUIRED or TOOLING mod.
- **OPTIONAL-LOOK:** player-side visuals only.
- **REMOVABLE:** nothing placed from it, and nothing depends on it.

### REQUIRED (map content; players need these)

| id | Name | Category | Placed in A-v10 (count) |
|---|---|---|---|
| 92245 | Bridge Expansion Pack | bridges | `BXP Truss Viaduct - Train` ×21, `BXP Truss Arch Bridge - Highway Oneway - 2 Lanes` ×8 |
| 124160 | Bridge Expansion Pack: B&P | bridges | `BXP SuspensionBridge03 - Six Lane Highway` ×1 |
| 140978 | Island-Type Bridge Pier | bridge props | `ITBP_Pillar Piers 01` ×50 |
| 154132 | ConcreteSet01 | concrete props | `Pillar4x16` ×81, `Pillar2x8` ×56, `BasePlateConcrete16` ×36, `Wall32x16` ×14, `BasePlateConcrete32` ×7, `BasePlateConcrete62` ×1 |
| 132188 | Truss Grantry Prop Pack | road props | `HWY Truss Gantry 2L  A` ×1. Opt-in runner step `--with po-gantry`; drop the mod if that step is dropped |
| 134056 | Asset-Dependency-Pack (Styx's) | grass | `Grass01–06 (Styx's)`, 2 227 in total |
| 137334 | Cliffs and Rocks Pack | rocks/cliffs | `CliffStack` ×378, `CliffRock5/6/3`, `CliffSide2/Left/Right`, `CliffSideTall`, `CliffSlim` (891 in total) |
| 146553 | Arid Rocks | rocks | `RZaridrock1–5`, 692 in total |
| 140176 | RZ Rocks | rocks | `rzrock1–8`, 601 in total |
| 146871 | Ferns | vegetation | `gaga - Fern 1/2`, 478 in total |
| 132403 | pd Long Grass | grass | `pd Long Grass` ×119 |
| 134147 | PD-WildFlower | vegetation | `PD-YellowFlower` ×127 |
| 132771 | pd Bloom | vegetation | `pd Bloom` ×26 |
| 159384 | Leafy Undergrowth (Dark) | vegetation | `ReikUndergrowth02Trimmed` ×52 |
| 159309 | Leafy Undergrowth (Light) | vegetation | `ReikUndergrowth01Trimmed` ×47 |
| 80095 | Traffic | data | Nothing placed and **not** a map prerequisite. Keep it for the build: the TM:PE data import goes through it (bridge `cs2_import_tmpe`). Players without it load the map without the lane rules |

### TOOLING (build only; players do not need these)

| id | Name | Note |
|---|---|---|
| local | CS2MCP | the bridge; legacy `Mods\` folder, loads with every playset |
| 74604 | Anarchy | placement overrides |
| 74324 | Move It | manual fixes |
| 77240 | Find It | asset search |
| 74285 | Scene Explorer | inspection |
| 74539 | Image Overlay | reference overlay |
| 140919 | Earth2Cities | only for the E2C calibration import; remove it once that experiment is closed |
| local | ImportedData/TextureAssets | imported textures (830 MB, changed 2026-10-20); the map embeds its terrain textures, so players do not need it |

### DEPENDENCY

| id | Name | Required by |
|---|---|---|
| 74417 | Unified Icon Library | Anarchy, Move It, Find It |
| 79634 | Asset Icon Library | Find It (each declares the other) |
| 75426 | I18n Everywhere | Time & Weather Anarchy. Removable together with TWA |

### OPTIONAL-LOOK (player-side, not map content)

| id | Name | Note |
|---|---|---|
| 94394 | Lumina | lighting/LUT |
| 161533 | Enhanced Graphics and Textures | rendering |
| 88893 | Time & Weather Anarchy | Reported: its "desert" profile pinned the climate at 31 °C. Keep it on the Main profile, or remove it with I18n 75426. I did not re-check the setting file |

### REMOVABLE now (nothing in A-v10 comes from it, and no dependency)

| id | Name | Why |
|---|---|---|
| 144834 | Terrain Textures Pack: Rocks | only in `optional/import-surfaces-rock.json` / `po-rock.json`, which the runner posts only with `--with rock,po-rock` |
| 144550 | Terrain Textures Pack - Grass | no prefab in any import file |
| 143898 | Terrain Textures Pack - Ground | only in optional surface files |
| 148077 | Dirty Surfaces Pack | only in `optional/import-surfaces-decals.json` / `sand-smooth-pack.json` (`--with decals,sand-pack`) |
| 135944 | DIY Stone Arch Rail / Road Bridge Kit | `StoneArchRailBridge` only in `optional/import-po-structure-br006-arches.json` (`--with po-br006-arches`) |
| 136168 | DIY Artdeco Bridge Kit / LA River Bridges | no prefab placed |
| 142603 | Generic Arched Truss Bridge | no prefab placed (`PittTrussBridge`, `TrussBridgeHollow`) |
| 143426 | Classic Rail Truss Bridge Kit | no prefab placed (`RailBridge1–3`, `RailBridgePillar`) |
| 133250 | Tigons Rail Infrastructure Pack | none of its 177 prefabs placed |
| 157964 | Art Deco Walls, Tunnels, and Bridges Set | no prefab placed |
| 162558 | Pillar Spacing | code mod; the imports place pillars through the bridge |
| 160320 | Bridge Builder | code mod; not used by the runner |
| local | ImportedData/1424637002, 1698185143, 865402566 | SC44 Pere Marquette train assets (imported); nothing placed |

**If you later re-enable an optional runner step:** `--with rock`, `po-rock`, `decals` or `sand-pack` brings back 144834 / 143898 / 148077, and `--with po-br006-arches` brings back 135944.

### MISSING

None.

`BasePlateConcrete62` is **not** missing. ConcreteSet01 154132 ships its own `BasePlateConcrete62`, and the saved map lists only 154132. The standalone BasePlateConcrete62 mod 153995 is installed but not needed. Keep it **out** of the playset: both mods define the same prefab name, which would make the name lookup ambiguous.

## Players vs build

**The published map's players need:**
- The Bridges & Ports DLC.
- The 15 content mods above: 92245, 124160, 140978, 154132, 132188, 134056, 137334, 146553, 140176, 146871, 132403, 134147, 132771, 159384, 159309.

These are exactly the `contentPrerequisites` the editor writes into the map. Traffic 80095 is recommended but not required.

All other prefabs in the A-v10 set are vanilla. No installed PDX mod contains them, and every one has a base-game locale entry (`Game/Locale.cok`). That includes rocks `Rock01–30`, `Boulder01–08`, `StandingStone*`, `Ruins*`, logs, the vanilla roads, highways, tracks and seaways, `Wooden Covered Bridge - 2 lanes`, `Truss Arch Bridge - Small Road - 2 Lanes` and the dam.

**Only the build needs:** CS2MCP, Anarchy, Move It, Find It, Scene Explorer, Image Overlay, Earth2Cities, Unified Icon Library, Asset Icon Library, ImportedData/TextureAssets, and Traffic (for the TM:PE import). Lumina, EGT and TWA (+ I18n) are personal visuals.
