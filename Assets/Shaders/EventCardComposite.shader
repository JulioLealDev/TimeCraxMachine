Shader "TimeCrax/EventCardComposite"
{
    Properties
    {
        _MainTex ("Template (Card Frame with Alpha)", 2D) = "white" {}
        _ImageTex ("Event Image", 2D) = "white" {}
        _ImageScale ("Image Scale", Vector) = (1, 1, 0, 0)
        _ImageOffset ("Image Offset", Vector) = (0, 0, 0, 0)
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Color ("Tint Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _ImageTex;
        float4 _ImageScale;
        float4 _ImageOffset;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;

            // UV ajustado para a imagem (com scale e offset)
            float2 imageUV = uv * _ImageScale.xy + _ImageOffset.xy;

            // Event image (background) com UV ajustado
            fixed4 imageColor = tex2D(_ImageTex, imageUV);

            // Template texture (card frame overlay)
            fixed4 templateColor = tex2D(_MainTex, uv);

            // Blend: where template is transparent, show image
            // where template is opaque, show template
            fixed4 finalColor = lerp(imageColor, templateColor, templateColor.a);

            // Apply tint
            finalColor *= _Color;

            o.Albedo = finalColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Standard"
}
