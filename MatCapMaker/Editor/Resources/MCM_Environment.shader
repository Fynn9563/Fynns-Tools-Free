Shader "Hidden/MCM_Environment"
{
    Properties
    {
        _EnvMap ("Environment Map", 2D) = "black" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Float) = 1
        _UseNormalMap ("Use Normal Map", Float) = 0
        _Intensity ("Intensity", Float) = 1
        _Blur ("Blur", Float) = 0
        _Rotation ("Rotation", Float) = 0
        _UVMode ("UV Mode", Int) = 1
        _Border ("Border", Float) = 0.5
        _OffsetX ("Offset X", Float) = 0
        _OffsetY ("Offset Y", Float) = 0
        _TilingX ("Tiling X", Float) = 1
        _TilingY ("Tiling Y", Float) = 1
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

            sampler2D _EnvMap;
            sampler2D _NormalMap;
            float4 _EnvMap_TexelSize;
            float _NormalStrength;
            float _UseNormalMap;
            float _Intensity;
            float _Blur;
            float _Rotation;
            int _UVMode;
            float _Border;
            float _OffsetX;
            float _OffsetY;
            float _TilingX;
            float _TilingY;

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

            float2 rotateUV(float2 uv, float angle)
            {
                float2 center = float2(0.5, 0.5);
                float2 centered = uv - center;

                float rad = angle * 3.14159265 / 180.0;
                float cosA = cos(rad);
                float sinA = sin(rad);

                float2 rotated;
                rotated.x = centered.x * cosA - centered.y * sinA;
                rotated.y = centered.x * sinA + centered.y * cosA;

                return rotated + center;
            }

            // Calculate matcap UV based on mode (inspired by Poiyomi shader)
            float2 getMatcapUV(float3 normal, int uvMode, float border)
            {
                float3 viewDir = float3(0, 0, 1);
                float2 matcapUV = float2(0, 0);

                switch(uvMode)
                {
                    // Mode 0: UTS Style - view-space normal with skew correction
                    case 0:
                    {
                        float3 viewNormal = normal * float3(-1, -1, 1);
                        float3 viewBase = viewDir * float3(-1, -1, 1) + float3(0, 0, 1);
                        float3 noSkewNormal = viewBase * dot(viewBase, viewNormal) / viewBase.z - viewNormal;
                        matcapUV = noSkewNormal.xy * border + 0.5;
                        break;
                    }
                    // Mode 1: Top Pinch - standard matcap using world up/right
                    case 1:
                    {
                        float3 worldViewUp = normalize(float3(0, 1, 0) - viewDir * dot(viewDir, float3(0, 1, 0)));
                        float3 worldViewRight = normalize(cross(viewDir, worldViewUp));
                        matcapUV = float2(dot(worldViewRight, normal), dot(worldViewUp, normal)) * border + 0.5;
                        break;
                    }
                    // Mode 2: Double Sided - uses reflection vector
                    case 2:
                    {
                        float3 reflection = reflect(-viewDir, normal);
                        matcapUV = float2(dot(reflection, float3(1, 0, 0)), dot(reflection, float3(0, 1, 0))) * border + 0.5;
                        break;
                    }
                    // Mode 3: Gradient/Fresnel - based on view angle (1D gradient)
                    case 3:
                    {
                        float fresnel = 1.0 - abs(dot(normal, viewDir));
                        matcapUV = float2(fresnel, fresnel);
                        break;
                    }
                    case 6:
                    {
                        float3 reflected = reflect(-viewDir, normal);
                        matcapUV = float2(atan2(reflected.z,reflected.y)/6.2831853+.5,asin(clamp(reflected.x,-1,1))/3.14159265+.5);
                        break;
                    }
                    // Mode 4: Direct Normal - simple normal.xy to UV (original method)
                    default:
                    {
                        matcapUV = normal.xy * border + 0.5;
                        break;
                    }
                }

                return matcapUV;
            }

            float3 perturbNormal(float3 normal, float2 normalMapUV)
            {
                float4 normalSample = tex2D(_NormalMap, normalMapUV);

                float3 tangentNormal;
                tangentNormal.xy = normalSample.wy * 2.0 - 1.0;
                if (normalSample.w < 0.01)
                {
                    tangentNormal.xy = normalSample.xy * 2.0 - 1.0;
                }
                tangentNormal.xy *= _NormalStrength;
                tangentNormal.z = sqrt(saturate(1.0 - dot(tangentNormal.xy, tangentNormal.xy)));

                float3 up = abs(normal.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
                float3 tangent = normalize(cross(up, normal));
                float3 bitangent = cross(normal, tangent);

                float3 perturbedNormal = tangent * tangentNormal.x + bitangent * tangentNormal.y + normal * tangentNormal.z;
                return normalize(perturbedNormal);
            }

            float2 applyUVTransforms(float2 baseUV)
            {
                float2 offsetUV = baseUV + float2(_OffsetX, _OffsetY);

                float2 rotatedUV = rotateUV(offsetUV, _Rotation);

                float2 tiledUV = (rotatedUV - 0.5) * float2(_TilingX, _TilingY) + 0.5;

                if (_TilingX > 1.0 || _TilingY > 1.0)
                {
                    return frac(tiledUV);
                }
                else
                {
                    return clamp(tiledUV, 0.001, 0.999);
                }
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = getSphereNormal(i.uv);

                if (length(normal) < 0.001)
                    return fixed4(0, 0, 0, 0);

                float2 baseUV = getMatcapUV(normal, _UVMode, _Border);

                float2 envUV = applyUVTransforms(baseUV);

                if (_UseNormalMap > 0.5)
                {
                    normal = perturbNormal(normal, envUV);
                    baseUV = getMatcapUV(normal, _UVMode, _Border);
                    envUV = applyUVTransforms(baseUV);
                }

                float4 envColor;

                if (_Blur > 0.01)
                {
                    float blurSize = _Blur * 0.02;
                    envColor = float4(0, 0, 0, 0);

                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            float2 offset = float2(x, y) * blurSize;
                            float2 sampleUV = clamp(envUV + offset, 0.001, 0.999);
                            envColor += tex2D(_EnvMap, sampleUV);
                        }
                    }
                    envColor /= 25.0;
                }
                else
                {
                    envColor = tex2D(_EnvMap, envUV);
                }

                float3 result = envColor.rgb * _Intensity;

                return fixed4(result, envColor.a * _Intensity);
            }
            ENDCG
        }
    }
    Fallback Off
}
