Shader "Hidden/MCM_MatCapBase"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _BaseTex ("Base Texture", 2D) = "white" {}
        _UseTexture ("Use Texture", Float) = 0
        _TextureStrength ("Texture Strength", Range(0, 1)) = 1
        _TextureBlendMode ("Texture Blend Mode", Int) = 2
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Roughness ("Roughness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _BaseColor;
            sampler2D _BaseTex;
            float _UseTexture;
            float _TextureStrength;
            int _TextureBlendMode;
            float _Metallic;
            float _Roughness;

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

            float3 getSphereNormal(float2 uv)
            {
                float2 centered = uv * 2.0 - 1.0;
                float r2 = dot(centered, centered);

                if (r2 > 1.0)
                    return float3(0, 0, 0);

                float z = sqrt(1.0 - r2);
                return normalize(float3(centered.x, centered.y, z));
            }

            float3 blendNormal(float3 base, float3 blend) { return blend; }
            float3 blendAdd(float3 base, float3 blend) { return base + blend; }
            float3 blendMultiply(float3 base, float3 blend) { return base * blend; }
            float3 blendScreen(float3 base, float3 blend) { return 1.0 - (1.0 - base) * (1.0 - blend); }
            float3 blendOverlay(float3 base, float3 blend)
            {
                return base < 0.5 ? 2.0 * base * blend : 1.0 - 2.0 * (1.0 - base) * (1.0 - blend);
            }
            float3 blendSoftLight(float3 base, float3 blend)
            {
                return blend < 0.5 ? 2.0 * base * blend + base * base * (1.0 - 2.0 * blend)
                                   : sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend);
            }

            float3 applyBlend(float3 base, float3 blend, int mode)
            {
                if (mode == 0) return blendNormal(base, blend);
                if (mode == 1) return blendAdd(base, blend);
                if (mode == 2) return blendMultiply(base, blend);
                if (mode == 3) return blendScreen(base, blend);
                if (mode == 4) return blendOverlay(base, blend);
                if (mode == 5) return blendSoftLight(base, blend);
                return blendMultiply(base, blend);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = getSphereNormal(i.uv);

                if (length(normal) < 0.001)
                    return fixed4(0, 0, 0, 0);

                float3 viewDir = float3(0, 0, 1);
                float NdotV = saturate(dot(normal, viewDir));

                float3 baseColor = _BaseColor.rgb;

                if (_UseTexture > 0.5)
                {
                    float3 texColor = tex2D(_BaseTex, i.uv).rgb;
                    float3 blended = applyBlend(baseColor, texColor, _TextureBlendMode);
                    baseColor = lerp(baseColor, blended, _TextureStrength);
                }

                float edgeFalloff = smoothstep(0.0, 0.5, NdotV);

                float ao = lerp(0.15, 1.0, pow(NdotV, 0.6));

                float roughnessShading = lerp(0.7, 1.0, pow(NdotV, lerp(0.5, 2.0, _Roughness)));

                float metallicDarkening = lerp(1.0, lerp(0.2, 1.0, pow(NdotV, 0.8)), _Metallic);

                float shadingFactor = ao * roughnessShading * metallicDarkening;

                float3 result = baseColor * shadingFactor;

                return fixed4(result, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
