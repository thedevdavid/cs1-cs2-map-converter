# Dakota Falls look → CS2 Lumina

Preset: [lumina/Dakota Falls.xml](lumina/Dakota%20Falls.xml). A copy is already in Lumina's preset folder:
`%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\Lumina\LuminaPresets\Dakota Falls.xml`.
Backup of your Lumina.xml as it was on 2026-10-19: [lumina/Lumina.xml.backup-2026-10-19](lumina/Lumina.xml.backup-2026-10-19).
The preset now uses the author's **Relight2Natural** LUT in **External** tonemapping mode (`LUTName = Relight2Natural`, `TonemappingMode = External`, LUT Contribution 1). The converted LUTs are in [lumina/luts/](lumina/luts/) and are installed in Lumina's `LUTS` folder. See [Relight LUTs](#relight-luts-installed) and [Apply](#apply).

Target: Lumina **v3.7** (pdx_mods/94394, installed), game 1.6.2f1. I decompiled the installed Lumina.dll to confirm the field semantics and file paths. None of this has been tested in game yet.

## Bottom line

Most of the CS1 stack (Relight, Relight2Natural LUT, Render It sky wavelengths) existed to remove CS1's yellow, flat vanilla lighting. CS2's HDRP lighting doesn't have that problem, so most of those settings have no counterpart. What carries over:

1. **Southern-hemisphere sun**: latitude −26, longitude 39.
2. **Strong, tight AO**: intensity 2.55, radius 1.85.
3. **Slightly cool white balance**: Temperature −5.
4. **Relight2Natural LUT on an ACES base**: the author's theme LUT, converted to `.cube` with ACES baked in and applied in Lumina's External mode. ACES is also the tonemapper set in the author's Render It profile.

Everything else is either neutral or listed under gaps below.

## How Lumina presets work (from the decompiled source)

- A preset is a full `GlobalVariables` XML file in `ModsData\Lumina\LuminaPresets\<name>.xml`.
- **Import** copies the preset over `ModsData\Lumina\Lumina.xml`, so every field is replaced. That includes app settings such as SaveAutomatically. The preset keeps your current app settings.
- The colour grading goes into Lumina's HDRP volume, which holds Color Adjustments, White Balance, Shadows/Midtones/Highlights, Tonemapping, SSAO, Contact Shadows, SSGI and SSR. Each `*Active=false` field leaves the game's default value alone.
- Ranges: PostExposure −5..3 EV. Contrast, Saturation, Temperature and Tint are −100..100. HueShift is −180..180. Lat −90..90, long −180..180. HDRP SSAO intensity is 0..4 and radius is 0.25..5 m.
- Lat/long only work when `LatLongEnabled=true`. The time-of-day patch is applied at startup or when that mod option is toggled.
- LUTs are `.cube` files in `ModsData\Lumina\LUTS`. They only apply when `TonemappingMode=External`, where the LUT replaces the tonemapper.

## Mapping: Relight and theme mix (Render It is in the next section)

| CS1 source | setting | CS1 value | Lumina field | preset value | note |
|---|---|---|---|---|---|
| Relight | Brightness | 1 | PostExposure | 0, inactive | neutral |
| Relight | Gamma | 1 | TonemappingGamma | inactive | neutral (that field only affects the Custom tonemapper) |
| Relight | Contrast | 1 | Contrast | 0, inactive | neutral |
| Relight | Temperature (global) | +0.05 | Temperature | **−5** combined | Relight's ±1 range ≈ Lumina's ±100, so +0.05 → +5. That is summed with the cooler sun and sky (next row). |
| Relight | Sun Temp / Sky Temp | −0.25 / −0.15 | (none per light) | folded into Temperature | Lumina has no per-light or sky colour temperature. In CS1 these cancelled vanilla's yellow sun, which CS2 doesn't have, so I folded only about −10 into the global value. Tune between −10 and +5. |
| Relight | Tint / Sun / Sky / Twilight Tint | 0 | Tint | 0, inactive | neutral |
| Relight | Moon Temp −0.75, Moon Light 0.25 | | (none) | | **gap**: there are no night-only settings |
| Relight | Sky Tonemapping, Shadow Smoothing, Low Shadow Bias checkboxes | unreadable | (none) | | **gap** (and the values are unknown anyway) |
| Theme mix | LUT | Relight2Natural | LUTName / TonemappingMode | **Relight2Natural, External** | ✅ converted from More Relight LUTs (1539181199) with ACES baked in (see [Relight LUTs](#relight-luts-installed)). Brighter and more contrasty in the midtones than plain ACES. Not checked in game. |
| Theme mix | latitude / longitude | −26 / 39 | Latitude / Longitude + LatLongEnabled | −26 / 39, true | the sun arcs through the northern sky, as in the CS1 map |
| Theme mix | sun size 0.82, anisotropy 0.651 | | AngularDiameter / SunFlareSize | not used | **gap**: only available through Custom Sun (see the sun intensity row) |
| Theme mix | Rayleigh 1.013, Mie 2.16 (hazy sun halo) | | (none) | | **gap**: Lumina doesn't expose HDRP Physically Based Sky. HDRI Sky can replace the sky, but it's static. |
| Theme mix | stars 2.2, outer space 1.26 | | spaceEmissionMultiplier | 100 (unchanged) | approximate; this field scales space emission at night |
| Theme mix | night zenith/horizon colours, moon texture/size/corona | | (none) | | **gap** |
| Theme mix | terrain, cliff, sand, pavement textures and grass colour offsets | Seychelles / Springwood / Owl's Albedo / Los Cabos | (not Lumina) | | **gap for Lumina**: CS2 handles this through the map theme or MapTextureReplacer (installed) |
| Theme mix | water textures (CO-Temperate) | | (not Lumina) | | **gap**: CS2 water is a simulation and can't be retextured |
| Theme mix | road and building textures | Seychelles | UseRoadTextures / road colours | off | Lumina's road wear option is a different feature |

## Render It! profile: every parameter

Source: [RenderIt-DakotaFalls.txt](RenderIt-DakotaFalls.txt), profile "Dakota Falls", active. ✅ = in the preset. ≈ = approximated, or handled by a CS2 game option. ❌ = gap.

### Sun, moon and shadows

| parameter | value | Lumina / CS2 | status |
|---|---|---|---|
| shadowType | 1 (soft) | CS2 shadows are always soft (HDRP PCSS/PCF) | ≈ |
| shadowResolution / shadowProjection | 2 (high) / 1 (stable fit) | CS2 Options → Graphics → Shadow quality = High | ≈ game option |
| sunIntensity | 4 | Custom Sun `SunIntensity` (0–100) exists, but it pins the sun's intensity every frame, which breaks dusk and night | ❌ left off |
| sunShadowStrength | 0.71 (shadows 29 % lifted) | no shadow-strength control in Lumina or HDRP | ❌. Nearest workaround: Shadows (SMH) lift or Contrast about −10. Not applied. |
| sunShadowBiasEnabled / sunShadowBias | false / 0.01 | disabled in CS1 anyway; HDRP bias isn't exposed | ❌ (no effect) |
| moonIntensity | 4 | no moon or night light control | ❌ |
| moonShadowStrength | 0.8 | none | ❌ |
| moonShadowBiasEnabled / moonShadowBias | false / 0.0415 | disabled in CS1 anyway | ❌ (no effect) |
| lightColors / skyColors / equatorColors / groundColors | all disabled, empty | nothing to port | ✅ (no-op) |

### Sky

| parameter | value | Lumina / CS2 | status |
|---|---|---|---|
| skyRayleighScattering / skyMieScattering / skyExposure | `.nan` (falls back to the theme: Rayleigh 1.013, Mie 2.16) | Lumina doesn't expose HDRP Physically Based Sky scattering | ❌ |
| skyRed/Green/BlueWaveLength | 680 / 550 / 440 | none; CS2's PBR sky uses fixed Rayleigh coefficients. These are CS1's default wavelengths, so in practice nothing is lost. | ❌ (low impact) |

### Fog

| parameter | value | Lumina / CS2 | status |
|---|---|---|---|
| fogEnabled | false (Render It fog off; vanilla CS1 fog applies) | no fog control in Lumina; CS2 keeps its own HDRP fog | ❌ (CS2 default) |
| fogDayNightEnabled | true | — | ❌ |
| fogHeight … fogScattering (10 values) | all `.nan` | unset, nothing to port | ✅ (no-op) |

### Antialiasing

| parameter | value | Lumina / CS2 | status |
|---|---|---|---|
| antialiasingTechnique | 2 (TAA) | CS2 Options → Graphics → Anti-aliasing = TAA (or DLSS/FSR) | ≈ game option |
| tAAJitterSpread / StationaryBlending / MotionBlending / Sharpen | 0.75 / 0.95 / 0.85 / 0.3 | HDRP TAA quality and sharpening aren't exposed by Lumina | ❌ (CS2 TAA defaults) |
| fXAAQuality | 2 | unused because TAA is active | ✅ (no-op) |

### Vanilla post-processing toggles

| parameter | value | Lumina / CS2 | status |
|---|---|---|---|
| vanillaTonemappingEnabled | true | `TonemappingMode = External`, with ACES baked into the Relight2Natural `.cube`. The game's filmic curve is closest to HDRP ACES. | ✅ |
| vanillaBloomEnabled | true | CS2's HDRP bloom is on by default; Lumina has no bloom control | ≈ (CS2 default) |
| vanillaColorCorrectionLutEnabled / LutName | true / None (the theme LUT, Relight2Natural, applies) | `LUTName = Relight2Natural`. See the LUT row in the mapping table above. | ✅ (converted) |

### Ambient occlusion

| parameter | value | Lumina / CS2 | status |
|---|---|---|---|
| ambientOcclusionEnabled | true | `IsScreenSpaceAmbientOcclusion = true` | ✅ |
| aOIntensity | 2.55 | `AmbientOcclusionIntensity = 2.55` (HDRP 0–4, same scale) | ✅ |
| aORadius | 1.85 | `AmbientOcclusionRadius = 1.85` (metres) | ✅ |
| aOSampleCount | 10 (Medium) | `AmbientOcclusionStepCount = 10`, `MaxRadiusInPixels = 40` | ✅ ≈ |
| aODownsampling | true | `AmbientOcclusionFullResolution = false`, `BilateralUpsample = true` | ✅ |
| aOAmbientOnly | false (AO darkens direct light too) | `AmbientOcclusionDirectLightingStrength = 0`. In HDRP, AO on direct light is a separate 0–1 setting; 0 keeps it ambient-only. Raise it to about 0.25 for the CS1 behaviour. | ≈ (kept at 0) |
| aOHighPrecision / aOForceForwardCompatibility | false / false | not relevant to HDRP | ✅ (no-op) |
| (none in CS1) | — | `TemporalAccumulation = true`, `GhostingReduction = 0.5`, to suit TAA | added |

### Post-processing bloom (Render It's own, off)

| parameter | value | status |
|---|---|---|
| bloomEnabled | false | ✅ (no-op): the author used vanilla bloom |
| bloomIntensity / Threshold / SoftKnee / Radius / AntiFlicker | 0.5 / 1.1 / 0.5 / 4 / false | inactive in CS1 and no Lumina field, so they are ignored |

### Colour grading block (off)

`colorGradingEnabled: false`, so none of these affected the CS1 image. The equivalent Lumina fields are listed for completeness, and they are inactive in the preset.

| parameter | value (neutral) | Lumina field | status |
|---|---|---|---|
| cGPostExposure | 0 | PostExposure | ✅ inactive |
| cGTemperature / cGTint | 0 / 0 | Temperature / Tint | ✅. Temperature is used for Relight instead (−5). |
| cGHueShift | 0 | HueShift | ✅ inactive |
| cGSaturation / cGContrast | 1 / 1 (PPv1 multipliers, 1 = neutral) | Saturation / Contrast (0 = neutral) | ✅ inactive |
| cGTonemapper | 2 (ACES) | TonemappingMode | ✅ ACES, baked into the External LUT (and it matches the vanilla tonemapper choice) |
| cGNeutralBlackIn / WhiteIn / BlackOut / WhiteOut / WhiteLevel / WhiteClip | 0.018 / 10 / 0 / 10 / 5.3 / 10 | Custom tonemapper toe/shoulder/gamma would come closest | ❌ (unused: the Neutral curve only applies when cGTonemapper = 1) |
| cGChannelMixer (3×3) | identity | none; Lumina doesn't expose HDRP ChannelMixer | ❌ (identity, so no visible loss) |

### Texture filtering

| parameter | value | status |
|---|---|---|
| general / buildings / networks / props / citizens / vehicles / trees: AnisoLevel | 4 / 3 / 4 / 3 / 3 / 3 / 3 | ❌ no Lumina equivalent; CS2 Options → Graphics → Texture quality / anisotropic filtering is global |
| …MipMapBias | 2 / 5 / 5 / 5 / 5 / 5 / 5 (sharper distant textures) | ❌ no equivalent; CS2 has no per-category mip bias |
| …LODIncluded | all false | ❌ (no effect) |

**Render It summary:**

- **In the preset:** AO (all six settings), tonemapping (ACES inside the Relight2Natural LUT), and the neutral colour-grading values.
- **CS2 game options:** shadow quality and TAA.
- **Real gaps that change the look:**
  - sun intensity 4;
  - sun shadow strength 0.71 (softer shadows);
  - moon intensity and moon shadow strength;
  - Mie/Rayleigh haze;
  - TAA sharpen 0.3;
  - mip bias (sharper distant textures).

## Apply

1. **Back up your current Lumina settings.** Importing overwrites `Lumina.xml`. A copy from today is in `lumina/`. You can also open Lumina's presets panel, type a name and click Export.
2. Make sure `Dakota Falls.xml` is in `...\ModsData\Lumina\LuminaPresets\`. It already is; if not, copy it from `lumina/`. The previous version of that copy (ACES, no LUT) is next to it as `Dakota Falls.xml.backup-20261019-055033`.
3. Make sure the five `Relight2*.cube` files are in `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\Lumina\LUTS\`. They already are; if not, copy them from `lumina/luts/`. Add files before starting the game, so that Lumina's LUT list includes them.
4. Start the game and load the Dakota Falls save. Open Lumina from the toolbar, go to Presets, select **Dakota Falls** and click Import.
5. In Lumina's tonemapping section, check that the mode is **External**, the LUT is **Relight2Natural** and LUT Contribution is 1. (`LUTName` is the file name without `.cube`.) To try another Relight LUT, keep External mode and pick it from the LUT list.
6. **What to check in game:**
   - **The LUT is applied.** Switching the mode to ACES and back should change the image. If nothing changes, make sure HDR display output is off (Lumina ignores the LUT in HDR) and that the `.cube` files are in `LUTS`.
   - **Midday brightness.** The converted Relight LUTs are brighter in the midtones than plain ACES, because in CS1 Relight darkened the scene before the LUT. If the image looks washed out, try one of these:
     - lower LUT Contribution to about 0.6–0.8;
     - set PostExposure to about −0.5 (with PostExposureActive on);
     - reconvert with `--exposure -0.5` to `-1`.
   - **Highlights and colour.** The sky and highlights shouldn't clip to flat white, and greens and water should look like Natural: neutral to slightly cool.
   - **Dusk and night.** These LUTs were made for CS1 with Relight, and users of the pack reported that nights are too dark. Compare against ACES at night.
   - **A/B against plain ACES.** `lumina/test/Identity_ACES.cube` is plain ACES as an External LUT. Copy it into `LUTS` to compare.
7. Open Options → Lumina and check that **Latitude and Longitude Adjustments** is on. The import sets this flag, but the sun patch is only applied at startup or when the option is toggled, so toggle it off and on, or restart the game once.
8. In CS2 graphics options (the Render It parts that are game settings), set Shadow quality to High, Anti-aliasing to TAA (or DLSS/FSR), and Texture quality / anisotropic filtering to the maximum.
9. Tune in Lumina: Temperature between −10 and +5 is the main setting to adjust. If shadows look heavier than in CS1, lower Contrast to about −10.

To undo: copy `lumina/Lumina.xml.backup-2026-10-19` back over `ModsData\Lumina\Lumina.xml`. To keep the preset but drop the LUT, set Tonemapping to ACES in Lumina.

## Open options (not done; ask before installing anything)

- **Sky haze, Mie and night colours** would need a mod that edits HDRP Physically Based Sky. Lumina doesn't do this.

## Converting CS1 LUTs (Relight2Natural) for Lumina

**Yes, they can be converted.** I wrote a converter and tested it on CS1 LUTs that are already on disk.

**What Lumina accepts** (from the decompiled `CubeLutLoader` / `UISystem`):
- **Format:** only `.cube` text files: `LUT_3D_SIZE n`, then n³ lines of `r g b`, red varying fastest. Any size is accepted and resampled to 32³. PNG strips are not accepted.
- **Location:** `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\Lumina\LUTS\`. Lumina lists every `*.cube` file in that folder.
- **How it's applied:** only in **Tonemapping = External** mode, where the LUT replaces the tonemapper. HDRP samples it with Unity **LogC** input and uses the result as **linear** colour, blended by LUT Contribution (HDRP `LutBuilder3D.compute`). It is ignored when HDR display output is on.

**Why a CS1 LUT needs remapping:** a CS1 LUT is a PNG strip, usually 1024×32 (32³; More Relight LUTs use 4096×64, 64³), applied in sRGB after CS1's own tonemapper. Copying the values 1:1 into a `.cube` gives a washed-out, wrong image in External mode. For each grid point, the converter therefore bakes in: LogC → linear → ACES (Hill fit) → sRGB → CS1 LUT → linear.

**Converter:** [lumina/cs1lut2cube.mjs](lumina/cs1lut2cube.mjs). It needs only Node, with no dependencies. It reads a `.png` strip, or a `.crp` asset (it extracts the PNG embedded in it).

```bash
node cs1lut2cube.mjs "<path>\Relight2Natural.crp" Relight2Natural.cube
```

- `--exposure <EV>` shifts the image before the ACES curve. CS1's tonemapper isn't ACES, so many CS1 LUTs come out brighter; try −0.5 to −1.
- `--mode raw` writes the plain sRGB→sRGB `.cube` for other tools. Don't use it for Lumina.
- `--identity` writes `Identity_ACES.cube`, which is plain ACES as an External LUT (a reference to compare against).

**Tests** (in `lumina/test/`): I converted three CS1 workshop LUT `.crp` files already on disk: Zirrah LUT (2859677843), Definite Vision LUT V1.3 (721093032) and Photorealistic LUT by Reaper (1199958905). None of the CS1 base-game LUTs are loose files.
- Each output has 32768 entries, the format Lumina's parser checks.
- The strip orientation was checked against the files: PNG row 0 = green 0.
- The grey ramps are monotonic, and each LUT's tint is preserved.
- **Not checked in game:** whether the result matches how the LUT looked in CS1.

### Relight LUTs (installed)

**Source:** More Relight LUTs by Ronyx69 (Workshop 1539181199, subscribed in CS1). There is one `.crp` per LUT in `D:\SteamLibrary\steamapps\workshop\content\255710\1539181199\`.

**Converter change:** these files hold a **4096×64 strip (64³)** rather than CS1's usual 32³. The converter originally rejected them, so it now reads 16³, 32³ and 64³ strips and always writes a 32³ `.cube` (Lumina resamples to 32³ anyway). The `--identity` output is byte-identical to before.

**Conversion:** default settings (`--mode lumina`, exposure 0), for example:

```bash
node cs1lut2cube.mjs "D:\SteamLibrary\steamapps\workshop\content\255710\1539181199\Relight2Natural.crp" luts/Relight2Natural.cube
```

**Copies:** the files in [lumina/luts/](lumina/luts/) and in `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\Lumina\LUTS\` are identical. In the table, "mid grey" is the R/G/B output at the middle LogC grid point (16 of 0–31); plain ACES gives 0.443 there.

| LUT | source `.crp` | `.cube` (32³) | mid grey, linear (ACES 0.443) | character |
|---|---|---|---|---|
| **Relight2Natural** | 258,669 B | 884,994 B | 0.773 / 0.780 / 0.768 | **Used by the Dakota Falls map author; set in the preset.** Neutral, very slightly cool. |
| Relight2Alpine | 257,797 B | 884,992 B | 0.649 / 0.647 / 0.600 | darkest of the five, warm (blue pulled down) |
| Relight2Average | 278,793 B | 884,994 B | 0.735 / 0.735 / 0.692 | warm |
| Relight2Lush | 253,958 B | 884,988 B | 0.853 / 0.830 / 0.782 | brightest, warm |
| Relight2North | 257,392 B | 884,990 B | 0.691 / 0.712 / 0.642 | greenish, with blue pulled down the most |

**Which one is recommended:** the pack author doesn't recommend any single LUT. The Workshop page only says they were made for Relight's default and recommended settings. Dakota Falls' author used **Relight2Natural**, so the preset uses it. Workshop commenters favour Average and Alpine, but that is user opinion. The Workshop item now shows as removed for the public, but the subscribed files are still on disk.

**Checks:**
- Each file has 32,768 entries.
- In raw mode, each primary lands on its own channel, so the strip layout matches the 32³ LUTs.
- Black maps to 0 and white to 1.
- The grey ramps are monotonic.
- **Not checked in game.**

## CS2 mods that close the gaps (researched 2026-10-19)

**Decision (2026-10-19): use only Lumina, Enhanced Graphics and Textures (161533) and Time & Weather Anarchy (88893).** SunGlasses and BetterMoonLight are not used. The settings for the chosen mods are in [GAP-MODS.md](GAP-MODS.md); the text for the published map description is in [MAP-DESCRIPTION.md](../../MAP-DESCRIPTION.md). The table below is the research record.

**Installed locally** (checked `.cache\Mods\pdx_mods` and `ModsData`):
- Lumina 3.7 (94394);
- **Time & Weather Anarchy** (88893);
- Filmstock LUT Pack (108745);
- MapTextureReplacer (76050).

SunGlasses (76849), BetterMoonLight (77923) and Enhanced Graphics and Textures (161533) were installed on 2026-10-19 so they could be inspected. Only EGT is used.

Versions and dates come from the Paradox Mods API. "Conflict" means what the mod page says; anything marked "unverified" is my inference. Mods that write the same HDRP volume as Lumina compete with it, and whichever writes last in a frame wins.

| gap | mod (Paradox id) | what it exposes | updated / game version | conflict with Lumina |
|---|---|---|---|---|
| sun intensity, CS1-style shadow strength | SunGlasses (76849), **not used** | sun disk size, sun bloom, lens flare, sky exposure, daytime ambient light, ground fill light (the author calls this the CS1 shadow-strength equivalent), exposure curve, colour correction for sun/sky/fog/clouds. Daytime only. | 2026-10-18, v2.5, 1.* | **Partial, documented.** Both set sun disk and flare, so keep Lumina's Custom Sun off (it is off in the preset). Lumina's HDRI sky would override SunGlasses' sky correction (not used here). The author hasn't tested them together in game. |
| moon intensity, night colours | BetterMoonLight (77923), **not used** | ambient (0–15), night sky light, moon light intensity, disk size, night/moon colour temperature 3500–10000 K, stars, aurora | 2026-05-01, v2.0.2, 1.* | none documented. May clash with T&W Anarchy's aurora. |
| shadow distance / resolution | **Enhanced Graphics and Textures** (161533) | shadow resolution and distance, small-caster threshold | 2026-10-17, v1.4.0, 1.6.* | none stated |
| shadow bias, cascades | RenderMastery (77104, by the Lumina author) | shadow distance, cascades, near-plane offset (≈ bias), AA mode, mipmap limit | 2025-10-29, built for 1.3.5. **Probably stale on 1.6.** | same author as Lumina |
| Mie/Rayleigh haze, sky tint | **no dedicated mod found** | partial: SunGlasses sky exposure and colour; vanilla Photo Mode "Atmosphere" settings reset outside Photo Mode | — | — |
| fog | **Time & Weather Anarchy** (88893, installed) | custom fog %, clouds, precipitation, aurora, time, lat/long, profiles | 2026-05-20, v3.2, 1.*; users report failures on 1.6.0 | The mod page says it is compatible with Lumina if Lumina's time slider stays off (it is off in the preset). **Both set lat/long:** Lumina's `PlanetarySystem.OnUpdate` patch wins. Set the same values (−26 / 39) in both. Its fog and cloud sliders reportedly stop working after Photo Mode. |
| fog (on/off only) | Nice Weather (79123) | toggles for no fog, clouds, rain, snow | 2026-01-12, 1.5.* | doesn't work alongside T&W Anarchy |
| fog, bloom, lens effects outside Photo Mode | Standalone Photo Effects (162288) | keeps Photo Mode volumetric clouds, lens and colour settings in normal gameplay | 2026-10-07, 1.6.2f1 | none stated; very new (15 subscribers); colour settings may overlap Lumina (unverified) |
| bloom intensity | SunGlasses "Sun Bloom" (not used) | bloom intensity (overrides Photo Mode bloom) | see above | see above |
| bloom quality in Photo Mode | Extended Photomode (159035) | stops vanilla lowering bloom/DoF quality in Photo Mode | 2026-09-16, 1.6.2 | — |
| **bloom threshold** | **no mod found** | — | — | — |
| TAA sharpening | **Enhanced Graphics and Textures** (161533) | TAA sharpening strength | see above | none stated |
| mip bias | **Enhanced Graphics and Textures** (161533) | mip bias (float, only on the road, sidewalk, gravel, rail and grass surfaces EGT edits), tree LOD, object detail | see above | none stated |
| LOD distance | LOD Control (141751); Auto Night LOD Extender (158089) | LOD distance 0–1000 %; higher LOD at night | 2026-09-19 / 2026-09-06, 1.6.2 / 1.6.* | — |
| **anisotropic filtering, per-category mip bias** | **no mod found** | — | — | — |

**Recommendation:** use **Enhanced Graphics and Textures** for the mip bias (−0.3 on the surfaces it edits) and leave its TAA sharpening and shadows at the game values. Use **Time & Weather Anarchy** for vanilla fog, no aurora, and the same lat/long as Lumina. The values are in [GAP-MODS.md](GAP-MODS.md).

Still uncovered, since SunGlasses and BetterMoonLight are not used:
- sun/moon intensity;
- shadow strength;
- night colours;
- Mie/Rayleigh scattering;
- bloom threshold;
- per-category anisotropic filtering.

Sources: Paradox Mods pages `https://mods.paradoxplaza.com/mods/<id>/Windows`, using the ids [76849](https://mods.paradoxplaza.com/mods/76849/Windows), [77923](https://mods.paradoxplaza.com/mods/77923/Windows), [161533](https://mods.paradoxplaza.com/mods/161533/Windows), [77104](https://mods.paradoxplaza.com/mods/77104/Windows), [88893](https://mods.paradoxplaza.com/mods/88893/Windows), [79123](https://mods.paradoxplaza.com/mods/79123/Windows), [162288](https://mods.paradoxplaza.com/mods/162288/Windows), [159035](https://mods.paradoxplaza.com/mods/159035/Windows), [141751](https://mods.paradoxplaza.com/mods/141751/Windows), [158089](https://mods.paradoxplaza.com/mods/158089/Windows) and [94394](https://mods.paradoxplaza.com/mods/94394/Windows). Also the Paradox Mods API (`api.paradox-interactive.com/mods?modId=<id>&os=Windows`), [BetterMoonLight source](https://github.com/Cmyna/BetterMoonLight), and the [CS2 Photo Mode dev diary](https://www.paradoxinteractive.com/games/cities-skylines-ii/features/cinematic-camera-photo-mode). HDRP External LUT behaviour comes from [LutBuilder3D.compute](https://github.com/Unity-Technologies/Graphics/blob/2022.3/staging/Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/LutBuilder3D.compute).
