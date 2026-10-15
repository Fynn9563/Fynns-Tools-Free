Shader "Hidden/MCM_SpotLight"
{
    Properties
    {
        _LightPos ("Light Position (xyz) + Intensity (w)", Vector) = (0.5, 0.5, 1, 1)
        _LightColor ("Light Color", Color) = (1, 1, 1, 1)
        _Range ("Range", Float) = 2
        _Blur ("Blur", Float) = 0.5
        _SpecularEnabled ("Specular Enabled", Float) = 1
        _SpecularIntensity ("Specular Intensity", Float) = 0.5
        _SpecularSize ("Specular Size", Float) = 1
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

            float4 _LightPos;
            float4 _LightColor;
            float _Range;
            float _Blur;
            float _SpecularEnabled;
            float _SpecularIntensity;
            float _SpecularSize;

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

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = getSphereNormal(i.uv);

                if (length(normal) < 0.001)
                    return fixed4(0, 0, 0, 0);

                float3 viewDir = float3(0, 0, 1);
                float3 lightDir = normalize(_LightPos.xyz);
                float intensity = _LightPos.w;

                float NdotL = dot(normal, lightDir);

                float blurAmount = max(0.001, _Blur);
                float diffuse = saturate((NdotL + blurAmount) / (1.0 + 2.0 * blurAmount));

                diffuse = pow(diffuse, lerp(2.0, 0.5, blurAmount));

                float rangeFactor = saturate(_Range);
                diffuse *= lerp(0.3, 1.0, rangeFactor);

                float specular = 0;
                if (_SpecularEnabled > 0.5)
                {
                    float3 halfVec = normalize(lightDir + viewDir);
                    float NdotH = saturate(dot(normal, halfVec));

                    float specPower = lerp(256.0, 8.0, saturate(_SpecularSize));
                    specular = pow(NdotH, specPower) * _SpecularIntensity;

                    if (_SpecularSize < 0.5)
                        specular *= 2.0;
                }

                float3 diffuseColor = _LightColor.rgb * intensity * diffuse;
                float3 specularColor = _LightColor.rgb * specular;

                float contribution = saturate(diffuse + specular);

                return fixed4(diffuseColor + specularColor, contribution);
            }
            ENDCG
        }
    }
    Fallback Off
}
