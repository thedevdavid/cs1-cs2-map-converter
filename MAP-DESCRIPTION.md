# Dakota Falls (CS2): published map description draft

This is the text for the published map's description: Map Settings → Description in the editor, and the long description on Paradox Mods.

**Rule:** the "Recommended mods and settings" section must be in the published description. Keep it in step with [cs1-playset/theme/GAP-MODS.md](cs1-playset/theme/GAP-MODS.md) and [cs1-playset/theme/LUMINA-MAPPING.md](cs1-playset/theme/LUMINA-MAPPING.md).

Final map: `dakota-A24d-2048` (run A accepted 2026-10-22). The Required mods list comes from [cs2-playset/PLAYSET-AUDIT.md](cs2-playset/PLAYSET-AUDIT.md) and matches the map's `contentPrerequisites`, plus Traffic 80095, which the editor does not list.

## Before publishing

- **Check the settings in game.** Nothing in the section below has been tested yet.
- **Resolve the LUT.** `Relight2Natural.cube` is converted from More Relight LUTs (CS1 Workshop 1539181199, which no longer shows publicly). Players need the file in `ModsData\Lumina\LUTS` for the External tonemapping step. Before shipping the `.cube` or the Lumina preset, get permission from that LUT author, or tell players to use ACES tonemapping instead. The text below offers ACES as the fallback.
- **Decide how players get the Lumina preset.** Either they enter the values by hand, as below, or you share `Dakota Falls.xml` somewhere.

---

## Text to paste

**Dakota Falls**

Dakota Falls is a 1:1 port of MAVER1CK's Cities: Skylines 1 map *Dakota Falls* (Steam Workshop 2972879554, "Map of the Year 2023") to Cities: Skylines II, rebuilt at true terrain heights. The Ceresota River runs through Colorado-style mountains. It splits at the start tile, drops over the broad stepped falls that give the map its name, and is held back by a large hydroelectric dam. In the west canyon, the national road runs through a rock-faced avalanche gallery. An interstate system with stack interchanges and flyovers crosses the map, and the Detroit-style suspension bridge carries six lanes over the river. On the cliff above the river fork stand the ruins of the old fort.

**Map facts**
- Playable area: 14.3 × 14.3 km (441 tiles), theme North American, height scale 2048 m with sea level at 93 m.
- Outside connections: 22 road border nodes, 4 train connections, air, ship and electricity.
- Resources: oil, ore, forest, fertile land and fish.
- 3 204 road, rail and power segments, pre-built from the original map's network.
- Bridges:
  - The Detroit river crossing: a six-lane suspension bridge with spans of 168 + 320 + 320 + 168 m.
  - Truss arch bridges at the red deck arch and the Ourense rail viaduct.
  - Truss viaducts on the rail girder spans.
  - Seven wooden covered bridges on the gravel roads.
  - Vanilla truss arch bridges at the smaller crossings.
- The west-canyon avalanche gallery: about 440 m of rock-faced concrete gallery over the national road.
- The hydroelectric dam on the Ceresota River, with its substation and high-voltage lines.
- Original trees, rocks, grass and flowers re-created from the CS1 placement.

**Required mods (Paradox Mods)**

The map does not load its content without these. The Bridges & Ports expansion is listed by the game itself.
- Bridge Expansion Pack (92245)
- Bridge Expansion Pack: B&P (124160)
- Island-Type Bridge Pier (140978)
- ConcreteSet01 (154132)
- Truss Grantry Prop Pack (132188)
- Leafy Tree by pdelmo (131824)
- Asset-Dependency-Pack (Styx's) (134056)
- Cliffs and Rocks Pack (137334)
- Arid Rocks (146553)
- RZ Rocks (140176)
- Ferns (146871)
- pd Long Grass (132403)
- PD-WildFlower (134147)
- pd Bloom (132771)
- Leafy Undergrowth (Dark) (159384)
- Leafy Undergrowth (Light) (159309)
- Traffic (80095): the map carries lane arrows and junction rules from the original map

**Recommended mods and settings (for the intended look)**

The map plays without them. They recreate the lighting of the original Cities: Skylines 1 map.

**Mods (Paradox Mods)**
- Lumina (94394)
- Enhanced Graphics and Textures (161533)
- Time & Weather Anarchy (88893)

Don't combine them with SunGlasses, RenderMastery, LOD Control, Auto Night LOD Extender or Nice Weather. They change the same settings.

**Game options → Graphics**
- Shadow quality: High
- Anti-aliasing: TAA
- Texture quality and anisotropic filtering: maximum

**Lumina**
- Options → Lumina → Latitude and Longitude Adjustments: on, then restart once. In the Lumina panel set Latitude −26 and Longitude 39 (a southern-hemisphere sun).
- Ambient occlusion: on. Intensity 2.55, Radius 1.85, Step count 10, Full resolution off.
- White balance: Temperature −5.
- Tonemapping: External with the Relight2Natural LUT, contribution 1. Without the LUT, use ACES.
- Leave Custom Sun, HDRI sky and the time-of-day slider off.

**Enhanced Graphics and Textures** (Options → Enhanced Graphics & Textures → General)
- Texture mip bias: −0.3. This applies to road, sidewalk, gravel, rail and grass surfaces you override in the mod's tabs.
- Leave everything else at its default: TAA sharpness 0, shadow resolution 0, shadow distance 0, LOD 100 %, bloom and sky exposure at their defaults.

**Time & Weather Anarchy** (in-game T&W panel)
- Create a profile "Dakota Falls" and link it to your save.
- Latitude −26, Longitude 39 (the same as Lumina).
- Time: Default. Weather and season: Default.
- Custom fog, clouds, precipitation, temperature and thunder: off.
- Custom aurora: on, Aurora 0, because there is no aurora this far south.
- Profile active: Always.


**Credits**
- **Original map:** *Dakota Falls* by **MAVER1CK** (Cities: Skylines 1, Steam Workshop 2972879554), commissioned by **reddfox**, who also wrote the lore. Mountain technique credited by the author to **ilcicci1983**; vanilla-asset bridge technique to **Teddy Radko**.
- **CS1 assets whose work was re-created here:**
  - Detroit Bridge [Ploppable] by **mr.pixelcat** (2931939156)
  - Hernando de Soto Bridge (1310328404)
  - RAILWAY 2 - Ourense Viaduct (2836620152)
  - JP Rock shed (2077702761)
  - Animated Waterfall (2072216451)
  - Mossy Grey Modular Rocks 2K (1981694823)
  - Paintable Prop Modular Rocks (2206365654)
  - Procedural Objects (3071492847), which carried the mountains, the falls and the dam
- **CS2 assets used:**
  - brianrocha (Bridge Expansion Pack, B&P)
  - Source Design (Island-Type Bridge Pier)
  - PrivateVasquez (ConcreteSet01)
  - Aether Viper (Truss Grantry Prop Pack)
  - pdelmo79 (Leafy Tree, pd Long Grass, PD-WildFlower, pd Bloom)
  - Hades_Styx (Asset-Dependency-Pack)
  - joergeske (Cliffs and Rocks Pack)
  - rzzirrah (RZ Rocks, Arid Rocks)
  - gagaXm (Ferns)
  - Reikenan (Leafy Undergrowth)
  - krzychu124 (Traffic)

**Changelog**
- **v1.0 — first release.** The full CS1 map rebuilt in CS2:
  - true-height terrain with the Procedural Objects mountains, falls steps and dam stamped into the heightmap
  - the road, rail and power network (3 204 segments), with bridges, the avalanche gallery and the hydro dam
  - water and the river system
  - trees, rocks and ground cover from the original placement
  - the fort ruins
  - 14 custom terrain textures and an edited temperate climate
  - TM:PE lane data
