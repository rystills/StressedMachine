Shader "Custom/Pillar"
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


            // defines
            #define numPillars 9
            #define particleRad .6
            uniform float _PillarHeights[numPillars];
            static const float PI = 3.14159265f;

            float distance(float2 p1, float2 p2, int k) {
                return sqrt(dot((p1 - p2) * 10, (p1 - p2)));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // config
                float aspectRatio = _ScreenParams.x / _ScreenParams.y;
                float2 correctedUv = float2(i.uv.x, 1 - i.uv.y / aspectRatio);

                // calculate total overlap percentage
                float overlapPerc = 0;
                for (int k = 0; k < numPillars; ++k) {
                    float ctime = cos(_PillarHeights[k] + _Time[3]);
                    if (ctime < PI / 2) ctime = pow(ctime, 4);
                    else ctime = pow(ctime, .25);
                    float2 center = float2((k + 0.5) / numPillars, ctime * 0.25f + 0.75f);
                    overlapPerc += max(0, particleRad - distance(center, correctedUv, k)) * _PillarHeights[k];
                }
                clip(overlapPerc - .0000001);
                overlapPerc *= (1/particleRad);

                // color
                #define green float3((overlapPerc - 1) * .7, overlapPerc, (overlapPerc - 1) * .5)
                #define tan float3(overlapPerc, (overlapPerc - 1) * .3, (overlapPerc - 1) * .1)
                float3 col = lerp(green, tan, i.uv.y);
                return fixed4(col, pow(max(0, (strength - .5) * 2), 4) * overlapPerc);
            }
            ENDCG
        }
    }
}
