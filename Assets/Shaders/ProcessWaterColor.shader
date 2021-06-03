Shader "Paint/ProcessWaterColor"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "red" {}
        [NoScaleOffset] _BlurredTex ("Blurred Texture", 2D) = "red" {}
        [NoScaleOffset] _StrokeTex ("Stroke Texture", 2D) = "red" {}
        _DryRate ("Dry Rate", Range(0.01, 1)) = 0.01
        _Wetness ("Wetness", Range(0.01, 1)) = 0.01
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
        sampler2D _BlurredTex;
        sampler2D _StrokeTex;
        float _DryRate;
        float _Wetness;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
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

        // 0: Copy layer data to waterColor buffer
        Pass
        {
            CGPROGRAM
            #pragma fragment frag
            
            float4 frag (v2f i) : SV_Target
            {
                const float4 layerData = tex2D(_MainTex, i.uv);

                // start dry:
                return float4(layerData.rgb, 0);
            }
            
            ENDCG
        }
        
        // 1: Process
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

            float random2(float2 pos)
            {
                return step(0.9, random(pos)); 
            }
            
            float4 frag (v2f i) : SV_Target
            {
                const float4 lastFrameData = tex2D(_MainTex, i.uv);
                float4 lastFrameDataBlurred = tex2D(_BlurredTex, i.uv);
                const float4 strokeData = tex2D(_StrokeTex, i.uv);

                float wetness = max(lastFrameDataBlurred.a, _Wetness);
                wetness = max(0, wetness - _DryRate);
                float pigments = 1 - saturate(0.3333 * (lastFrameData.r + lastFrameData.g + lastFrameData.b)) * 0.995;

                float3 color = lerp(lastFrameData.rgb, lastFrameDataBlurred.rgb, saturate(wetness) * pigments);

                float strokeOpacity = strokeData.a; //saturate(1 - wetnessDelta);
                color.rgb = lerp(color.rgb, min(color.rgb, strokeData.rgb), strokeOpacity);
                wetness += strokeData.a;
              

                
                return saturate(float4(color.rgb, wetness));
            }
            
            ENDCG
        }
        
        // 2: Copy waterColor buffer to layer map
        Pass
        {
            CGPROGRAM
            #pragma fragment frag
            
            float4 frag (v2f i) : SV_Target
            {
                const float4 layerData = tex2D(_MainTex, i.uv);

                // start dry:
                return float4(layerData.rgb, 1);
            }
            
            ENDCG
        }
    }
}
