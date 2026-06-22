Shader "Borzblade/Retro Render Toolkit/PSX PS2 Cutout Lit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0, 2)) = 1
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.45
        _ShadowCutoff("Shadow Cutoff", Range(0, 1)) = 0.45
        [Toggle(_RETRO_DITHER_FADE)] _DitherCutoutFadeEnabled("Dithered Cutout Fade", Float) = 0
        _DitherFadeAmount("Dither Fade Amount", Range(0, 1)) = 0
        _DitherFadeStart("Dither Fade Start", Float) = 25
        _DitherFadeEnd("Dither Fade End", Float) = 60
        [Toggle(_RETRO_CAMERA_FADE)] _CameraFadeEnabled("Camera Fade", Float) = 0
        _CameraFadeDistance("Camera Fade Distance", Float) = 0.4
        [Toggle(_RETRO_DISTANCE_FADE)] _DistanceFadeEnabled("Distance Fade", Float) = 0
        [Toggle(_RETRO_BACKFACE_TINT)] _BackfaceTintEnabled("Backface Tint", Float) = 0
        _BackfaceTint("Backface Tint Color", Color) = (0.68, 0.75, 0.62, 1)

        _SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.45
        _SpecularIntensity("PS2 Specular Intensity", Range(0, 3)) = 0.65
        _SpecularPower("PS2 Specular Power", Range(4, 128)) = 36
        [Enum(Legacy Per Pixel,0,PS1 Off,1,PS2 Vertex,2)] _RetroSpecularMode("Specular Mode", Float) = 0
        [Enum(Standard URP,0,Vertex Lit,1,Flat Lit,2,Unlit,3)] _RetroLightingModel("Lighting Model", Float) = 0

        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}

        [Toggle(_RETRO_VERTEX_SNAP)] _VertexSnapEnabled("Vertex Snap Enabled", Float) = 0
        _VertexSnapStrength("Vertex Snap Strength", Range(0, 1)) = 0.08
        _VertexSnapResolution("Vertex Snap Resolution", Range(24, 1024)) = 240
        _VertexSnapDistanceFade("Vertex Snap Distance Fade", Range(0, 1)) = 0.35
        _VertexSnapFadeStart("Snap Fade Start", Float) = 5
        _VertexSnapFadeEnd("Snap Fade End", Float) = 45
        _VertexSnapSeamReduction("Snap Seam Reduction", Range(0, 1)) = 0
        [Enum(Screen,0,View World,1)] _VertexSnapSpace("Vertex Snap Space", Float) = 0
        [Toggle(_RETRO_SNAP_ANCHORS)] _VertexSnapUseAnchors("Use Baked Snap Anchors", Float) = 0
        [Toggle] _VertexDrawDistanceEnabled("Vertex Draw Distance Enabled", Float) = 0
        _VertexDrawDistance("Vertex Draw Distance", Float) = 0
        _VertexDrawDistanceFade("Vertex Draw Distance Fade", Range(0, 64)) = 0

        [Toggle(_RETRO_VERTEX_WOBBLE)] _VertexWobbleEnabled("Vertex Wobble Enabled", Float) = 0
        _VertexWobbleStrength("Vertex Wobble Strength", Range(0, 1)) = 0.04
        _VertexWobbleSpeed("Vertex Wobble Speed", Range(0, 12)) = 2.5
        _VertexWobbleScale("Vertex Wobble Scale", Range(0.1, 16)) = 4

        [Toggle(_RETRO_UV_PIXEL)] _UvPixelEnabled("UV Pixelation Enabled", Float) = 1
        _UvPixelStrength("UV Pixelation Strength", Range(0, 1)) = 0.35
        _UvPixelResolution("UV Pixelation Resolution", Range(16, 2048)) = 256
        _UvPixelAspect("UV Pixelation Aspect", Range(0.25, 4)) = 1
        _MipBias("Mip Bias", Range(-3, 3)) = 0

        [Toggle(_RETRO_AFFINE)] _AffineEnabled("Affine Warp Enabled", Float) = 0
        _AffineStrength("Affine Warp Strength", Range(0, 1)) = 0.18
        [Enum(Stable,0,Classic,1)] _AffineMode("Affine Mode", Float) = 0

        [Toggle(_RETRO_POSTERIZE)] _PosterizeEnabled("Posterize Enabled", Float) = 1
        _PosterizeSteps("Posterize Steps", Range(2, 64)) = 18
        _PaletteStrength("Palette Strength", Range(0, 1)) = 0.2
        _PaletteSteps("Palette Steps", Range(2, 64)) = 32

        [Toggle(_RETRO_DITHER)] _DitherEnabled("Dither Enabled", Float) = 1
        _DitherStrength("Dither Strength", Range(0, 1)) = 0.18
        _DitherScale("Dither Scale", Range(0.25, 8)) = 1

        [Toggle(_RETRO_FOG)] _RetroFogEnabled("Retro Fog Enabled", Float) = 0
        _RetroFogColor("Retro Fog Color", Color) = (0.42, 0.46, 0.50, 1)
        _RetroFogStart("Retro Fog Start", Float) = 18
        _RetroFogEnd("Retro Fog End", Float) = 70
        _RetroFogDensity("Retro Fog Density", Range(0.001, 1)) = 0.035
        _RetroFogSteps("Retro Fog Steps", Range(0, 16)) = 6
        [Enum(Linear,0,Exponential,1)] _RetroFogBlendMode("Retro Fog Blend Mode", Float) = 0

        _LightBands("Shadow/Light Bands", Range(0, 12)) = 0
        _ShadowBandStrength("Band Strength", Range(0, 1)) = 0.25
        [Toggle(_RETRO_RIM)] _RimEnabled("Rim Enabled", Float) = 1
        [HDR] _RimColor("Rim Color", Color) = (0.45, 0.55, 0.8, 1)
        _RimIntensity("Rim Intensity", Range(0, 4)) = 0.25
        _RimPower("Rim Power", Range(0.25, 8)) = 2.5

        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1
        _Surface("__surface", Float) = 1
        _Blend("__blend", Float) = 0
        [ToggleUI] _TwoSidedEnabled("Two Sided", Float) = 1
        _CullMode("Cull Mode", Float) = 2
        _Cull("__cull", Float) = 0
        [ToggleUI] _AlphaClip("__clip", Float) = 1
        [HideInInspector] _SrcBlend("__src", Float) = 5
        [HideInInspector] _DstBlend("__dst", Float) = 10
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 10
        [HideInInspector] _ZWrite("__zw", Float) = 0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0
        _QueueOffset("Queue Offset", Float) = 0

        [HideInInspector] _MainTex("Base Map", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "UniversalMaterialType" = "SimpleLit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        HLSLINCLUDE
        #define BUMP_SCALE_NOT_SUPPORTED 0

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Core/PSXPS2RetroCommon.hlsl"
        #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        #endif

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseMap_TexelSize;
            half4 _BaseColor;
            half4 _SpecColor;
            half4 _EmissionColor;
            half4 _RimColor;
            half4 _BackfaceTint;
            half _BumpScale;
            half _Cutoff;
            half _ShadowCutoff;
            half _DitherFadeAmount;
            half _DitherFadeStart;
            half _DitherFadeEnd;
            half _CameraFadeDistance;
            half _Smoothness;
            half _SpecularIntensity;
            half _SpecularPower;
            half _RetroSpecularMode;
            half _RetroLightingModel;
            half _Surface;
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
            half _LightBands;
            half _ShadowBandStrength;
            half _RimIntensity;
            half _RimPower;
        CBUFFER_END

        struct RetroAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 texcoord : TEXCOORD0;
            float2 staticLightmapUV : TEXCOORD1;
            float2 dynamicLightmapUV : TEXCOORD2;
            float4 snapAnchorOS : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct RetroVaryings
        {
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;

            #ifdef _NORMALMAP
                half4 normalWS : TEXCOORD2;
                half4 tangentWS : TEXCOORD3;
                half4 bitangentWS : TEXCOORD4;
            #else
                half3 normalWS : TEXCOORD2;
            #endif

            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                half4 fogFactorAndVertexLight : TEXCOORD5;
            #else
                half fogFactor : TEXCOORD5;
            #endif

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord : TEXCOORD6;
            #endif

            DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);

            #ifdef DYNAMICLIGHTMAP_ON
                float2 dynamicLightmapUV : TEXCOORD8;
            #endif

            #ifdef USE_APV_PROBE_OCCLUSION
                float4 probeOcclusion : TEXCOORD9;
            #endif

            float3 affineUVAndW : TEXCOORD10;
            half4 retroData : TEXCOORD11;
            half3 vertexLighting : TEXCOORD12;
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float3 RetroApplyVertexWobble(float3 positionOS, float3 normalOS)
        {
            #if defined(_RETRO_VERTEX_WOBBLE)
                positionOS = BorzRetroApplyVertexWobbleOS(positionOS, normalOS, _VertexWobbleStrength, _VertexWobbleSpeed, _VertexWobbleScale);
            #endif
            return positionOS;
        }

        VertexPositionInputs RetroGetVertexPositionInputs(float3 positionOS, float3 snapAnchorOS)
        {
            VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);

            #if defined(_RETRO_VERTEX_SNAP)
                #if defined(_RETRO_SNAP_ANCHORS)
                    vertexInput = BorzRetroApplyVertexSnapAnchored(vertexInput, TransformObjectToHClip(snapAnchorOS), _VertexSnapStrength, _VertexSnapResolution, _VertexSnapDistanceFade, _VertexSnapFadeStart, _VertexSnapFadeEnd, _VertexSnapSeamReduction, _VertexSnapSpace);
                #else
                    vertexInput = BorzRetroApplyVertexSnap(vertexInput, _VertexSnapStrength, _VertexSnapResolution, _VertexSnapDistanceFade, _VertexSnapFadeStart, _VertexSnapFadeEnd, _VertexSnapSeamReduction, _VertexSnapSpace);
                #endif
            #endif

            return vertexInput;
        }

        half RetroBayer4(float2 pixel)
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

        half3 RetroGetGeometryNormalWS(RetroVaryings input)
        {
            #ifdef _NORMALMAP
                return input.normalWS.xyz;
            #else
                return input.normalWS;
            #endif
        }

        float2 RetroModifyUV(float2 uv, RetroVaryings input)
        {
            #if defined(_RETRO_AFFINE)
                uv = BorzRetroApplyAffineMode(uv, input.affineUVAndW.xy, input.affineUVAndW.z, _AffineStrength, _AffineMode, RetroGetGeometryNormalWS(input), GetWorldSpaceNormalizeViewDir(input.positionWS), input.positionCS);
            #endif

            #if defined(_RETRO_UV_PIXEL)
                uv = BorzRetroPixelateUV(uv, _UvPixelStrength, _UvPixelResolution, _UvPixelAspect);
            #endif

            return uv;
        }

        half4 RetroSampleBase(float2 uv)
        {
            return half4(SAMPLE_TEXTURE2D_BIAS(_BaseMap, sampler_BaseMap, uv, _MipBias));
        }

        void RetroInitializeSurfaceData(float2 uv, out SurfaceData surfaceData)
        {
            surfaceData = (SurfaceData)0;

            half4 albedoAlpha = RetroSampleBase(uv);
            surfaceData.alpha = saturate(albedoAlpha.a * _BaseColor.a);
            surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
            surfaceData.albedo = AlphaModulate(surfaceData.albedo, surfaceData.alpha);
            surfaceData.metallic = 0.0h;
            surfaceData.specular = _RetroSpecularMode < 0.5h ? _SpecColor.rgb * _SpecularIntensity : half3(0.0h, 0.0h, 0.0h);
            surfaceData.smoothness = _Smoothness;
            surfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
            surfaceData.occlusion = 1.0h;
            surfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
            surfaceData.clearCoatMask = 0.0h;
            surfaceData.clearCoatSmoothness = 0.0h;
        }

        void RetroInitializeInputData(RetroVaryings input, half3 normalTS, out InputData inputData)
        {
            inputData = (InputData)0;
            inputData.positionWS = input.positionWS;
            inputData.positionCS = input.positionCS;

            #ifdef _NORMALMAP
                half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
                inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
                inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
            #else
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.normalWS = input.normalWS;
            #endif

            inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
            inputData.viewDirectionWS = SafeNormalize(viewDirWS);

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                inputData.shadowCoord = input.shadowCoord;
            #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
            #else
                inputData.shadowCoord = float4(0, 0, 0, 0);
            #endif

            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
            #else
                inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
                inputData.vertexLighting = half3(0, 0, 0);
            #endif

            inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
        }

        void RetroInitializeBakedGIData(RetroVaryings input, inout InputData inputData)
        {
            #if defined(_SCREEN_SPACE_IRRADIANCE)
                inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
            #elif defined(DYNAMICLIGHTMAP_ON)
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
            #elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
                inputData.bakedGI = SAMPLE_GI(input.vertexSH, GetAbsolutePositionWS(inputData.positionWS), inputData.normalWS, inputData.viewDirectionWS, input.positionCS.xy, input.probeOcclusion, inputData.shadowMask);
            #else
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
            #endif
        }

        half3 RetroAddPS2Specular(InputData inputData)
        {
            Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
            half3 halfDir = SafeNormalize(mainLight.direction + inputData.viewDirectionWS);
            half spec = pow(saturate(dot(inputData.normalWS, halfDir)), max(_SpecularPower, 1.0h));
            return spec * mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * _SpecColor.rgb * _SpecularIntensity;
        }

        half3 RetroAddSpecular(InputData inputData, half3 vertexSpecular)
        {
            if (_RetroSpecularMode < 0.5h)
            {
                return RetroAddPS2Specular(inputData);
            }

            if (_RetroSpecularMode > 1.5h)
            {
                return vertexSpecular;
            }

            return half3(0.0h, 0.0h, 0.0h);
        }

        half3 RetroApplyRim(half3 color, InputData inputData)
        {
            #if defined(_RETRO_RIM)
                half rim = pow(1.0h - saturate(dot(inputData.normalWS, inputData.viewDirectionWS)), max(_RimPower, 0.001h));
                color += rim * _RimColor.rgb * _RimIntensity;
            #endif
            return color;
        }

        half3 RetroApplyBands(half3 color)
        {
            return BorzRetroApplyLightBands(color, _LightBands, _ShadowBandStrength);
        }

        half3 RetroApplyColorGrade(half3 color, float4 positionCS)
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

        void RetroClipCutout(half alpha, half cutoff, float3 positionWS, float4 positionCS)
        {
            half useDistanceFade = 0.0h;
            half useCameraFade = 0.0h;

            #if defined(_RETRO_DITHER_FADE)
                #if defined(_RETRO_DISTANCE_FADE)
                    useDistanceFade = 1.0h;
                #endif

                #if defined(_RETRO_CAMERA_FADE)
                    useCameraFade = 1.0h;
                #endif

                BorzRetroClipCutoutAlpha(alpha, cutoff, positionWS, positionCS, _DitherScale, _DitherFadeAmount, _DitherFadeStart, _DitherFadeEnd, _CameraFadeDistance, useDistanceFade, useCameraFade);
            #else
                BorzRetroClipCutoutAlpha(alpha, cutoff, positionWS, positionCS, _DitherScale, 0.0h, _DitherFadeStart, _DitherFadeEnd, _CameraFadeDistance, useDistanceFade, useCameraFade);
            #endif
        }

        half RetroGetTransparentFade(float3 positionWS)
        {
            half useDistanceFade = 0.0h;
            half useCameraFade = 0.0h;

            #if defined(_RETRO_DITHER_FADE)
                #if defined(_RETRO_DISTANCE_FADE)
                    useDistanceFade = 1.0h;
                #endif

                #if defined(_RETRO_CAMERA_FADE)
                    useCameraFade = 1.0h;
                #endif

                return BorzRetroCutoutFadeAmount(positionWS, _DitherFadeAmount, _DitherFadeStart, _DitherFadeEnd, _CameraFadeDistance, useDistanceFade, useCameraFade);
            #else
                return 0.0h;
            #endif
        }

        RetroVaryings RetroForwardVertex(RetroAttributes input)
        {
            RetroVaryings output = (RetroVaryings)0;

            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            float3 positionOS = RetroApplyVertexWobble(input.positionOS.xyz, input.normalOS);
            VertexPositionInputs vertexInput = RetroGetVertexPositionInputs(positionOS, input.snapAnchorOS.xyz);
            VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

            #if defined(_FOG_FRAGMENT)
                half fogFactor = 0;
            #else
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
            #endif

            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            output.positionWS = vertexInput.positionWS;
            output.positionCS = vertexInput.positionCS;
            float2 affineUvTimesW;
            float affineW;
            BorzRetroEncodeAffineUV(output.uv, vertexInput.positionCS, affineUvTimesW, affineW);
            output.affineUVAndW = float3(affineUvTimesW, affineW);

            #ifdef _NORMALMAP
                half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
                output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
                output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
            #else
                output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
            #endif

            output.retroData = half4(0.0h, 0.0h, 0.0h, 0.0h);
            #if defined(_RETRO_FOG)
                output.retroData.x = BorzRetroFogFactor(vertexInput.positionWS, _RetroFogStart, _RetroFogEnd, _RetroFogDensity, _RetroFogSteps, _RetroFogBlendMode);
            #endif
            output.retroData.yzw = BorzRetroMainLightVertexSpecular(vertexInput.positionWS, NormalizeNormalPerVertex(normalInput.normalWS), _SpecularPower, _SpecularIntensity, _SpecColor.rgb);
            output.vertexLighting = BorzRetroVertexLighting(vertexInput.positionWS, normalInput.normalWS);

            OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
            #ifdef DYNAMICLIGHTMAP_ON
                output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
            #endif
            OUTPUT_SH4(vertexInput.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.vertexSH, output.probeOcclusion);

            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
            #else
                output.fogFactor = fogFactor;
            #endif

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                output.shadowCoord = GetShadowCoord(vertexInput);
            #endif

            return output;
        }

        void RetroForwardFragment(
            RetroVaryings input,
            #if defined(_RETRO_BACKFACE_TINT)
                FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC,
            #endif
            out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
        )
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = RetroModifyUV(input.uv, input);

            SurfaceData surfaceData;
            RetroInitializeSurfaceData(uv, surfaceData);
            surfaceData.alpha *= 1.0h - RetroGetTransparentFade(input.positionWS);
            BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);

            #ifdef LOD_FADE_CROSSFADE
                LODFadeCrossFade(input.positionCS);
            #endif

            InputData inputData;
            RetroInitializeInputData(input, surfaceData.normalTS, inputData);
            RetroInitializeBakedGIData(input, inputData);

            half4 color = BorzRetroApplyLightingModel(_RetroLightingModel, surfaceData, inputData, input.vertexLighting, RetroAddSpecular(inputData, input.retroData.yzw));
            if (_RetroLightingModel < 0.5h || (_RetroLightingModel > 1.5h && _RetroLightingModel < 2.5h))
            {
                color.rgb += RetroAddSpecular(inputData, input.retroData.yzw);
            }
            color.rgb = RetroApplyRim(color.rgb, inputData);
            color.rgb = RetroApplyBands(color.rgb);
            #if defined(_RETRO_BACKFACE_TINT)
                half isFront = IS_FRONT_VFACE(frontFace, 1.0h, 0.0h);
                color.rgb = lerp(color.rgb * _BackfaceTint.rgb, color.rgb, isFront);
            #endif
            color.rgb = RetroApplyColorGrade(color.rgb, input.positionCS);
            #if defined(_RETRO_FOG)
                color.rgb = BorzRetroApplyFog(color.rgb, input.retroData.x, _RetroFogColor);
            #endif
            color.rgb = MixFog(color.rgb, inputData.fogCoord);
            color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
            outColor = color;

            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
        }

        struct RetroDepthAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 texcoord : TEXCOORD0;
            float4 snapAnchorOS : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct RetroDepthVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD1;
            #if defined(_ALPHATEST_ON)
                float2 uv : TEXCOORD0;
            #endif
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        RetroDepthVaryings RetroDepthOnlyVertex(RetroDepthAttributes input)
        {
            RetroDepthVaryings output = (RetroDepthVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            float3 positionOS = RetroApplyVertexWobble(input.positionOS.xyz, input.normalOS);
            VertexPositionInputs vertexInput = RetroGetVertexPositionInputs(positionOS, input.snapAnchorOS.xyz);
            output.positionCS = vertexInput.positionCS;
            output.positionWS = vertexInput.positionWS;
            #if defined(_ALPHATEST_ON)
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            #endif
            return output;
        }

        half4 RetroDepthOnlyFragment(RetroDepthVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            #if defined(_ALPHATEST_ON)
                RetroClipCutout(RetroSampleBase(input.uv).a * _BaseColor.a, _Cutoff, input.positionWS, input.positionCS);
            #endif
            BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);

            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

            return 0;
        }

        struct RetroDepthNormalsVaryings
        {
            float4 positionCS : SV_POSITION;
            #if defined(_ALPHATEST_ON)
                float2 uv : TEXCOORD0;
            #endif
            float3 normalWS : TEXCOORD1;
            float3 positionWS : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        RetroDepthNormalsVaryings RetroDepthNormalsVertex(RetroDepthAttributes input)
        {
            RetroDepthNormalsVaryings output = (RetroDepthNormalsVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            float3 positionOS = RetroApplyVertexWobble(input.positionOS.xyz, input.normalOS);
            VertexPositionInputs vertexInput = RetroGetVertexPositionInputs(positionOS, input.snapAnchorOS.xyz);
            output.positionCS = vertexInput.positionCS;
            output.positionWS = vertexInput.positionWS;
            #if defined(_ALPHATEST_ON)
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            #endif

            VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
            output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
            return output;
        }

        void RetroDepthNormalsFragment(
            RetroDepthNormalsVaryings input,
            out half4 outNormalWS : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
        )
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            #if defined(_ALPHATEST_ON)
                RetroClipCutout(RetroSampleBase(input.uv).a * _BaseColor.a, _Cutoff, input.positionWS, input.positionCS);
            #endif
            BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);

            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

            #if defined(_GBUFFER_NORMALS_OCT)
                float3 normalWS = normalize(input.normalWS);
                float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
                float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
                half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
                outNormalWS = half4(packedNormalWS, 0.0);
            #else
                outNormalWS = half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            #endif

            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
        }

        float3 _LightDirection;
        float3 _LightPosition;

        float4 RetroGetShadowPositionHClip(RetroDepthAttributes input)
        {
            float3 positionOS = RetroApplyVertexWobble(input.positionOS.xyz, input.normalOS);
            VertexPositionInputs vertexInput = RetroGetVertexPositionInputs(positionOS, input.snapAnchorOS.xyz);
            float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - vertexInput.positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

            float4 positionCS = TransformWorldToHClip(ApplyShadowBias(vertexInput.positionWS, normalWS, lightDirectionWS));
            positionCS = ApplyShadowClamping(positionCS);
            return positionCS;
        }

        RetroDepthVaryings RetroShadowVertex(RetroDepthAttributes input)
        {
            RetroDepthVaryings output = (RetroDepthVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            #if defined(_ALPHATEST_ON)
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            #endif
            float3 positionOS = RetroApplyVertexWobble(input.positionOS.xyz, input.normalOS);
            output.positionWS = TransformObjectToWorld(positionOS);
            output.positionCS = RetroGetShadowPositionHClip(input);
            return output;
        }

        half4 RetroShadowFragment(RetroDepthVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            #if defined(_ALPHATEST_ON)
                RetroClipCutout(RetroSampleBase(input.uv).a * _BaseColor.a, _ShadowCutoff, input.positionWS, input.positionCS);
            #endif
            BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif
            return 0;
        }

        struct RetroMetaAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
            float2 uv2 : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct RetroMetaVaryings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD1;
                float4 LightCoord : TEXCOORD2;
            #endif
        };

        RetroMetaVaryings RetroMetaVertex(RetroMetaAttributes input)
        {
            RetroMetaVaryings output = (RetroMetaVaryings)0;
            output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
            output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
            return output;
        }

        half4 RetroMetaFragment(RetroMetaVaryings input) : SV_Target
        {
            SurfaceData surfaceData;
            RetroInitializeSurfaceData(input.uv, surfaceData);

            MetaInput metaInput;
            metaInput.Albedo = surfaceData.albedo + surfaceData.specular * surfaceData.smoothness * 0.25h;
            metaInput.Emission = surfaceData.emission;
            #ifdef EDITOR_VISUALIZATION
                metaInput.VizUV = input.VizUV;
                metaInput.LightCoord = input.LightCoord;
            #endif
            return UnityMetaFragment(metaInput);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex RetroForwardVertex
            #pragma fragment RetroForwardFragment

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local _RETRO_SNAP_ANCHORS
            #pragma shader_feature_local _RETRO_VERTEX_WOBBLE
            #pragma shader_feature_local_fragment _RETRO_UV_PIXEL
            #pragma shader_feature_local_fragment _RETRO_AFFINE
            #pragma shader_feature_local_fragment _RETRO_POSTERIZE
            #pragma shader_feature_local_fragment _RETRO_DITHER
            #pragma shader_feature_local_fragment _RETRO_RIM
            #pragma shader_feature_local_fragment _RETRO_DITHER_FADE
            #pragma shader_feature_local_fragment _RETRO_DISTANCE_FADE
            #pragma shader_feature_local_fragment _RETRO_CAMERA_FADE
            #pragma shader_feature_local_fragment _RETRO_BACKFACE_TINT
            #pragma shader_feature_local _RETRO_FOG

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
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
            #pragma vertex RetroShadowVertex
            #pragma fragment RetroShadowFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local _RETRO_SNAP_ANCHORS
            #pragma shader_feature_local _RETRO_VERTEX_WOBBLE
            #pragma shader_feature_local_fragment _RETRO_DITHER_FADE
            #pragma shader_feature_local_fragment _RETRO_DISTANCE_FADE
            #pragma shader_feature_local_fragment _RETRO_CAMERA_FADE
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
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
            #pragma vertex RetroDepthOnlyVertex
            #pragma fragment RetroDepthOnlyFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local _RETRO_SNAP_ANCHORS
            #pragma shader_feature_local _RETRO_VERTEX_WOBBLE
            #pragma shader_feature_local_fragment _RETRO_DITHER_FADE
            #pragma shader_feature_local_fragment _RETRO_DISTANCE_FADE
            #pragma shader_feature_local_fragment _RETRO_CAMERA_FADE
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex RetroDepthNormalsVertex
            #pragma fragment RetroDepthNormalsFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local _RETRO_SNAP_ANCHORS
            #pragma shader_feature_local _RETRO_VERTEX_WOBBLE
            #pragma shader_feature_local_fragment _RETRO_DITHER_FADE
            #pragma shader_feature_local_fragment _RETRO_DISTANCE_FADE
            #pragma shader_feature_local_fragment _RETRO_CAMERA_FADE
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex RetroMetaVertex
            #pragma fragment RetroMetaFragment
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature EDITOR_VISUALIZATION
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "Borzblade.RetroRenderToolkit.Editor.PSXPS2CutoutShaderGUI"
}
