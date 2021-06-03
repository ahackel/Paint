// Upgrade NOTE: replaced 'defined USE_TEXTURE' with 'defined (USE_TEXTURE)'

Shader "Paint/BlendBrush"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        [Toggle(USE_TEXTURE)] _UseTexture ("Use Texture", float) = 1
        _Hardness ("Hardness", Range(0, 1)) = 1
    }
    
    CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #pragma multi_compile __ USE_TEXTURE

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
        float _Hardness;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

        float sdRoundedBox( float2 p, float2 b, in float r )
        {
            float2 q = abs(p)-b+r;
            return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r;
        }

        float sdBox( float2 p, float2 b )
        {
            float2 d = abs(p)-b;
            return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
        }

        float sdCircle( float2 p, float r )
        {
            return length(p) - r;
        }

        float4 frag (v2f i) : SV_Target
        {
#if defined (USE_TEXTURE)
            float4 tex = tex2D(_MainTex, i.uv) * _Color;
            //tex.a = step(0.5, tex.a);
            return tex;
#else
            float d = 1 - saturate(distance(i.uv, 0.5) * 2);
            float2 a = abs(i.uv - 0.5) * 2;
            d = -sdCircle(i.uv - 0.5, 0.5);
            float alpha = smoothstep(0, saturate(1 - _Hardness), d) * _Color.a;
            return float4(_Color.rgb, alpha);
#endif
        }

    ENDCG
    
    SubShader
    {
        Tags { "RenderType"="Overlay" }
        
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha one
        BlendOp Add 
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
