Shader "Paint/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TextureSize("TextureSize", Vector) = (1, 1, 1, 1)
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
            float4 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        float4 _TextureSize;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv.xy = v.uv;
            o.uv.zw = 1 / _TextureSize.xy;
            return o;
        }
    ENDCG
    
    SubShader
    {
        Lighting Off
        Blend One Zero
        Cull Off
        ZWrite Off
        ZTest Always

        // Horizontal
        Pass
        {
            CGPROGRAM
            #pragma fragment frag
            
			float random (float2 p)
			{
				float3 p3 = frac(float3(p.xyx) * .1031);
    			p3 += dot(p3, p3.yzx + 33.33);
    			return frac((p3.x + p3.y) * p3.z);
			}
            
            float4 frag (v2f i) : SV_Target
            {
                const float spread = 64;
                const float2 gravity = float2(0, 0);
                float2 offset = float2(spread * (random(i.vertex + _Time.xy) - 0.5), spread * (random(i.vertex.yx + _Time.yx) - 0.5));
                offset += gravity;
                float4 sample0 = tex2D(_MainTex, i.uv.xy);
                float4 sample1 = tex2D(_MainTex, i.uv.xy + i.uv.zw * offset);
                float wetness = lerp(sample0.a, sample1.a, 0.2);
                float wetnessDiff = abs(sample0.a - sample1.a);
                float4 result = lerp(sample0, sample1, wetnessDiff + wetness);

                //dry:
                const float dryTime = 1;
                
                wetness = saturate(wetness - 1 / (dryTime * 60));
                result.a = wetness;
                
                return result;
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
