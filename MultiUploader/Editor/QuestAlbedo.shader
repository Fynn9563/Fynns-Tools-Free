Shader "Hidden/FynnsTools/MultiUploader/Albedo"
{
    Properties { _MainTex("Texture", 2D) = "white" {} _Color("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _Color;
            fixed4 frag(v2f_img i) : SV_Target { return tex2D(_MainTex, i.uv) * _Color; }
            ENDCG
        }
    }
}
