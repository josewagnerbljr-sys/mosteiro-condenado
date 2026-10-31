Shader "MongeArcano/PinturaVC"
{
    // Pintura de miniatura: o brilho (cavidades escuras, arestas claras) e a máscara de realce vêm gravados no UV0 de cada vértice.
    Properties
    {
        _MainTex ("(não usado)", 2D) = "white" {}
        _Color ("Cor base", Color) = (0.3, 0.25, 0.35, 1)
        _Color2 ("Cor do realce", Color) = (0.9, 0.7, 0.3, 1)
        _Glossiness ("Suavidade", Range(0,1)) = 0.25
        _Metallic ("Metal", Range(0,1)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        fixed4 _Color; fixed4 _Color2; half _Glossiness; half _Metallic;
        struct Input { float2 uv_MainTex; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float v = saturate(IN.uv_MainTex.x);
            float a = saturate(IN.uv_MainTex.y);
            float3 c = lerp(_Color.rgb, _Color2.rgb, a);
            o.Albedo = c * (0.35 + 0.9 * v);
            o.Metallic = lerp(_Metallic, 0.65, a);
            o.Smoothness = lerp(_Glossiness, 0.55, a);
        }
        ENDCG
    }
    Fallback "Diffuse"
}
