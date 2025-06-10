Shader "UI/InvertedCircle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _CircleCenter ("Circle Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Circle Radius", Float) = 0.1
        _FadeWidth ("Fade Width", Float) = 0.01
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float2 _CircleCenter;
            float _Radius;
            float _FadeWidth;
            float _AspectRatio;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Adjust UV coordinates for aspect ratio
                float2 adjustedUV = i.texcoord;
                adjustedUV.x = (adjustedUV.x - 0.5) * _AspectRatio + 0.5;
                float2 adjustedCenter = _CircleCenter;
                adjustedCenter.x = (adjustedCenter.x - 0.5) * _AspectRatio + 0.5;

                float dist = distance(adjustedUV, adjustedCenter);
                float alpha = smoothstep(_Radius, _Radius + _FadeWidth, dist);
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}