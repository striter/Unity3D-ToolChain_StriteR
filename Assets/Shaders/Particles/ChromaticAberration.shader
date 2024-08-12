Shader "Game/Particles/ChromaticAberration"
{
	Properties
	{
		_DistortTex("DistortTex",2D) = "white"{}
		_MaskTex("Mask",2D) = "white"{}
		_DistortStrength("Distort Strength",Range(0,100))=.005
		_AberrationStrength("Aberration Strength",Range(0,20)) = 0.005
		[Toggle(_VERTEXSTREAM)]_VertexStream("Vertex Stream",int) = 0
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
			#pragma shader_feature _VERTEXSTREAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Shaders/Library/Common.hlsl"
			struct a2v
			{
				float3 positionOS : POSITION;
				float4 color : COLOR;
				float4 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uvDistort:TEXCOORD0;
				float2 customData:TEXCOORD1;
				float4 screenPos:TEXCOORD2;
				float2 uvMask:TEXCOORD3;
				float4 color : COLOR;
			};
			TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
			TEXTURE2D(_DistortTex);SAMPLER(sampler_DistortTex);
			TEXTURE2D(_MaskTex);SAMPLER(sampler_MaskTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_CameraOpaqueTexture_TexelSize)
				INSTANCING_PROP(float4,_DistortTex_ST)
				INSTANCING_PROP(float4,_MaskTex_ST)
				INSTANCING_PROP(float,_DistortStrength)
				INSTANCING_PROP(float,_AberrationStrength)
			INSTANCING_BUFFER_END
			v2f vert(a2v v)
			{
				v2f o;
				o.color = v.color;
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uvDistort = TRANSFORM_TEX(v.uv.xy,_DistortTex);
				o.screenPos = ComputeScreenPos(o.positionCS);
				o.uvMask = TRANSFORM_TEX(v.uv,_MaskTex);
				#if _VERTEXSTREAM
					o.customData = v.uv.zw;
				#else
					o.customData = 1;
				#endif
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 mask = SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,i.uvMask).rg;
				float2 baseUV= i.screenPos.xy/i.screenPos.w;
				float2 distort = (SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex,i.uvDistort).rg*2-1) * _DistortStrength * _CameraOpaqueTexture_TexelSize.xy * i.customData.x * mask.r;
				float2 abberation = _AberrationStrength * _CameraOpaqueTexture_TexelSize.xy * i.customData.y * mask.g;
				float r = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, baseUV + (distort+abberation)).r;
				float g = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, baseUV + distort).g;
				float b = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, baseUV + (distort-abberation)).b;
				return float4(r,g,b,1) * i.color ;
			}
			ENDHLSL
		}
	}
}
