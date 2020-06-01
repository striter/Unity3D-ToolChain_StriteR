Shader "Game/UI/UI_OverlayOpaqueBlurBG" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
			_StencilComp("Stencil Comparison", Float) = 8
			_Stencil("Stencil ID", Float) = 0
			_StencilOp("Stencil Operation", Float) = 0
			_StencilWriteMask("Stencil Write Mask", Float) = 255
			_StencilReadMask("Stencil Read Mask", Float) = 255
			_ColorMask("Color Mask", Float) = 15

			[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}
	SubShader { 
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
			LOD 200

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			PASS
		{

		CGPROGRAM
#pragma target 2.0
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile __ UNITY_UI_ALPHACLIP
#include "UnityCG.cginc"
		sampler2D _MainTex;
	sampler2D _CameraUIOverlayBlurTexture;

	struct a2f
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
	};
	struct v2f
	{
		float4 vertex   : SV_POSITION;
		fixed4 color : COLOR;
		half2 uv  : TEXCOORD0;
		float2 screenPos:TEXCOORD1;
	};
	v2f vert(a2f i)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(i.vertex);
		float4 screenPos = ComputeScreenPos(o.vertex);
		o.screenPos = screenPos.xy / screenPos.w;
		o.uv = i.texcoord;
		o.color = i.color;
		return o;
	}
	fixed4 frag(v2f v) :COLOR
	{
	return  tex2D(_CameraUIOverlayBlurTexture,v.screenPos)*v.color;
	}
	ENDCG
	}
	}
}
