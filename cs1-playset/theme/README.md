# Dakota Falls visual settings (CS1): source for a later CS2 Lumina recreation

**CS2 port:** see [LUMINA-MAPPING.md](LUMINA-MAPPING.md) and the Lumina preset in `lumina/` (not tested in game yet).

| file | source | contents |
|---|---|---|
| `RenderIt-DakotaFalls.txt` | description of collection 2972871641 (author MAVER1CK) | full Render It! profile "Dakota Falls" |
| `relight-settings-2972881605.png` | screenshot on Workshop item 2972881605 (Dakota Falls - Theme mix) | Relight panel |
| `DakotaFalls-ThemeMix.xml` | `ThemeMix.xml` inside 2972881605 | Theme Mixer 2.5 mix: atmosphere, terrain textures, water, weather; LUT `Relight2Natural` (More Relight LUTs 1539181199) |

## Relight values (transcribed from the screenshot)

| setting | value |
|---|---|
| Brightness / Gamma / Contrast | 1 / 1 / 1 |
| Temperature / Tint | 0.05 / 0 |
| Sun Temperature / Sun Tint | -0.25 / 0 |
| Sky Temperature / Sky Tint | -0.15 / 0 |
| Moon Temperature / Moon Tint / Moon Light | -0.75 / 0 / 0.25 |
| Twilight Tint | 0 |
| Enable Sky Tonemapping, Disable Shadow Smoothing, Force Low Shadow Bias | checkbox states are not readable in the screenshot |

## Render It! highlights
- Sun intensity 4, sun shadow strength 0.71; moon intensity 4, moon shadow strength 0.8.
- Sky wavelengths are R 680, G 550, B 440. Scattering and exposure are `.nan`, which means the theme defaults are used.
- Fog, bloom and colour grading are off. Ambient occlusion is on (intensity 2.55, radius 1.85). Vanilla bloom, tonemapping and LUT are on, and TAA is used (antialiasing technique 2).

## Theme mix highlights
- Location: latitude -26, longitude 39 (from Australia Coast). Sun size 0.82, anisotropy 0.651, Rayleigh 1.013, Mie 2.16.
- The source themes are Seychelles, Springwood, Owl's Albedo, r69-coastal, Australia Coast and Los Cabos.
- Water comes from CO-Temperate, which is inside Theme Mixer 2.5.
