Shader "Game/UI/SpriteDissolve"
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
		_Progress("Progress",Range(0,1))=1
		
		[Header(Gradient)]
		_GradientWidth("Gradient Width",Range(0.01,1))=0.1
		[HDR]_GradientColor("Gradient Color",Color)=(1,1,1,1)
		
		[Header(Particle)]
		_Dissolve("Dissolve Mask",2D)= "white"{}
		_DissolveClip("Dissolve Clip",Range(0,1))=0
		_DissolveVanishBegin("Dissolve Vanish Begin",Range(0,1))=.1
		_DissolveVanishWidth("Dissolve Vanish Width",Range(0.01,1))=.1
		[HDR]_DissolveColor("Dissolve Color",Color)=(1,1,1,1)
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
				float4 uv  : TEXCOORD0;
				float2 maskUV:TEXCOORD1;
				float4 positionHCS:TEXCOORD2;
			};
		
			float _Progress;
			float _GradientWidth;
			float4 _GradientColor;
			float4 _DissolveColor;
			float4 _Dissolve_ST;
			float _DissolveClip;
			float _DissolveVanishBegin;
			float _DissolveVanishWidth;
			TEXTURE2D(_Dissolve);SAMPLER(sampler_Dissolve);
		
			v2f vert(appdata_t i)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(i.positionOS);
				o.positionHCS=o.positionCS;
				o.uv = float4(TRANSFORM_TEX(i.uv,_MainTex),i.uv);
				o.maskUV= i.uv*_Dissolve_ST.xy + _Time.y*(_Dissolve_ST.zw);
				o.color = i.color;
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half4 finalCol = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy);

				half vertical = 1-i.uv.w;
				half vanish = step(_Progress,vertical);
				half value = saturate(invlerp(_Progress+_GradientWidth,_Progress,vertical))*vanish;

				
				finalCol.rgb = lerp(finalCol.rgb,_GradientColor.rgb,value*_GradientColor.a);
				finalCol.a *= vanish;
				
				half mask = SAMPLE_TEXTURE2D(_Dissolve,sampler_Dissolve,i.maskUV).r;
				half alpha = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,float2(i.uv.x,1-_Progress)).a;


				half particleVanish = saturate(invlerp(vertical+_DissolveVanishBegin+_DissolveVanishWidth,vertical+_DissolveVanishBegin,_Progress))*(1-vanish);
				mask*= particleVanish;
				mask*=step(_DissolveClip,mask);
				mask*=alpha;
				half4 particle =  _DissolveColor * mask;
				finalCol += particle;
				return finalCol;
			}
		ENDHLSL
		}
	}
}
