Shader "Paint/CopyPaintBuffer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    
    CGINCLUDE
        #include "UnityCG.cginc"
        #pragma vertex vert

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

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }
    ENDCG
    
    SubShader
    {
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, One One
        Cull Off
        ZWrite Off
        ZTest Always

        // Horizontal
        Pass
        {
            CGPROGRAM
            #pragma fragment frag
            
            float4 frag (v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv.xy);
                //tex.a *= _Color.a;
                return tex;
            }
            
            ENDCG
        }

//        // Vertical
//        Pass
//        {
//            CGPROGRAM
//            #pragma fragment frag
//            
//            float4 frag (v2f i) : SV_Target
//            {
//                float4 sample0 = tex2D(_MainTex, i.uv.xy);
//                float4 sample1 = tex2D(_MainTex, i.uv.xy + i.uv.zw * float2(0, 4));
//                float4 sample2 = tex2D(_MainTex, i.uv.xy - i.uv.zw * float2(0, 4));
//                return 0.25 * (2 * sample0 + sample1 + sample2);
//            }
//            
//            ENDCG
//        }
    }
}
