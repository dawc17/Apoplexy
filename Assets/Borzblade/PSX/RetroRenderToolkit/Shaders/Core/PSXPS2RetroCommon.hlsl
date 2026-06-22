#ifndef BORZBLADE_RETRO_RENDER_TOOLKIT_COMMON_INCLUDED
#define BORZBLADE_RETRO_RENDER_TOOLKIT_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

half BorzRetroBayer4(float2 pixel)
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

half BorzRetroSmoothStrength(half strength)
{
    strength = saturate(strength);
    return strength * strength * (3.0h - 2.0h * strength);
}

float2 BorzRetroPixelateUV(float2 uv, half enabledStrength, half resolution, half aspect)
{
    float2 grid = max(float2(resolution, resolution * aspect), 1.0);
    float2 pixelated = (floor(uv * grid) + 0.5) / grid;
    return lerp(uv, pixelated, BorzRetroSmoothStrength(enabledStrength));
}

float2 BorzRetroAffineWarpUV(float2 uv, half3 normalWS, half3 viewDirWS, float4 positionCS, half strength)
{
    half warpStrength = BorzRetroSmoothStrength(strength);
    if (warpStrength <= 0.0001h)
    {
        return uv;
    }

    half grazing = 1.0h - saturate(abs(dot(normalize(normalWS), normalize(viewDirWS))));
    float w = positionCS.w;
    float safeW = w < 0.0 ? -max(abs(w), 0.00001) : max(abs(w), 0.00001);
    float2 ndc = positionCS.xy / safeW;
    float depthWeight = saturate(abs(w) * 0.035);
    float2 skew = ndc * (0.006 + depthWeight * 0.018) * grazing * warpStrength;
    float2 stableGrid = max(float2(96.0, 96.0 * (_ScreenParams.y / max(_ScreenParams.x, 1.0))), 1.0);
    float2 warped = (floor((uv + skew) * stableGrid) + 0.5) / stableGrid;
    return lerp(uv, warped, grazing * warpStrength);
}

float2 BorzRetroApplyAffineMode(float2 perspectiveUV, float2 affineUvTimesW, float affineClipW, half strength, half mode, half3 normalWS, half3 viewDirWS, float4 positionCS)
{
    half affineStrength = BorzRetroSmoothStrength(strength);
    if (affineStrength <= 0.0001h)
    {
        return perspectiveUV;
    }

    if (mode > 0.5h)
    {
        float safeW = affineClipW < 0.0 ? -max(abs(affineClipW), 0.00001) : max(abs(affineClipW), 0.00001);
        float2 affineUV = affineUvTimesW / safeW;
        return lerp(perspectiveUV, affineUV, saturate(affineStrength));
    }

    return BorzRetroAffineWarpUV(perspectiveUV, normalWS, viewDirWS, positionCS, affineStrength);
}

void BorzRetroEncodeAffineUV(float2 uv, float4 positionCS, out float2 uvTimesW, out float clipW)
{
    float w = positionCS.w;
    clipW = w < 0.0 ? -max(abs(w), 0.00001) : max(abs(w), 0.00001);
    uvTimesW = uv * clipW;
}

float2 BorzRetroDecodeAffineUV(float2 perspectiveUV, float2 affineUvTimesW, float affineClipW, half strength)
{
    half affineStrength = BorzRetroSmoothStrength(strength);
    if (affineStrength <= 0.0001h)
    {
        return perspectiveUV;
    }

    float safeW = affineClipW < 0.0 ? -max(abs(affineClipW), 0.00001) : max(abs(affineClipW), 0.00001);
    float2 affineUV = affineUvTimesW / safeW;
    return lerp(perspectiveUV, affineUV, saturate(affineStrength));
}

void BorzRetroClipVertexDrawDistance(float3 positionWS, half enabled, half drawDistance, half fadeDistance, float4 positionCS, half ditherScale)
{
    if (enabled <= 0.5h || drawDistance <= 0.0001h)
    {
        return;
    }

    float distanceToCamera = distance(GetCameraPositionWS(), positionWS);
    half fadeWidth = max(fadeDistance, 0.0h);
    if (fadeWidth <= 0.0001h)
    {
        clip(drawDistance - distanceToCamera);
        return;
    }

    half fade = saturate((distanceToCamera - drawDistance) / max(fadeWidth, 0.0001h));
    half dither = BorzRetroBayer4(positionCS.xy * max(ditherScale, 0.001h));
    clip(dither - fade);
}

float3 BorzRetroApplyVertexWobbleOS(float3 positionOS, float3 normalOS, half strength, half speed, half scale)
{
    half wobbleStrength = BorzRetroSmoothStrength(strength);
    if (wobbleStrength <= 0.0001h)
    {
        return positionOS;
    }

    float safeScale = max(scale, 0.001h);
    float steppedTime = floor(_Time.y * 12.0) / 12.0;
    float3 quantizedPosition = floor(positionOS * safeScale * 4.0 + 0.5) * 0.25;
    float phase = dot(quantizedPosition, float3(0.73, 1.37, 2.11)) + steppedTime * speed;
    float seed = frac(sin(dot(positionOS.xz, float2(12.9898, 78.233))) * 43758.5453);
    float wobble = sin(phase + seed * 6.2831853) * 0.65 + sin(phase * 1.73 + 1.7) * 0.35;
    wobble = floor(wobble * 6.0 + 0.5) / 6.0;

    float3 normalDir = normalize(normalOS + float3(0.0001, 0.0001, 0.0001));
    float3 sideDir = normalize(float3(seed - 0.5, 0.0, frac(seed * 7.13) - 0.5) + normalDir * 0.35);
    float3 wobbleDir = normalize(lerp(normalDir, sideDir, 0.35));
    return positionOS + wobbleDir * wobble * wobbleStrength * 0.025;
}

float2 BorzRetroSnapNdc(float2 ndc, half resolution)
{
    float2 grid = max(float2(resolution * (_ScreenParams.x / max(_ScreenParams.y, 1.0)), resolution), 1.0);
    return (floor(ndc * grid) + 0.5) / grid;
}

half BorzRetroVertexSnapStrength(VertexPositionInputs vertexInput, half strength, half distanceFadeAmount, half fadeStart, half fadeEnd)
{
    half baseStrength = BorzRetroSmoothStrength(strength);
    if (baseStrength <= 0.0001h)
    {
        return 0.0h;
    }

    float distanceToCamera = distance(GetCameraPositionWS(), vertexInput.positionWS);
    float fadeRange = max(fadeEnd - fadeStart, 0.001);
    float distanceFade = 1.0 - saturate((distanceToCamera - fadeStart) / fadeRange);
    float snapStrength = baseStrength * lerp(1.0, distanceFade, saturate(distanceFadeAmount));
    snapStrength *= lerp(0.65, 1.0, saturate(abs(vertexInput.positionCS.w) * 0.08));
    snapStrength *= lerp(1.0h, 0.82h, smoothstep(0.72h, 1.0h, baseStrength));
    return saturate(snapStrength);
}

VertexPositionInputs BorzRetroApplyScreenVertexSnap(VertexPositionInputs vertexInput, half snapStrength, half resolution, half seamReduction)
{
    if (snapStrength <= 0.0001h)
    {
        return vertexInput;
    }

    float w = vertexInput.positionCS.w;
    float safeW = w < 0.0 ? -max(abs(w), 0.00001) : max(abs(w), 0.00001);
    float2 ndc = vertexInput.positionCS.xy / safeW;
    float2 snappedNdc = BorzRetroSnapNdc(ndc, clamp(resolution, 16.0h, 2048.0h));
    snappedNdc = lerp(snappedNdc, ndc, saturate(seamReduction) * 0.45);
    vertexInput.positionCS.xy = lerp(vertexInput.positionCS.xy, snappedNdc * safeW, saturate(snapStrength));
    return vertexInput;
}

VertexPositionInputs BorzRetroApplyViewVertexSnap(VertexPositionInputs vertexInput, half snapStrength, half resolution)
{
    if (snapStrength <= 0.0001h)
    {
        return vertexInput;
    }

    float3 positionVS = TransformWorldToView(vertexInput.positionWS);
    float grid = max(clamp(resolution, 16.0h, 2048.0h) * 0.08, 1.0);
    float3 snappedVS = (floor(positionVS * grid) + 0.5) / grid;
    float3 stableVS = lerp(positionVS, snappedVS, saturate(snapStrength));
    vertexInput.positionCS = mul(UNITY_MATRIX_P, float4(stableVS, 1.0));
    return vertexInput;
}

VertexPositionInputs BorzRetroApplyVertexSnap(VertexPositionInputs vertexInput, half strength, half resolution, half distanceFadeAmount, half fadeStart, half fadeEnd, half seamReduction, half snapSpace)
{
    half snapStrength = BorzRetroVertexSnapStrength(vertexInput, strength, distanceFadeAmount, fadeStart, fadeEnd);
    if (snapSpace > 0.5h)
    {
        return BorzRetroApplyViewVertexSnap(vertexInput, snapStrength, resolution);
    }

    return BorzRetroApplyScreenVertexSnap(vertexInput, snapStrength, resolution, seamReduction);
}

VertexPositionInputs BorzRetroApplyVertexSnap(VertexPositionInputs vertexInput, half strength, half resolution, half distanceFadeAmount, half fadeStart, half fadeEnd, half seamReduction)
{
    return BorzRetroApplyVertexSnap(vertexInput, strength, resolution, distanceFadeAmount, fadeStart, fadeEnd, seamReduction, 0.0h);
}

VertexPositionInputs BorzRetroApplyVertexSnap(VertexPositionInputs vertexInput, half strength, half resolution, half distanceFadeAmount, half fadeStart, half fadeEnd)
{
    return BorzRetroApplyVertexSnap(vertexInput, strength, resolution, distanceFadeAmount, fadeStart, fadeEnd, 0.0h);
}

VertexPositionInputs BorzRetroApplyVertexSnapAnchored(VertexPositionInputs vertexInput, float4 anchorPositionCS, half strength, half resolution, half distanceFadeAmount, half fadeStart, half fadeEnd, half seamReduction, half snapSpace)
{
    if (snapSpace > 0.5h)
    {
        return BorzRetroApplyVertexSnap(vertexInput, strength, resolution, distanceFadeAmount, fadeStart, fadeEnd, seamReduction, snapSpace);
    }

    half snapStrength = BorzRetroVertexSnapStrength(vertexInput, strength, distanceFadeAmount, fadeStart, fadeEnd);
    if (snapStrength <= 0.0001h)
    {
        return vertexInput;
    }

    float w = vertexInput.positionCS.w;
    float safeW = w < 0.0 ? -max(abs(w), 0.00001) : max(abs(w), 0.00001);
    float2 ndc = vertexInput.positionCS.xy / safeW;
    float2 snappedNdc = BorzRetroSnapNdc(ndc, clamp(resolution, 16.0h, 2048.0h));

    float anchorW = anchorPositionCS.w;
    float safeAnchorW = anchorW < 0.0 ? -max(abs(anchorW), 0.00001) : max(abs(anchorW), 0.00001);
    float2 anchorNdc = anchorPositionCS.xy / safeAnchorW;
    float2 anchorSnappedNdc = BorzRetroSnapNdc(anchorNdc, clamp(resolution, 16.0h, 2048.0h));

    half anchorBlend = saturate(seamReduction);
    snappedNdc = lerp(lerp(snappedNdc, ndc, anchorBlend * 0.35), anchorSnappedNdc, anchorBlend * 0.65);
    vertexInput.positionCS.xy = lerp(vertexInput.positionCS.xy, snappedNdc * safeW, saturate(snapStrength));
    return vertexInput;
}

VertexPositionInputs BorzRetroApplyVertexSnapAnchored(VertexPositionInputs vertexInput, float4 anchorPositionCS, half strength, half resolution, half distanceFadeAmount, half fadeStart, half fadeEnd, half seamReduction)
{
    return BorzRetroApplyVertexSnapAnchored(vertexInput, anchorPositionCS, strength, resolution, distanceFadeAmount, fadeStart, fadeEnd, seamReduction, 0.0h);
}

half BorzRetroCutoutFadeMask(float3 positionWS, half startDistance, half endDistance, half cameraDistance, half useDistanceFade, half useCameraFade)
{
    half fadeMask = 0.0h;

    if (useDistanceFade > 0.5h)
    {
        float distanceToCamera = distance(GetCameraPositionWS(), positionWS);
        fadeMask = max(fadeMask, saturate((distanceToCamera - startDistance) / max(endDistance - startDistance, 0.0001h)));
    }

    if (useCameraFade > 0.5h)
    {
        half cameraFade = 1.0h - saturate(distance(GetCameraPositionWS(), positionWS) / max(cameraDistance, 0.0001h));
        fadeMask = max(fadeMask, cameraFade);
    }

    return fadeMask;
}

half BorzRetroCutoutFadeAmount(float3 positionWS, half fadeAmount, half fadeStart, half fadeEnd, half cameraFadeDistance, half useDistanceFade, half useCameraFade)
{
    half hasFadeMode = saturate(useDistanceFade + useCameraFade);
    return saturate(fadeAmount) * hasFadeMode * BorzRetroCutoutFadeMask(positionWS, fadeStart, fadeEnd, cameraFadeDistance, useDistanceFade, useCameraFade);
}

void BorzRetroClipCutoutAlpha(half alpha, half cutoff, float3 positionWS, float4 positionCS, half ditherScale, half fadeAmount, half fadeStart, half fadeEnd, half cameraFadeDistance, half useDistanceFade, half useCameraFade)
{
    half clippedAlpha = saturate(alpha);
    half effectiveCutoff = saturate(cutoff);
    clip(clippedAlpha - effectiveCutoff);

    half fade = BorzRetroCutoutFadeAmount(positionWS, fadeAmount, fadeStart, fadeEnd, cameraFadeDistance, useDistanceFade, useCameraFade);
    if (fade > 0.0001h)
    {
        half dither = BorzRetroBayer4(positionCS.xy * max(ditherScale, 0.001h));
        clip(dither - fade);
    }
}

half3 BorzRetroPosterize(half3 color, half posterSteps, half paletteStrength, half paletteSteps, half dither, half ditherStrength)
{
    half safePaletteSteps = max(paletteSteps - 1.0h, 1.0h);
    half3 palette = floor(saturate(color) * safePaletteSteps + dither) / safePaletteSteps;
    color = lerp(color, palette, saturate(paletteStrength));

    half safePosterSteps = max(posterSteps - 1.0h, 1.0h);
    return floor(saturate(color) * safePosterSteps + dither * ditherStrength) / safePosterSteps;
}

half3 BorzRetroApplyLightBands(half3 color, half bands, half strength)
{
    half roundedBands = floor(bands + 0.5h);
    if (roundedBands > 1.0h && strength > 0.0h)
    {
        half lum = max(Luminance(color), 0.0001h);
        half bandedLum = floor(lum * roundedBands + 0.5h) / roundedBands;
        color *= lerp(1.0h, bandedLum / lum, saturate(strength));
    }

    return color;
}

half BorzRetroFogFactor(float3 positionWS, half startDistance, half endDistance, half density, half steps, half blendMode)
{
    float distanceToCamera = distance(GetCameraPositionWS(), positionWS);
    half linearFog = saturate((distanceToCamera - startDistance) / max(endDistance - startDistance, 0.0001h));
    half exponentialFog = 1.0h - exp2(-max(distanceToCamera - startDistance, 0.0) * max(density, 0.0001h) * 1.442695h);
    half fog = blendMode < 0.5h ? linearFog : exponentialFog;

    half roundedSteps = floor(steps + 0.5h);
    if (roundedSteps > 1.0h)
    {
        fog = floor(fog * roundedSteps + 0.5h) / roundedSteps;
    }

    return saturate(fog);
}

half3 BorzRetroApplyFog(half3 color, half fogFactor, half4 fogColor)
{
    return lerp(color, fogColor.rgb, saturate(fogFactor) * fogColor.a);
}

half3 BorzRetroMainLightVertexSpecular(float3 positionWS, half3 normalWS, half specularPower, half specularIntensity, half3 specularColor)
{
    Light mainLight = GetMainLight();
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
    half3 halfDir = SafeNormalize(mainLight.direction + viewDirWS);
    half spec = pow(saturate(dot(NormalizeNormalPerVertex(normalWS), halfDir)), max(specularPower, 1.0h));
    return spec * mainLight.color * specularColor * specularIntensity;
}

half3 BorzRetroVertexLighting(float3 positionWS, half3 normalWS)
{
    half3 normal = NormalizeNormalPerVertex(normalWS);
    Light mainLight = GetMainLight();
    half3 lighting = SampleSH(normal);
    lighting += mainLight.color * saturate(dot(normal, mainLight.direction)) * mainLight.distanceAttenuation * mainLight.shadowAttenuation;

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
        lighting += VertexLighting(positionWS, normal);
    #endif

    return max(lighting, half3(0.0h, 0.0h, 0.0h));
}

half3 BorzRetroFlatNormalWS(float3 positionWS, half3 fallbackNormalWS)
{
    float3 dx = ddx(positionWS);
    float3 dy = ddy(positionWS);
    half3 flatNormal = NormalizeNormalPerPixel(cross(dy, dx));
    return dot(flatNormal, flatNormal) > 0.0001h ? flatNormal : NormalizeNormalPerPixel(fallbackNormalWS);
}

half4 BorzRetroApplyLightingModel(half lightingModel, SurfaceData surfaceData, inout InputData inputData, half3 vertexLighting, half3 vertexSpecular)
{
    if (lightingModel > 2.5h)
    {
        return half4(surfaceData.albedo + surfaceData.emission, surfaceData.alpha);
    }

    if (lightingModel > 1.5h)
    {
        inputData.normalWS = BorzRetroFlatNormalWS(inputData.positionWS, inputData.normalWS);
        return UniversalFragmentBlinnPhong(inputData, surfaceData);
    }

    if (lightingModel > 0.5h)
    {
        half3 color = surfaceData.albedo * max(vertexLighting, half3(0.02h, 0.02h, 0.02h)) + surfaceData.emission + vertexSpecular;
        return half4(color, surfaceData.alpha);
    }

    return UniversalFragmentBlinnPhong(inputData, surfaceData);
}

half BorzRetroDistanceFade(float3 positionWS, half startDistance, half endDistance)
{
    float distanceToCamera = distance(GetCameraPositionWS(), positionWS);
    return saturate((distanceToCamera - startDistance) / max(endDistance - startDistance, 0.0001h));
}

half BorzRetroHash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float3 BorzRetroWindOffsetOS(float3 positionOS, float3 normalOS, float4 tangentOS, float2 uv, half vertexMask, half strength, half distance, half speed, half scale, float4 direction, half uvHeightMask)
{
    half heightMask = lerp(1.0h, saturate((uv.y - 0.04h) / 0.96h), saturate(uvHeightMask));
    heightMask *= heightMask;
    half mask = saturate(vertexMask) * heightMask;
    if (mask <= 0.0001h || strength <= 0.0001h || distance <= 0.0001h)
    {
        return 0.0;
    }

    float safeScale = max(scale, 0.001h);
    float steppedTime = floor(_Time.y * 18.0) / 18.0;
    float seed = BorzRetroHash12(floor(positionOS.xz * safeScale * 7.0 + positionOS.yy * 3.0));
    float phase = dot(positionOS, float3(1.37, 2.11, 0.83)) * safeScale + steppedTime * speed + seed * 6.2831853;

    float flutterA = sin(phase * 2.3 + seed * 4.7);
    float flutterB = sin(phase * 3.9 + direction.x * 1.3 + direction.y * 2.1);
    flutterA = floor(flutterA * 7.0 + 0.5) / 7.0;
    flutterB = floor(flutterB * 7.0 + 0.5) / 7.0;

    float3 normalDir = normalize(normalOS + float3(0.0001, 0.0001, 0.0001));
    float3 fallbackAxis = abs(normalDir.y) < 0.99 ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
    float3 fallbackTangent = normalize(cross(fallbackAxis, normalDir));
    float3 tangentDir = length(tangentOS.xyz) > 0.0001 ? normalize(tangentOS.xyz) : fallbackTangent;
    float3 bitangentDir = normalize(cross(normalDir, tangentDir) * (tangentOS.w < 0.0 ? -1.0 : 1.0));
    float2 localDirection = normalize(direction.xy + float2(0.0001, 0.0));
    float3 localFlutterDir = normalize(tangentDir * localDirection.x + bitangentDir * localDirection.y + normalDir * 0.25);

    float maxAmplitude = max(distance, 0.0h) * saturate(strength) * mask;
    float3 offsetOS = (localFlutterDir * flutterA * 0.7 + normalDir * abs(flutterB) * 0.35 + bitangentDir * (flutterA - flutterB) * 0.18) * maxAmplitude;
    float magnitude = length(offsetOS);
    return magnitude > maxAmplitude ? offsetOS * (maxAmplitude / max(magnitude, 0.00001)) : offsetOS;
}

#endif
