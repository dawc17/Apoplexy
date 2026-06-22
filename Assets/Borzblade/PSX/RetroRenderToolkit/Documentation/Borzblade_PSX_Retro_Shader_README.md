# Borzblade PSX/PS2 Retro Render Toolkit for URP

A practical guide for setting up, tuning, and troubleshooting the Borzblade PSX/PS2 Retro Render Toolkit in Unity URP.

## Overview

Borzblade PSX/PS2 Retro Render Toolkit for URP is a shader and renderer toolkit for building a PlayStation-inspired look in Unity. It combines retro material shaders with an optional fullscreen renderer feature.

The package root is:

`Assets/Borzblade/PSX/RetroRenderToolkit/`

Use the toolkit when you want control over vertex snap, texture pixelation, affine-style UV distortion, color reduction, ordered dither, cutout shadows, foliage wind, terrain treatment, simple retro water, outlines, and a final fullscreen retro pass.

## What's Included

- URP lit retro shaders.
- Terrain shader support.
- Cutout and foliage shaders.
- Unlit cutout shader.
- Lit and unlit sprite shaders.
- Retro transparent water shader.
- Fullscreen retro renderer feature.
- Depth fog renderer feature.
- Screen-space outline renderer feature.
- Material outline shader.
- Editor toolkit window.
- Material presets.
- Material converter.
- Diagnostics tools.
- Documentation and sample materials.

## Not Included

- Third-party character, environment, prop, or demo-scene assets shown in screenshots are not included unless specifically listed.
- The package focuses on shaders, renderer tools, presets, materials, and documentation.

## Third-Party Assets / Screenshots

Promotional screenshots may use separate character, environment, prop, terrain, or demo assets to show the shaders in context. Those assets are not part of this package unless they are explicitly listed as included content.

## Compatibility

| Item | Value |
| --- | --- |
| Tested Unity version | `6000.4.1f1` |
| Tested URP version | `17.4.0` |
| Supported render pipeline | Universal Render Pipeline |
| Unsupported render pipelines | Built-in Render Pipeline and HDRP |
| Main menu path | `Tools > Borzblade > Retro Render Toolkit` |
| Toolkit window | `Tools > Borzblade > Retro Render Toolkit > Open Toolkit` |

The shaders are authored for URP. If a material turns pink, check that the active render pipeline asset is URP and that the shader files still exist under the toolkit folder.

## Folder Layout

| Folder | Purpose |
| --- | --- |
| `Art` | Placeholder textures, icons, previews, and alpha card example textures. |
| `Demo` | Demo scenes, prefabs, and demo materials when present. |
| `Documentation` | Markdown docs and this manual. |
| `Editor` | Toolkit window, installer, material converter, material factory, and shader inspectors. |
| `Materials` | Sample materials and preset materials. |
| `Runtime` | Renderer feature and runtime settings classes. |
| `Samples` | Optional material, terrain, foliage, and renderer comparison sample folders. |
| `Settings` | Renderer feature materials, global profiles, and presets. |
| `Shaders` | Core includes, material shaders, terrain shaders, and fullscreen shader. |

## Quick Start

1. Open the Unity project.
2. Confirm the project is using a URP render pipeline asset.
3. Open `Tools > Borzblade > Retro Render Toolkit > Open Toolkit`.
4. In the `Setup` tab, press `Install Retro Renderer Feature`.
5. Press `Create Default Materials`.
6. Assign materials from:
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Hybrid`
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Terrain`
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Cutout`
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Foliage`
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/UnlitCutout`
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Water`
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Outline`
   - `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Sprites`
7. Optional: press `Install Depth Fog Feature` if you want proper scene fog from camera depth.
8. Optional: press `Install Screen Outline Feature` if you want screen-space outlines.
9. In the `Renderer` tab, choose a fullscreen preset such as `Clean PS2`, `Crunchy PSX`, `Horror PSX`, `Dark Fantasy`, `VHS / CRT`, `Mobile Fast`, or `Off/Neutral`.
10. Use the material inspector presets to tune each material type.

A good starting setup:

- `PSXPS2_CleanPS2` for characters, weapons, and important props.
- `PSXPS2_CrunchyPSX` for surfaces that can handle a rougher low-resolution look.
- `PSXPS2_TerrainHybrid` for Unity Terrain.
- `M_RetroFoliage_Grass`, `M_RetroFoliage_Bush`, or `M_RetroFoliage_TreeLeaves` for foliage cards.
- `M_RetroWater_CleanPS2` for transparent water planes.
- `M_RetroOutline_Black` when you need an inverted-hull mesh outline material.
- `M_RetroSpriteLit_Clean` or `M_RetroSpriteUnlit_Clean` for SpriteRenderer materials.
- `Mobile Fast` renderer and material presets for lower-end targets.

## Shader Selection Guide

| Shader | Unity shader name | Best use |
| --- | --- | --- |
| Hybrid Lit | `Borzblade/Retro Render Toolkit/PSX PS2 Hybrid Lit` | Main lit material for characters, buildings, rocks, props, weapons, and most opaque meshes. |
| Terrain Lit | `Borzblade/Retro Render Toolkit/PSX PS2 Terrain Lit` | Unity Terrain material/template using TerrainLayers, normals, masks, holes, height blend, shadows, and URP terrain add-pass fallback. |
| Cutout Lit | `Borzblade/Retro Render Toolkit/PSX PS2 Cutout Lit` | Transparent alpha cards with lighting and shaped shadows, such as fences, grates, ropes, hair cards, beard cards, cloth tears, and leaves. |
| Foliage Lit | `Borzblade/Retro Render Toolkit/PSX PS2 Foliage Lit` | Transparent foliage material with wind, color variation, and top/bottom foliage tint. Best for grass, bushes, leaves, pine branches, and dead plants. |
| Unlit Cutout | `Borzblade/Retro Render Toolkit/PSX PS2 Unlit Cutout` | Low-cost transparent alpha cards, far foliage, sprite props, simple background cards, and mobile use. |
| Water | `Borzblade/Retro Render Toolkit/PSX PS2 Water` | Transparent retro water surfaces with stepped waves, foam, depth fade, fresnel tint, and retro texture/color controls. |
| Material Outline | `Borzblade/Retro Render Toolkit/PSX PS2 Material Outline` | Inverted-hull mesh outline material for selected characters, props, and readable silhouettes. |
| Sprite Lit | `Borzblade/Retro Render Toolkit/PSX PS2 Sprite Lit` | SpriteRenderer-friendly transparent sprite shader with URP lighting, tint, vertex color, retro geometry, and retro color controls. |
| Sprite Unlit | `Borzblade/Retro Render Toolkit/PSX PS2 Sprite Unlit` | Lower-cost SpriteRenderer-friendly transparent sprite shader for UI-like props, pickups, flat FX, and mobile sprites. |
| Fullscreen Global Pass | `Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Global Pass` | Renderer feature shader for final-screen pixelation, color steps, dither, scanlines, vignette, contrast, saturation, and bleed. |
| Depth Fog Pass | `Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Depth Fog` | Fullscreen depth-based fog with linear, exponential, stepped, and dithered fog options. |
| Screen Outline Pass | `Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Screen Outline` | Renderer feature shader for depth/normal based screen outlines. |

## Lighting Models

Hybrid, Cutout, Foliage, and Sprite Lit materials include a `Lighting Model` control.

| Mode | Use it when |
| --- | --- |
| `Standard URP` | You want the existing URP-style lit behavior with normal maps, specular, shadows, and lightmapping where the shader supports it. This is the default for existing mesh materials. |
| `Vertex Lit` | You want cheaper, older-looking lighting that changes across vertices instead of every pixel. Good for props, sprites, and low-poly meshes. |
| `Flat Lit` | You want hard face lighting for a low-poly look. It ignores the smooth vertex normal in the final lit result. |
| `Unlit` | You want texture, tint, emission, alpha, material fog, and retro color controls without scene lighting. |

These modes do not replace the separate unlit cutout shader. Use `PSX PS2 Unlit Cutout` or `PSX PS2 Sprite Unlit` when you want the cheapest dedicated unlit path.

## Toolkit Window

Open the toolkit from:

`Tools > Borzblade > Retro Render Toolkit > Open Toolkit`

| Tab | What it does |
| --- | --- |
| `Setup` | Shows pipeline status, finds renderer assets, installs renderer features, creates default materials, opens docs, and pings the package root. |
| `Renderer` | Selects URP renderer assets, installs/edits the retro renderer feature, edits depth fog, edits screen outlines, and applies renderer presets. |
| `Materials` | Creates default materials, cutout/foliage materials, water/outline materials, sprite materials, quick new materials, and lists toolkit materials. |
| `Converter` | Converts selected materials or all materials in a selected folder to a toolkit shader, including water, material outline, and sprite shaders. |
| `Presets` | Applies material presets to selected materials. |
| `Terrain` | Creates/selects the terrain material and applies terrain presets. |
| `Cutout/Foliage` | Creates cutout, foliage, and unlit cutout material sets. |
| `Demo/Docs` | Opens package folders and documentation. |
| `Diagnostics` | Checks Unity version, pipeline asset, selected materials, required folders, shader availability, renderer feature installation, and old folder leftovers. |

## Installing The Fullscreen Renderer Feature

The fullscreen pass is optional. Use it when you want the whole camera image to share the same retro treatment.

Install it from either place:

- Toolkit window: `Setup > Install Retro Renderer Feature`
- Menu item: `Tools > Borzblade > Retro Render Toolkit > Install Retro Renderer Feature`

The installer:

- Creates or updates `Assets/Borzblade/PSX/RetroRenderToolkit/Settings/GlobalProfiles/PSXPS2_GlobalPass.mat`.
- Checks known renderer assets such as `Assets/Settings/PC_Renderer.asset` and `Assets/Settings/Mobile_Renderer.asset`.
- Searches all renderer data assets in the project.
- Adds `PSX PS2 Retro Pass` to each discovered URP renderer asset when it is missing.
- Assigns the global pass material to the renderer feature.
- Enables the feature and saves assets.

The runtime class is:

`Borzblade.RetroRenderToolkit.RetroRendererFeature`

A compatibility wrapper named `PSXPS2RetroRendererFeature` is also present so older renderer feature sub-assets continue to resolve.

The pass runs after URP post-processing. It fetches the color buffer and skips work when the feature is disabled or `Intensity` is zero.

## Installing The Screen Outline Feature

The screen outline feature is optional. It uses the camera depth and normals textures, then writes outlines before the final fullscreen retro pass.

Install it from either place:

- Toolkit window: `Setup > Install Screen Outline Feature`
- Renderer tab: `Screen Outline > Install Screen Outline Feature`
- Menu item: `Tools > Borzblade > Retro Render Toolkit > Install Screen Outline Feature`

The runtime class is:

`Borzblade.RetroRenderToolkit.PSXPS2ScreenOutlineRendererFeature`

Use screen outlines for broad scene readability. Use the material outline shader when a specific mesh needs a controlled silhouette.

## Installing The Depth Fog Feature

Depth fog is the main fog system for scenes. It samples the camera depth texture, computes fog from distance, and blends it before the final retro fullscreen pass. That means the fog also receives the final pixelation, color reduction, and screen treatment.

Install it from either place:

- Toolkit window: `Setup > Install Depth Fog Feature`
- Renderer tab: `Depth Fog > Install Depth Fog Feature`
- Menu item: `Tools > Borzblade > Retro Render Toolkit > Install Depth Fog Feature`

The runtime class is:

`Borzblade.RetroRenderToolkit.PSXPS2DepthFogRendererFeature`

The pass material is created at:

`Assets/Borzblade/PSX/RetroRenderToolkit/Settings/GlobalProfiles/PSXPS2_DepthFog.mat`

Use depth fog for the whole scene. Use material `Retro Fog` only when a specific shader needs a local stylized fade that should not depend on the camera depth texture.

## Fullscreen Renderer Settings

| Setting | Range | What it controls | Notes |
| --- | --- | --- | --- |
| `Effect Enabled` | On/off | Turns the renderer feature settings on or off. | Use this for quick comparisons. |
| `Intensity` | `0` to `1` | Blends between the original image and the processed image. | Start near `0.35` for subtle PS2. Use higher values for a stronger PSX look. |
| `Pixel Scale` | `1` to `12` | Size of screen-space pixel blocks. | `1` is clean. `2` or `3` is visibly chunky. |
| `Pixelation Mode` | Scale / Fixed Vertical Resolution | Chooses between the normal pixel scale and a fixed vertical resolution target. | Use fixed vertical resolution when you want a consistent 240p-style or 360p-style output across different window sizes. |
| `Fixed Vertical Resolution` | `64` to `1080` | Target vertical resolution when fixed mode is enabled. | `240` is a classic low-resolution baseline. |
| `Color Steps` | `2` to `64` | Final color quantization. | `28` is balanced. `12` to `16` is much harsher. |
| `Dither Strength` | `0` to `1` | Ordered Bayer dither before color quantization. | Useful with low color steps. Disable it first when optimizing. |
| `Dither Pattern Mode` | Procedural / Texture | Uses the built-in procedural Bayer pattern or a user-supplied texture. | The package does not include third-party dither textures. Assign your own if needed. |
| `Dither Pattern Texture` | Texture | Custom dither pattern used when texture mode is selected. | Point-filtered grayscale textures work best. |
| `Dither Pattern Scale` | `0.25` to `16` | Scales the custom dither texture sampling. | Higher values make the pattern larger on screen. |
| `Dither Threshold` | `0` to `1` | Shifts the custom dither texture around the midpoint. | `0.5` is neutral. |
| `Scanline Strength` | `0` to `1` | Darkens alternating scanline pattern. | Keep low unless the screen treatment is intentional. |
| `Vignette Strength` | `0` to `1` | Darkens image edges. | Useful for horror and darker scenes. Easy to overdo. |
| `Saturation` | `0` to `2` | Pushes color toward grayscale or stronger color. | `1` is neutral. |
| `Contrast` | `0` to `2` | Adjusts final contrast. | Small changes are usually enough. |
| `Color Bleed` | `0` to `1` | Offsets color sampling horizontally. | Low values add a soft analog feel. High values look broken. |
| `Color Tint` | Color | Multiplies the final image. | Keep close to white for normal use. |
| `Gamma` | `0.2` to `3` | Adjusts midtone response. | Small changes are best. |
| `Black Level` | `0` to `1` | Raises the black cutoff before quantization. | Useful for darker horror looks. |
| `Dither Scale` | `0.25` to `8` | Scales the screen-space dither pattern. | `1` matches the default Bayer pattern. |
| `CRT Mask Strength` | `0` to `1` | Adds a simple RGB stripe mask. | Keep low unless the camera is meant to look like a display capture. |
| `Chromatic Offset` | `0` to `1` | Separates red/blue sampling horizontally. | Low values are enough. |
| `Noise Strength` | `0` to `1` | Adds light temporal grain. | Use sparingly. |
| `Horizontal Jitter` | `0` to `1` | Adds per-line horizontal wobble. | Better for VHS/CRT presets than clean PS2. |
| `Curvature` | `0` to `1` | Bends screen UVs outward. | Optional; leave off for a plain game-camera look. |
| `Global Fog Enabled` | On/off | Adds a simple fullscreen fog tint before final quantization. | Kept for older presets and quick tinting. Prefer Depth Fog for normal scene fog. |
| `Global Fog Color` | Color | Fog tint color. | Alpha also controls tint strength. |
| `Global Fog Intensity` | `0` to `1` | Blends the fog tint over the final image. | Keep low for subtle haze. |

## Depth Fog Settings

Depth fog reads scene depth, so it follows actual camera distance instead of guessing per material. It works best when the active URP renderer has depth texture support available.

| Setting | What it controls | Notes |
| --- | --- | --- |
| `Effect Enabled` | Turns the depth fog pass on or off. | The renderer feature can also be disabled from the renderer asset. |
| `Pass Event` | When the pass runs in URP. | Default is `BeforeRenderingPostProcessing`, before the final retro pass. |
| `Fog Color` | Fog tint and alpha strength. | Alpha multiplies the final fog amount. |
| `Intensity` | Overall blend strength. | Start around `0.25` to `0.45`. |
| `Start Distance` | Distance where fog begins. | Use scene scale units. |
| `End Distance` | Distance where linear fog reaches full strength. | Also used to normalize stepped fog. |
| `Density` | Exponential fog density. | Used by exponential blend modes. |
| `Blend Mode` | Linear, Exponential, or Exponential Squared. | Linear is predictable. Exponential modes feel denser near the far range. |
| `Steps` | Quantizes the fog amount. | Higher values are smoother. `0` or `1` disables stepping. |
| `Dither Strength` | Breaks up fog bands with ordered dither. | Useful when `Steps` is low. |
| `Dither Scale` | Scale of the fog dither pattern. | `1` is the default 4x4 Bayer scale. |
| `Affect Sky` | Applies fog to sky/background depth. | Leave off when the skybox should stay clear. |

### Renderer Presets

| Preset | Settings | Use case |
| --- | --- | --- |
| `Clean PS2` | Intensity `0.35`, Pixel Scale `1`, Color Steps `40`, Dither `0.04`, Scanlines `0.02`, Vignette `0.06`, Saturation `1.03`, Contrast `1.03`, Bleed `0.02` | Subtle final pass for a cleaner low-poly look. |
| `Crunchy PSX` | Intensity `0.85`, Pixel Scale `3`, Color Steps `12`, Dither `0.34`, Scanlines `0.15`, Vignette `0.18`, Saturation `0.95`, Contrast `1.14`, Bleed `0.14` | Strong screen-space PSX treatment. |
| `Horror PSX` | Intensity `0.78`, Pixel Scale `2`, Color Steps `16`, Dither `0.28`, Scanlines `0.12`, Vignette `0.38`, Saturation `0.78`, Contrast `1.22`, Bleed `0.10` | Darker, lower-color scenes. |
| `Dark Fantasy` | Intensity `0.60`, Pixel Scale `1.5`, Color Steps `20`, Dither `0.18`, Scanlines `0.06`, Vignette `0.26`, Saturation `0.90`, Contrast `1.12`, Bleed `0.08` | Moody fantasy scenes. |
| `Mobile Fast` | Intensity `0.28`, Pixel Scale `1`, Color Steps `28`, Dither `0`, Scanlines `0`, Vignette `0.05`, Saturation `1`, Contrast `1`, Bleed `0` | Low-cost mild treatment. |
| `Off/Neutral` | Intensity `0`, Pixel Scale `1`, Color Steps `64`, Dither `0`, Scanlines `0`, Vignette `0`, Saturation `1`, Contrast `1`, Bleed `0` | Disables the fullscreen effect. |
| `VHS / CRT` | Moderate pixel scale, scanlines, color bleed, mask, chromatic offset, noise, line jitter, and slight curvature. | A stronger analog-display pass. |

## Material Presets

Presets are starting points. Apply one, then tune the material in context with your lighting, textures, camera distance, and target platform.

| Preset group | Presets |
| --- | --- |
| Hybrid Lit | `Hybrid High`, `Crunchy PSX`, `Clean PS2`, `Mobile Fast` |
| Terrain Lit | `Mountain Path` / `Hybrid Terrain`, `Crunchy PSX Terrain`, `Clean PS2 Terrain`, `Mobile Terrain` |
| Cutout Lit | `Clean Cutout`, `Crunchy PSX Cutout`, `Dither Fade Cutout`, `Hair / Beard Card`, `Fence / Grate`, `Mobile Cutout` |
| Foliage Lit | `Grass Card`, `Bush Leaves`, `Tree Leaves`, `Pine Branch`, `Dead Bush`, `Dark Fantasy`, `Mobile Fast` |
| Unlit Cutout | `Distant Foliage`, `Sprite Prop`, `Dither Fade Card`, `Mobile Fast` |
| Water | `Clean PS2`, `Crunchy PSX`, `Swamp`, `Mobile Fast` |
| Material Outline | `Black`, `Warm`, `Crunchy` |

The `Presets` tab also has a `My Presets` section. It can save the selected material's geometry, texture modifier, color, lighting, wind, fog, and retro renderer-related shader values as a `RetroMaterialPreset` asset under:

`Assets/Borzblade/PSX/RetroRenderToolkit/Settings/UserPresets/`

User presets do not store the base texture or base color. That keeps them useful across different props, foliage cards, and characters. You can load a saved preset onto selected materials, delete it with confirmation, or export/import it as JSON.

## Hybrid Lit Shader

Use `PSX PS2 Hybrid Lit` for most lit mesh assets.

Good uses:

- Characters and enemies.
- Weapons, armor, tools, pickups, crates, doors, walls, roofs, stones, ruins, props, and architecture.
- Opaque materials that need shadows, normal maps, emission, rim light, PS2-style specular, and retro texture/color controls.

Use another shader for these cases:

- Unity Terrain: use `PSX PS2 Terrain Lit`.
- Alpha-card foliage: use `PSX PS2 Foliage Lit` or `PSX PS2 Cutout Lit`.
- Cheap distant cards or sprite props: use `PSX PS2 Unlit Cutout`.
- Transparent water planes: use `PSX PS2 Water`.
- Mesh outlines: use `PSX PS2 Material Outline` or the screen outline renderer feature.

### Hybrid Lit Inspector Sections

| Section | Important controls |
| --- | --- |
| `Surface` | Base Map, Base Color, Normal Map, Normal Scale, Alpha Clip Threshold, Surface Type, Blend Mode, Cull. |
| `Geometry Modifiers` | Vertex Snap, Vertex Snap Strength, Vertex Snap Resolution, Distance Fade, Fade Start/End, Snap Seam Reduction, optional baked snap anchors, Vertex Wobble, Wobble Strength, Speed, Scale. |
| `Texture Modifiers` | UV Pixelation, UV Pixel Strength, UV Pixel Resolution, UV Pixel Aspect, Mip Bias, Affine Warp, Affine Strength. |
| `Color Modifiers` | Posterize, Posterize Steps, Palette Strength, Palette Steps, Dither, Dither Strength, Dither Scale. |
| `Lighting Modifiers` | Shadow/Light Bands, Band Strength, Rim, Rim Color, Rim Intensity, Rim Power, Specular Mode, PS2 specular settings. |
| `Retro Fog` | Per-material fog toggle, color, start/end, density, steps, and linear/exponential blend mode. |
| `Advanced` | Render queue, alpha state, receive shadows, and low-level render state controls. |

### Hybrid Lit Presets

| Preset | Main behavior |
| --- | --- |
| `Hybrid High` | Balanced default. Vertex snap on, mild wobble, UV pixelation, posterization, dither, rim light, and medium specular. |
| `Crunchy PSX` | Strong vertex snap, low snap resolution, higher wobble, heavy UV pixelation, affine warp, low posterize steps, stronger palette compression, dither, and light bands. |
| `Clean PS2` | Vertex snap and wobble off, high UV resolution, subtle posterization, no dither, rim enabled, stronger glossy specular. |
| `Mobile Fast` | Lower-cost settings. Moderate vertex snap, wobble off, affine off, no dither, rim off, lower specular. |

## Cutout Lit Shader

Use `PSX PS2 Cutout Lit` for transparent alpha-card geometry that should receive lighting and cast shaped shadows.

Good uses:

- Fences and grates.
- Hair and beard cards.
- Torn cloth and banners.
- Ropes, branches, leaves, and card details that are not using the foliage shader.
- Any alpha texture where a square shadow would be wrong.

The visible pass uses transparent blending, while the depth and shadow passes still use alpha cutoff. This keeps soft texture alpha visible in URP while preserving shaped card silhouettes for shadows and depth.

### Cutout Settings

| Setting | What it does |
| --- | --- |
| `Alpha Cutoff` | Controls clipping in depth and normal passes. Higher values remove more pixels from depth silhouettes. |
| `Shadow Cutoff` | Controls clipping in the shadow pass. Raise it if shadows are too thick. Lower it if shadows disappear. |
| `Dithered Cutout Fade` | Fades card visibility by distance or camera fade. The forward pass fades alpha; depth and shadow passes use dithered coverage. |
| `Dither Fade Amount` | Maximum dither fade amount. |
| `Dither Fade Start/End` | Distance range for distance-based dither fade. |
| `Camera Fade` | Dither-fades cards close to the camera. Useful for grass or leaves near the view. |
| `Camera Fade Distance` | Distance where close-camera fade is strongest. |
| `Distance Fade` | Enables distance-based dither fade using the fade start/end values. |
| `Backface Tint` | Tints reverse faces so two-sided cards read less flat. |
| `Two Sided` | Disables culling so cards are visible from both sides. Useful for leaves and hair. |

### Cutout Presets

| Preset | Use case |
| --- | --- |
| `Clean Cutout` | General card material with subtle retro treatment. |
| `Crunchy PSX Cutout` | Stronger vertex snap, affine warp, pixelation, posterization, palette compression, dither, and light bands. |
| `Dither Fade Cutout` | Distance fade for cards that should disappear gradually. |
| `Hair / Beard Card` | Lower cutoff, tuned shadow cutoff, two-sided rendering, lower rim/specular. |
| `Fence / Grate` | Higher cutoff, backface culling, moderate vertex snap, good for hard alpha shapes. |
| `Mobile Cutout` | Lower-cost dither, rim, and specular settings. |

## Foliage Lit Shader

Use `PSX PS2 Foliage Lit` for grass, bushes, tree leaves, pine branches, and plant cards. It starts from the transparent alpha-card workflow and adds wind and foliage color controls.

### Foliage / Wind Settings

| Setting | What it does |
| --- | --- |
| `Wind Enabled` | Enables vertex movement for foliage. |
| `Wind Strength` | Multiplier for the local jiggle amount. Keep it low for tree leaves and moderate for grass. |
| `Jiggle Amplitude` (`_WindDistance`) | Maximum local flutter amount. It no longer sweeps the whole card sideways. UV and vertex masks can reduce it per vertex. |
| `Wind Speed` | Animation speed. Very high values can shimmer with vertex snap. |
| `Wind Scale` | Spatial frequency of wind variation. |
| `Wind Direction` | Local tangent/bitangent bias for the flutter direction. |
| `Vertex Color Alpha Wind Mask` | Uses vertex alpha as a wind mask. Paint darker alpha near stems to anchor them. |
| `UV Height Wind Mask` | Uses UV height so the top of a card moves more than the bottom. Useful for grass. |
| `Top/Bottom Gradient` | Multiplies color by a vertical foliage gradient. |
| `Top Color` and `Bottom Color` | Gradient colors for foliage tinting. |
| `Color Variation` | Adds per-position color variation to reduce repetition. |
| `Hue/Saturation/Brightness Variation` | Controls the variation channels. |
| `Mobile Lighting` | Simplifies foliage lighting for lower cost. |

### Foliage Presets

| Preset | Use case |
| --- | --- |
| `Grass Card` | Stronger wind, lower cutoff, green top/bottom gradient. |
| `Bush Leaves` | Balanced bush preset with moderate wind and green variation. |
| `Tree Leaves` | Lower wind, smaller wind scale, lower specular/rim. |
| `Pine Branch` | Higher cutoff and darker green palette. |
| `Dead Bush` | Brown dry plant coloring and more saturation variation. |
| `Dark Fantasy` | Blue-gray fantasy foliage with stronger posterization, dither, and light bands. |
| `Mobile Fast` | Lower wind, no foliage variation, mobile lighting on, dither and rim off. |

## Unlit Cutout Shader

Use `PSX PS2 Unlit Cutout` when lighting is not needed or the object is far from the camera.

Good uses:

- Distant tree cards.
- Sprite-like prop cards.
- Background card silhouettes.
- Cheap mobile foliage.
- Simple cards where lighting cost is not worth it.

This shader supports Base Map, Base Color, transparent alpha blending, Alpha Cutoff for depth, Shadow Cutoff, Emission Color, Emission Map, UV pixelation, posterization, palette compression, ordered dither, dithered distance fade, two-sided state, ShadowCaster, and DepthOnly passes.

It does not use normal maps, PS2 specular, rim lighting, or foliage wind.

## Sprite Shaders

Use the sprite shaders on Unity `SpriteRenderer` objects when you want sprite texture assignment, renderer tint, and sprite vertex color support.

| Shader | Use case |
| --- | --- |
| `PSX PS2 Sprite Lit` | Sprites that should respond to URP scene lighting. It supports the shared `Lighting Model` control, normal map, specular, emission, retro geometry, UV pixelation, affine modes, posterization, dither, material fog, and vertex draw distance. |
| `PSX PS2 Sprite Unlit` | Sprites that should stay flat and cheap. It keeps tint, emission, retro geometry, UV pixelation, affine modes, posterization, dither, material fog, and vertex draw distance. |

Create default sprite materials from:

`Tools > Borzblade > Retro Render Toolkit > Create Sprite Materials`

The generated materials are stored in:

`Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Sprites`

Sprite shaders use transparent blending. They follow Unity's normal SpriteRenderer sorting rules, so sorting layer, order in layer, material queue offset, and camera transparency sorting still matter.

## Water Shader

Use `PSX PS2 Water` for simple transparent water planes, ponds, streams, pools, and stylized shoreline surfaces.

Main controls:

| Setting | What it does |
| --- | --- |
| `Surface Tint`, `Shallow Color`, `Deep Color` | Sets the base water palette. Depth fade blends between shallow and deep colors when the camera depth texture is available. |
| `Alpha` | Controls transparent blend strength. |
| `Wave / Foam Noise` | Optional noise texture for wave and foam variation. |
| `Vertex Wave Strength`, `Wave Speed`, `Wave Scale`, `Wave Steps` | Controls stepped water motion. Lower steps look more retro. |
| `Depth Fade` and `Depth Fade Distance` | Uses scene depth for water color depth. Requires camera depth texture support in URP. |
| `Foam Color`, `Foam Distance`, `Foam Strength` | Adds simple edge foam from depth difference and noise. |
| `Fresnel Color`, `Fresnel Intensity`, `Fresnel Power` | Adds a rim tint at grazing angles. |
| `Retro Fog` | Optional per-material fog for stylized water tinting. Use depth fog for the main scene haze. |

Water uses transparent blending, so it can still be affected by normal Unity transparency sorting. Keep large water surfaces simple, avoid stacking many transparent planes, and use the mobile preset when depth fade is not needed.

## Outline Features

There are two outline options.

| Option | Use it when |
| --- | --- |
| Screen outline renderer feature | You want camera-wide outlines from scene depth and normals. Install it from the `Setup` or `Renderer` tab. |
| Material outline shader | You want an intentional silhouette on a specific mesh. Assign `PSX PS2 Material Outline` to a duplicate mesh or outline submesh. |

Screen outline settings:

| Setting | What it does |
| --- | --- |
| `Pass Event` | Controls where the outline pass runs. The default is before post-processing so the final retro pass also affects the outlines. |
| `Outline Color` | Final outline color. |
| `Intensity` | Overall strength. |
| `Thickness` | Neighbor sampling distance in screen pixels. |
| `Depth Sensitivity` | How strongly depth changes create outlines. |
| `Normal Sensitivity` | How strongly normal changes create outlines. |
| `Distance Fade Start/End` | Fades outlines over distance. |
| `Blend` | Blends the outline color over the scene. |

Material outline settings:

| Setting | What it does |
| --- | --- |
| `Outline Color` | Mesh outline color and alpha. |
| `Outline Thickness` | Object-space normal extrusion. |
| `Distance Fade` | Softens the outline over distance. |
| `Vertex Snap` and `Dither` | Optional retro treatment for the outline mesh. |

## Terrain Lit Shader

Use `PSX PS2 Terrain Lit` as the material/template for Unity Terrain.

Setup:

1. Open `Tools > Borzblade > Retro Render Toolkit > Open Toolkit`.
2. Go to the `Terrain` tab.
3. Press `Create Terrain Material`.
4. Select your Terrain object.
5. In the Terrain component, assign `Assets/Borzblade/PSX/RetroRenderToolkit/Materials/Terrain/PSXPS2_TerrainHybrid.mat` to the material/template slot.
6. Continue painting TerrainLayers with Unity's normal Terrain tools.

TerrainLayers still handle:

- Albedo textures.
- Normal maps.
- Mask maps.
- Tiling and offsets.
- Metallic and smoothness values.
- Height data.

The retro terrain material adds vertex snap, optional terrain vertex wobble, TerrainLayer UV pixelation, posterization, palette compression, ordered dither, optional light bands, optional material fog, terrain holes support, and the required depth, shadow, normal, scene selection, and meta passes. When Unity needs extra terrain passes for more than four painted layers, the shader uses URP's built-in terrain add pass so the main terrain shader imports reliably.

### Terrain Presets

| Preset | Main behavior |
| --- | --- |
| `Mountain Path` / `Hybrid Terrain` | Balanced default. Vertex snap on, UV pixelation on, posterization and dither on, light bands off. |
| `Crunchy PSX Terrain` | Stronger snap, wobble on, low UV resolution, low posterize steps, stronger palette/dither, and light bands on. |
| `Clean PS2 Terrain` | Subtle snap, high snap resolution, very low UV pixelation, higher posterize steps, low dither, no light bands. |
| `Mobile Terrain` | Vertex snap on, TerrainLayer UV pixelation off, dither off, light bands off, instanced per-pixel normal off. |

Unity Terrain note: reliable height blending can break when a terrain needs extra add passes for more than four layers. If height blending looks wrong, reduce the number of active painted layers per terrain tile or turn height blending off.

## Material Parameter Reference

### Surface And Texture Inputs

| Property | Applies to | Meaning |
| --- | --- | --- |
| `Base Map` / `_BaseMap` | Mesh material shaders | Main texture. Alpha is used by cutout shaders. |
| `Sprite Texture` / `_MainTex` | Sprite shaders | SpriteRenderer texture. Supports sprite tint and renderer vertex color. |
| `Base Color` / `_BaseColor` | Mesh material shaders | Multiplies the base texture. Alpha also affects cutout opacity. |
| `Tint` / `_Color` | Sprite shaders | Multiplies the sprite texture and SpriteRenderer color. |
| `Normal Map` / `_BumpMap` | Lit shaders | Tangent-space normal map. |
| `Normal Scale` / `_BumpScale` | Lit shaders | Strength of the normal map. |
| `Emission Color` / `_EmissionColor` | Hybrid, Cutout, Foliage, Unlit Cutout, Sprite, Water | HDR emission multiplier where supported. |
| `Emission Map` / `_EmissionMap` | Hybrid, Cutout, Foliage, Unlit Cutout, Sprite | Emission texture. |
| `Mip Bias` / `_MipBias` | Hybrid, Cutout, Foliage, Unlit Cutout, Water, Sprite | Biases texture mip level. Negative values sharpen, positive values soften. |

### Geometry Controls

| Property | Meaning | Typical values |
| --- | --- | --- |
| `Vertex Snap Enabled` | Quantizes clip-space vertex positions to a low-resolution grid. | On for PSX, off or subtle for clean PS2. |
| `Vertex Snap Strength` | Blend amount between normal geometry and snapped geometry. | `0.05` to `0.35` subtle, `0.55+` strong. |
| `Vertex Snap Resolution` | Grid resolution used for snapping. Lower means chunkier movement. | `128` to `240` PSX, `320+` cleaner. |
| `Vertex Snap Space` | Chooses screen-space snap or view/world-style snap. | Screen is the classic default. View/World feels more like object-space jitter and can be calmer on some meshes. |
| `Vertex Snap Distance Fade` | Fades snap by camera distance. | Use higher values to reduce distant crawl. |
| `Snap Fade Start/End` | Distance range for snap fade. | Adjust to your scene scale. |
| `Snap Seam Reduction` | Reduces visible face gaps caused by heavy clip-space snapping. | `0` is rawer. `0.15` to `0.35` is usually enough. |
| `Use Baked Snap Anchors` | Uses UV4 snap-anchor data baked by the toolkit. | Optional. Use it for meshes that crack at shared edges. |
| `Vertex Draw Distance` | Clips geometry past a material-controlled distance when enabled. | Off by default. Use for props that should disappear at a retro draw distance. |
| `Vertex Draw Distance Fade` | Dither-fades the cutoff instead of using a hard pop. | `0` gives a hard cutoff. Small fade widths are usually enough. |
| `Vertex Wobble Enabled` | Adds animated normal-direction wobble. | Use carefully on characters and foliage. |
| `Vertex Wobble Strength` | Amount of wobble. | `0.04` to `0.12` subtle, `0.25+` stylized. |
| `Vertex Wobble Speed` | Animation speed. | Keep moderate to avoid shimmer. |
| `Vertex Wobble Scale` | Spatial frequency of wobble. | Higher values vary more across the mesh. |

### Texture Controls

| Property | Meaning | Notes |
| --- | --- | --- |
| `UV Pixelation Enabled` | Quantizes UV coordinates before sampling. | Use for chunky texture sampling. |
| `UV Pixelation Strength` | Blend amount between original and pixelated UVs. | Start around `0.08` to `0.22`. |
| `UV Pixelation Resolution` | Virtual texture grid resolution. | Lower values such as `128` look rougher. |
| `UV Pixelation Aspect` | Non-square UV pixel aspect multiplier. | Leave `1` unless you want stretched texel blocks. |
| `Affine Warp Enabled` | Simulates affine UV interpolation by carrying a clip-W UV pair from vertex to fragment. | Strongest on large polygons viewed at grazing angles. |
| `Affine Warp Strength` | Blend between perspective-correct UVs and affine UVs. | `0` is normal. `1` is full affine. |
| `Affine Mode` | Chooses Stable or Classic affine behavior. | Stable is the default. Classic is stronger and closer to older PSX-style texture swimming. |

### Color Controls

| Property | Meaning | Notes |
| --- | --- | --- |
| `Posterize Enabled` | Reduces color precision per material. | Useful even with fullscreen color steps. |
| `Posterize Steps` | Number of color steps. | `10` to `16` crunchy, `24` balanced, `36+` clean. |
| `Palette Strength` | Blends toward a palette-compressed color. | Higher values create stronger color grouping. |
| `Palette Steps` | Number of palette steps. | `16` crunchy, `32` balanced. |
| `Dither Enabled` | Adds ordered Bayer dither. | Helps banding and adds retro texture. |
| `Dither Strength` | Amount of ordered dither. | Keep lower on mobile or high-resolution displays. |
| `Dither Scale` | Screen-space dither scale. | `1` is the default. |

### Lighting Controls

| Property | Meaning | Notes |
| --- | --- | --- |
| `Specular Color` | Color of PS2-style highlight. | Neutral gray/white fits most materials. |
| `Smoothness` | Smoothness used by lit shading. | Higher values make tighter highlights. |
| `PS2 Specular Intensity` | Extra main-light specular intensity. | Clean PS2 can use stronger values than PSX. |
| `PS2 Specular Power` | Specular exponent. | Higher values make smaller highlights. |
| `Specular Mode` | Chooses Legacy Per Pixel, PS1 Off, or PS2 Vertex specular. | Existing materials keep Legacy by default. PSX presets disable specular. PS2 presets use vertex specular. |
| `Lighting Model` | Chooses Standard URP, Vertex Lit, Flat Lit, or Unlit. | Hybrid, Cutout, Foliage, and Sprite Lit support it. Standard URP is the compatibility default for mesh shaders. |
| `Shadow/Light Bands` | Quantizes perceived lighting intensity. | Use `4` to `6` for stylized banding. |
| `Band Strength` | Blend amount for lighting bands. | Avoid high values on faces unless that is the intended style. |
| `Rim Enabled` | Adds view-dependent rim light. | Good for characters and silhouettes. |
| `Rim Color` | Rim light color. | Use scene lighting as a guide. |
| `Rim Intensity` | Rim brightness. | Keep subtle for clean PS2. |
| `Rim Power` | Rim falloff. | Higher values make a thinner rim. |

### Retro Fog Controls

`Hybrid Lit`, `Cutout Lit`, `Foliage Lit`, `Unlit Cutout`, `Terrain Lit`, and `Water` include optional per-material retro fog. It is calculated from camera distance, then quantized by `Fog Steps`. Use `Linear` for direct start/end fog. Use `Exponential` when you want fog to build up more gradually from the camera.

Material fog only changes color. It does not change alpha, render queue, or transparency sorting.

For normal whole-scene fog, install `PSXPS2DepthFogRendererFeature` instead. Depth fog uses the camera depth texture, so it handles mixed objects more consistently than per-material fog.

## Converting Existing Materials

Open the converter here:

`Tools > Borzblade > Retro Render Toolkit > Open Toolkit > Converter`

| Option | Meaning |
| --- | --- |
| `Target Shader` | Converts to Hybrid, Terrain, Cutout, Foliage, Unlit Cutout, Water, or Material Outline. |
| `Create Copies` | Copies materials instead of overwriting originals. Recommended for first tests. |
| `Preserve Properties` | Preserves common base texture, color, normal, emission, cutoff, cull, alpha clip, smoothness, and queue values. |
| `Backup If Overwriting` | Creates a backup material before overwriting an original. |
| `Apply Preset` | Applies a toolkit preset after conversion. |
| `Dry Run` | Lists what would be converted before doing it. |

Suggested workflow:

1. Select one or more materials in the Project window.
2. Open the `Converter` tab.
3. Keep `Create Copies` on for the first pass.
4. Keep `Preserve Properties` on.
5. Pick a target shader and preset.
6. Press `Dry Run`.
7. If the preview is correct, press `Convert Selected Materials`.
8. Inspect the new material copies in a test scene.
9. Only overwrite originals after the copies are verified.

For folder conversion, select a folder in the Project window and press `Convert Materials In Selected Folder`.

## Practical Recipes

### Clean PS2 Scene

1. Install the renderer feature.
2. Apply renderer preset `Clean PS2`.
3. Use `PSXPS2_CleanPS2` for characters and important props.
4. Use `Clean PS2 Terrain` for terrain.
5. Keep vertex snap off or low on hero characters.
6. Use UV pixelation around `0.08` to `0.18`.
7. Keep dither low or disabled.
8. Use higher posterize steps such as `36` or `40`.

### Crunchy PSX Scene

1. Apply renderer preset `Crunchy PSX`.
2. Use `PSXPS2_CrunchyPSX` on props and surfaces that can handle stronger artifacts.
3. Use `Crunchy PSX Terrain`.
4. Enable affine warp on surfaces that can tolerate distortion.
5. Keep faces, UI-facing items, and key weapons less extreme if readability matters.
6. Use cutout shaders for alpha cards so shadows match silhouettes.

### Dark Fantasy / Horror Scene

1. Apply renderer preset `Dark Fantasy` or `Horror PSX`.
2. Lower saturation through the renderer or through material colors.
3. Raise vignette carefully.
4. Use light bands on terrain and non-character props.
5. Keep character rim light subtle so silhouettes stay readable.
6. If enemies or UI become hard to see, reduce vignette and contrast first.

### Foliage Cards

1. Use `PSX PS2 Foliage Lit`.
2. Keep `Two Sided` enabled for grass and leaves.
3. Assign a texture with a real alpha channel to `Base Map`.
4. Tune texture alpha and `Base Color` alpha until the visible card shape is clean.
5. Tune `Shadow Cutoff` separately so shadows are not too full or too thin.
6. Enable wind, but keep vertex snap and wobble subtle.
7. Use `UV Height Wind Mask` for grass cards so bases remain stable.
8. Use `Vertex Color Alpha Wind Mask` if the mesh has vertex colors painted.

### Fences, Hair, And Grates

1. Use `PSX PS2 Cutout Lit`.
2. Use the texture alpha for the visible card shape.
3. For hair and beard cards, use lower cutoff for depth/shadow silhouettes and two-sided rendering.
4. For fences and grates, use a higher cutoff and backface culling when the model has real thickness.
5. Check shadows in-scene. Adjust `Shadow Cutoff` when only the shadow needs correction.

### Mobile / Low Cost

1. Use renderer preset `Mobile Fast`.
2. Use material preset `Mobile Fast`.
3. Prefer `Unlit Cutout` for distant cards.
4. Disable dither where possible.
5. Keep screen pixel scale at `1`.
6. Avoid heavy vertex wobble.
7. Disable terrain instanced per-pixel normals if terrain cost is high.
8. Use fewer overlapping foliage cards.

## Texture Import Tips

- Smaller source textures usually fit the style better.
- Try point filtering on textures that are meant to stay crisp.
- Keep alpha textures clean for transparent card materials.
- Use `Alpha Cutoff` and `Shadow Cutoff` to tighten depth and shadow silhouettes when soft alpha edges look too broad.
- Use `Mip Bias` if mip levels make surfaces too blurry or too noisy.
- Use normal maps lightly on crunchy PSX materials. Too much surface detail can fight the low-resolution look.

## Troubleshooting

### Materials Turn Pink

Check:

1. The active pipeline is URP.
2. Shader files exist under `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders`.
3. The package root has been reimported.
4. The toolkit `Diagnostics` tab reports the shaders as available.
5. `PSXPS2_GlobalPass.mat` uses `Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Global Pass` if only the fullscreen pass is failing.

### Fullscreen Effect Does Not Appear

Check:

1. Open the toolkit `Renderer` tab.
2. Confirm your active URP renderer asset appears in the list.
3. Press `Install Feature`.
4. Select the renderer asset and confirm `PSX PS2 Retro Pass` is present.
5. Confirm `Effect Enabled` is on.
6. Confirm `Intensity` is greater than zero.
7. Make sure the camera uses the renderer that has the feature.

### Foliage Appears As Squares

Use `Cutout Lit`, `Foliage Lit`, or `Unlit Cutout`. Assign a base texture with alpha. The visible pass blends transparency; use `Alpha Cutoff` for depth silhouettes.

If the object still appears square, check that the imported texture actually has alpha and that the material uses the correct texture.

### Shadows Are Square

Use a cutout shader so the `ShadowCaster` pass clips alpha. Then tune `Shadow Cutoff`.

### Transparent Sorting Looks Wrong

Cutout, foliage, and unlit cutout now use transparent blending in the visible pass. If sorting looks wrong, reduce overlapping cards, split dense card clusters, or adjust `Queue Offset` on specific materials.

### Too Much Geometry Crawling

Lower:

- `Vertex Snap Strength`
- `Vertex Wobble Strength`
- Fullscreen `Pixel Scale`
- `Affine Warp Strength`

Raise:

- `Vertex Snap Resolution`
- `Posterize Steps`

For important hero characters, use `Clean PS2` or a custom material with vertex snap disabled.

If faces separate at shared edges, lower `Vertex Snap Strength` first. If the mesh still cracks, raise `Snap Seam Reduction`. For problem meshes, use `Tools > Borzblade > Retro Render Toolkit > Bake Snap Anchors For Selection`, then enable `Use Baked Snap Anchors` on the material. The baker creates duplicate mesh assets under `Assets/Borzblade/PSX/RetroRenderToolkit/Generated/SnapAnchors/`.

### Scene Is Too Dark

Lower:

- Renderer `Vignette Strength`
- Renderer `Contrast`
- Material `Shadow/Light Bands`
- Material `Band Strength`

Raise scene lighting or material `Base Color` if the scene is still too dark.

### Foliage Shimmers Or Moves Too Far

Lower:

- `Wind Strength`
- `Wind Speed`
- `Vertex Snap Strength`
- `Vertex Wobble Strength`
- `Dither Strength`

Also check that the texture alpha edge is clean.

`Jiggle Amplitude` (`_WindDistance`) is the hard maximum local flutter amount. If changing it has no visible effect, check that `Wind Enabled` is on, `Wind Strength` is above zero, and the UV height or vertex color alpha wind mask is not keeping the card anchored. The foliage shader is designed to keep the card anchored and jiggle leaves in place.

### Depth Fog Does Not Appear

Check that `PSXPS2DepthFogRendererFeature` is installed on the renderer used by the active camera. The pass needs scene depth. If the fog looks too smooth, raise `Steps` or `Dither Strength`. If the sky becomes fogged, turn off `Affect Sky`.

### Textures Look Too Smooth

PSX-style materials usually need point-filtered textures. The material inspectors warn when an assigned texture is not using Point filtering and include a button to update the texture importer.

## Build And Source Control Notes

- Keep materials that reference toolkit shaders in your project so Unity includes the shaders in builds.
- Keep `PSXPS2_GlobalPass.mat` if you use the fullscreen renderer feature.
- Commit Unity `.meta` files for documentation and material assets.
- Keep the compatibility wrapper `PSXPS2RetroRendererFeature` unless you also migrate existing renderer feature sub-assets.
- Runtime code uses the namespace `Borzblade.RetroRenderToolkit`.
- Editor code uses the namespace `Borzblade.RetroRenderToolkit.Editor`.

## File Reference

Important files:

- `Assets/Borzblade/PSX/RetroRenderToolkit/Runtime/Rendering/PSXPS2RetroRendererFeature.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Runtime/Rendering/PSXPS2DepthFogRendererFeature.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Runtime/Rendering/PSXPS2ScreenOutlineRendererFeature.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Fullscreen/PSXPS2GlobalPass.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Fullscreen/PSXPS2DepthFog.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Hybrid/PSXPS2HybridLit.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Terrain/PSXPS2TerrainLit.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Terrain/PSXPS2TerrainAddPass.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Cutout/PSXPS2CutoutLit.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Foliage/PSXPS2FoliageLit.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Unlit/PSXPS2UnlitCutout.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Water/PSXPS2Water.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Sprites/PSXPS2SpriteLit.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Sprites/PSXPS2SpriteUnlit.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Outline/PSXPS2ScreenOutline.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Outline/PSXPS2MaterialOutline.shader`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Core/PSXPS2RetroCommon.hlsl`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Editor/Windows/RetroRenderToolkitWindow.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Editor/Tools/PSXPS2RetroInstaller.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Editor/Tools/RetroMaterialConverter.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Editor/Tools/RetroRenderToolkitMaterialFactory.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Editor/Tools/RetroMaterialPreset.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Editor/Tools/RetroSnapAnchorBaker.cs`
- `Assets/Borzblade/PSX/RetroRenderToolkit/Editor/ShaderGUI/PSXPS2SpriteShaderGUI.cs`

## Final Checklist

Before shipping or testing a scene:

1. URP is active.
2. The renderer feature is installed on the renderer used by the camera.
3. `PSXPS2_GlobalPass.mat` exists.
4. Default or custom toolkit materials exist.
5. Main opaque meshes use `Hybrid Lit`.
6. Terrain uses `Terrain Lit` as the material/template.
7. Leaves, grass, hair cards, fences, and grates use cutout or foliage shaders.
8. Alpha-card shadows have been checked.
9. Water planes use the water shader and have been checked for transparency sorting.
10. SpriteRenderers use the sprite shaders when sprite tint, vertex color, atlas behavior, or sorting layers matter.
11. Screen or material outlines are only enabled where they improve readability.
12. The renderer preset has been tuned for the scene, including fixed vertical resolution if used.
13. Depth fog is used for scene fog, and material fog is only enabled where a specific shader needs local stylized fog.
14. Strong vertex snap has been checked for edge gaps, with `Snap Seam Reduction` or baked snap anchors used only where needed.
15. Foliage `Jiggle Amplitude` is tuned near the camera and does not slide cards sideways.
16. Mobile builds use the mobile presets or reduced dither, scanline, bleed, outline, and depth-fade settings.
