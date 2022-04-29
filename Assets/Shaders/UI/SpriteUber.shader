Shader "Game/UI/SpriteUber"
{   
	Properties
	{
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

		[Header(Additional)]
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[NoScaleOffset][ToggleTex(_ALPHAMASK)]_AlphaMask("Alpha Mask",2D)="white"{}
		[Toggle(_BSC)]_BSC("Brightness Saturation Contrast",int)=0
		[Foldout(_BSC)]_Brightness("Brightness",Range(0,2))=1
		[Foldout(_BSC)]_Saturation("Saturation",Range(0,2))=1
		[Foldout(_BSC)]_Contrast("Contrast",Range(0,2))=1
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
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

		Pass
		{
		HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Assets/Shaders/Library/PostProcess.hlsl"

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

			//Additional
			#pragma shader_feature_local_fragment _ALPHAMASK
			#pragma shader_feature_local_fragment _BSC
			float4 _Color;
			half _Brightness;
			half _Saturation;
			half _Contrast;
			TEXTURE2D(_AlphaMask); SAMPLER(sampler_AlphaMask);
		
			v2f vert(appdata_t i)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(i.positionOS);
				o.positionHCS=o.positionCS;
				o.uv = TRANSFORM_TEX(i.uv,_MainTex);
				o.color = i.color;
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half4 finalCol = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_Color;
			
				#if _ALPHAMASK
					finalCol.a *= SAMPLE_TEXTURE2D(_AlphaMask,sampler_AlphaMask,i.uv).r;
				#endif

				#if _BSC
					finalCol.rgb *=_Brightness;
					finalCol.rgb = lerp(.5h, finalCol.rgb, _Contrast);
					finalCol.rgb = Saturation(finalCol.rgb,_Saturation);
				#endif
				return finalCol;
			}
		ENDHLSL
		}
	}
}
