Shader "Unlit/StippleTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale",Range(1,10))=1
        _Transparency("Transparency",Range(0,1))=.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionHCS:TEXCOORD1;
            };

            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half _Transparency;
            half _Scale;
            CBUFFER_END
            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS=o.positionCS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                half2 positionNDC=TransformHClipToNDC(i.positionHCS);
                float2 screenPos=positionNDC*_ScreenParams.xy;
                screenPos/=_Scale;
                clip(_Transparency-dither01(screenPos) );
                float4 col = tex2D(_MainTex, i.uv);
                col=float4(positionNDC,0,1);
                return col;
            }
            ENDHLSL
        }
    }
}
