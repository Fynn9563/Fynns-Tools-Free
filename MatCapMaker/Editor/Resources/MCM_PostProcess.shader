Shader "Hidden/MCM_PostProcess"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}

        _BlurRadius ("Blur Radius", Float) = 1

        _SharpenStrength ("Sharpen Strength", Float) = 0.5

        _ChromaticIntensity ("Chromatic Intensity", Float) = 0.5

        _GrainStrength ("Grain Strength", Float) = 0.3
        _GrainScale ("Grain Scale", Float) = 1
        _GrainTime ("Grain Time", Float) = 0
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex, _BloomTex;
    float _BloomThreshold, _BloomStrength;
    float4 _MainTex_TexelSize;
    float _BlurRadius;
    float _SharpenStrength;
    float _ChromaticIntensity;
    float _GrainStrength;
    float _GrainScale;
    float _GrainTime;

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    v2f vert(appdata_img v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }

    bool isInSphere(float2 uv)
    {
        float2 centered = uv * 2.0 - 1.0;
        float r2 = dot(centered, centered);
        return r2 <= 1.0;
    }

    float random(float2 st)
    {
        return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest Always Cull Off ZWrite Off

        // Pass 0: Gaussian Blur Horizontal
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBlurH

            float4 fragBlurH(v2f i) : SV_Target
            {
                if (!isInSphere(i.uv))
                    return fixed4(0, 0, 0, 0);

                float4 color = float4(0, 0, 0, 0);
                float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

                float2 texelSize = _MainTex_TexelSize.xy * _BlurRadius;

                color += tex2D(_MainTex, i.uv) * weights[0];

                [unroll] for (int j = 1; j < 5; j++)
                {
                    color += tex2D(_MainTex, i.uv + float2(texelSize.x * j, 0)) * weights[j];
                    color += tex2D(_MainTex, i.uv - float2(texelSize.x * j, 0)) * weights[j];
                }

                return color;
            }
            ENDCG
        }

        // Pass 1: Gaussian Blur Vertical
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBlurV

            float4 fragBlurV(v2f i) : SV_Target
            {
                if (!isInSphere(i.uv))
                    return fixed4(0, 0, 0, 0);

                float4 color = float4(0, 0, 0, 0);
                float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

                float2 texelSize = _MainTex_TexelSize.xy * _BlurRadius;

                color += tex2D(_MainTex, i.uv) * weights[0];

                [unroll] for (int j = 1; j < 5; j++)
                {
                    color += tex2D(_MainTex, i.uv + float2(0, texelSize.y * j)) * weights[j];
                    color += tex2D(_MainTex, i.uv - float2(0, texelSize.y * j)) * weights[j];
                }

                return color;
            }
            ENDCG
        }

        // Pass 2: Sharpen (Unsharp Mask)
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragSharpen

            fixed4 fragSharpen(v2f i) : SV_Target
            {
                if (!isInSphere(i.uv))
                    return fixed4(0, 0, 0, 0);

                float2 texelSize = _MainTex_TexelSize.xy;

                float4 center = tex2D(_MainTex, i.uv);

                float4 top = tex2D(_MainTex, i.uv + float2(0, texelSize.y));
                float4 bottom = tex2D(_MainTex, i.uv - float2(0, texelSize.y));
                float4 left = tex2D(_MainTex, i.uv - float2(texelSize.x, 0));
                float4 right = tex2D(_MainTex, i.uv + float2(texelSize.x, 0));

                float4 edge = center * 4.0 - top - bottom - left - right;

                float4 result = center + edge * _SharpenStrength;

                return float4(saturate(result.rgb), center.a);
            }
            ENDCG
        }

        // Pass 3: Chromatic Aberration
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragChromatic

            fixed4 fragChromatic(v2f i) : SV_Target
            {
                if (!isInSphere(i.uv))
                    return fixed4(0, 0, 0, 0);

                float2 center = float2(0.5, 0.5);
                float2 dir = i.uv - center;
                float dist = length(dir);

                float2 offset = dir * dist * _ChromaticIntensity * 0.02;

                float r = tex2D(_MainTex, i.uv + offset).r;
                float g = tex2D(_MainTex, i.uv).g;
                float b = tex2D(_MainTex, i.uv - offset).b;
                float a = tex2D(_MainTex, i.uv).a;

                return float4(r, g, b, a);
            }
            ENDCG
        }

        // Pass 4: Film Grain
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragGrain

            fixed4 fragGrain(v2f i) : SV_Target
            {
                if (!isInSphere(i.uv))
                    return fixed4(0, 0, 0, 0);

                float4 color = tex2D(_MainTex, i.uv);

                float2 noiseUV = i.uv * _GrainScale * 100.0;
                float noise = random(noiseUV + _GrainTime) * 2.0 - 1.0;

                float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float grainAmount = _GrainStrength * (1.0 - abs(luminance - 0.5) * 2.0);

                color.rgb += noise * grainAmount;

                return float4(saturate(color.rgb), color.a);
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBloomExtract
            float4 fragBloomExtract(v2f i) : SV_Target
            {
                float4 color=tex2D(_MainTex,i.uv);
                float brightness=max(color.r,max(color.g,color.b));
                float weight=max(0,brightness-_BloomThreshold)/max(brightness,.0001);
                return float4(color.rgb*weight*color.a,0);
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBloomCombine
            float4 fragBloomCombine(v2f i) : SV_Target
            {
                if (!isInSphere(i.uv)) return 0;
                float4 color=tex2D(_MainTex,i.uv);
                return float4(color.rgb+tex2D(_BloomTex,i.uv).rgb*_BloomStrength,color.a);
            }
            ENDCG
        }
    }
    Fallback Off
}
