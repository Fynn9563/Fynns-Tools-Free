Shader "Hidden/MCM_RimEffect"
{
    Properties
    {
        _RimLightIntensity ("Rim Light Intensity", Float) = 0.3
        _RimLightBlur ("Rim Light Blur", Float) = 0.5
        _RimLightColor ("Rim Light Color", Color) = (1, 1, 1, 1)
        _RimShadeIntensity ("Rim Shade Intensity", Float) = 0
        _RimShadeBlur ("Rim Shade Blur", Float) = 0.5
        _RimShadeColor ("Rim Shade Color", Color) = (0, 0, 0, 1)
        _Emissive ("Emissive", Float) = 0
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

            float _RimLightIntensity;
            float _RimLightBlur;
            float4 _RimLightColor;
            float _RimShadeIntensity;
            float _RimShadeBlur;
            float4 _RimShadeColor;
            float _Emissive;

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
                float NdotV = saturate(dot(normal, viewDir));

                float rim = 1.0 - NdotV;

                float rimLightPower = max(0.1, 1.0 / max(0.01, _RimLightBlur));
                float rimLight = pow(rim, rimLightPower) * _RimLightIntensity;
                float3 rimLightContrib = _RimLightColor.rgb * rimLight;

                float rimShadePower = max(0.1, 1.0 / max(0.01, _RimShadeBlur));
                float rimShade = pow(rim, rimShadePower) * _RimShadeIntensity;
                float3 rimShadeContrib = _RimShadeColor.rgb * rimShade;

                float3 emissiveContrib = float3(_Emissive, _Emissive, _Emissive) * (1.0 - NdotV * 0.5);

                float3 result = rimLightContrib - rimShadeContrib + emissiveContrib;
                float alpha = max(rimLight, rimShade);

                return fixed4(result, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
