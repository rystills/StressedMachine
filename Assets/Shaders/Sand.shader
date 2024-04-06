Shader "Custom/Sand"
{
    Properties
    {
        strength("strength",float) = 0
        isPurple("isPurple",float) = 0
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
            float isPurple;

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

            float2 distance(float2 p1, float2 p2) {
                return sqrt(dot(p1 - p2, p1 - p2));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // config
                float aspectRatio = _ScreenParams.x / (float)_ScreenParams.y;
                float2 correctedUv = float2(i.uv.x, 1 - i.uv.y / aspectRatio);
                #define particleRad .04
                #define numParts 400.
                
                // calculate total overlap percentage
                float overlapPerc = 0;
                int start = max(0, correctedUv.x - particleRad) * numParts;
                int end = min(numParts, start + 2 * particleRad * numParts);
                for (int k = start; k < end; ++k) {
                    float2 center = float2(k/numParts, (2100 + _Time[0]) * (2 + .0002 * UNITY_PI * (pow(numParts/2 - k, 2))) % (1 + particleRad));
                    overlapPerc += max(0, particleRad - distance(center, correctedUv));
                }
                clip(overlapPerc == 0 ? -1 : 1);
                overlapPerc *= (1/particleRad);
                
                // color
                float3 col = isPurple ? float3(overlapPerc, (overlapPerc - 1) * .6, overlapPerc)
                                      : float3(overlapPerc, overlapPerc, (overlapPerc - 1) * .6);
                return fixed4(col.xyz, strength * strength * overlapPerc);
            }
            ENDCG
        }
    }
}
