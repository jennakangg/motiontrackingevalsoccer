Shader "UI/HDRContrastShaderGray"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Contrast("Contrast", Float) = 1.0 // Control contrast adjustment
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float _Contrast;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the grayscale texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Discard fully transparent pixels
                if (texColor.a < 0.01) discard;

                // Apply contrast adjustment to the grayscale value
                float midpoint = 0.2; // Midpoint for contrast adjustment
                float adjustedGray = (texColor.r - midpoint) * _Contrast + midpoint;

                // Clamp the grayscale value to the valid range [0, 1]
                adjustedGray = saturate(adjustedGray);

                return fixed4(adjustedGray, adjustedGray, adjustedGray, texColor.a);
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}