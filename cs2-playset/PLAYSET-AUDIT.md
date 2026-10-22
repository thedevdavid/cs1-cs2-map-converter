# DakotaFalls-E2C playset audit: final map dakota-A24d-2048 (2026-10-22)

## Sources
- **Final map:** `contentPrerequisites` in `Maps\…\dakota-A24d-2048.cok` (`.MapMetadata`), the accepted run A map (A-v24 build).
- **Placed prefabs:** every prefab name in `net-converter/out/x1/*.json` and `out/trees/import-trees-01…08.json`, plus `out/x1/optional/import-po-structure-gantry.json`. The po-gantry step was part of the A-v24 build.
- **Prefab-to-mod mapping:** `node scan-assets.mjs --all` scans every installed `pdx_mods` `.cok` bundle. Each prefab name is matched case-insensitively against the bundles' `.Prefab` names; names that no package contains are vanilla or DLC.
- **Playset:** `playset_config.json`, DakotaFalls-E2C (active, 41 entries). **Dependencies:** `pdx_mods_cache.json` `dependencies`.
- **TM:PE:** `setup-plan.md`, A-v24 chain: TM:PE import 38 created / 30 not needed, saved in the map.

The final map's prerequisites and the prefab mapping agree exactly: **Bridges & Ports DLC + 16 mods**, listed in the first table below.

## Per mod

### REQUIRED: map content (players need these)

| id | Name | Placed in A-v24 (source file) |
|---|---|---|
| 92245 | Bridge Expansion Pack | `BXP Truss Viaduct - Train` (rail girder spans), `BXP Truss Arch Bridge - Highway Oneway - 2 Lanes` (BR006/BR007) — import-bridges |
| 124160 | Bridge Expansion Pack: B&P | `BXP SuspensionBridge03 - Six Lane Highway` (Detroit crossing BR001) — import-bridges-fixed |
| 140978 | Island-Type Bridge Pier | `ITBP_Pillar Piers 01` — import-po-structure-piers |
| 154132 | ConcreteSet01 | `Pillar4x16` (piers), `BasePlateConcrete32` (PO structures), `BasePlateConcrete16`, `Pillar2x8`, `Pillar2x16` (rock-shed gallery) |
| 132188 | Truss Grantry Prop Pack | `HWY Truss Gantry 2L  A` — po-gantry (in the build) |
| 131824 | Leafy Tree by pdelmo | `pd Leafy Tree`, 28 899 trees — import-trees |
| 134056 | Asset-Dependency-Pack (Styx's) | `Grass01–06 (Styx's)` — import-objects |
| 137334 | Cliffs and Rocks Pack | `CliffStack`, `CliffRock3/5/6`, `CliffSide2/Left/Right`, `CliffSideTall`, `CliffSlim` — import-objects, import-po-rocks, import-rock-shed |
| 146553 | Arid Rocks | `RZaridrock1–5` — import-objects |
| 140176 | RZ Rocks | `rzrock1–8` — import-objects, import-po-rocks |
| 146871 | Ferns | `gaga - Fern 1/2` — import-objects |
| 132403 | pd Long Grass | `pd Long Grass` — import-objects |
| 134147 | PD-WildFlower | `PD-YellowFlower` — import-objects |
| 132771 | pd Bloom | `pd Bloom` — import-objects |
| 159384 | Leafy Undergrowth (Dark) | `ReikUndergrowth02Trimmed` — import-objects |
| 159309 | Leafy Undergrowth (Light) | `ReikUndergrowth01Trimmed` — import-objects |
| 80095 | Traffic | No prefab. The map carries the TM:PE lane data (38 nodes). The editor does **not** list it in `contentPrerequisites`; add it by hand |

Everything else the build places is **vanilla**: no installed PDX mod contains it, and every name has a base-game locale entry. That covers:
- rocks `Rock01–30`, `Boulder01–08`, `StandingStone01`
- the fort: `RuinsCastle01`, `RuinsStoneFormation01/02`, `RuinsStoneCairns01–03`, `RuinsOldMill01`, `OldCastle01`
- trees `PineTree01`, `SpruceTree01`, `EuropeanSpruce01`, `OakTree01`, `NA_HickoryTree01`, `NA_LindenTree01`, `NA_LondonPlaneTree01`, `EU_AlderTree01`, `EU_ChestnutTree01`, `GreenBushWild01/02`, `FlowerBushWild01/02`
- `Wooden Covered Bridge - 2 lanes` (gravel trestles), `Truss Arch Bridge - Small Road - 2 Lanes`, `TrussArchBridge01`
- the hydro dam (`Hydroelectric_Power_Plant_01 Dam` / `HydroelectricPowerPlant01`), `TransformerStation01`
- all highways, roads, tracks, seaways, power lines and outside connections
- the surfaces `Agriculture Surface 01`, `Grass Surface 01`, `Pavement Surface 01`, `Sand Surface 01/02`

The pack surfaces (Terrain Textures, Dirty Surfaces) and `StoneArchRailBridge` appear only in `optional/` files that the A-v24 build did not post.

### Build tooling (players do not need these)

| id | Name | Note |
|---|---|---|
| local | CS2MCP | the bridge (legacy `Mods\` folder, loads with every playset) |
| 74285 | Scene Explorer | inspection |
| 74539 | Image Overlay | reference overlay |
| 140919 | Earth2Cities | E2C calibration experiment only |
| 74417 | Unified Icon Library | needed by Anarchy, Move It, Find It |
| 79634 | Asset Icon Library | needed by Find It (each declares the other) |
| 75426 | I18n Everywhere | needed by Time & Weather Anarchy |
| local | ImportedData/TextureAssets | imported textures; the map embeds its 14 terrain textures |
| 74604 / 74324 / 77240 | Anarchy / Move It / Find It | build tools; recommended to players only for editing |

### Player-side look (not map content)

| id | Name | Note |
|---|---|---|
| 94394 | Lumina | lighting/LUT |
| 161533 | Enhanced Graphics and Textures | rendering |
| 88893 | Time & Weather Anarchy | The "desert" profile (`ModsSettings\TimeWeatherAnarchy\Profiles\cd22e0d9….json`) has `EnableCustomTemperature: true`, `Temperature: 31`, which pins the climate at 31 °C. The description's "Dakota Falls" profile settings avoid this |

### Not used by the final map

| id | Name | Why |
|---|---|---|
| 144834 | Terrain Textures Pack: Rocks | only in optional surface files (`--with rock,po-rock`), not posted |
| 144550 | Terrain Textures Pack - Grass | no prefab in any import file (disabled in the playset) |
| 143898 | Terrain Textures Pack - Ground | no prefab in any import file (disabled) |
| 148077 | Dirty Surfaces Pack | only in optional surface files (`--with decals,sand-pack`), not posted |
| 135944 | DIY Stone Arch Rail / Road Bridge Kit | only in `optional/import-po-structure-br006-arches.json`, not posted (BR006 is variant a) |
| 136168 | DIY Artdeco Bridge Kit / LA River Bridges | nothing placed (disabled) |
| 142603 | Generic Arched Truss Bridge | nothing placed (disabled) |
| 143426 | Classic Rail Truss Bridge Kit | nothing placed (disabled) |
| 133250 | Tigons Rail Infrastructure Pack | none of its 177 prefabs placed |
| 157964 | Art Deco Walls, Tunnels, and Bridges Set | nothing placed (disabled) |
| 162558 | Pillar Spacing | code mod, unused (disabled) |
| 160320 | Bridge Builder | code mod, unused by the runner |
| local | ImportedData/1424637002, 1698185143, 865402566 | SC44 Pere Marquette train imports; nothing placed |
| 153995 | BasePlateConcrete62 | not in the playset. ConcreteSet01 ships the same prefab; keep it out to avoid an ambiguous name |

**Optional runner steps, if ever re-enabled** (`net-converter/tools/import-run.mjs --with …`):
- `rock`: 151 patches of `gaga TTP - Pebbles 1L dark` on slopes ≥ 40°. Needs 144834.
- `po-rock`: 63 patches of `gaga TTP - Pebbles 2L` on the CS1 PO mountain / rock-work / falls-step footprints. Needs 144834.
- `decals`: the 41 CS1 dirt/mud decals as 16 × 16 m `PDO23 Dirty Surface 01` / `PDO23 Soil Surface 01` squares. Needs 148077.
- `sand-pack`: 387 patches of `PDO23 Dirty Sand Surface 01`, instead of the vanilla Sand Surface 01. Needs 148077.
- `po-br006-arches`: BR006 variant b, 7 `StoneArchRailBridge` under the deck. Needs 135944.

## The three lists (act on these in Skyve)

Each mod appears in exactly one list.

### 1. Can be deleted from the playset

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

Build-time only. Keep them while you still edit the map; they are never published:
- CS2MCP (local)
- 74285 Scene Explorer
- 74539 Image Overlay
- 140919 Earth2Cities
- 74417 Unified Icon Library
- 79634 Asset Icon Library
- 75426 I18n Everywhere
- local ImportedData/TextureAssets

### 2. Must be a dependency on PDX Mods (publish page "Required mods")

The editor adds the Bridges & Ports DLC by itself.
- 92245 Bridge Expansion Pack
- 124160 Bridge Expansion Pack: B&P
- 140978 Island-Type Bridge Pier
- 154132 ConcreteSet01
- 132188 Truss Grantry Prop Pack
- 131824 Leafy Tree by pdelmo
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
- 80095 Traffic (lane data; not added by the editor)

### 3. Recommendation in the map description

- 94394 Lumina
- 161533 Enhanced Graphics and Textures
- 88893 Time & Weather Anarchy (with the description's "Dakota Falls" profile, never "desert")
- 74604 Anarchy, 74324 Move It, 77240 Find It: only for players who want to edit the map
