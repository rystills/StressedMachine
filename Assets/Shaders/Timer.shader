Shader "Custom/Timer"
{
    Properties
    {
        stateCompletion("stateCompletion",float) = 0
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

            float stateCompletion;

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
                // config
                float aspectRatio = _ScreenParams.x / (float)_ScreenParams.y;

                if ((i.uv.x > .01 && i.uv.x < .99 && i.uv.y > .01 * aspectRatio && i.uv.y < 1 - (.01 * aspectRatio))
                 || (i.uv.x < stateCompletion * 4               && i.uv.y >= 1 - (.01 * aspectRatio) && i.uv.x > .01 && i.uv.x < .99)
                 || (i.uv.y > 1 - ((stateCompletion - .25) * 4) && i.uv.x >= .99                     && i.uv.y > .01 * aspectRatio)
                 || (i.uv.x > 1 - ((stateCompletion - .5) * 4)  && i.uv.y <= .01 * aspectRatio       && i.uv.x > .01)
                 || (i.uv.y < (stateCompletion - .75) * 4       && i.uv.x <= .01))
                    clip(-1);
                return fixed4(1, 1, 0, .2 - 20 * min(min(min(i.uv.x, i.uv.y / aspectRatio), 1 - i.uv.x), (1 - i.uv.y) / aspectRatio));
            }
            ENDCG
        }
    }
}
