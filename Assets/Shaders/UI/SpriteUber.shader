Shader "Game/UI/SpriteUber"
{   
	Properties
	{
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[NoScaleOffset][ToggleTex(_ALPHAMASK)]_AlphaMask("Alpha Mask",2D)="white"{}
		[Toggle(_BSC)]_BSC("Brightness Saturation Contrast",int)=0
		[Foldout(_BSC)]_Brightness("Brightness",Range(0,2))=1
		[Foldout(_BSC)]_Saturation("Saturation",Range(0,2))=1
		[Foldout(_BSC)]_Contrast("Contrast",Range(0,2))=1
		[Toggle(_DISTORT)]_Distort("Distort",int)=0
		[Foldout(_DISTORT)]_DistortTex("Texture",2D)="black"{}
		[Foldout(_DISTORT)]_DistortStrength("Strength",Range(0,200))=1
		
		[Header(Misc)]
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
		
		[Header(PreRenderData)]
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
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

        Lighting Off
        Cull [_Cull]
        ZWrite [_ZWrite]
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
				half4 uv : TEXCOORD0;
				float4 positionHCS:TEXCOORD1;
			};

			//Additional
			#pragma shader_feature_local_fragment _ALPHAMASK
			#pragma shader_feature_local_fragment _BSC
			#pragma shader_feature_local_fragment _DISTORT
			float4 _Color;
			half _Brightness;
			half _Saturation;
			half _Contrast;
			half _DistortStrength;
			half4 _DistortTex_ST;
			TEXTURE2D(_AlphaMask); SAMPLER(sampler_AlphaMask);
			TEXTURE2D(_DistortTex);SAMPLER(sampler_DistortTex);
		
			v2f vert(appdata_t i)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(i.positionOS);
				o.positionHCS=o.positionCS;
				o.uv = half4( TRANSFORM_TEX(i.uv,_MainTex),TRANSFORM_TEX_FLOW_INSTANCE(i.uv,_DistortTex));
				o.color = i.color;
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half2 uv = i.uv.xy;
				#if _DISTORT
					half2 distortSample = SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex,i.uv.zw).xy*2-1;
					uv+= _MainTex_TexelSize.xy*distortSample*_DistortStrength;
				#endif
				
				half4 finalCol = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv)*_Color;
			
				#if _ALPHAMASK
					finalCol.a *= SAMPLE_TEXTURE2D(_AlphaMask,sampler_AlphaMask,uv).r;
				#endif

				#if _BSC
					finalCol.rgb *=_Brightness;
					finalCol.rgb = lerp(.5h, finalCol.rgb, _Contrast);
					finalCol.rgb = Saturation(finalCol.rgb,_Saturation);
				#endif
				clip(finalCol.a-0.01);
				return finalCol;
			}
		ENDHLSL
		}
	}
}
