Shader "Custom/Death"
{
    Properties
    {
        startTime("startTime",float) = 0
        animOutDir("animOutDir",float) = 0
    }
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
                float timeFrac = (_Time[1] - startTime) / animOutDir;

                // fade to/from white
                if (timeFrac <= .5f) {
                    return fixed4(1, 1, 1, timeFrac * 2);
                }
                return fixed4(1, 1, 1, 1 - (timeFrac - .5f) * 2);
            }
            ENDCG
        }
    }
}
