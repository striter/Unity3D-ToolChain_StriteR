
Shader "Game/UI/Sprite_BSC"
{   
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
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
		ColorMask[_ColorMask]

		Pass
		{
		HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#define ICOLOR
			#include "Assets/Shaders/Library/Common.hlsl"

			#pragma multi_compile_local __ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float3 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
				float3 worldPosition : TEXCOORD1;
			};

			float4 _Color;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
		
			v2f vert(appdata_t i)
			{
				v2f o;
				o.worldPosition = TransformObjectToWorld(i.vertex);
				o.vertex = TransformObjectToHClip(i.vertex);

				o.texcoord = i.texcoord;

				o.color = i.color;
				return o;
			}
			
			float4 frag(v2f i) : SV_Target
			{
				float4 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord)*i.color*_Color;
				return float4(albedo.rgb,albedo.a);
			}
		ENDHLSL
		}
	}
}
