Shader "Game/Effects/DissolveEdge"
{
	Properties
	{
		_DissolveAmount("_Dissolve Amount",Range(0,1)) = 1
		_DissolveTex("Dissolve Map",2D) = "white"{}
		_DissolveWidth("_Dissolve Width",Range(0,1)) = .1
		[HDR]_DissolveColor("_Dissolve Color",Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags{"RenderType" = "Dissolve"  "Queue" = "Geometry"}
		Cull Off

		HLSLINCLUDE
		#include "Assets/Shaders/Library/Common.hlsl"
		#include "Assets/Shaders/Library/Lighting.hlsl"

		TEXTURE2D(_DissolveTex);SAMPLER(sampler_DissolveTex);
		INSTANCING_BUFFER_START
		INSTANCING_PROP(float4,_DissolveTex_ST)
		INSTANCING_PROP(float,_DissolveAmount)
		INSTANCING_PROP(float,_DissolveWidth)
		INSTANCING_PROP(float4,_DissolveColor)
		INSTANCING_BUFFER_END
		ENDHLSL
		
		Pass
		{
			NAME "EDGE"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			struct a2v
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				o.uv = TRANSFORM_TEX_INSTANCE(v.positionOS.xz,_DissolveTex);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float dissolve = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex,i.uv).r - INSTANCE(_DissolveAmount);
				clip(step(0,dissolve)*step( dissolve, INSTANCE(_DissolveWidth) )-0.01);
				return INSTANCE( _DissolveColor);
			}
			ENDHLSL
		}
	}
}
