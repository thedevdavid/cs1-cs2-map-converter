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
| 88893 | Time & Weather Anarchy | Its "desert" profile (`ModsSettingsTimeWeatherAnarchyProfiles22e0d9….json`) has `EnableCustomTemperature: true` and `Temperature: 31`, which pins the climate at 31 °C. The active profile is not stored in those files. Keep it on the Main profile, or remove it with I18n 75426 |

### REMOVABLE now (nothing in A-v10 comes from it, and no dependency)

| id | Name | Why |
|---|---|---|
| 144834 | Terrain Textures Pack: Rocks | only in `optional/import-surfaces-rock.json` / `po-rock.json`, which the runner posts only with `--with rock,po-rock` |
| 144550 | Terrain Textures Pack - Grass | no prefab in any import file |
| 143898 | Terrain Textures Pack - Ground | no prefab in any import file, optional ones included |
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

**If you later re-enable an optional runner step** (`net-converter/tools/import-run.mjs --with …`; all are surface paints or objects that are off by default):
- `rock`: `optional/import-surfaces-rock.json`, 151 patches of `gaga TTP - Pebbles 1L dark` (brown scree) on every slope ≥ 40° of the true-height heightmap (patches ≥ 400 m²). Needs 144834.
- `po-rock`: `optional/import-surfaces-po-rock.json`, 63 patches of `gaga TTP - Pebbles 2L` over the footprints of the CS1 PO mountains, rock work and falls steps (patches ≥ 100 m²). Needs 144834.
- `decals`: `optional/import-surfaces-decals.json`, the 41 CS1 R69 Big Decal Dirt/Mud as 16 × 16 m squares: 33 `PDO23 Dirty Surface 01` (dirt) and 8 `PDO23 Soil Surface 01` (mud). Needs 148077.
- `sand-pack`: `optional/import-surfaces-sand-smooth-pack.json`, 387 patches of `PDO23 Dirty Sand Surface 01`. This is the pack version of the smoothed sand that the default run paints with vanilla `Sand Surface 01`; use one or the other. Needs 148077.
- `po-br006-arches`: `optional/import-po-structure-br006-arches.json`, BR006 variant b: 7 `StoneArchRailBridge` objects under the deck. The default is variant a, without kit objects. Needs 135944.

Without these steps, the default run uses vanilla only: vanilla rock objects, Sand Surface 01/02, Agriculture and Pavement surfaces, and terrain paint.

### MISSING

None.

`BasePlateConcrete62` is **not** missing. ConcreteSet01 154132 ships its own `BasePlateConcrete62`, and the saved map lists only 154132. The standalone BasePlateConcrete62 mod 153995 is installed but not needed. Keep it **out** of the playset: both mods define the same prefab name, which would make the name lookup ambiguous.

## Players vs build

**The published map's players need:**
- The Bridges & Ports DLC.
- The 15 content mods above: 92245, 124160, 140978, 154132, 132188, 134056, 137334, 146553, 140176, 146871, 132403, 134147, 132771, 159384, 159309.

These are exactly the `contentPrerequisites` the editor writes into the map. Traffic 80095 is recommended but not required.

All other prefabs in the A-v10 set are vanilla. No installed PDX mod contains them, and every one has a base-game locale entry (`Game/Locale.cok`). That includes rocks `Rock01–30`, `Boulder01–08`, `StandingStone*`, `Ruins*`, logs, the vanilla roads, highways, tracks and seaways, `Wooden Covered Bridge - 2 lanes`, `Truss Arch Bridge - Small Road - 2 Lanes` and the dam. Trees (`PineTree01`, `SpruceTree01`, `EuropeanSpruce01`, `BirchTree01`, `OakTree01`, `NA_Hickory`, `NA_Linden`, `GreenBushWild*`, `FlowerBushWild*`) and the surfaces the runner posts (`Agriculture Surface 01`, `Grass Surface 01` (fields file only), `Pavement Surface 01`, `Sand Surface 01/02`) are vanilla too. The PDO23 and gaga TTP pack surfaces appear only in the `optional/` files, which are not posted.

**Only the build needs:** CS2MCP, Anarchy, Move It, Find It, Scene Explorer, Image Overlay, Earth2Cities, Unified Icon Library, Asset Icon Library, ImportedData/TextureAssets, and Traffic (for the TM:PE import). Lumina, EGT and TWA (+ I18n) are personal visuals.

## The three lists

Each mod appears in exactly one list.

### 1. Can be deleted

Never used by the map. Items marked *build-time only* stay in the playset until the build is finished; remove them before publishing.

- 144834 Terrain Textures Pack: Rocks
- 144550 Terrain Textures Pack - Grass
- 143898 Terrain Textures Pack - Ground
- 148077 Dirty Surfaces Pack
- 135944 DIY Stone Arch Rail / Road Bridge Kit
- 136168 DIY Artdeco Bridge Kit / LA River Bridges
- 142603 Generic Arched Truss Bridge
- 143426 Classic Rail Truss Bridge Kit
- 133250 Tigons Rail Infrastructure Pack
- 157964 Art Deco Walls, Tunnels, and Bridges Set
- 162558 Pillar Spacing
- 160320 Bridge Builder
- local ImportedData 1424637002, 1698185143, 865402566 (SC44 train imports)
- 153995 BasePlateConcrete62: not in the playset and not needed; ConcreteSet01 ships the same prefab
- CS2MCP (local): *build-time only, remove before publishing*
- 74285 Scene Explorer: *build-time only, remove before publishing*
- 74539 Image Overlay: *build-time only, remove before publishing*
- 140919 Earth2Cities: *build-time only (E2C calibration), remove before publishing*
- 74417 Unified Icon Library: *build-time only; players who install Anarchy, Move It or Find It get it automatically*
- 79634 Asset Icon Library: *build-time only; comes with Find It*
- 75426 I18n Everywhere: *build-time only; comes with Time & Weather Anarchy*
- local ImportedData/TextureAssets: *build-time only; the map embeds its textures*

### 2. Must be a dependency on PDX Mods

The published map needs these to load its content.

- Bridges & Ports DLC (`BridgesAndPorts`, a DLC prerequisite, not a mod)
- 92245 Bridge Expansion Pack
- 124160 Bridge Expansion Pack: B&P
- 140978 Island-Type Bridge Pier
- 154132 ConcreteSet01
- 132188 Truss Grantry Prop Pack (only while the `po-gantry` step is kept)
- 134056 Asset-Dependency-Pack (Styx's)
- 137334 Cliffs and Rocks Pack
- 146553 Arid Rocks
- 140176 RZ Rocks
- 146871 Ferns
- 132403 pd Long Grass
- 134147 PD-WildFlower
- 132771 pd Bloom
- 159384 Leafy Undergrowth (Dark)
- 159309 Leafy Undergrowth (Light)
- 80095 Traffic (lane data; the editor does not add it to `contentPrerequisites` itself)

### 3. Must be a recommendation in the map description

Player-side look or convenience; the map loads without them.

- 94394 Lumina
- 161533 Enhanced Graphics and Textures
- 88893 Time & Weather Anarchy (if kept: Main profile, not "desert")
- 74604 Anarchy (for players who edit the map)
- 74324 Move It (for players who edit the map)
- 77240 Find It (for players who edit the map)
