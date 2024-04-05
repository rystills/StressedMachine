Shader "Custom/Wave"
{
    Properties
    {
        strength("strength",float) = 0
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

            float strength;

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
                // slowly fade in oscillating gradient
                float adjY = (1 - abs(.5 - i.uv.y)) * sqrt(strength) + sin(i.uv.x + _Time[3]) * .2f;
                float3 col = float3(adjY, adjY * 2, .25f + 2*adjY);
                return fixed4(col.xyz, strength * strength);
            }
            ENDCG
        }
    }
}
