Shader "Borzblade/Retro Render Toolkit/PSX PS2 Material Outline"
{
    Properties
    {
        [MainColor] _OutlineColor("Outline Color", Color) = (0.02, 0.018, 0.015, 1)
        _OutlineThickness("Outline Thickness", Range(0, 0.2)) = 0.025
        _DistanceFadeStart("Distance Fade Start", Float) = 8
        _DistanceFadeEnd("Distance Fade End", Float) = 55
        _DistanceFadeStrength("Distance Fade Strength", Range(0, 1)) = 0.25

        [Toggle(_RETRO_VERTEX_SNAP)] _VertexSnapEnabled("Vertex Snap Enabled", Float) = 0
        _VertexSnapStrength("Vertex Snap Strength", Range(0, 1)) = 0.1
        _VertexSnapResolution("Vertex Snap Resolution", Range(24, 1024)) = 240
        _VertexSnapDistanceFade("Vertex Snap Distance Fade", Range(0, 1)) = 0.35
        _VertexSnapFadeStart("Snap Fade Start", Float) = 5
        _VertexSnapFadeEnd("Snap Fade End", Float) = 45
        _VertexSnapSeamReduction("Snap Seam Reduction", Range(0, 1)) = 0
        [Enum(Screen,0,View World,1)] _VertexSnapSpace("Vertex Snap Space", Float) = 0
        [Toggle] _VertexDrawDistanceEnabled("Vertex Draw Distance Enabled", Float) = 0
        _VertexDrawDistance("Vertex Draw Distance", Float) = 0
        _VertexDrawDistanceFade("Vertex Draw Distance Fade", Range(0, 64)) = 0

        [Toggle(_RETRO_DITHER)] _DitherEnabled("Dither Enabled", Float) = 0
        _DitherStrength("Dither Strength", Range(0, 1)) = 0.08
        _DitherScale("Dither Scale", Range(0.25, 8)) = 1

        [HideInInspector] _SrcBlend("__src", Float) = 5
        [HideInInspector] _DstBlend("__dst", Float) = 10
        [HideInInspector] _ZWrite("__zw", Float) = 0
        _Cull("__cull", Float) = 1
        _QueueOffset("Queue Offset", Float) = -1
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
        LOD 120

        Pass
        {
            Name "Inverted Hull Outline"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            ZTest LEqual
            Cull Front

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma shader_feature_local _RETRO_VERTEX_SNAP
            #pragma shader_feature_local_fragment _RETRO_DITHER
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Borzblade/PSX/RetroRenderToolkit/Shaders/Core/PSXPS2RetroCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _OutlineThickness;
                half _DistanceFadeStart;
                half _DistanceFadeEnd;
                half _DistanceFadeStrength;
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
                half _DitherStrength;
                half _DitherScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float3 positionWS : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings OutlineVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 normalOS = normalize(input.normalOS + float3(0.0001, 0.0001, 0.0001));
                float3 positionOS = input.positionOS.xyz + normalOS * _OutlineThickness;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                #if defined(_RETRO_VERTEX_SNAP)
                    vertexInput = BorzRetroApplyVertexSnap(vertexInput, _VertexSnapStrength, _VertexSnapResolution, _VertexSnapDistanceFade, _VertexSnapFadeStart, _VertexSnapFadeEnd, _VertexSnapSeamReduction, _VertexSnapSpace);
                #endif

                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 OutlineFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                BorzRetroClipVertexDrawDistance(input.positionWS, _VertexDrawDistanceEnabled, _VertexDrawDistance, _VertexDrawDistanceFade, input.positionCS, _DitherScale);

                half4 color = _OutlineColor;
                half fade = BorzRetroDistanceFade(input.positionWS, _DistanceFadeStart, _DistanceFadeEnd);
                color.a *= lerp(1.0h, 1.0h - fade, saturate(_DistanceFadeStrength));

                #if defined(_RETRO_DITHER)
                    half dither = BorzRetroBayer4(input.positionCS.xy * _DitherScale);
                    color.rgb += (dither - 0.5h) * _DitherStrength * 0.08h;
                #endif

                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
    CustomEditor "Borzblade.RetroRenderToolkit.Editor.PSXPS2MaterialOutlineShaderGUI"
}
