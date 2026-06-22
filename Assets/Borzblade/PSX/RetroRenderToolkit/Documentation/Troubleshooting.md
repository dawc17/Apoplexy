# Troubleshooting

## Pink Materials

Make sure the project is using URP and the shaders exist under `Assets/Borzblade/PSX/RetroRenderToolkit/Shaders`. Reimport the package root if Unity imported scripts before shaders.

## Renderer Feature Not Visible

Open the toolkit window and press `Install Retro Renderer Feature`. If editing fails, select the renderer asset from the Renderer tab and inspect its renderer feature list manually.

## Foliage Appears As Squares

Use Cutout Lit, Foliage Lit, or Unlit Cutout. Assign an alpha texture to `_BaseMap`. The visible pass blends transparency; `_Cutoff` is mainly for depth and shadow silhouettes.

## Cutout Or Foliage Disappears

Lower `Dither Fade Amount`, check `Distance Fade` and `Camera Fade`, and make sure the texture alpha is not mostly gray. Full-opacity pixels should stay visible unless the material is intentionally faded out.

## Shadows Are Square

Use the cutout or foliage shaders so the ShadowCaster pass clips by alpha. Tune `Shadow Cutoff` separately if the shadow is too thick or too thin.

## Transparent Sorting Issues

Cutout, foliage, and unlit cutout now use transparent blending for the visible pass. If sorting looks wrong, reduce overlapping cards, split dense card clusters, or adjust `Queue Offset` on specific materials.

## Too Much Wobble

Lower Vertex Wobble Strength or turn it off. For foliage, leave wind on and keep vertex snap/wobble subtle. If vertex snap looks too harsh, raise Vertex Snap Resolution, lower Vertex Snap Strength, or try the other `Snap Space` mode.

## Foliage Jiggle Moves Too Little Or Too Far

Adjust `Jiggle Amplitude` (`_WindDistance`) for the maximum local flutter amount, then use `Wind Strength` for how fully the shader uses that amount. Use `UV Height Mask` or vertex alpha masking to keep card bases stable. The shader is meant to jiggle leaves in place, not sweep the whole card sideways.

## Depth Fog Missing Or Too Smooth

Install `PSXPS2DepthFogRendererFeature` from the toolkit. The active camera must use the URP renderer that has the feature, and the renderer must provide a depth texture. Increase `Steps` and `Dither Strength` for a more retro fog edge; lower them for smoother fog.

## Screen Outline Missing

Install `PSXPS2ScreenOutlineRendererFeature` from the toolkit. The feature needs camera depth and normals; the installer requests both through URP, but the active camera still needs to use the renderer that has the feature.

## Water Looks Flat

Use the water shader on a simple plane, enable camera depth texture support for depth fade, and assign a noise texture if you want more wave and foam variation. Use the Mobile preset when depth fade is not needed.

## Sprite Tint Or Atlas Textures Look Wrong

Use `PSX PS2 Sprite Lit` or `PSX PS2 Sprite Unlit` on SpriteRenderer objects. These shaders read the SpriteRenderer color, sprite vertex color, `_MainTex`, and normal Unity sprite sorting settings.

## Fixed Resolution Pixelation Looks Too Strong

Switch `Pixelation Mode` back to Scale, or raise `Fixed Vertical Resolution`. For custom dither textures, use point filtering and tune `Dither Pattern Scale` and `Dither Threshold` before raising overall dither strength.

## Scene Too Dark

Raise renderer saturation/contrast carefully, reduce vignette, lower material light banding, or use Clean PS2 material presets.

## Mobile Performance

Use Mobile Fast renderer and material presets. Prefer Unlit Cutout for distant cards and keep fullscreen pixelation, dither, scanline, bleed, outline, and water depth fade low.
