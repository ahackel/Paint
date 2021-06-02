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
            
            float4 frag (v2f i) : SV_Target
            {
                const float4 lastFrameData = tex2D(_MainTex, i.uv);
                const float4 lastFrameDataBlurred = tex2D(_BlurredTex, i.uv);
                const float4 strokeData = tex2D(_StrokeTex, i.uv);

                float wetness = max(lastFrameData.a, _Wetness);
                wetness = saturate(wetness - _DryRate);

                float3 color = lerp(lastFrameData.rgb, lastFrameDataBlurred.rgb, wetness);
                color.rgb = lerp(color.rgb, strokeData.rgb, strokeData.a);
                wetness = max(wetness, strokeData.a);

                return saturate(float4(color, wetness));
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
