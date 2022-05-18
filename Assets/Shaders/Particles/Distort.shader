Shader "Game/Particles/Distort"
{
	Properties
	{
		_DistortTex("DistortTex",2D) = "white"{}
		_DistortStrength("Distort Strength",Range(0,0.1))=.005
	}
	SubShader
	{
		Tags{"Queue" = "Transparent"}
		Pass
		{		
			Cull Back
			Blend Off
			ZWrite On
			
			name "Main"
			HLSLPROGRAM
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Shaders/Library/Common.hlsl"
			struct a2v
			{
				float3 positionOS : POSITION;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD0;
				float4 screenPos:TEXCOORD1;
			};
			TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
			TEXTURE2D(_DistortTex);SAMPLER(sampler_DistortTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_DistortTex_ST)
				INSTANCING_PROP(float,_DistortStrength)
			INSTANCING_BUFFER_END
			v2f vert(a2v v)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv =  TRANSFORM_TEX(v.uv,_DistortTex);
				o.screenPos = ComputeScreenPos(o.positionCS);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 baseUV= i.screenPos.xy/i.screenPos.w;
				float2 distort = (SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex,i.uv).rg*2-1) * _DistortStrength;
				return  SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, baseUV + distort);
			}
			ENDHLSL
		}
	}
}
