Shader "Custom/StatsOverlay"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float startTime;
            float animOutDir;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fac = sin(_Time[3] *.3);
                fac = fac * fac;
                // 'wiper' moving between botleft and topright + thin border outline
                return fixed4(0, 0, 0, .5 - min(sqrt(dot(fac - i.uv, fac - i.uv)), 20 * min(min(min(i.uv.y, 1 - i.uv.y), i.uv.x), 1 - i.uv.x)));
            }
            ENDCG
        }
    }
}
