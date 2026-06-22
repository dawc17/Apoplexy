# Borzblade PSX/PS2 Retro Render Toolkit for URP

The Borzblade Retro Render Toolkit is a self-contained URP package for building a hybrid PSX/PS2 look in Unity. It includes lit mesh, terrain, cutout, foliage, unlit cutout, sprite, water, outline, depth fog, and fullscreen renderer effects under one package root:

`Assets/Borzblade/PSX/RetroRenderToolkit/`

## Quick Setup

1. Open `Tools > Borzblade > Retro Render Toolkit > Open Toolkit`.
2. On the Setup tab, press `Install Retro Renderer Feature`.
3. Press `Create Default Materials`.
4. Assign the created materials from `Materials/Hybrid`, `Materials/Terrain`, `Materials/Cutout`, `Materials/Foliage`, `Materials/UnlitCutout`, `Materials/Sprites`, `Materials/Water`, or `Materials/Outline`.
5. Optional: press `Install Depth Fog Feature` for camera depth-based scene fog.
6. Optional: press `Install Screen Outline Feature` for screen-space outlines.
7. Tune the fullscreen look from the Renderer tab.

## Shaders

- `Borzblade/Retro Render Toolkit/PSX PS2 Hybrid Lit`: main lit mesh shader with vertex snap, wobble, UV pixelation, affine warp, posterization, palette compression, ordered dithering, PS2 specular, rim light, emission, alpha clip, transparency, shadows, depth normals, and lightmapping.
- `Borzblade/Retro Render Toolkit/PSX PS2 Terrain Lit`: terrain shader that works with Unity TerrainLayers, height blend, holes, normals, masks, shadows, depth normals, and URP terrain add-pass fallback.
- `Borzblade/Retro Render Toolkit/PSX PS2 Cutout Lit`: transparent alpha-card lit shader for leaves, fences, hair cards, ropes, cloth tears, grates, branches, and similar card geometry.
- `Borzblade/Retro Render Toolkit/PSX PS2 Foliage Lit`: transparent foliage shader plus wind strength/distance controls, top/bottom gradient, world color variation, and foliage presets for grass, bushes, leaves, pine, dry plants, dark fantasy foliage, and mobile.
- `Borzblade/Retro Render Toolkit/PSX PS2 Unlit Cutout`: cheap transparent alpha-card shader for distant foliage, sprite-like props, background cards, and mobile.
- `Borzblade/Retro Render Toolkit/PSX PS2 Sprite Lit`: SpriteRenderer-compatible transparent shader with renderer tint, vertex color, atlas-friendly sampling, URP lighting, retro geometry, and retro color controls.
- `Borzblade/Retro Render Toolkit/PSX PS2 Sprite Unlit`: lower-cost SpriteRenderer-compatible shader for flat sprites, pickups, card props, simple effects, and mobile.
- `Borzblade/Retro Render Toolkit/PSX PS2 Water`: transparent retro water with stepped waves, depth fade, foam, fresnel tint, and retro color controls.
- `Borzblade/Retro Render Toolkit/PSX PS2 Material Outline`: inverted-hull outline material for selected meshes.
- `Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Depth Fog`: fullscreen depth-based fog pass with linear, exponential, stepped, and dithered fog options.

## Fullscreen Renderer Feature

The renderer feature is `PSXPS2RetroRendererFeature`. Existing renderer references are preserved through a compatibility wrapper, while new code lives in the `Borzblade.RetroRenderToolkit` namespace.

Controls include intensity, pixel scale, fixed vertical resolution, color steps, ordered dither, custom dither texture support, scanlines, vignette, saturation, contrast, bleed, tint, gamma, CRT mask, chromatic offset, noise, line jitter, and curvature. Renderer presets are available from the toolkit window: Clean PS2, Crunchy PSX, Horror PSX, Dark Fantasy, VHS / CRT, Mobile Fast, and Off/Neutral.

Use `PSXPS2DepthFogRendererFeature` for normal scene fog. It samples camera depth, runs before the final retro pass by default, and can keep the sky untouched unless `Affect Sky` is enabled.

Material `Retro Fog` is still available on supported shaders for stylized object fog. It is off by default and should be treated as a local art-control, not the main scene fog system.

The screen outline feature is `PSXPS2ScreenOutlineRendererFeature`. It uses depth and normals, runs before the final retro pass, and can be installed from the toolkit window.

## Lighting And Retro Geometry

Hybrid Lit, Cutout Lit, Foliage Lit, and Sprite Lit materials include a `Lighting Model` selector: Standard URP, Vertex Lit, Flat Lit, or Unlit. Standard URP is the default for existing mesh materials.

Shared retro geometry controls include screen-space or view/world vertex snap, stable or classic affine mapping, and optional vertex draw distance. Vertex draw distance is off by default so upgrades do not hide existing objects.

## Cutout And Foliage Best Practices

Cutout, foliage, and unlit cutout materials now render the visible pass as transparent alpha cards. `Alpha Cutoff` and `Shadow Cutoff` are still used by depth and shadow passes so cards keep shaped silhouettes. Keep vertex snap and wobble subtle on foliage cards because heavy movement makes leaf silhouettes shimmer.

For shadows, tune `Shadow Cutoff` separately from visual opacity. For sorting issues, reduce card overlap or use small queue offsets on specific materials.

Foliage wind uses anchored local jiggle. `Jiggle Amplitude` (`_WindDistance`) is the maximum local flutter amount, and `Wind Strength` is a 0-1 multiplier after UV height and vertex color alpha masks are applied. It should not push the whole card sideways across the scene.

## Presets And Snap Anchors

The Presets tab includes `My Presets` for saving texture-agnostic material presets under `Settings/UserPresets`. Presets store retro controls, lighting, wind, fog, and color modifier values, but not base textures or base color.

For meshes that show gaps with strong vertex snap, use `Tools > Borzblade > Retro Render Toolkit > Bake Snap Anchors For Selection`. This creates duplicate meshes with UV4 snap-anchor data under `Generated/SnapAnchors`.

## Terrain

Create the terrain material from the Terrain tab or `Tools > Borzblade > Retro Render Toolkit > Create Terrain Material`. Assign it to the Terrain component material/template slot. TerrainLayers remain in Unity's normal terrain painting system.

## Converter

The Converter tab can convert selected materials, or all materials in a selected folder, to Hybrid Lit, Cutout Lit, Foliage Lit, Unlit Cutout, Sprite Lit, Sprite Unlit, Water, or Material Outline. It can create copies, preserve common texture/color/normal/emission/cutoff/render queue values, apply presets, and back up overwritten materials.

## Troubleshooting

See `Troubleshooting.md` for pink materials, missing renderer features, square foliage, square shadows, sorting issues, excessive wobble, dark scenes, and mobile performance notes.
