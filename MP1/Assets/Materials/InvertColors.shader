Shader "Custom/InvertColors"
{
    Properties
    {
        _Alpha ("Strength", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        ZWrite Off
        ZTest Always
        Blend OneMinusDstColor OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f    { float4 pos : SV_POSITION; };

            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // Blend mode (OneMinusDst, OneMinusSrc) achieves full inversion:
            // result = (1 - dst) * src_alpha + dst * (1 - src_alpha)
            // With src = (1,1,1) at full alpha this becomes: result = 1 - dst
            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, _Alpha);
            }
            ENDCG
        }
    }
}
