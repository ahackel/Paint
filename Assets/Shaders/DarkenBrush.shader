Shader "Paint/DarkenBrush"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Wetness ("Wetness", Range(0, 1)) = 0
    }
    
    CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _Color;
        float _Wetness; 

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

        float4 frag (v2f i) : SV_Target
        {
            const float4 tex = tex2D(_MainTex, i.uv);
            const float opacity = tex.a * (1 - _Wetness);
            float wetness = tex.a * _Wetness;
            // tex.a = step(0.5, tex.a);
            return float4(lerp(1, tex.rgb * _Color.rgb, opacity), wetness);
        }

    ENDCG
    
    SubShader
    {
        Tags { "RenderType"="Overlay" }
        
        Lighting Off
        Blend One One
        BlendOp Min, Max
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            ENDCG
        }
    }
}
