Shader "Hidden/MCM_Noise"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _Strength ("Strength", Float) = 0.5
        _Blur ("Blur", Float) = 0
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

            sampler2D _NoiseTex;
            float _Strength;
            float _Blur;
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

            bool isInSphere(float2 uv)
            {
                float2 centered = uv * 2.0 - 1.0;
                float r2 = dot(centered, centered);
                return r2 <= 1.0;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (!isInSphere(i.uv))
                    return fixed4(0, 0, 0, 0);

                float2 tiledUV = i.uv * float2(_TilingX, _TilingY);

                float2 offset = float2((_TilingX - 1.0) * 0.5, (_TilingY - 1.0) * 0.5);
                tiledUV -= offset;

                float4 noiseColor;

                if (_Blur > 0.01)
                {
                    float blurSize = _Blur * 0.01;
                    noiseColor = float4(0, 0, 0, 0);

                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            float2 blurOffset = float2(x, y) * blurSize;
                            noiseColor += tex2D(_NoiseTex, tiledUV + blurOffset);
                        }
                    }
                    noiseColor /= 25.0;
                }
                else
                {
                    noiseColor = tex2D(_NoiseTex, tiledUV);
                }

                float gray = dot(noiseColor.rgb, float3(0.299, 0.587, 0.114));

                float3 result = noiseColor.rgb * _Strength;

                return fixed4(result, gray * _Strength);
            }
            ENDCG
        }
    }
    Fallback Off
}
