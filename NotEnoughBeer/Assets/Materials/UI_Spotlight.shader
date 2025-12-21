Shader "UI/SpotlightOverlay"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (0,0,0,0.75)
        _Center ("Center (0-1)", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius", Range(0,1)) = 0.2
        _Softness ("Softness", Range(0.001,1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // afstand i screen-UV (0..1)
                float d = distance(i.uv, _Center.xy);

                // 0 inde i radius, 1 udenfor (med blød kant)
                float edge0 = _Radius;
                float edge1 = _Radius + _Softness;
                float mask = smoothstep(edge0, edge1, d);

                // mask=0 => gennemsigtig (hul), mask=1 => fuldt overlay
                float alpha = _Color.a * mask;

                return float4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
