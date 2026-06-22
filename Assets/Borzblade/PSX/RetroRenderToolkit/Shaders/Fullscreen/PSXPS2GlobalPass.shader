Shader "Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Global Pass"
{
    Properties
    {
        _Intensity("Intensity", Range(0, 1)) = 0.65
        _PixelScale("Pixel Scale", Range(1, 12)) = 1
        _PixelationMode("Pixelation Mode", Float) = 0
        _FixedVerticalResolution("Fixed Vertical Resolution", Range(64, 1080)) = 240
        _ColorSteps("Color Steps", Range(2, 64)) = 28
        _DitherStrength("Dither Strength", Range(0, 1)) = 0.12
        _DitherPatternMode("Dither Pattern Mode", Float) = 0
        _DitherPatternTexture("Dither Pattern Texture", 2D) = "gray" {}
        _DitherPatternScale("Dither Pattern Scale", Range(0.25, 16)) = 1
        _DitherThreshold("Dither Threshold", Range(0, 1)) = 0.5
        _ScanlineStrength("Scanline Strength", Range(0, 1)) = 0.08
        _VignetteStrength("Vignette Strength", Range(0, 1)) = 0.12
        _Saturation("Saturation", Range(0, 2)) = 1.05
        _Contrast("Contrast", Range(0, 2)) = 1.05
        _Bleed("Color Bleed", Range(0, 1)) = 0.08
        _ColorTint("Color Tint", Color) = (1, 1, 1, 1)
        _Gamma("Gamma", Range(0.2, 3)) = 1
        _BlackLevel("Black Level", Range(0, 1)) = 0
        _DitherScale("Dither Scale", Range(0.25, 8)) = 1
        _CrtMaskStrength("CRT Mask Strength", Range(0, 1)) = 0
        _ChromaticOffset("Chromatic Offset", Range(0, 1)) = 0
        _NoiseStrength("Noise Strength", Range(0, 1)) = 0
        _HorizontalJitter("Horizontal Jitter", Range(0, 1)) = 0
        _Curvature("Curvature", Range(0, 1)) = 0
        _GlobalFogEnabled("Global Fog Enabled", Float) = 0
        _GlobalFogColor("Global Fog Color", Color) = (0.42, 0.46, 0.50, 1)
        _GlobalFogIntensity("Global Fog Intensity", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Overlay"
        }

        Pass
        {
            Name "PSX PS2 Global Pass"

            ZTest Always
            ZWrite Off
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_DitherPatternTexture);
            SAMPLER(sampler_DitherPatternTexture);

            CBUFFER_START(UnityPerMaterial)
                half _Intensity;
                half _PixelScale;
                half _PixelationMode;
                half _FixedVerticalResolution;
                half _ColorSteps;
                half _DitherStrength;
                half _DitherPatternMode;
                half _DitherPatternScale;
                half _DitherThreshold;
                half _ScanlineStrength;
                half _VignetteStrength;
                half _Saturation;
                half _Contrast;
                half _Bleed;
                half4 _ColorTint;
                half _Gamma;
                half _BlackLevel;
                half _DitherScale;
                half _CrtMaskStrength;
                half _ChromaticOffset;
                half _NoiseStrength;
                half _HorizontalJitter;
                half _Curvature;
                half _GlobalFogEnabled;
                half4 _GlobalFogColor;
                half _GlobalFogIntensity;
            CBUFFER_END

            half Bayer4(float2 pixel)
            {
                uint x = (uint)pixel.x & 3u;
                uint y = (uint)pixel.y & 3u;
                uint index = x + y * 4u;
                half value = 0.0h;
                value = index == 0u ? 0.0h : value;
                value = index == 1u ? 8.0h : value;
                value = index == 2u ? 2.0h : value;
                value = index == 3u ? 10.0h : value;
                value = index == 4u ? 12.0h : value;
                value = index == 5u ? 4.0h : value;
                value = index == 6u ? 14.0h : value;
                value = index == 7u ? 6.0h : value;
                value = index == 8u ? 3.0h : value;
                value = index == 9u ? 11.0h : value;
                value = index == 10u ? 1.0h : value;
                value = index == 11u ? 9.0h : value;
                value = index == 12u ? 15.0h : value;
                value = index == 13u ? 7.0h : value;
                value = index == 14u ? 13.0h : value;
                value = index == 15u ? 5.0h : value;
                return (value + 0.5h) / 16.0h;
            }

            half DitherValue(float2 pixel, float2 uv)
            {
                half dither = Bayer4(pixel / max(_DitherScale, 0.001h));
                if (_DitherPatternMode > 0.5h)
                {
                    float2 patternUv = pixel / max(_DitherPatternScale * 64.0h, 1.0h);
                    dither = SAMPLE_TEXTURE2D(_DitherPatternTexture, sampler_DitherPatternTexture, patternUv).r;
                    dither = saturate(dither - _DitherThreshold + 0.5h);
                }

                return dither;
            }

            half3 AdjustSaturationContrast(half3 color)
            {
                half luminance = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                color = lerp(luminance.xxx, color, _Saturation);
                color = (color - 0.5h) * _Contrast + 0.5h;
                color = max(color - _BlackLevel, 0.0h) / max(1.0h - _BlackLevel, 0.0001h);
                color *= _ColorTint.rgb;
                color = pow(saturate(color), rcp(max(_Gamma, 0.001h)));
                return color;
            }

            half Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 ApplyCurvature(float2 uv)
            {
                if (_Curvature <= 0.0001h)
                {
                    return uv;
                }

                float2 centered = uv * 2.0 - 1.0;
                float radius = dot(centered, centered);
                centered *= 1.0 + radius * _Curvature * 0.12;
                return centered * 0.5 + 0.5;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = ApplyCurvature(input.texcoord);
                half pixelScale = max(_PixelScale, 1.0h);
                float2 screenSize = max(_ScreenParams.xy, 1.0);
                if (_PixelationMode > 0.5h)
                {
                    pixelScale = max(screenSize.y / max(_FixedVerticalResolution, 1.0h), 1.0h);
                }

                if (_HorizontalJitter > 0.0h)
                {
                    half lineNoise = Hash21(float2(floor(input.positionCS.y), floor(_Time.y * 24.0)));
                    uv.x += (lineNoise - 0.5h) * _HorizontalJitter * _BlitTexture_TexelSize.x * pixelScale * 4.0;
                }

                uv = saturate(uv);
                float2 pixelUv = (floor(uv * screenSize / pixelScale) + 0.5) * pixelScale / screenSize;

                // The raylib pipeline blended against an image that had already
                // been rendered at 640x320. Sampling the full-resolution UV here
                // leaks sharp Unity pixels back in whenever intensity is below 1.
                half4 original = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, pixelUv, _BlitMipLevel);
                half4 sampled = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, pixelUv, _BlitMipLevel);

                if (_ChromaticOffset > 0.0h)
                {
                    float2 chromaOffset = float2(_BlitTexture_TexelSize.x * pixelScale * _ChromaticOffset * 3.0, 0.0);
                    sampled.r = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, saturate(pixelUv + chromaOffset), _BlitMipLevel).r;
                    sampled.b = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, saturate(pixelUv - chromaOffset), _BlitMipLevel).b;
                }

                if (_Bleed > 0.0h)
                {
                    float2 bleedOffset = float2(_BlitTexture_TexelSize.x * pixelScale, 0.0);
                    half3 left = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, pixelUv - bleedOffset, _BlitMipLevel).rgb;
                    half3 right = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, pixelUv + bleedOffset, _BlitMipLevel).rgb;
                    sampled.rgb = lerp(sampled.rgb, half3(left.r, sampled.g, right.b), saturate(_Bleed));
                }

                half dither = DitherValue(input.positionCS.xy, pixelUv);
                half steps = max(_ColorSteps, 2.0h);
                sampled.rgb = AdjustSaturationContrast(sampled.rgb);
                sampled.rgb += (Hash21(input.positionCS.xy + floor(_Time.y * 30.0)) - 0.5h) * _NoiseStrength * 0.08h;
                sampled.rgb = lerp(sampled.rgb, _GlobalFogColor.rgb, saturate(_GlobalFogEnabled * _GlobalFogIntensity) * _GlobalFogColor.a);
                sampled.rgb += (dither - 0.5h) * _DitherStrength / steps;
                half quantizeLevels = max(steps - 1.0h, 1.0h);
                sampled.rgb = floor(saturate(sampled.rgb) * quantizeLevels + dither * _DitherStrength) / quantizeLevels;

                half scanline = sin(input.positionCS.y * 3.14159265h);
                sampled.rgb *= 1.0h - saturate((1.0h - scanline) * 0.5h * _ScanlineStrength);

                if (_CrtMaskStrength > 0.0h)
                {
                    half stripe = frac(input.positionCS.x / 3.0h);
                    half3 mask = stripe < 0.333h ? half3(1.08h, 0.82h, 0.82h) : (stripe < 0.666h ? half3(0.82h, 1.05h, 0.82h) : half3(0.82h, 0.82h, 1.08h));
                    sampled.rgb *= lerp(half3(1.0h, 1.0h, 1.0h), mask, _CrtMaskStrength);
                }

                float2 centered = input.texcoord * 2.0 - 1.0;
                half vignette = saturate(dot(centered, centered) * _VignetteStrength);
                sampled.rgb *= 1.0h - vignette;

                return half4(lerp(original.rgb, sampled.rgb, saturate(_Intensity)), original.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
