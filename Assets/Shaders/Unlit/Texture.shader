Shader "Game/Unlit/Texture"
{
    Properties
    {
        [NoScaleOffset]_MainTex("Main Texture",2D)="white"{}
        [Toggle(_HSL)]_HSL("HSL",int)=0
        [Foldout(_HSL)]_HueShift("Hue Shift",Range(-180,180))=0
        [Foldout(_HSL)]_Saturation("Saturation",Range(-100,100))=0
        [Foldout(_HSL)]_Brightness("Brightness",Range(-100,100))=0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend Off
        ZWrite Off
        ZTest Less
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _HSB
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
            };

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                #if _HSL
                    col.rgb=HSL(col.rgb);
                #endif
                return col;
            }
            ENDHLSL
        }
    }
}
