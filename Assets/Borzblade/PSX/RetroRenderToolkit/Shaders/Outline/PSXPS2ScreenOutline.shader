Shader "Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Screen Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0.015, 0.012, 0.01, 1)
        _Intensity("Intensity", Range(0, 1)) = 0.85
        _Thickness("Thickness", Range(0.25, 6)) = 1.25
        _DepthSensitivity("Depth Sensitivity", Range(0, 64)) = 18
        _NormalSensitivity("Normal Sensitivity", Range(0, 16)) = 4
        _DistanceFadeStart("Distance Fade Start", Float) = 12
        _DistanceFadeEnd("Distance Fade End", Float) = 85
        _Blend("Blend", Range(0, 1)) = 0.9
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
            Name "PSX PS2 Screen Outline"

            ZTest Always
            ZWrite Off
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _Intensity;
                half _Thickness;
                half _DepthSensitivity;
                half _NormalSensitivity;
                half _DistanceFadeStart;
                half _DistanceFadeEnd;
                half _Blend;
            CBUFFER_END

            half EdgeAt(float2 uv, float2 offset, float centerEyeDepth, float3 centerNormal)
            {
                float neighborEyeDepth = LinearEyeDepth(SampleSceneDepth(uv + offset), _ZBufferParams);
                float3 neighborNormal = normalize(SampleSceneNormals(uv + offset));
                half depthEdge = saturate(abs(neighborEyeDepth - centerEyeDepth) / max(centerEyeDepth, 0.001) * _DepthSensitivity);
                half normalEdge = saturate((1.0h - dot(centerNormal, neighborNormal)) * _NormalSensitivity);
                return max(depthEdge, normalEdge);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                float centerEyeDepth = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float3 centerNormal = normalize(SampleSceneNormals(uv));
                float2 texel = _ScreenParams.zw - 1.0;
                float2 stepUv = texel * max(_Thickness, 0.25h);

                half edge = 0.0h;
                edge = max(edge, EdgeAt(uv, float2(stepUv.x, 0.0), centerEyeDepth, centerNormal));
                edge = max(edge, EdgeAt(uv, float2(-stepUv.x, 0.0), centerEyeDepth, centerNormal));
                edge = max(edge, EdgeAt(uv, float2(0.0, stepUv.y), centerEyeDepth, centerNormal));
                edge = max(edge, EdgeAt(uv, float2(0.0, -stepUv.y), centerEyeDepth, centerNormal));
                edge = max(edge, EdgeAt(uv, stepUv, centerEyeDepth, centerNormal) * 0.75h);
                edge = max(edge, EdgeAt(uv, -stepUv, centerEyeDepth, centerNormal) * 0.75h);

                half distanceFade = 1.0h - saturate((centerEyeDepth - _DistanceFadeStart) / max(_DistanceFadeEnd - _DistanceFadeStart, 0.0001h));
                edge = saturate(edge * _Intensity * distanceFade);

                half3 outlined = lerp(source.rgb, _OutlineColor.rgb, edge * _Blend * _OutlineColor.a);
                return half4(outlined, source.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
