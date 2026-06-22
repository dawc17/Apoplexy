Shader "Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Depth Fog"
{
    Properties
    {
        _FogColor("Fog Color", Color) = (0.42, 0.46, 0.50, 1)
        _Intensity("Intensity", Range(0, 1)) = 0.45
        _StartDistance("Start Distance", Float) = 18
        _EndDistance("End Distance", Float) = 85
        _Density("Density", Range(0.001, 1)) = 0.035
        _BlendMode("Blend Mode", Float) = 0
        _Steps("Steps", Range(0, 32)) = 8
        _DitherStrength("Dither Strength", Range(0, 1)) = 0.08
        _DitherScale("Dither Scale", Range(0.25, 8)) = 1
        _AffectSky("Affect Sky", Float) = 0
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
            Name "PSX PS2 Depth Fog"

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
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FogColor;
                half _Intensity;
                half _StartDistance;
                half _EndDistance;
                half _Density;
                half _BlendMode;
                half _Steps;
                half _DitherStrength;
                half _DitherScale;
                half _AffectSky;
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

            half DepthFogFactor(float eyeDepth)
            {
                half startDistance = min(_StartDistance, _EndDistance);
                half endDistance = max(_StartDistance, _EndDistance);
                half linearFog = saturate((eyeDepth - startDistance) / max(endDistance - startDistance, 0.0001h));
                half densityDistance = max(eyeDepth - startDistance, 0.0) * max(_Density, 0.0001h);
                half exponentialFog = 1.0h - exp2(-densityDistance * 1.442695h);
                half exponentialSquaredFog = 1.0h - exp2(-densityDistance * densityDistance * 1.442695h);

                half fog = linearFog;
                fog = _BlendMode > 0.5h ? exponentialFog : fog;
                fog = _BlendMode > 1.5h ? exponentialSquaredFog : fog;
                return saturate(fog);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel);
                float rawDepth = SampleSceneDepth(uv);

                #if UNITY_REVERSED_Z
                    half isSky = rawDepth <= 0.00001 ? 1.0h : 0.0h;
                #else
                    half isSky = rawDepth >= 0.99999 ? 1.0h : 0.0h;
                #endif

                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                half fog = DepthFogFactor(eyeDepth);
                fog = lerp(fog, 1.0h, isSky * saturate(_AffectSky));
                fog *= 1.0h - isSky * (1.0h - saturate(_AffectSky));

                half dither = Bayer4(input.positionCS.xy / max(_DitherScale, 0.001h));
                fog = saturate(fog + (dither - 0.5h) * _DitherStrength);

                half roundedSteps = floor(_Steps + 0.5h);
                if (roundedSteps > 1.0h)
                {
                    fog = floor(fog * roundedSteps + 0.5h) / roundedSteps;
                }

                half blend = saturate(fog * _Intensity * _FogColor.a);
                return half4(lerp(source.rgb, _FogColor.rgb, blend), source.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
