Shader "Apoplexy/UI/Scanline Overlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _LineColor ("Line Color", Color) = (0, 0, 0, 0.26)
        _LineSpacing ("Line Spacing", Float) = 3
        _LineHeight ("Line Height", Float) = 1
        _SweepColor ("Sweep Color", Color) = (1, 1, 1, 0.08)
        _SweepHeight ("Sweep Height", Float) = 2
        _SweepSpeed ("Sweep Speed", Float) = 46
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 screenPosition : TEXCOORD0;
            };

            fixed4 _LineColor;
            sampler2D _MainTex;
            float _LineSpacing;
            float _LineHeight;
            fixed4 _SweepColor;
            float _SweepHeight;
            float _SweepSpeed;

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.color = input.color;
                output.screenPosition = ComputeScreenPos(output.vertex);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 screenUv = input.screenPosition.xy / max(input.screenPosition.w, 0.0001);
                float y = screenUv.y * _ScreenParams.y;

                float spacing = max(_LineSpacing, 1.0);
                float lineHeight = saturate(_LineHeight / spacing);
                float scanlineMask = step(frac(y / spacing), lineHeight);

                float sweepY = frac(_Time.y * _SweepSpeed / max(_ScreenParams.y, 1.0)) * _ScreenParams.y;
                float sweep = 1.0 - step(_SweepHeight, abs(y - sweepY));

                fixed4 color = lerp(_LineColor * scanlineMask, _SweepColor, saturate(sweep));
                color *= input.color;
                return color;
            }
            ENDCG
        }
    }
}
