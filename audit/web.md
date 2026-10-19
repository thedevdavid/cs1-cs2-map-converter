# Dakota Falls: audit against public sources

Date: 2026-10-19. Read-only audit; no pipeline files were changed. Steam pages were read in the built-in browser because WebFetch got HTTP 429. Comments came from Steam's comment endpoint (all 81). The 31 preview images were downloaded and viewed. Local references: `cs1-playset/AUDIT.md`, `net-converter/out/stats.md`, `net-converter/out/manual-objects.md`, `cs1-exporter/SCHEMA.md` (v3), and the v1 dump `%LOCALAPPDATA%\DakotaExport\map-20261018-132303.json`.

## Sources

| What | URL |
|---|---|
| Map (Workshop) | https://steamcommunity.com/sharedfiles/filedetails/?id=2972879554 |
| Change notes | https://steamcommunity.com/sharedfiles/filedetails/changelog/2972879554 |
| "Dakota Falls - Required collection" (65 items) | https://steamcommunity.com/sharedfiles/filedetails/?id=2970743270 |
| "Dakota Falls - Theme Mix" collection (12 items, holds the Render It! settings text) | https://steamcommunity.com/sharedfiles/filedetails/?id=2972871641 |
| "Dakota Falls - Theme mix" item (13 KB, Relight settings screenshot) | https://steamcommunity.com/sharedfiles/filedetails/?id=2972881605 |
| Author MAVER1CK | https://steamcommunity.com/profiles/76561199008432694 |
| Video on the map page: Infrastructurist, "Building a Highly-Detailed Realistic American Town From Scratch" (2023-07-23, 1 h 36 min) | https://www.youtube.com/watch?v=t5K4YOyWoko |
| Infrastructurist, "American Realistic County - Dakota Falls" series, e.g. ep. 2023-09-17 (the author's ilcicci1983 comment links it) | https://www.youtube.com/watch?v=DeV3NLPgL3k |
| Gaseous Stranger, "Infrastructure of Dakota Falls" (Short, 2024-11-09) | https://www.youtube.com/watch?v=rHC-_Ecdy5o |
| Gaseous Stranger, "Detailed Mountain Suburbia - Dakota Falls EP2" (2024-11-30) | https://www.youtube.com/watch?v=xdna11mP7-o |
| Third-party collection "Inspirational Cities:Skylines 1 Maps" (lists the map) | https://steamcommunity.com/workshop/filedetails/?id=3055738641 |
| Skymods mirror of the theme mix (403 from here, not read) | https://smods.ru/archives/103327 |

Searches on Reddit and the Paradox forums found no thread about the map. Search snippets claimed the Workshop item had been "removed for violating guidelines". That is wrong: the live page on 2026-10-19 shows the item, its Subscribe button and new comments.

## The Workshop item

- **Title / author:** Dakota Falls by MAVER1CK, commissioned by reddfox, who also wrote the lore. "Map of the Year 2023" (CS Map Makers Discord). Tags: Map. Header line: "AMERICAN | FICTIONAL | REALISTIC | MASS TRANSIT".
- **Dates / size:** posted 7 May 2023 11:48. There is exactly one change note, with the same timestamp, so the map was never updated after release. 44.610 MB. 38 438 unique visitors, 10 558 subscribers, 917 favourites, 209 ratings, 81 comments, 0 discussions.
- **Required items on the page:** only the **Mass Transit** DLC. The page declares no Workshop dependencies. Its dependency list is the one-click collection 2970743270, whose last update was 8 May 2023.
- **In 3 collections by the author:** Required collection (65), Theme Mix (12), Maver1ck's Maps (7).
- **Description, key points:**
  - The concept: "What would Mississippi look like if it was to flow through Colorado?" The map has Colorado mountains and a Mississippi-style river ("Ceresota River"). River features are modelled on the Mississippi around Minneapolis, i.e. St. Anthony Falls and its dams and locks. The lore puts the falls at a rocky ford named for the Dakota Nation.
  - About 740 000 trees. **Tree Control is required; Tree Anarchy (temporary fix) makes the trees disappear.**
  - Roads are stacked closely at different heights, which causes texture glitches. ROTTERdam is recommended.
  - An "electricity cube" sits in the dam and is wired to the start tile by power lines.
  - "**This is a Procedural heavy map. Mountains, waterfalls and many bridges are procedural objects.**"
  - Connections: Road 7, Air 4, Ship 1, Rail 3.
  - Visuals: LUT **Relight2Natural** (More Relight LUTs 1539181199); a custom theme mix; Render It! settings in the theme collection; Relight settings in a screenshot; **Hide It! with "hide shore effect"**, otherwise trees look dead; Daylight Classic "all to classic", which the author calls optional.
  - Thanks: ilcicci1983 for the mountain-making technique, Teddy Radko for his tutorial on bridges built from vanilla assets.
- **Render It! profile "Dakota Falls"** (theme collection text): sun intensity 4, sun shadow strength 0.71, vanilla bloom, tonemapping and LUT on (vanilla LUT None), ambient occlusion on, Render It! fog off, AA technique 2. **Relight** (theme-mix screenshot): brightness, gamma and contrast 1; temperature 0.05; sun temperature −0.25; sky temperature −0.15; moon temperature −0.75; moon light 0.25; sky tonemapping on. The local copy is `cs1-playset/theme/RenderIt-DakotaFalls.txt`.
- **Author's other CS1 items:** The Himalayas, Kamet Valley, Saint John, Rendsburg, Olympia, San Marino (×2), Puli – Nantou, Collab 81 [F3], theme mixes (MARITIME, Puli, EURO, Olympia), and props (Raft, Balloons, Industrial Garage, Marlboro Man, California Route Marker). None of these bundles Dakota Falls' dependencies beyond the two Dakota collections.

### Comments: what matters for the port

| Comment | Relevance |
|---|---|
| anjum_raihan (Nov 2025): "Custom made bridge near the dam has **one way monorail tracks**…"; the dam's power fluctuates and has **4 dam electricity cubes** | Confirms the monorail segments are a decorative bridge structure, not transit (finding 3). The 4 cubes match the dump (4 × `Dam Electricity_Data`). |
| Author to gameransh2: "used too many mass transit assets and also there is a **custom bridge that is made with mass transit assets**" | The Mass Transit DLC requirement comes from that bridge. |
| Teddy Radko: "I see monorail bridge" | Same bridge; Radko's technique uses monorail tracks as the arches. |
| HollowingWind: with assets missing, "train bridge … track isn't rendered", "**the huge dam is completely gone and the waterfall is also missing. The effects aren't there and the rocks**" | The dam, waterfalls, effects and many rocks exist only as Procedural Objects or Effects Module data (finding 1). |
| Author to mr.pixelcat: "that **big blue bridge is yours**" | The blue suspension bridge is mr.pixelcat's **Detroit Bridge [Ploppable]** (Ambassador Bridge), already MANUAL in `manual-objects.md`. |
| Author to Fancy: no trees with Tree Anarchy; vrazz35th: "rocks won't load" | Load requirements the playset already covers. |
| bakepotatou: "networks not listed … NetExt2?"; author: "never used Network Extensions 2 … collection is complete" | No NE2 dependency. |
| кiBe: a train bridge near the dam lets trains use one track; outside traffic prefers the 2-lane national road | Gameplay only. |
| Sepi: FPS drop at the small road tunnel near the dam's power lines | Very dense detailing there. |
| dylanscottstansbury: start tile "reminds me of Durango, CO"; Gaseous Stranger: "Rocky Mountains combined with the Mississippi River", town inspired by Boulder | Real-world look references. |

No comment names a water level, a water colour or a different LUT. One player (Infrastructurist) used the LUT **Leviante** (1848026059) with the author's theme mix. That was the player's choice, not the author's.

### Preview images: what they show

31 images; they carry captions on the image.

- **Look:** high summer. Dark-green conifer forest (Pines, EverGreen) covers the mountains and river valleys. Open plains are tan and dry, with scattered pines, bare-earth erosion gullies and surface rocks (Brown Surface Rocks). Light haze, soft daylight. The water is grey-blue and slightly hazy, with white foam at the falls. The terrain textures are a light sandy-tan cliff/ruined texture (Owl's Albedo / Australia Coast in the mix) and a green-yellow grass.
- **"PO MOUNTAINS" (img 9, also the title image) and "THE FLATIRONS OF COLORADO" (img 10):** the big flat-topped grey rock peaks and the tilted Flatiron slabs are **Procedural Objects**, not terrain. In the "OVERVIEW" image the dark rock masses in the NW and SW corners are PO mountains sitting on the heightmap.
- **Falls (img 2, 22):** a broad multi-step falls across the river with rock islands, plus a gorge waterfall next to a **rock shed** on the road (JP Rock shed, img 22). Foam is Animated Waterfall / Effects Module.
- **"DAM" (img 4):** a large concrete gated spillway dam across the river, with a substation (Extra Large Substation) and H-frame 345 kV lines. A white arch bridge carries the national road just downstream.
- **"VANILLA ASSET BRIDGE" (img 5):** the same white multi-arch highway bridge. In the dump the arches are the monorail segments (finding 3).
- **"PO BRIDGE" / "ANOTHER PO BRIDGE" (img 7, 8, 9):** a long deck viaduct over a red arch through a pine valley, and a green truss arch over a rocky gorge. Both are Procedural Objects.
- **Other bridges:** the blue suspension Detroit Bridge (img 18, 23–25), a grey through-arch (Hernando de Soto, img 14), a white multi-span through-arch rail/road bridge (img 27), and RAILWAY 2 girder bridges.
- **"FORT" (img 6, 27):** white castle ruins on a cliff at the river fork (Castle Ruins and Castle Wall networks).
- **"FLOODS CAN CAUSE A LOT OF DAMAGE" (img 11):** a red covered bridge swept off its abutment, with a wrecked bus and debris on a rocky ford. Neither the covered bridge nor the wrecked bus is in the building inventory, so they are almost certainly Procedural Objects.
- **Interchanges (img 13–20):** USRP interstates with flyovers, cloverleaf and partial-cloverleaf exits, and a diamond at a 4-lane national road. Lanes have **IMT chevrons/gore hatching** and USRP lights and signals.
- **Farmland (img 24–26, overview):** about 40 **centre-pivot irrigation circles** plus rectangular plots in 3 clusters on the east side, with corn rows (Corn Stalk) in the close-up.
- **"START AREA" (img 12):** the tile where the river forks; it holds the fort, the railway and the national road.
- **"RESOURCES" (img 30):** ore (white) on the western mountains; oil (black) round the large eastern lake and the SW lake; fertile land in the valley; forest everywhere else.
- **"SUNSET HARBOR" (img 31):** fishing areas along the whole river, the lakes and the southern delta.

## Findings, ordered by impact on the port

1. **Procedural Objects carry the map's signature features, and v0.3.0 exports their placement but not their shape.** The author, the screenshots and a player's "missing assets" report all point to PO for the mountains (the flat-topped peaks and the Flatirons, including the peaks above CS1's 1 024 m ceiling), the waterfalls, the big dam, at least two road bridges (deck viaduct, green truss arch) and probably the covered bridge and wrecked bus. `ProceduralObjectsExporter.cs` writes position, rotation, base prefab, `vertexCount` and `localBounds` only ("no vertices are exported"). Edited meshes (`meshStatus` 2) therefore come out as bounding boxes.
   - **Lost:** the actual mountain shapes, the falls rock steps and the dam outline. The bounding boxes give footprint and height, enough for a rough heightmap bump and nothing more.
   - **Significance:** very high. These are 4 of the 6 captioned showcase subjects, and the corner peaks dominate the overview.
   - Ten collection items appear nowhere in the v1 dump as buildings, props, trees or nets, so they can only live in PO data or as network sub-items: Hyperion's Gate - PO Elements Pack, Sidewalks & More Decals + PO Textures, VC Erosion Rock, VC Rock Outcrops, VC_Rocky_Streambed, Paintable Prop Modular Rocks, Mossy Grey Modular Rocks 2K, JP Rock shed, Tunnel Entrances and Animated Waterfall. Their counts will show in `proceduralObjects.byPrefab`.
2. **Effects Module [PO] data is not read, and it must be.** The foam at the falls and the dam spillway (the turquoise-white mist in img 2, 4, 22) comes from Effects Module attached to PO objects, with Animated Waterfall as the base asset. This is the "falls" of Dakota Falls, so its placement has to come out of the export even though CS2 rebuilds the look with its own water/effect tools.
   - **Where the data is** (decompiled `EffectsPOModule.dll` 2632235148 and `ProceduralObjects.dll` 3071492847):
     - Each PO object has `List<POModule> m_modules`; the exporter already counts it as `moduleCount`.
     - The effect is a module of class `EffectsPOModule.EffectsModule : ProceduralObjects.POModule`, with public fields `effect` (vanilla `ParticleEffect`; its `.name` is the effect id), `Width`, `Height`, `magnitude`, `acceleration`, `direction` (Vector3, local to the object), `offset` (Vector3, local) and `emitRate` (byte: 0 = every frame, 1 = every 2 frames, 2 = 0.1 s, 3 = 0.3 s, 4 = 0.6 s, n > 4 = n − 4 s).
     - The emitter sits in world space at `parentObject.m_position + parentObject.m_rotation * offset`, points along `m_rotation * direction`, and covers a `Width × Height` area.
   - **Generic read:** `POModule._get_data(bool forSaveGame)` is public and returns `Dictionary<string,string>`. For the Effects Module the keys are `effectName`, `width`, `height`, `magnitude`, `acceleration`, `direction`, `offset` and `emitRate`, i.e. exactly what PO saves. Calling it by reflection on every module, together with the module's type name, also covers any other PO module the map uses, with no Effects-specific code.
   - **Lost today:** every effect position, size, direction and kind.
3. **Monorail segments are the arches of the vanilla-asset bridge below the dam; the converter skips them.** The 192 segments of `Monorail Oneway Track` and `Monorail Track - No Pylons` lie at x 944…1345, z 4061…4290, y 139…159 m. Two comments and the "VANILLA ASSET BRIDGE" screenshot identify them as the Teddy Radko-style white multi-arch bridge that carries the national road over the river. The converter skips them ("CS2 has no monorail").
   - **Lost:** the arch structure. The road deck still imports, with CS2 default pillars.
   - **Significance:** medium. It is a captioned landmark next to the dam. This is not a missing dependency: the Mass Transit DLC is owned.
4. **The dam is not a building.** Besides the PO (finding 1), the 7 `Quay` segments at x 893…1434, z 4512…4681 lie at the dam (Dam Electricity at about 1550, 4690), so the dam's retaining walls are probably partly built from Quay. The converter maps Quay to QuayMedium01, which is fine. The hand-placed CS2 hydro dam in `manual-objects.md` remains the right replacement for the 4 electricity cubes. The author's single cube is really 4 stacked cubes, which explains the fluctuating power players report.
5. **Network Skins Continued data is not read.** Per-segment skins (removed or changed streetlights, trees, road colour, pillar style) live in Network Skins' own serialized data. The exporter reads only `NetSegment` and prefab fields.
   - **Lost:** whatever skins the author applied, e.g. hidden lights on the gravel roads or vanilla-asset bridge segments without pillars.
   - **Significance:** low for CS2, because CS2 rebuilds props per its own road prefab. It cannot be measured from the screenshots.
6. **Intersection Marking Tool markings are not read.** Interstate gores and lane splits show IMT chevron fills and hatching (img 14, 16, 19), and the collection includes USRP Intersection Marking Templates.
   - **Lost:** all custom markings.
   - **Significance:** low; vanilla CS2 draws its own markings and offers no marking-painting equivalent without mods.
7. **Node Controller Renewal, Adaptive Networks and TM:PE data are not read.** Respectively: node shapes, corner offsets and hidden crossings; AN flags (sidewalk, median and pillar toggles); lane arrows, speed limits, priority signs and junction restrictions.
   - **Lost:** smooth custom ramp joins and gameplay rules.
   - **Significance:** low. CS2 regenerates junction geometry, and the converter already handles near-parallel ramps. Exporting TM:PE rules is not worth the effort.
8. **Theme Mixer 2.5 mix and Hide It! settings are not read.** The exporter's `mapSettings.mapTheme` holds only the base `m_MapThemeMetaData`. The look comes from the mix "Dakota Falls - Theme mix" (2972881605): textures from Owl's Albedo, Coastal, Springwood, Seychelles, Australia Coast HD and Los Cabos, plus water and sky colours. The mix file is local (Workshop folder 2972881605), so it can be read offline; no exporter change is needed.
   - **Lost:** terrain texture choice and water colour.
   - **Significance:** medium for matching the look. CS2 picks textures and water by climate/theme. Use the screenshots (summer, tan dry plains, dark conifers, grey-blue water) as the reference.
9. **Centre-pivot fields need checking.** The overview shows about 40 circles in 3 clusters east of the river. The dump has 269 Field Network segments, which the converter turns into 67 polygons covering 3.05 km². Check that the polygons are circles and not merged blobs (compare `import-surfaces-fields.json` with img 26 and 29). If any circles are PO decals ("Sidewalks & More Decals + PO Textures"), they will appear in `-po.json`, not in the field networks.
   - **Significance:** medium; the circles are the most recognisable feature in the overview.
10. **Natural resources and fishing.** The v1 dump has no `naturalResources`; v2/v3 add them. The "RESOURCES" image is a checklist for the next export: ore on the W mountains, oil round the E lake and the SW lake, fertility in the valleys. Stats currently use a SYNTHETIC fertility layer. Fish are recomputed by CS2 (setup-plan, bridge 0.9.7), so the Sunset Harbor view needs nothing.
11. **Castle Wall (12 segments, kept skipped) is half of the "FORT" landmark.** It shows in the start-area and fort images. CS2 has no twin, so the converter's decision stands; the fort needs hand work around RuinsCastle01.
12. **Prop Painter: Revisited colours are not read.** The rocks are tinted (Paintable Prop Modular Rocks, "rock colours" in AUDIT). The exporter writes props without colour; PO colour is exported.
    - **Significance:** low. The CS2 replacement rocks have their own colours.
13. **Connection counts differ slightly from the description.** The description says Road 7, Air 4, Ship 1, Rail 3. The dump has road 13 buildings (8 in / 8 out), train 4, plane 4, ship 1. Road in/out pairs and the extra rail connection are probably fine (`connections-check.md` covers them), but rail 4 vs 3 deserves one look.
14. **Dependencies: nothing required is unaccounted for.**
    - The map page declares only Mass Transit (owned). All 65 collection items are in `AUDIT.md`. Four were replaced by successors (Procedural Objects → 3071492847, Adaptive Networks → 3683231517, Network Skins → 3702700108, Prop Painter → 2685506044).
    - **Hidden dependencies** (used in the dump or LSM report but not in the collection), already added by AUDIT from LSM: USRP 1-Lane Ramp, USRP 2/3/4-Lane Highway, USRP Interstate Extensions, USRP Concrete Interstates Redux, RAILWAY 2 station tracks, girder, truss and yard packs, Urban Overpasses, Inspection/Pit Tracks, Railway Decorative Networks and Railway Replacer.
    - Every net and building prefab id in the v1 dump belongs to a collection item or to that LSM set; no other hidden dependency exists.
    - The theme collection's Relight, More Relight LUTs and Render It! are not installed. They affect visuals only, and the LUT name is recorded above.
15. **Stale note in the exporter README.** `cs1-exporter/README.md` "Notes and limits" still says Procedural Objects "are not exported", which contradicts v3. It is documentation only and left untouched here.

## Before the export

- **PO mesh geometry (exporter):** for every PO object with `meshStatus` 2 (or for all PO objects), add at least one of these:
  - a decimated vertex dump, or
  - a world-space height raster, i.e. the max PO-mesh y per heightmap cell on the 1081 grid, written like `-water.png`.

  Either one makes the PO mountains, Flatirons, falls steps and dam stampable into the CS2 heightmap. Without them, finding 1 can only be rebuilt from bounding boxes and screenshots. This is the one change with real impact.
- **Effects Module (exporter, required):** in `ProceduralObjectsExporter.cs`, for each PO object write a `modules[]` array next to `moduleCount`. Each entry holds:
  - `type`: the module's runtime type full name;
  - `data`: the dictionary from `_get_data(true)`, called by reflection;
  - for `EffectsPOModule.EffectsModule` additionally `worldPosition` = `m_position + m_rotation * offset` and `worldDirection` = `m_rotation * direction`.

  Add `stats` counts per `effectName`. This keeps the falls and spillway foam locations and kinds. Effects Module [PO] (2632235148) must be enabled during the export: without it the module type is not registered, so `m_modules` will not contain the effects. Whether PO keeps the raw data in that case was not checked.
- **Monorail (converter, after export):** do not discard the 192 monorail segments. Write them to a "decor" layer, or at least into `manual-objects.md`, as the outline of the arch bridge near (1150, 4170).
- **CS1 playset:** keep Procedural Objects (3071492847) and Effects Module [PO] enabled. Without them `-po.json` is not written and the dam and falls are absent, exactly as in HollowingWind's report. Tree Control must be on and Tree Anarchy off, or the tree export is empty. Nothing else is missing for the export. More Relight LUTs and Render It! are needed only if reference screenshots in the author's look are wanted.
- **Offline, no export needed:** read the Theme Mixer mix from the local Workshop folder 2972881605 to record the texture and water-colour sources for choosing the CS2 theme.
