Shader "Hidden/Fynn/SDKCaptureBackground"
{
    Properties { _MainTex ("Capture", 2D) = "black" {} _Background ("Background", 2D) = "white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _Background;
            float4 _BackgroundScale;
            v2f_img vert(appdata_img v)
            {
                v2f_img o;
                #if defined(UNITY_REVERSED_Z)
                o.pos = float4(v.vertex.xy, 0, 1);
                #else
                o.pos = float4(v.vertex.xy, 1, 1);
                #endif
                o.uv = v.texcoord;
                #if UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1 - o.uv.y;
                #endif
                return o;
            }
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 background = tex2D(_Background, (i.uv - 0.5) * _BackgroundScale.xy + 0.5);
                return fixed4(background.rgb, 1);
            }
            ENDCG
        }
    }
}
