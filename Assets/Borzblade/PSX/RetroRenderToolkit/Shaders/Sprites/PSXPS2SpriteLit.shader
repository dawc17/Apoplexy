Shader "Borzblade/Retro Render Toolkit/PSX PS2 Sprite Lit"
{
    Properties
    {
        [PerRendererData][MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0, 2)) = 1
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.01

        _SpecColor("Specular Color", Color) = (0.35, 0.35, 0.35, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.25
        _SpecularIntensity("Specular Intensity", Range(0, 3)) = 0.25
        _SpecularPower("Specular Power", Range(4, 128)) = 24
        [Enum(Standard URP,0,Vertex Lit,1,Flat Lit,2,Unlit,3)] _RetroLightingModel("Lighting Model", Float) = 1

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

        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 0
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
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex SpriteVertex
            #pragma fragment SpriteFragment
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local _RETRO_VERTEX_WOBBLE
            #pragma shader_feature_local_fragment _RETRO_UV_PIXEL
            #pragma shader_feature_local_fragment _RETRO_AFFINE
            #pragma shader_feature_local_fragment _RETRO_POSTERIZE
            #pragma shader_feature_local_fragment _RETRO_DITHER
            #pragma shader_feature_local _RETRO_FOG
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
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
                half4 _SpecColor;
                half4 _EmissionColor;
                half4 _RetroFogColor;
                half _BumpScale;
                half _Cutoff;
                half _Smoothness;
                half _SpecularIntensity;
                half _SpecularPower;
                half _RetroLightingModel;
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
                float4 tangentOS : TANGENT;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                half4 color : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half4 normalWS : TEXCOORD3;
                half4 tangentWS : TEXCOORD4;
                half4 bitangentWS : TEXCOORD5;
                half fogFactor : TEXCOORD6;
                float3 affineUVAndW : TEXCOORD7;
                half4 retroData : TEXCOORD8;
                half3 vertexLighting : TEXCOORD9;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 ApplySpriteFlip(float3 positionOS)
            {
                positionOS.xy *= _Flip.xy;
                return positionOS;
            }

            Varyings SpriteVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS = ApplySpriteFlip(input.positionOS.xyz);
                #if defined(_RETRO_VERTEX_WOBBLE)
                    positionOS = BorzRetroApplyVertexWobbleOS(positionOS, input.normalOS, _VertexWobbleStrength, _VertexWobbleSpeed, _VertexWobbleScale);
                #endif

                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                #if defined(_RETRO_VERTEX_SNAP)
                    vertexInput = BorzRetroApplyVertexSnap(vertexInput, _VertexSnapStrength, _VertexSnapResolution, _VertexSnapDistanceFade, _VertexSnapFadeStart, _VertexSnapFadeEnd, _VertexSnapSeamReduction, _VertexSnapSpace);
                #endif

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
                output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
                output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                float2 affineUvTimesW;
                float affineW;
                BorzRetroEncodeAffineUV(output.uv, vertexInput.positionCS, affineUvTimesW, affineW);
                output.affineUVAndW = float3(affineUvTimesW, affineW);

                output.retroData = half4(0.0h, 0.0h, 0.0h, 0.0h);
                #if defined(_RETRO_FOG)
                    output.retroData.x = BorzRetroFogFactor(vertexInput.positionWS, _RetroFogStart, _RetroFogEnd, _RetroFogDensity, _RetroFogSteps, _RetroFogBlendMode);
                #endif
                output.retroData.yzw = BorzRetroMainLightVertexSpecular(vertexInput.positionWS, NormalizeNormalPerVertex(normalInput.normalWS), _SpecularPower, _SpecularIntensity, _SpecColor.rgb);
                output.vertexLighting = BorzRetroVertexLighting(vertexInput.positionWS, normalInput.normalWS);
                return output;
            }

            float2 ModifyUV(float2 uv, Varyings input)
            {
                #if defined(_RETRO_AFFINE)
                    uv = BorzRetroApplyAffineMode(uv, input.affineUVAndW.xy, input.affineUVAndW.z, _AffineStrength, _AffineMode, input.normalWS.xyz, GetWorldSpaceNormalizeViewDir(input.positionWS), input.positionCS);
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

            half3 AddSpecular(InputData inputData, half3 vertexSpecular)
            {
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
                half3 halfDir = SafeNormalize(mainLight.direction + inputData.viewDirectionWS);
                half spec = pow(saturate(dot(inputData.normalWS, halfDir)), max(_SpecularPower, 1.0h));
                return spec * mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * _SpecColor.rgb * _SpecularIntensity + vertexSpecular * 0.35h;
            }

            half4 SpriteFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = ModifyUV(input.uv, input);
                half4 sprite = SAMPLE_TEXTURE2D_BIAS(_MainTex, sampler_MainTex, uv, _MipBias) * input.color;
                clip(sprite.a - _Cutoff);
                BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.alpha = sprite.a;
                surfaceData.albedo = AlphaModulate(sprite.rgb, sprite.a);
                surfaceData.metallic = 0.0h;
                surfaceData.specular = _SpecColor.rgb * _SpecularIntensity;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
                surfaceData.occlusion = 1.0h;
                surfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
                inputData.normalWS = TransformTangentToWorld(surfaceData.normalTS, inputData.tangentToWorld);
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = SafeNormalize(half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w));
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
                inputData.vertexLighting = input.vertexLighting;
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.shadowMask = half4(1.0h, 1.0h, 1.0h, 1.0h);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                half4 color = BorzRetroApplyLightingModel(_RetroLightingModel, surfaceData, inputData, input.vertexLighting, AddSpecular(inputData, input.retroData.yzw));
                if (_RetroLightingModel < 0.5h || (_RetroLightingModel > 1.5h && _RetroLightingModel < 2.5h))
                {
                    color.rgb += AddSpecular(inputData, input.retroData.yzw);
                }
                color.rgb = ApplyColorGrade(color.rgb, input.positionCS);
                #if defined(_RETRO_FOG)
                    color.rgb = BorzRetroApplyFog(color.rgb, input.retroData.x, _RetroFogColor);
                #endif
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return half4(color.rgb, sprite.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "Borzblade.RetroRenderToolkit.Editor.PSXPS2SpriteShaderGUI"
}
