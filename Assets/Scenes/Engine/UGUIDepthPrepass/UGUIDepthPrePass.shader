Shader "Hidden/UGUIDepthPrePass"
{   
	Properties
	{
		[Header(PreRenderData)]
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
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
        Cull Off
        ZWrite On
        ZTest LEqual
        Blend Off
        ColorMask 0

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
		
			v2f vert(appdata_t i)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(i.positionOS);
				o.positionHCS=o.positionCS;
				o.uv =  TRANSFORM_TEX(i.uv,_MainTex);
				o.color = i.color;
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half4 finalCol = i.color * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
				clip(finalCol.a-1);
				return 1;
			}
		ENDHLSL
		}
	}
}
