# Dakota Falls look: gap mods beyond Lumina

This file covers the CS1 settings that [LUMINA-MAPPING.md](LUMINA-MAPPING.md) lists as gaps. It gives the CS2 mods that cover them and the settings that reproduce the CS1 Render It!/Relight/Theme Mixer numbers in this folder.

**Decision (2026-10-19):** only **Lumina**, **Enhanced Graphics and Textures (EGT)** and **Time & Weather Anarchy (T&W)** are used. Every setting I had rated low confidence is dropped, and so are SunGlasses (76849) and BetterMoonLight (77923). The CS1 gaps they would have covered stay open: sun disk size, shadow fill, sky haze and colour, and moon and night lighting.

The requirements and settings below go into the published map description. The draft is in [../../MAP-DESCRIPTION.md](../../MAP-DESCRIPTION.md).

**Nothing has been tested in game.**

**Confidence:**
- **high**: the source shows the CS1 value maps directly, or the setting should stay at its default.
- **med**: the reasoning is sound, but the CS1 and CS2 units don't convert exactly.

## Sources

Settings, ranges and defaults come from the installed mod DLLs, decompiled with `ilspycmd`. The DLLs are in `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\.cache\Mods\pdx_mods\`:
- EGT: `161533_11`
- T&W: `88893_17`
- Lumina: `94394_51`

The Render It! source is [keallu/CSL-RenderIt](https://github.com/keallu/CSL-RenderIt).

Correction to the earlier notes: Render It's profile stores some values as **list positions, not values**:
- `…MipMapBias 2` means a bias of **−0.3**, and `5` means 0.0.
- `…AnisoLevel 3` means 4×, and `4` means 5×.
- `shadowResolution 2` means built-in "High", about a 2048 px map.

## 1. Lumina (94394, v3.7)

Unchanged from LUMINA-MAPPING: preset `lumina/Dakota Falls.xml`. Its key values:

| Field | Value |
|---|---|
| Location | `LatLongEnabled` true, latitude −26, longitude 39 |
| Ambient occlusion | intensity 2.55, radius 1.85, step count 10 |
| White balance | Temperature −5 |
| Tonemapping | External, LUT `Relight2Natural`, contribution 1 |
| Must stay off | `CustomSunEnabled`, `HDRISkyEnabled`, `ViewTimeOfDaySlider` |

Lumina reports T&W as incompatible when its time-of-day slider is on. Lumina's lat/long patch overrides T&W's (see §3).

## 2. Enhanced Graphics and Textures (161533, v1.4.0)

| | |
|---|---|
| Options UI | Options → **Enhanced Graphics & Textures** → tab **General**. The same settings are in the in-game Graphics window (graphics app button). |
| Settings file | `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Advanced_Graphics.coc`. It sits in the LocalLow root, not in `ModsSettings`. |
| Covers | mip bias (only on the surfaces EGT edits); TAA sharpen left at the game value |

0 means "no override" where noted.

| Setting (property) | Range / default | **Value** | Reason | Conf. |
|---|---|---|---|---|
| TAA sharpness (`TaaSharpenStrength`) | 0 or 0.05–2 / 0 | **0** (no override) | CS1's 0.3 is PPv2's default sharpening, so the matching choice is to keep the game's own value. | med |
| Adjust TAA stability | off / off | **off** | Render It's TAA blending values are the PPv2 defaults. | med |
| Texture mip bias (`SurfaceTextureMipBias`) | −1…1 / 0 (negative = sharper) | **−0.3** | Render It sets "general" textures to −0.3. EGT applies the bias only to the road, sidewalk, gravel, rail and grass surfaces you override in its tabs, and forces 16× anisotropic filtering on those. | med |
| Shadow resolution | 0 or 512–8192 / 0 | **0** | CS2's own High (4096) is already sharper than CS1's High (about 2048). | high |
| Shadow distance / Small shadow casters | 0 / 0 | **0 / 0** | Render It sets neither. | high |
| Tree LOD distance / Global object detail | 100 / 100 % | **100 / 100** | Render It's LOD flags are all off. | high |
| Geometry cache / Terrain tessellation / Patch size | 4096 / 5 / 64 | **defaults** | – | high |
| Bloom / Bloom intensity / Sun Bloom / Sky Exposure | GameDefault / 0.2 / 1 / 0 | **defaults** | CS1 used vanilla bloom and sky exposure. At these values EGT adds no override. | high |

## 3. Time & Weather Anarchy (88893, v3.2)

| | |
|---|---|
| Options UI | Options → Time & Weather Anarchy has only the hotkeys and the Day/Night hours. Everything else is in the **in-game T&W panel** (toolbar icon), saved as a profile. |
| Settings files | `ModsSettings\TimeWeatherAnarchy.coc` (selected profile) and `ModsSettings\TimeWeatherAnarchy\Profiles\<guid>.json` |
| Editor | Does nothing in the editor (verified in the decompiled `TimeAndWeatherControlSystem`). |

Create a profile "Dakota Falls" and link it to the save:

| Field | **Value** | Reason | Conf. |
|---|---|---|---|
| Latitude / Longitude | **−26 / 39** | Matches the theme mix. T&W writes these to `PlanetarySystem` on every update. Lumina's `OnUpdate` transpiler reads its own values instead, so this only matters if Lumina's lat/long is off. | high |
| Time option | **Default** | CS1 ran a normal day/night cycle. | high |
| Weather / season | **Default** | Every weather field in the theme mix is unset. | med |
| Custom fog | **off** | Render It fog was off, so CS1's vanilla fog applied. The CS2 counterpart is vanilla fog. | high |
| Custom clouds / precipitation / temperature / thunder | **off** | nothing set in CS1 | high |
| Custom aurora | **on, Aurora 0** | CS1 Dakota had no aurora, and an aurora at −26° is wrong. | med |
| Profile active time | **Always** | – | high |

## Apply order

1. Import the Lumina preset; it overwrites `Lumina.xml`. Keep Custom Sun, HDRI and the time-of-day slider off.
2. Set the EGT values in Options.
3. With the Dakota Falls save loaded, create the T&W profile and link it to the save.
4. Game options: Shadow quality High, Anti-aliasing TAA, Texture quality and anisotropic filtering at maximum.

## Conflicts

| Pair | Overlap | Resolution |
|---|---|---|
| T&W ↔ Lumina | Lat/long (Lumina wins); Lumina's time-of-day slider | Same lat/long in both; Lumina's slider off. |
| EGT ↔ Lumina | Road materials (EGT surface editor vs Lumina road textures and colours) | The preset has `UseRoadTextures` off. Don't use both on roads. |
| EGT ↔ LOD Control / Auto Night LOD Extender / RenderMastery | Global LOD | Don't use those. |
| T&W ↔ Nice Weather | Weather overrides | Don't use Nice Weather. |

## Recommendation for the editor playset DakotaFalls-E2C

**Add none of the three; they are for playing only.**
- T&W does nothing in the editor.
- EGT always writes terrain tessellation and patch size and adjusts LOD and shadows, which adds risk to editing for no benefit.
- Lumina isn't in E2C either.

All three go in the playset used to play the finished map, and in the published map's requirements.
