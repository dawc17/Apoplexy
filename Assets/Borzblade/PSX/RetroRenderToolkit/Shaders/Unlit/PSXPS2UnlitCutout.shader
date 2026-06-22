Shader "Borzblade/Retro Render Toolkit/PSX PS2 Unlit Cutout"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.45
        _ShadowCutoff("Shadow Cutoff", Range(0, 1)) = 0.45

        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}

        [Toggle(_RETRO_UV_PIXEL)] _UvPixelEnabled("UV Pixelation Enabled", Float) = 1
        _UvPixelStrength("UV Pixelation Strength", Range(0, 1)) = 0.2
        _UvPixelResolution("UV Pixelation Resolution", Range(16, 2048)) = 256
        _UvPixelAspect("UV Pixelation Aspect", Range(0.25, 4)) = 1
        _MipBias("Mip Bias", Range(-3, 3)) = 0

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

        [Toggle(_RETRO_DITHER_FADE)] _DitherCutoutFadeEnabled("Dithered Cutout Fade", Float) = 0
        _DitherFadeAmount("Dither Fade Amount", Range(0, 1)) = 0
        _DitherFadeStart("Dither Fade Start", Float) = 25
        _DitherFadeEnd("Dither Fade End", Float) = 60

        [ToggleUI] _TwoSidedEnabled("Two Sided", Float) = 1
        _CullMode("Cull Mode", Float) = 2
        _Cull("__cull", Float) = 0
        [ToggleUI] _AlphaClip("__clip", Float) = 1
        [HideInInspector] _SrcBlend("__src", Float) = 5
        [HideInInspector] _DstBlend("__dst", Float) = 10
        [HideInInspector] _ZWrite("__zw", Float) = 0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0
        _QueueOffset("Queue Offset", Float) = 0

        [HideInInspector] _MainTex("Base Map", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"
        }
        LOD 120

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Core/PSXPS2RetroCommon.hlsl"
        #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        #endif

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _EmissionColor;
            half _Cutoff;
            half _ShadowCutoff;
            half _UvPixelStrength;
            half _UvPixelResolution;
            half _UvPixelAspect;
            half _MipBias;
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
            half _DitherFadeAmount;
            half _DitherFadeStart;
            half _DitherFadeEnd;
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
            half fogFactor : TEXCOORD2;
            half retroFogFactor : TEXCOORD3;
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float2 RetroModifyUV(float2 uv)
        {
            #if defined(_RETRO_UV_PIXEL)
                uv = BorzRetroPixelateUV(uv, _UvPixelStrength, _UvPixelResolution, _UvPixelAspect);
            #endif
            return uv;
        }

        half4 RetroSampleBase(float2 uv)
        {
            return half4(SAMPLE_TEXTURE2D_BIAS(_BaseMap, sampler_BaseMap, uv, _MipBias));
        }

        void RetroClipCutout(half alpha, float3 positionWS, float4 positionCS, half cutoff)
        {
            #if defined(_RETRO_DITHER_FADE)
                BorzRetroClipCutoutAlpha(alpha * _BaseColor.a, cutoff, positionWS, positionCS, _DitherScale, _DitherFadeAmount, _DitherFadeStart, _DitherFadeEnd, 0.0h, 1.0h, 0.0h);
            #else
                BorzRetroClipCutoutAlpha(alpha * _BaseColor.a, cutoff, positionWS, positionCS, _DitherScale, 0.0h, _DitherFadeStart, _DitherFadeEnd, 0.0h, 0.0h, 0.0h);
            #endif
        }

        half RetroGetTransparentFade(float3 positionWS)
        {
            #if defined(_RETRO_DITHER_FADE)
                return BorzRetroCutoutFadeAmount(positionWS, _DitherFadeAmount, _DitherFadeStart, _DitherFadeEnd, 0.0h, 1.0h, 0.0h);
            #else
                return 0.0h;
            #endif
        }

        half3 RetroApplyColor(half3 color, float4 positionCS)
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

        Varyings UnlitVertex(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.positionCS = vertexInput.positionCS;
            output.positionWS = vertexInput.positionWS;
            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            #if defined(_FOG_FRAGMENT)
                output.fogFactor = 0;
            #else
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
            #endif
            output.retroFogFactor = 0.0h;
            #if defined(_RETRO_FOG)
                output.retroFogFactor = BorzRetroFogFactor(vertexInput.positionWS, _RetroFogStart, _RetroFogEnd, _RetroFogDensity, _RetroFogSteps, _RetroFogBlendMode);
            #endif
            return output;
        }

        half4 UnlitFragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = RetroModifyUV(input.uv);
            half4 baseSample = RetroSampleBase(uv);

            half3 color = baseSample.rgb * _BaseColor.rgb;
            half4 emissionSample = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv);
            color += emissionSample.rgb * _EmissionColor.rgb;
            color = RetroApplyColor(color, input.positionCS);
            #if defined(_RETRO_FOG)
                color = BorzRetroApplyFog(color, input.retroFogFactor, _RetroFogColor);
            #endif
            color = MixFog(color, input.fogFactor);
            half alpha = baseSample.a * _BaseColor.a * (1.0h - RetroGetTransparentFade(input.positionWS));
            return half4(color, alpha);
        }

        struct DepthVaryings
        {
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        DepthVaryings DepthVertex(Attributes input)
        {
            DepthVaryings output = (DepthVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.positionCS = vertexInput.positionCS;
            output.positionWS = vertexInput.positionWS;
            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            return output;
        }

        half4 DepthFragment(DepthVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            float2 uv = RetroModifyUV(input.uv);
            RetroClipCutout(RetroSampleBase(uv).a, input.positionWS, input.positionCS, _Cutoff);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif
            return 0;
        }

        float3 _LightDirection;
        float3 _LightPosition;

        float4 GetShadowPositionHClip(Attributes input)
        {
            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - vertexInput.positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
            return ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(vertexInput.positionWS, normalWS, lightDirectionWS)));
        }

        DepthVaryings ShadowVertex(Attributes input)
        {
            DepthVaryings output = (DepthVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionCS = GetShadowPositionHClip(input);
            output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            return output;
        }

        half4 ShadowFragment(DepthVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            RetroClipCutout(RetroSampleBase(input.uv).a, input.positionWS, input.positionCS, _ShadowCutoff);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif
            return 0;
        }
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _RETRO_UV_PIXEL
            #pragma shader_feature_local_fragment _RETRO_POSTERIZE
            #pragma shader_feature_local_fragment _RETRO_DITHER
            #pragma shader_feature_local _RETRO_FOG
            #pragma shader_feature_local_fragment _RETRO_DITHER_FADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _RETRO_UV_PIXEL
            #pragma shader_feature_local_fragment _RETRO_DITHER_FADE
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _RETRO_UV_PIXEL
            #pragma shader_feature_local_fragment _RETRO_DITHER_FADE
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "Borzblade.RetroRenderToolkit.Editor.PSXPS2UnlitCutoutShaderGUI"
}
