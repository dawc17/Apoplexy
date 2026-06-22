#ifndef BORZBLADE_RETRO_RENDER_TOOLKIT_TERRAIN_RETRO_MACROS_INCLUDED
#define BORZBLADE_RETRO_RENDER_TOOLKIT_TERRAIN_RETRO_MACROS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Core/PSXPS2RetroCommon.hlsl"

float3 RetroTerrainApplyWobbleOS(float3 positionOS)
{
#if defined(_TERRAIN_RETRO_VERTEX_WOBBLE)
    float3 wobbled = BorzRetroApplyVertexWobbleOS(positionOS, float3(0.0, 1.0, 0.0), _TerrainRetroVertexWobbleStrength, _TerrainRetroVertexWobbleSpeed, _TerrainRetroVertexWobbleScale);
    positionOS.y = wobbled.y;
#endif

    return positionOS;
}

VertexPositionInputs RetroTerrainGetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs vertexInput = GetVertexPositionInputs(RetroTerrainApplyWobbleOS(positionOS));

#if defined(_TERRAIN_RETRO_VERTEX_SNAP)
    vertexInput = BorzRetroApplyVertexSnap(vertexInput, _TerrainRetroVertexSnapStrength, _TerrainRetroVertexSnapResolution, _TerrainRetroVertexSnapDistanceFade, _TerrainRetroVertexSnapFadeStart, _TerrainRetroVertexSnapFadeEnd, _TerrainRetroVertexSnapSeamReduction, _TerrainRetroVertexSnapSpace);
#endif

    return vertexInput;
}

float3 RetroTerrainTransformObjectToWorld(float3 positionOS)
{
    return TransformObjectToWorld(RetroTerrainApplyWobbleOS(positionOS));
}

float4 RetroTerrainTransformObjectToHClip(float3 positionOS)
{
    return RetroTerrainGetVertexPositionInputs(positionOS).positionCS;
}

float2 RetroTerrainModifySplatUV(float2 uv)
{
#if defined(_TERRAIN_RETRO_UV_PIXEL)
    uv = BorzRetroPixelateUV(uv, _TerrainRetroUvPixelStrength, _TerrainRetroUvPixelResolution, _TerrainRetroUvPixelAspect);
#endif

    return uv;
}

half RetroTerrainBayer4(float2 pixel)
{
    float2 p = floor(fmod(abs(pixel), 4.0));
    half value = 0.0h;

    if (p.y < 0.5)
    {
        value = p.x < 0.5 ? 0.0h : (p.x < 1.5 ? 8.0h : (p.x < 2.5 ? 2.0h : 10.0h));
    }
    else if (p.y < 1.5)
    {
        value = p.x < 0.5 ? 12.0h : (p.x < 1.5 ? 4.0h : (p.x < 2.5 ? 14.0h : 6.0h));
    }
    else if (p.y < 2.5)
    {
        value = p.x < 0.5 ? 3.0h : (p.x < 1.5 ? 11.0h : (p.x < 2.5 ? 1.0h : 9.0h));
    }
    else
    {
        value = p.x < 0.5 ? 15.0h : (p.x < 1.5 ? 7.0h : (p.x < 2.5 ? 13.0h : 5.0h));
    }

    return (value + 0.5h) / 16.0h;
}

half3 RetroTerrainModifyAlbedo(half3 albedo)
{
#if defined(_TERRAIN_RETRO_POSTERIZE)
    half paletteSteps = max(_TerrainRetroPaletteSteps - 1.0h, 1.0h);
    half3 palette = floor(saturate(albedo) * paletteSteps + 0.5h) / paletteSteps;
    albedo = lerp(albedo, palette, saturate(_TerrainRetroPaletteStrength));
#endif

    return albedo;
}

half3 RetroTerrainApplyLitColorGrade(half3 color, float2 normalizedScreenSpaceUV)
{
#if defined(_TERRAIN_RETRO_DITHER)
    float2 ditherPixel = normalizedScreenSpaceUV * _ScreenParams.xy / max(_TerrainRetroDitherScale, 0.001);
    half dither = RetroTerrainBayer4(ditherPixel) - 0.5h;
    half divisor = max(_TerrainRetroPosterizeSteps, 2.0h);
    color += dither.xxx * (_TerrainRetroDitherStrength / divisor);
#endif

#if defined(_TERRAIN_RETRO_LIGHT_BANDS)
    half bands = max(_TerrainRetroLightBands, 1.0h);
    half luma = max(Luminance(color), 0.0001h);
    half bandedLuma = floor(luma * bands + 0.5h) / bands;
    color *= lerp(1.0h, bandedLuma / luma, saturate(_TerrainRetroBandStrength));
#endif

#if defined(_TERRAIN_RETRO_POSTERIZE)
    half posterizeSteps = max(_TerrainRetroPosterizeSteps - 1.0h, 1.0h);
    color = floor(saturate(color) * posterizeSteps + 0.5h) / posterizeSteps;
#endif

    return saturate(color);
}

void RetroTerrainInitializeBRDFData(half3 albedo, half metallic, half3 specular, half smoothness, half alpha, out BRDFData outBRDFData)
{
    InitializeBRDFData(RetroTerrainModifyAlbedo(albedo), metallic, specular, smoothness, alpha, outBRDFData);
}

half4 RetroTerrainUniversalFragmentPBR(
    InputData inputData,
    half3 albedo,
    half metallic,
    half3 specular,
    half smoothness,
    half occlusion,
    half3 emission,
    half alpha)
{
    half4 color = UniversalFragmentPBR(inputData, RetroTerrainModifyAlbedo(albedo), metallic, specular, smoothness, occlusion, emission, alpha);
    color.rgb = RetroTerrainApplyLitColorGrade(color.rgb, inputData.normalizedScreenSpaceUV);
#if defined(_TERRAIN_RETRO_FOG)
    half fogFactor = BorzRetroFogFactor(inputData.positionWS, _TerrainRetroFogStart, _TerrainRetroFogEnd, _TerrainRetroFogDensity, _TerrainRetroFogSteps, _TerrainRetroFogBlendMode);
    color.rgb = BorzRetroApplyFog(color.rgb, fogFactor, _TerrainRetroFogColor);
#endif
    return color;
}

#define GetVertexPositionInputs(positionOS) RetroTerrainGetVertexPositionInputs(positionOS)
#define TransformObjectToWorld(positionOS) RetroTerrainTransformObjectToWorld(positionOS)
#define TransformObjectToHClip(positionOS) RetroTerrainTransformObjectToHClip(positionOS)
#undef TRANSFORM_TEX
#define TRANSFORM_TEX(tex,name) RetroTerrainModifySplatUV((tex).xy * name##_ST.xy + name##_ST.zw)
#define InitializeBRDFData RetroTerrainInitializeBRDFData
#define UniversalFragmentPBR RetroTerrainUniversalFragmentPBR

#endif
