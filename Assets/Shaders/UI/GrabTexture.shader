Shader "Game/UI/GrabTexture"
{   
	Properties
	{
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
        Blend One Zero
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
				half2 uv : TEXCOORD0;
				float4 positionHCS:TEXCOORD1;
			};

			TEXTURE2D(_GrabTexture);
			SAMPLER(sampler_GrabTexture);

			v2f vert(appdata_t i)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(i.positionOS);
				o.positionHCS=o.positionCS;
				o.uv = i.uv;
				o.color = i.color;
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half2 uv = i.uv.xy;
				half4 finalCol = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv) * i.color;

				float2 screenUV = TransformHClipToNDC(i.positionHCS);
				finalCol *= SAMPLE_TEXTURE2D(_GrabTexture,sampler_GrabTexture,screenUV);
				return finalCol;
			}
		ENDHLSL
		}
	}
}
