Shader "Borzblade/Retro Render Toolkit/PSX PS2 Sprite Unlit"
{
    Properties
    {
        [PerRendererData][MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1, 1, 1, 1)
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.01

        [Toggle(_RETRO_VERTEX_SNAP)] _VertexSnapEnabled("Vertex Snap Enabled", Float) = 0
        _VertexSnapStrength("Vertex Snap Strength", Range(0, 1)) = 0.08
        _VertexSnapResolution("Vertex Snap Resolution", Range(24, 1024)) = 240
        _VertexSnapDistanceFade("Vertex Snap Distance Fade", Range(0, 1)) = 0.35
        _VertexSnapFadeStart("Snap Fade Start", Float) = 5
        _VertexSnapFadeEnd("Snap Fade End", Float) = 45
        _VertexSnapSeamReduction("Snap Seam Reduction", Range(0, 1)) = 0
        [Enum(Screen,0,View World,1)] _VertexSnapSpace("Vertex Snap Space", Float) = 0
        [Toggle] _VertexDrawDistanceEnabled("Vertex Draw Distance Enabled", Float) = 0
        _VertexDrawDistance("Vertex Draw Distance", Float) = 0
        _VertexDrawDistanceFade("Vertex Draw Distance Fade", Range(0, 64)) = 0

        [Toggle(_RETRO_VERTEX_WOBBLE)] _VertexWobbleEnabled("Vertex Wobble Enabled", Float) = 0
        _VertexWobbleStrength("Vertex Wobble Strength", Range(0, 1)) = 0.04
        _VertexWobbleSpeed("Vertex Wobble Speed", Range(0, 12)) = 2.5
        _VertexWobbleScale("Vertex Wobble Scale", Range(0.1, 16)) = 4

        [Toggle(_RETRO_UV_PIXEL)] _UvPixelEnabled("UV Pixelation Enabled", Float) = 1
        _UvPixelStrength("UV Pixelation Strength", Range(0, 1)) = 0.2
        _UvPixelResolution("UV Pixelation Resolution", Range(16, 2048)) = 256
        _UvPixelAspect("UV Pixelation Aspect", Range(0.25, 4)) = 1
        _MipBias("Mip Bias", Range(-3, 3)) = 0

        [Toggle(_RETRO_AFFINE)] _AffineEnabled("Affine Warp Enabled", Float) = 0
        _AffineStrength("Affine Warp Strength", Range(0, 1)) = 0.18
        [Enum(Stable,0,Classic,1)] _AffineMode("Affine Mode", Float) = 0

        [Toggle(_RETRO_POSTERIZE)] _PosterizeEnabled("Posterize Enabled", Float) = 1
        _PosterizeSteps("Posterize Steps", Range(2, 64)) = 24
        _PaletteStrength("Palette Strength", Range(0, 1)) = 0.12
        _PaletteSteps("Palette Steps", Range(2, 64)) = 32

        [Toggle(_RETRO_DITHER)] _DitherEnabled("Dither Enabled", Float) = 1
        _DitherStrength("Dither Strength", Range(0, 1)) = 0.12
        _DitherScale("Dither Scale", Range(0.25, 8)) = 1

        [Toggle(_RETRO_FOG)] _RetroFogEnabled("Retro Fog Enabled", Float) = 0
        _RetroFogColor("Retro Fog Color", Color) = (0.42, 0.46, 0.50, 1)
        _RetroFogStart("Retro Fog Start", Float) = 18
        _RetroFogEnd("Retro Fog End", Float) = 70
        _RetroFogDensity("Retro Fog Density", Range(0.001, 1)) = 0.035
        _RetroFogSteps("Retro Fog Steps", Range(0, 16)) = 6
        [Enum(Linear,0,Exponential,1)] _RetroFogBlendMode("Retro Fog Blend Mode", Float) = 0

        [HideInInspector] _RendererColor("Renderer Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip("Flip", Vector) = (1, 1, 1, 1)
        [HideInInspector] _SrcBlend("__src", Float) = 5
        [HideInInspector] _DstBlend("__dst", Float) = 10
        [HideInInspector] _ZWrite("__zw", Float) = 0
        _QueueOffset("Queue Offset", Float) = 0

        [HideInInspector] _BaseMap("Base Map", 2D) = "white" {}
        [HideInInspector] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }
        LOD 150

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex SpriteVertex
            #pragma fragment SpriteFragment
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local _RETRO_VERTEX_WOBBLE
            #pragma shader_feature_local_fragment _RETRO_UV_PIXEL
            #pragma shader_feature_local_fragment _RETRO_AFFINE
            #pragma shader_feature_local_fragment _RETRO_POSTERIZE
            #pragma shader_feature_local_fragment _RETRO_DITHER
            #pragma shader_feature_local _RETRO_FOG
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Core/PSXPS2RetroCommon.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _RendererColor;
                float4 _Flip;
                half4 _EmissionColor;
                half4 _RetroFogColor;
                half _Cutoff;
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
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                half4 color : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 normalWS : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                float3 affineUVAndW : TEXCOORD5;
                half retroFog : TEXCOORD6;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings SpriteVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS = input.positionOS.xyz;
                positionOS.xy *= _Flip.xy;
                #if defined(_RETRO_VERTEX_WOBBLE)
                    positionOS = BorzRetroApplyVertexWobbleOS(positionOS, input.normalOS, _VertexWobbleStrength, _VertexWobbleSpeed, _VertexWobbleScale);
                #endif

                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                #if defined(_RETRO_VERTEX_SNAP)
                    vertexInput = BorzRetroApplyVertexSnap(vertexInput, _VertexSnapStrength, _VertexSnapResolution, _VertexSnapDistanceFade, _VertexSnapFadeStart, _VertexSnapFadeEnd, _VertexSnapSeamReduction, _VertexSnapSpace);
                #endif

                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                float2 affineUvTimesW;
                float affineW;
                BorzRetroEncodeAffineUV(output.uv, vertexInput.positionCS, affineUvTimesW, affineW);
                output.affineUVAndW = float3(affineUvTimesW, affineW);

                output.retroFog = 0.0h;
                #if defined(_RETRO_FOG)
                    output.retroFog = BorzRetroFogFactor(vertexInput.positionWS, _RetroFogStart, _RetroFogEnd, _RetroFogDensity, _RetroFogSteps, _RetroFogBlendMode);
                #endif
                return output;
            }

            float2 ModifyUV(float2 uv, Varyings input)
            {
                #if defined(_RETRO_AFFINE)
                    uv = BorzRetroApplyAffineMode(uv, input.affineUVAndW.xy, input.affineUVAndW.z, _AffineStrength, _AffineMode, input.normalWS, GetWorldSpaceNormalizeViewDir(input.positionWS), input.positionCS);
                #endif

                #if defined(_RETRO_UV_PIXEL)
                    uv = BorzRetroPixelateUV(uv, _UvPixelStrength, _UvPixelResolution, _UvPixelAspect);
                #endif

                return uv;
            }

            half3 ApplyColorGrade(half3 color, float4 positionCS)
            {
                half dither = 0.5h;
                #if defined(_RETRO_DITHER)
                    dither = BorzRetroBayer4(positionCS.xy * _DitherScale);
                    color += (dither - 0.5h) * _DitherStrength * 0.08h;
                #endif

                #if defined(_RETRO_POSTERIZE)
                    color = BorzRetroPosterize(color, _PosterizeSteps, _PaletteStrength, _PaletteSteps, dither, _DitherStrength);
                #endif

                return color;
            }

            half4 SpriteFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = ModifyUV(input.uv, input);
                half4 sprite = SAMPLE_TEXTURE2D_BIAS(_MainTex, sampler_MainTex, uv, _MipBias) * input.color;
                clip(sprite.a - _Cutoff);
                BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);

                half3 color = sprite.rgb + SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
                color = ApplyColorGrade(color, input.positionCS);
                #if defined(_RETRO_FOG)
                    color = BorzRetroApplyFog(color, input.retroFog, _RetroFogColor);
                #endif
                color = MixFog(color, input.fogFactor);
                return half4(color, sprite.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "Borzblade.RetroRenderToolkit.Editor.PSXPS2SpriteShaderGUI"
}
