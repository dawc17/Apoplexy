# Quick Start

1. Open `Tools > Borzblade > Retro Render Toolkit > Open Toolkit`.
2. Press `Install Retro Renderer Feature`.
3. Press `Create Default Materials`.
4. Optional: press `Install Depth Fog Feature` if you want scene fog based on camera depth.
5. Optional: press `Install Screen Outline Feature` if you want screen-space outlines.
6. Assign `PSXPS2_CleanPS2`, `PSXPS2_HybridHigh`, cutout/foliage materials, sprite materials, water materials, or outline materials to scene objects.
7. For Terrain, create/select `PSXPS2_TerrainHybrid` and assign it to the Terrain material/template slot.
8. Tune the fullscreen pass, depth fog, and outlines from the Renderer tab.
9. Optional: save your own material looks from `Presets > My Presets`.
10. Optional: bake snap anchors from `Tools > Borzblade > Retro Render Toolkit > Bake Snap Anchors For Selection` for meshes that crack with heavy vertex snap.

## Recommended Presets

Clean PS2: crisp, subtle pixel treatment, light posterization, low dither.

Crunchy PSX: stronger pixel grid, lower color steps, visible dither, more aggressive material crunch.

Horror PSX: dark vignette, lower saturation, stronger contrast, heavier dither.

VHS / CRT: stronger analog pass with scanlines, mask, noise, chromatic offset, and slight curvature.

Mobile Fast: low-cost material settings, minimal fullscreen work, no heavy dither.

## New Controls Worth Checking

- `Snap Seam Reduction`: reduces gaps from strong clip-space vertex snap.
- `Snap Space`: switches vertex snap between screen-space and view/world-style snapping.
- `Affine Strength`: blends from normal perspective UVs to full affine UVs.
- `Affine Mode`: keeps the stable Borzblade warp by default, or uses a stronger classic PSX-style affine mode.
- `Lighting Model`: switches Hybrid, Cutout, Foliage, and Sprite Lit materials between Standard URP, Vertex Lit, Flat Lit, and Unlit.
- `Vertex Draw Distance`: optional material-level draw cutoff with dither fade. It is off by default.
- `Pixelation Mode`: switches the fullscreen pass between pixel scale and fixed vertical resolution.
- `Dither Pattern`: uses the built-in procedural pattern or your own dither texture.
- `Jiggle Amplitude`: maximum local foliage flutter. It replaces broad sideways foliage travel.
- `Retro Fog`: per-material fog for Hybrid, Cutout, Foliage, Unlit Cutout, Terrain, and Water.
- `Depth Fog`: proper fullscreen fog from camera depth. Use this for main scene fog.
