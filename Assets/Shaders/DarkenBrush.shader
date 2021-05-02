Shader "Paint/DarkenBrush"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
            float4 color : COLOR0;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.color = v.color;
            return o;
        }

        float4 frag (v2f i) : SV_Target
        {
            float4 tex = tex2D(_MainTex, i.uv);
            tex.a = step(0.5, tex.a);
            return float4(tex.rgb * i.color.rgb + (1 - tex.a), tex.a);
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
