Shader "Hidden/Imposter_AlbedoAlpha"
{
    Properties
    {
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaCutoff("Range",Range(0.01,1))=0.01
        _MainTex("_MainTex",2D) = "white"
    }
    SubShader
    {
        Blend Off
        Cull Back
        ZWrite On
        ZTest LEqual
        HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "../Imposter.hlsl"
            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			#pragma shader_feature_local_fragment _ALPHACLIP
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float,_AlphaCutoff)
            INSTANCING_BUFFER_END
            
			#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float4 sample = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                AlphaClip(sample.a);
                return float4(sample.rgb,1);
            }
		ENDHLSL
        
		Tags{"LightMode" = "UniversalForward"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
