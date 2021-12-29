Shader "Game/UI/SpriteUber"
{   
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		[Header(Additional)]
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[Toggle(_ALPHAMASK)]_Mask("Alpha Mask",int)=0
		
		[Toggle(_NOISEMASK)]_NoiseMask("Noise Mask",int)=0
		[Foldout(_NOISEMASK)]_NoiseTex("NoiseTex",2D)="white"{}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#define ICOLOR
			#include "Assets/Shaders/Library/Common.hlsl"

			#pragma shader_feature_local_fragment _ALPHAMASK
			#pragma shader_feature_local _NOISEMASK

			struct appdata_t
			{
				float3 positionOS   : POSITION;
				float4 color    : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS   : SV_POSITION;
				float4 color : COLOR;
				half2 uv  : TEXCOORD0;
				float4 positionHCS:TEXCOORD1;
			};

			float4 _Color;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_NoiseTex);
			SAMPLER(sampler_NoiseTex);
			float4 _NoiseTex_ST;
		
			v2f vert(appdata_t i)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(i.positionOS);
				o.positionHCS=o.positionCS;

				o.uv = i.uv;

				o.color = i.color;
				return o;
			}
			
			float4 frag(v2f i) : SV_Target
			{
				float3 color=i.color.rgb*_Color.rgb;
				float alpha=i.color.a*_Color.a;
				float4 texCol = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
				#ifndef _ALPHAMASK
					color*=texCol.rgb;
				#endif
					alpha*=texCol.a;

				#if _NOISEMASK
					float2 ndc=TransformHClipToNDC(i.positionHCS);
					ndc.y+=_Time.y*.1;
					float noise=SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,TRANSFORM_TEX(ndc,_NoiseTex)).r;
					alpha*=noise;
				#endif
				
				return float4(color,alpha);
			}
		ENDHLSL
		}
	}
}
