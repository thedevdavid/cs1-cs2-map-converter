# Dakota Falls (CS2): published map description draft

This is the text for the published map's description: Map Settings → Description in the editor, and the long description on Paradox Mods.

**Rule:** the "Recommended mods and settings" section must be in the published description. Keep it in step with [cs1-playset/theme/GAP-MODS.md](cs1-playset/theme/GAP-MODS.md) and [cs1-playset/theme/LUMINA-MAPPING.md](cs1-playset/theme/LUMINA-MAPPING.md).

Still to write: the map intro, credits for the original CS1 map, map facts (size, resources, outside connections) and the changelog.

## Before publishing

- **Check the settings in game.** Nothing in the section below has been tested yet.
- **Resolve the LUT.** `Relight2Natural.cube` is converted from More Relight LUTs (CS1 Workshop 1539181199, which no longer shows publicly). Players need the file in `ModsData\Lumina\LUTS` for the External tonemapping step. Before shipping the `.cube` or the Lumina preset, get permission from that LUT author, or tell players to use ACES tonemapping instead. The text below offers ACES as the fallback.
- **Decide how players get the Lumina preset.** Either they enter the values by hand, as below, or you share `Dakota Falls.xml` somewhere.

---

## Text to paste

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
