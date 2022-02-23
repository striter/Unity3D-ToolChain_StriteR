Shader "Game/Unlit/Texture"
{
    Properties
    {
        [NoScaleOffset]_MainTex("Main Texture",2D)="white"{}
        _HueShift("Hue Shift",Range(-180,180))=0
        _Saturation("Saturation",Range(-100,100))=0
        _Brightness("Brightness",Range(-100,100))=0
        [Toggle(_FOGOFF)]_Enable("Fog Off",int)=0
    }
    
    SubShader
    {
        Tags { "Queue"="Geometry" }
        Blend Off
        ZWrite On
        ZTest Less
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _FOGOFF
            #pragma multi_compile_fog
            #if _FOGOFF
                #define NFOG
            #endif
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Additional/Algorithms/HSL.hlsl"
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            struct a2v
            {
                half3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                V2F_FOG(1)
            };

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                FOG_TRANSFER(o);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 col= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb;
                col=HSL(col);
                FOG_MIX(i,col)
                return half4(col,1);
            }
            ENDHLSL
        }
    }
}
