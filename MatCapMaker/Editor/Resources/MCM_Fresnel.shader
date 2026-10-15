Shader "Hidden/MCM_Fresnel"
{
    Properties
    {
        _FresnelPower ("Fresnel Power", Float) = 3
        _FresnelColor ("Fresnel Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Float) = 1
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

            float _FresnelPower;
            float4 _FresnelColor;
            float _Intensity;

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

                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                float3 result = _FresnelColor.rgb * fresnel * _Intensity;

                return fixed4(result, fresnel);
            }
            ENDCG
        }
    }
    Fallback Off
}
