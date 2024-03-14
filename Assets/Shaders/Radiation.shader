Shader "Custom/Radiation"
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

            // https://www.shadertoy.com/view/4djSRW
            float3 hash32(float2 p)
            {
	            float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
                p3 += dot(p3, p3.yxz + 33.33);
                return frac((p3.xxy + p3.yzz) * p3.zyx);
            }

            float3 GenNoise(float2 uv) {
                float2 pos = (uv * .152 + _Time[1] % 1 * 80 + 900);
                return hash32(pos.xy);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // slowly fade in noise, then transition to solid color
                float3 noise = GenNoise(i.uv);
                float facStr = max(strength - .8, 0) * 2.5f;
                float3 col = lerp(noise, float3(facStr, 0, facStr/2), facStr * facStr);
                return fixed4(col.xyz, strength * strength);
            }
            ENDCG
        }
    }
}
