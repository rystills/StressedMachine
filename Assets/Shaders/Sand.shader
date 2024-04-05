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
                float aspectRatio = _ScreenParams.x / (float)_ScreenParams.y;
                #define particleRad .02
                #define doubleNumParts 50.

                int cell = 1 + int(i.uv.x * doubleNumParts);
                cell = (cell + (cell % 2 == 0 ? 0 : -1));
                float cellX = cell / doubleNumParts;
                float cellY = (_Time[3] * (.6 + .0006 * UNITY_PI * (pow(doubleNumParts/2 - cell,2)))) % (1 / aspectRatio);
                
                float dist = distance(float2(cellX, (1 / aspectRatio) - cellY), float2(i.uv.x, i.uv.y / aspectRatio));
                clip(dist > particleRad ? -1 : 1);
                float distPerc = 1 - (dist/particleRad);

                float3 col = isPurple ? float3(distPerc, 0, distPerc)
                                      : float3(distPerc, distPerc, 0);
                return fixed4(col.xyz, strength * strength * distPerc);
            }
            ENDCG
        }
    }
}
