Shader "Hidden/MCM_Blend"
{
    Properties
    {
        _BaseTex ("Base Texture", 2D) = "black" {}
        _BlendTex ("Blend Texture", 2D) = "black" {}
        _BlendMode ("Blend Mode", Int) = 0
        _Opacity ("Opacity", Range(0, 1)) = 1
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

            sampler2D _BaseTex;
            sampler2D _BlendTex;
            int _BlendMode;
            float _Opacity;

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

            float3 blendNormal(float3 base, float3 blend) { return blend; }
            float3 blendAdd(float3 base, float3 blend) { return base + blend; }
            float3 blendMultiply(float3 base, float3 blend) { return base * blend; }
            float3 blendScreen(float3 base, float3 blend) { return 1.0 - (1.0 - base) * (1.0 - blend); }

            float3 blendOverlay(float3 base, float3 blend)
            {
                float3 result;
                result.r = base.r < 0.5 ? 2.0 * base.r * blend.r : 1.0 - 2.0 * (1.0 - base.r) * (1.0 - blend.r);
                result.g = base.g < 0.5 ? 2.0 * base.g * blend.g : 1.0 - 2.0 * (1.0 - base.g) * (1.0 - blend.g);
                result.b = base.b < 0.5 ? 2.0 * base.b * blend.b : 1.0 - 2.0 * (1.0 - base.b) * (1.0 - blend.b);
                return result;
            }

            float3 blendSoftLight(float3 base, float3 blend)
            {
                float3 result;
                result = blend < 0.5
                    ? 2.0 * base * blend + base * base * (1.0 - 2.0 * blend)
                    : sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend);
                return result;
            }

            float3 blendColorDodge(float3 base, float3 blend)
            {
                return base / max(1.0 - blend, 0.001);
            }

            float3 blendColorBurn(float3 base, float3 blend)
            {
                return 1.0 - (1.0 - base) / max(blend, 0.001);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 base = tex2D(_BaseTex, i.uv);
                float4 blend = tex2D(_BlendTex, i.uv);

                if (blend.a < 0.001)
                    return base;

                float3 result;

                switch(_BlendMode)
                {
                    case 0: // Normal - replace
                        result = blend.rgb;
                        break;
                    case 1: // Add - brighten
                        result = base.rgb + blend.rgb;
                        break;
                    case 2: // Multiply - darken
                        result = base.rgb * blend.rgb;
                        break;
                    case 3: // Screen - lighten
                        result = blendScreen(base.rgb, blend.rgb);
                        break;
                    case 4: // Overlay - contrast
                        result = blendOverlay(base.rgb, blend.rgb);
                        break;
                    case 5: // Soft Light - subtle contrast
                        result = blendSoftLight(base.rgb, blend.rgb);
                        break;
                    case 6: // Color Dodge - bright highlights
                        result = blendColorDodge(base.rgb, blend.rgb);
                        break;
                    case 7: // Color Burn - deep shadows
                        result = blendColorBurn(base.rgb, blend.rgb);
                        break;
                    default:
                        result = blend.rgb;
                        break;
                }

                float effectiveOpacity = _Opacity * blend.a;
                result = lerp(base.rgb, result, effectiveOpacity);

                float finalAlpha = max(base.a, blend.a);

                return fixed4(saturate(result), finalAlpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
