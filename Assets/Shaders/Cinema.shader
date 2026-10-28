Shader "Hidden/MongeArcano/Cinema"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Vinheta;
            float _Saturacao;
            float _Contraste;
            float _Bloom;
            float _Grao;
            float _Tempo;
            float4 _Sombra;
            float4 _Realce;

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            float3 brilho(float2 uv, float2 o)
            {
                float3 s = tex2D(_MainTex, uv + o).rgb;
                return max(s - 0.62, 0.0);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float3 c = tex2D(_MainTex, uv).rgb;

                float2 px = _MainTex_TexelSize.xy * 3.0;
                float3 b = brilho(uv, float2(px.x, 0.0)) + brilho(uv, float2(-px.x, 0.0))
                         + brilho(uv, float2(0.0, px.y)) + brilho(uv, float2(0.0, -px.y))
                         + brilho(uv, float2(px.x, px.y) * 0.7) + brilho(uv, float2(-px.x, px.y) * 0.7)
                         + brilho(uv, float2(px.x, -px.y) * 0.7) + brilho(uv, float2(-px.x, -px.y) * 0.7);
                float2 px2 = px * 2.6;
                b += brilho(uv, float2(px2.x, 0.0)) + brilho(uv, float2(-px2.x, 0.0))
                   + brilho(uv, float2(0.0, px2.y)) + brilho(uv, float2(0.0, -px2.y));
                c += (b / 12.0) * _Bloom * 2.2;

                float lum = dot(c, float3(0.299, 0.587, 0.114));
                c = lerp(float3(lum, lum, lum), c, _Saturacao);
                c = (c - 0.5) * _Contraste + 0.5;
                float3 tom = lerp(_Sombra.rgb, _Realce.rgb, saturate(lum * 1.3));
                c *= tom;

                float d = distance(uv, float2(0.5, 0.5));
                float v = smoothstep(0.82, 0.22, d * _Vinheta);
                c *= lerp(0.25, 1.0, v);

                c += (rand(uv * _ScreenParams.xy + _Tempo) - 0.5) * _Grao;
                return fixed4(saturate(c), 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
