Shader "Borzblade/Retro Render Toolkit/PSX PS2 Water"
{
    Properties
    {
        [MainTexture] _BaseMap("Surface Map", 2D) = "white" {}
        [MainColor] _BaseColor("Surface Tint", Color) = (0.72, 0.90, 0.92, 0.55)
        _ShallowColor("Shallow Color", Color) = (0.26, 0.70, 0.72, 1)
        _DeepColor("Deep Color", Color) = (0.05, 0.20, 0.34, 1)
        _Alpha("Alpha", Range(0, 1)) = 0.58

        [NoScaleOffset] _NoiseMap("Wave / Foam Noise", 2D) = "gray" {}
        _WaveStrength("Vertex Wave Strength", Range(0, 1)) = 0.12
        _WaveSpeed("Wave Speed", Range(0, 8)) = 1.1
        _WaveScale("Wave Scale", Range(0.05, 12)) = 1.8
        _WaveSteps("Wave Steps", Range(2, 16)) = 6
        _WaveDirection("Wave Direction", Vector) = (1, 0.35, 0, 0)
        _NormalDistortion("Normal Distortion", Range(0, 1)) = 0.16

        [Toggle(_WATER_DEPTH_FADE)] _DepthFadeEnabled("Depth Fade", Float) = 1
        _DepthFadeDistance("Depth Fade Distance", Range(0.05, 12)) = 2.4
        _FoamColor("Foam Color", Color) = (0.88, 0.96, 0.88, 1)
        _FoamDistance("Foam Distance", Range(0.02, 4)) = 0.35
        _FoamStrength("Foam Strength", Range(0, 1)) = 0.45

        _FresnelColor("Fresnel Color", Color) = (0.75, 0.95, 1, 1)
        _FresnelIntensity("Fresnel Intensity", Range(0, 2)) = 0.35
        _FresnelPower("Fresnel Power", Range(0.25, 8)) = 2.5

        [Toggle(_RETRO_VERTEX_SNAP)] _VertexSnapEnabled("Vertex Snap Enabled", Float) = 0
        _VertexSnapStrength("Vertex Snap Strength", Range(0, 1)) = 0.12
        _VertexSnapResolution("Vertex Snap Resolution", Range(24, 1024)) = 240
        _VertexSnapDistanceFade("Vertex Snap Distance Fade", Range(0, 1)) = 0.45
        _VertexSnapFadeStart("Snap Fade Start", Float) = 5
        _VertexSnapFadeEnd("Snap Fade End", Float) = 45
        _VertexSnapSeamReduction("Snap Seam Reduction", Range(0, 1)) = 0
        [Enum(Screen,0,View World,1)] _VertexSnapSpace("Vertex Snap Space", Float) = 0
        [Toggle] _VertexDrawDistanceEnabled("Vertex Draw Distance Enabled", Float) = 0
        _VertexDrawDistance("Vertex Draw Distance", Float) = 0
        _VertexDrawDistanceFade("Vertex Draw Distance Fade", Range(0, 64)) = 0

        [Toggle(_RETRO_VERTEX_WOBBLE)] _VertexWobbleEnabled("Vertex Wobble Enabled", Float) = 0
        _VertexWobbleStrength("Vertex Wobble Strength", Range(0, 1)) = 0.03
        _VertexWobbleSpeed("Vertex Wobble Speed", Range(0, 12)) = 1.4
        _VertexWobbleScale("Vertex Wobble Scale", Range(0.1, 16)) = 3

        [Toggle(_RETRO_UV_PIXEL)] _UvPixelEnabled("UV Pixelation Enabled", Float) = 1
        _UvPixelStrength("UV Pixelation Strength", Range(0, 1)) = 0.28
        _UvPixelResolution("UV Pixelation Resolution", Range(16, 2048)) = 192
        _UvPixelAspect("UV Pixelation Aspect", Range(0.25, 4)) = 1
        _MipBias("Mip Bias", Range(-3, 3)) = 0

        [Toggle(_RETRO_AFFINE)] _AffineEnabled("Affine Warp Enabled", Float) = 0
        _AffineStrength("Affine Warp Strength", Range(0, 1)) = 0.14
        [Enum(Stable,0,Classic,1)] _AffineMode("Affine Mode", Float) = 0

        [Toggle(_RETRO_POSTERIZE)] _PosterizeEnabled("Posterize Enabled", Float) = 1
        _PosterizeSteps("Posterize Steps", Range(2, 64)) = 24
        _PaletteStrength("Palette Strength", Range(0, 1)) = 0.12
        _PaletteSteps("Palette Steps", Range(2, 64)) = 32

        [Toggle(_RETRO_DITHER)] _DitherEnabled("Dither Enabled", Float) = 1
        _DitherStrength("Dither Strength", Range(0, 1)) = 0.08
        _DitherScale("Dither Scale", Range(0.25, 8)) = 1

        [Toggle(_RETRO_FOG)] _RetroFogEnabled("Retro Fog Enabled", Float) = 0
        _RetroFogColor("Retro Fog Color", Color) = (0.42, 0.46, 0.50, 1)
        _RetroFogStart("Retro Fog Start", Float) = 18
        _RetroFogEnd("Retro Fog End", Float) = 70
        _RetroFogDensity("Retro Fog Density", Range(0.001, 1)) = 0.035
        _RetroFogSteps("Retro Fog Steps", Range(0, 16)) = 6
        [Enum(Linear,0,Exponential,1)] _RetroFogBlendMode("Retro Fog Blend Mode", Float) = 0

        _Cull("__cull", Float) = 2
        [HideInInspector] _SrcBlend("__src", Float) = 5
        [HideInInspector] _DstBlend("__dst", Float) = 10
        [HideInInspector] _ZWrite("__zw", Float) = 0
        _QueueOffset("Queue Offset", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 180

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex WaterVertex
            #pragma fragment WaterFragment
            #pragma shader_feature_local _WATER_DEPTH_FADE
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local _RETRO_VERTEX_WOBBLE
            #pragma shader_feature_local_fragment _RETRO_UV_PIXEL
            #pragma shader_feature_local_fragment _RETRO_AFFINE
            #pragma shader_feature_local_fragment _RETRO_POSTERIZE
            #pragma shader_feature_local_fragment _RETRO_DITHER
            #pragma shader_feature_local_fragment _RETRO_FOG
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Core/PSXPS2RetroCommon.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;
                half4 _FresnelColor;
                float4 _WaveDirection;
                half _Alpha;
                half _WaveStrength;
                half _WaveSpeed;
                half _WaveScale;
                half _WaveSteps;
                half _NormalDistortion;
                half _DepthFadeDistance;
                half _FoamDistance;
                half _FoamStrength;
                half _FresnelIntensity;
                half _FresnelPower;
                half _VertexSnapStrength;
                half _VertexSnapResolution;
                half _VertexSnapDistanceFade;
                half _VertexSnapFadeStart;
                half _VertexSnapFadeEnd;
                half _VertexSnapSeamReduction;
                half _VertexSnapSpace;
                half _VertexDrawDistanceEnabled;
                half _VertexDrawDistance;
                half _VertexDrawDistanceFade;
                half _VertexWobbleStrength;
                half _VertexWobbleSpeed;
                half _VertexWobbleScale;
                half _UvPixelStrength;
                half _UvPixelResolution;
                half _UvPixelAspect;
                half _MipBias;
                half _AffineStrength;
                half _AffineMode;
                half _PosterizeSteps;
                half _PaletteStrength;
                half _PaletteSteps;
                half _DitherStrength;
                half _DitherScale;
                half4 _RetroFogColor;
                half _RetroFogStart;
                half _RetroFogEnd;
                half _RetroFogDensity;
                half _RetroFogSteps;
                half _RetroFogBlendMode;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float3 affineUVAndW : TEXCOORD4;
                half retroFogFactor : TEXCOORD5;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 ApplyWaterWaveOS(float3 positionOS)
            {
                float2 direction = normalize(_WaveDirection.xy + float2(0.0001, 0.0));
                float steppedTime = floor(_Time.y * 12.0) / 12.0;
                float phaseA = dot(positionOS.xz, direction) * max(_WaveScale, 0.001h) + steppedTime * _WaveSpeed;
                float phaseB = dot(positionOS.xz, direction.yx * float2(-1.0, 1.0)) * max(_WaveScale * 0.73h, 0.001h) + steppedTime * _WaveSpeed * 0.63h;
                float wave = (sin(phaseA) + sin(phaseB) * 0.55) * 0.5;
                wave = floor(wave * max(_WaveSteps, 2.0h) + 0.5) / max(_WaveSteps, 2.0h);
                positionOS.y += wave * _WaveStrength * 0.18h;
                return positionOS;
            }

            VertexPositionInputs GetRetroWaterPositionInputs(float3 positionOS)
            {
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                #if defined(_RETRO_VERTEX_SNAP)
                    vertexInput = BorzRetroApplyVertexSnap(vertexInput, _VertexSnapStrength, _VertexSnapResolution, _VertexSnapDistanceFade, _VertexSnapFadeStart, _VertexSnapFadeEnd, _VertexSnapSeamReduction, _VertexSnapSpace);
                #endif
                return vertexInput;
            }

            float2 ModifyWaterUV(float2 uv, Varyings input, half3 viewDirWS)
            {
                #if defined(_RETRO_AFFINE)
                    uv = BorzRetroApplyAffineMode(uv, input.affineUVAndW.xy, input.affineUVAndW.z, _AffineStrength, _AffineMode, input.normalWS, viewDirWS, input.positionCS);
                #endif

                #if defined(_RETRO_UV_PIXEL)
                    uv = BorzRetroPixelateUV(uv, _UvPixelStrength, _UvPixelResolution, _UvPixelAspect);
                #endif

                return uv;
            }

            half3 ApplyWaterColorGrade(half3 color, float4 positionCS)
            {
                half dither = 0.5h;
                #if defined(_RETRO_DITHER)
                    dither = BorzRetroBayer4(positionCS.xy * _DitherScale);
                    color += (dither - 0.5h) * _DitherStrength * 0.08h;
                #endif

                #if defined(_RETRO_POSTERIZE)
                    color = BorzRetroPosterize(color, _PosterizeSteps, _PaletteStrength, _PaletteSteps, dither, _DitherStrength);
                #endif

                return saturate(color);
            }

            Varyings WaterVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS = ApplyWaterWaveOS(input.positionOS.xyz);
                #if defined(_RETRO_VERTEX_WOBBLE)
                    positionOS = BorzRetroApplyVertexWobbleOS(positionOS, input.normalOS, _VertexWobbleStrength, _VertexWobbleSpeed, _VertexWobbleScale);
                #endif

                VertexPositionInputs vertexInput = GetRetroWaterPositionInputs(positionOS);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = NormalizeNormalPerVertex(TransformObjectToWorldNormal(input.normalOS));
                output.retroFogFactor = 0.0h;
                #if defined(_RETRO_FOG)
                    output.retroFogFactor = BorzRetroFogFactor(vertexInput.positionWS, _RetroFogStart, _RetroFogEnd, _RetroFogDensity, _RetroFogSteps, _RetroFogBlendMode);
                #endif
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                float2 affineUvTimesW;
                float affineW;
                BorzRetroEncodeAffineUV(output.uv, vertexInput.positionCS, affineUvTimesW, affineW);
                output.affineUVAndW = float3(affineUvTimesW, affineW);
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                return output;
            }

            half4 WaterFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float steppedTime = floor(_Time.y * 12.0) / 12.0;
                float2 direction = normalize(_WaveDirection.xy + float2(0.0001, 0.0));
                float2 uv = ModifyWaterUV(input.uv + direction * steppedTime * _WaveSpeed * 0.035, input, viewDirWS);
                BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);
                half4 surface = SAMPLE_TEXTURE2D_BIAS(_BaseMap, sampler_BaseMap, uv, _MipBias) * _BaseColor;
                half noiseA = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv * max(_WaveScale, 0.001h)).r;
                half noiseB = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv * max(_WaveScale * 1.7h, 0.001h) - direction * steppedTime * _WaveSpeed * 0.025).g;

                half depthBlend = 0.55h;
                half foamMask = 0.0h;
                #if defined(_WATER_DEPTH_FADE)
                    float2 screenUV = input.screenPos.xy / max(input.screenPos.w, 0.00001);
                    float sceneEyeDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                    float waterEyeDepth = max(input.positionCS.w, 0.0001);
                    half depthDelta = saturate((sceneEyeDepth - waterEyeDepth) / max(_DepthFadeDistance, 0.0001h));
                    depthBlend = depthDelta;
                    half foamDepth = 1.0h - saturate((sceneEyeDepth - waterEyeDepth) / max(_FoamDistance, 0.0001h));
                    foamMask = saturate((foamDepth + noiseA * 0.45h - 0.55h) * _FoamStrength * 3.0h);
                #endif

                half3 waterNormal = normalize(input.normalWS + half3(noiseA - 0.5h, 0.0h, noiseB - 0.5h) * _NormalDistortion);
                Light mainLight = GetMainLight();
                half lightTerm = saturate(dot(waterNormal, mainLight.direction)) * 0.5h + 0.5h;
                half fresnel = pow(1.0h - saturate(dot(waterNormal, viewDirWS)), max(_FresnelPower, 0.001h));
                half steppedNoise = floor(noiseA * max(_WaveSteps, 2.0h) + 0.5h) / max(_WaveSteps, 2.0h);

                half3 color = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthBlend);
                color = lerp(color, surface.rgb, surface.a);
                color *= lerp(0.72h, 1.18h, lightTerm);
                color += _FresnelColor.rgb * fresnel * _FresnelIntensity;
                color = lerp(color, _FoamColor.rgb, foamMask * _FoamColor.a);
                color += (steppedNoise - 0.5h) * 0.06h;
                color = ApplyWaterColorGrade(color, input.positionCS);
                #if defined(_RETRO_FOG)
                    color = BorzRetroApplyFog(color, input.retroFogFactor, _RetroFogColor);
                #endif

                half alpha = saturate(_Alpha + fresnel * _FresnelIntensity * 0.12h + foamMask * 0.25h);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "Borzblade.RetroRenderToolkit.Editor.PSXPS2WaterShaderGUI"
}
