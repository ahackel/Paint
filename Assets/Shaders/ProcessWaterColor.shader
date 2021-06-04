Shader "Paint/ProcessWaterColor"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "red" {}
        [NoScaleOffset] _BlurredTex ("Blurred Texture", 2D) = "red" {}
        [NoScaleOffset] _StrokeTex ("Stroke Texture", 2D) = "red" {}
        [NoScaleOffset] _DiffusionTex ("Diffusion Texture", 2D) = "grey" {}
        _DryRate ("Dry Rate", Range(0, 1)) = 0.01
        _Wetness ("Wetness", Range(0, 1)) = 0.01
        _Diffusion ("Diffusion", Range(0, 1)) = 0.5
        _DiffusionScale ("Diffusion Scale", Vector) = (1, 1, 1, 1)
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
        sampler2D _DiffusionTex;
        float _DryRate;
        float _Wetness;
        float _Diffusion;
        float2 _DiffusionScale;

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
                float2 diffusionUvs = i.uv; // + _Diffusion * (tex2D(_DiffusionTex, i.uv * _DiffusionScale) - 0.5);
                const float4 lastFrameData = tex2D(_MainTex, i.uv);
                float4 lastFrameDataBlurred = tex2D(_BlurredTex, diffusionUvs);
                const float4 strokeData = tex2D(_StrokeTex, i.uv);

                float pigments = lerp(1, 0.3, saturate(0.3333 * (lastFrameData.r + lastFrameData.g + lastFrameData.b)));
                float wetness = max(lastFrameData.a, _Wetness);
                wetness = max(0, wetness - _DryRate);

                float3 color = lerp(lastFrameData.rgb, lastFrameDataBlurred.rgb, saturate(wetness) * pigments);

                float strokeOpacity = strokeData.a;
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
