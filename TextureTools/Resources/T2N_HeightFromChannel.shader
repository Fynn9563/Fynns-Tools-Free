Shader "Hidden/T2N_HeightFromChannel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Channel ("Channel", Float) = 4
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

            sampler2D _MainTex;
            float _Channel;

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata_img v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float h = (_Channel == 0) ? c.r
                        : (_Channel == 1) ? c.g
                        : (_Channel == 2) ? c.b
                        : (_Channel == 3) ? c.a
                        : dot(c.rgb, float3(0.299,0.587,0.114));
                return fixed4(h, 0, 0, 0);
            }
            ENDCG
        }
    }
    Fallback Off
}
