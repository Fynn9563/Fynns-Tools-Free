Shader "Hidden/MCM_EnvironmentCubemap"
{
    Properties
    {
        _CubeMap ("Cubemap", Cube) = "" {}
        _Intensity ("Intensity", Float) = 1
        _Blur ("Blur/Smoothness", Float) = 1
        _Rotation ("Rotation", Vector) = (0, 0, 0, 0)
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

            samplerCUBE _CubeMap;
            float _Intensity;
            float _Blur;
            float3 _Rotation;

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

            // Rotate around Y axis (from Poiyomi)
            float3 RotateAroundYInDegrees(float3 dir, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, dir.xz), dir.y).xzy;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = getSphereNormal(i.uv);

                if (length(normal) < 0.001)
                    return fixed4(0, 0, 0, 0);

                float3 viewDir = float3(0, 0, 1);
                float3 reflDir = reflect(-viewDir, normal);

                if (any(_Rotation.xyz))
                {
                    reflDir = RotateAroundYInDegrees(reflDir.yxz, _Rotation.x).yxz;
                    reflDir = RotateAroundYInDegrees(reflDir.xyz, _Rotation.y).xyz;
                    reflDir = RotateAroundYInDegrees(reflDir.xzy, _Rotation.z).xzy;
                }

                float mipLevel = (1.0 - _Blur) * (1.0 - _Blur) * 8.0;
                float4 cubeColor = texCUBElod(_CubeMap, float4(reflDir, mipLevel));

                float3 result = cubeColor.rgb * _Intensity;

                return fixed4(result, cubeColor.a * _Intensity);
            }
            ENDCG
        }
    }
    Fallback Off
}
