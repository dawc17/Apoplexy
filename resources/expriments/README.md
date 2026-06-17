# PSX shader experiment

This folder contains a raylib/GLSL conversion of the portable fullscreen pieces from `PSX.zip`.

The original package is a Unity URP shader toolkit. Unity-specific pieces such as renderer features, material inspectors, shader keywords, URP lighting includes, terrain passes, sprite passes, and editor tooling do not directly apply to this project. The files here are intentionally small experiment shaders that can be loaded by raylib like the existing shaders in `resources/shaders/`.

## Files

- `psx_global.vs`: fullscreen/post-process vertex shader with optional screen-space vertex snapping.
- `psx_global.fs`: fullscreen/post-process fragment shader with pixelation, Bayer dithering, color quantization, scanlines, vignette, saturation/contrast, tint, gamma, black level, color bleed, chromatic offset, noise, horizontal jitter, curvature, and simple fog tint.

## Expected uniforms

Raylib provides:

- `texture0`
- `colDiffuse`
- `mvp`

Set these from game code:

- `virtualResolution`: usually `{640, 320}` or the render target size.
- `screenSize`: current output size or render target size.
- `time`: `GetTime()`.
- `intensity`: `0.0` to `1.0`.
- `pixelScale`: `1.0` and up.
- `fixedVerticalResolution`: for example `240.0`.
- `useFixedVerticalResolution`: `0.0` or `1.0`.
- `colorSteps`: for example `28.0`.
- `ditherStrength`: for example `0.12`.
- `ditherScale`: for example `1.0`.
- `scanlineStrength`: for example `0.08`.
- `vignetteStrength`: for example `0.12`.
- `saturation`: for example `1.05`.
- `contrast`: for example `1.05`.
- `colorBleed`: for example `0.08`.
- `colorTint`: for example `{1.0, 1.0, 1.0}`.
- `gammaValue`: for example `1.0`.
- `blackLevel`: for example `0.0`.
- `chromaticOffset`: for example `0.0` to `0.35`.
- `noiseStrength`: for example `0.02`.
- `horizontalJitter`: for example `0.0` to `0.25`.
- `curvature`: for example `0.0` to `0.25`.
- `fogColor`: for example `{0.42, 0.46, 0.50}`.
- `fogAmount`: `0.0` to `1.0`.
- `vertexSnapStrength`: `0.0` to `1.0`.

## Integration note

This can replace the current `monoShader` experiment in `Game::draw()`:

1. Load `resources/expriments/psx_global.vs` and `resources/expriments/psx_global.fs`.
2. Set the uniforms above before drawing `sceneTarget.texture`.
3. Draw the render texture exactly like the current monochrome pass.

The typo in `expriments` matches the requested folder name.
