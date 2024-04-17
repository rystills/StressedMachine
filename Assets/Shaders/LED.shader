Shader "Custom/LED"
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

            float distance(float2 p1, float2 p2) {
                return sqrt(dot(p1 - p2, p1 - p2));
            }

            // defines
            #define particleRad .3
            #define numParts 18
            uniform float _PartColInds[numParts];

            fixed4 frag (v2f i) : SV_Target
            {
                // config
                float aspectRatio = _ScreenParams.x / (float)_ScreenParams.y;
                float2 correctedUv = float2(i.uv.x, 1 - i.uv.y / aspectRatio);
                float3 colors[3] = { float3(1,0,0), float3(0,1,0), float3(0,0,1) };
                #define orbitRadius .2

                // calculate total overlap color
                float3 overlapPerc = float3(0,0,0);
                for (int i = 0; i < numParts; ++i) {
                    // evenly spaced x, 'random' y
                    float2 basePos = float2((i + .5f) / numParts,
                                            ((i + .5f) / numParts * 12) % (1 + particleRad));

                    // 'random' orbit around base position
                    float rotSpeed = (i+1) * 6.37 % 10;
                    float theta = rotSpeed * _Time[1];
                    float2 center = float2(basePos.x + orbitRadius * cos(theta), basePos.y + orbitRadius * sin(theta));
                    
                    overlapPerc += max(0, particleRad - distance(center, correctedUv)) * colors[(int)_PartColInds[i]];
                }
                clip(length(overlapPerc) - .0000001);
                overlapPerc *= (1/particleRad);

                return fixed4(overlapPerc, strength * strength * pow(length(overlapPerc), 2));
            }
            ENDCG
        }
    }
}
