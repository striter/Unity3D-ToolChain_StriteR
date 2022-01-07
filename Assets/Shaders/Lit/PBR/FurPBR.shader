Shader "Game/Lit/FurPBR"
{
	Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(Fur)]
		_FurTex("Texure",2D)="white"{}
		_FurLength("Length",Range(0,1))=0.1
		_FurAlphaClip("Alpha Clip",Range(0,1))=0.5
		_FurShadow("Inner Shadow",Range(0,1))=0.5
		_FURUVDelta("UV Delta",Range(0,2))=0.1
		_FurGravity("Gravity",Range(0,1))=.1
		
		[Header(PBR)]
		[ToggleTex(_PBRMAP)] [NoScaleOffset]_PBRTex("PBR Tex(Roughness.Metallic.AO)",2D)="white"{}
		[Fold(_PBRMAP)]_Glossiness("Glossiness",Range(0,1))=1
        [Fold(_PBRMAP)]_Metallic("Metalness",Range(0,1))=0
	
		[Header(Render Options)]
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaClipRange("Range",Range(0.01,1))=0.01
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend Off
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		
		HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			#pragma multi_compile_instancing
			#define SHELLCOUNT 32.0

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_FurTex);SAMPLER(sampler_FurTex);
		
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4,_FurTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float,_FurAlphaClip)
				INSTANCING_PROP(float,_FurLength)
				INSTANCING_PROP(float,_FurShadow)
				INSTANCING_PROP(float,_FURUVDelta)
				INSTANCING_PROP(float,_FurGravity)
			INSTANCING_BUFFER_END
			
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			
			#pragma shader_feature_local_fragment _PBRMAP
			#pragma shader_feature_local_fragment _NORMALMAP
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
			
            #pragma multi_compile_fog
            #pragma target 3.5

			float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return lightSurface.NDL;
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half glossiness = surface.smoothness;
				half roughness=surface.roughness;
				half sqrRoughness=surface.roughness2;
				half anisotropic=surface.anisotropic;
				half3 normal=surface.normal;
				half NDV=surface.NDV;
				half NDH=lightSurface.NDH;
				half NDL=lightSurface.NDL;
				half3 tangent=surface.tangent;
				half3 halfDir=lightSurface.halfDir;
				
				half normalDistribution = NDF_CookTorrance(NDH,sqrRoughness);
				normalDistribution=clamp(normalDistribution,0,100.h);
				return normalDistribution;
			}
			
			float GetNormalizationTerm(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return InvVF_GGX(lightSurface.LDH,surface.roughness);
			}
			
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"

		ENDHLSL
		Pass
		{
			NAME "SKIN"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#define SHELLINDEX 0
			#define SKIN

			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		
		Pass
		{
			NAME "FUR0"
			Tags{"LightMode" = "FUR0"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 0
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR1"
			Tags{"LightMode" = "FUR1"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 1
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR2"
			Tags{"LightMode" = "FUR2"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 2
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR3"
			Tags{"LightMode" = "FUR3"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 3
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR4"
			Tags{"LightMode" = "FUR4"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 4
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR5"
			Tags{"LightMode" = "FUR5"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 5
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR6"
			Tags{"LightMode" = "FUR6"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 6
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR7"
			Tags{"LightMode" = "FUR7"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 7
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR8"
			Tags{"LightMode" = "FUR8"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 8
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR9"
			Tags{"LightMode" = "FUR9"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 9
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR10"
			Tags{"LightMode" = "FUR10"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 10
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR11"
			Tags{"LightMode" = "FUR11"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 11
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR12"
			Tags{"LightMode" = "FUR12"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 12
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR13"
			Tags{"LightMode" = "FUR13"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 13
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR14"
			Tags{"LightMode" = "FUR14"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 14
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR15"
			Tags{"LightMode" = "FUR15"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 15
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR16"
			Tags{"LightMode" = "FUR16"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 16
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR17"
			Tags{"LightMode" = "FUR17"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 17
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		
				Pass
		{
			NAME "FUR18"
			Tags{"LightMode" = "FUR18"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 18
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
				Pass
		{
			NAME "FUR19"
			Tags{"LightMode" = "FUR19"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 19
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR20"
			Tags{"LightMode" = "FUR20"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 20
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR21"
			Tags{"LightMode" = "FUR21"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 21
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR22"
			Tags{"LightMode" = "FUR22"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 22
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR23"
			Tags{"LightMode" = "FUR23"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 23
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR24"
			Tags{"LightMode" = "FUR24"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 24
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR25"
			Tags{"LightMode" = "FUR25"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 25
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR26"
			Tags{"LightMode" = "FUR26"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 26
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
				Pass
		{
			NAME "FUR27"
			Tags{"LightMode" = "FUR26"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 27
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
				Pass
		{
			NAME "FUR27"
			Tags{"LightMode" = "FUR27"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 27
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
				Pass
		{
			NAME "FUR28"
			Tags{"LightMode" = "FUR28"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 28
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR29"
			Tags{"LightMode" = "FUR29"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 29
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR30"
			Tags{"LightMode" = "FUR30"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 30
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		Pass
		{
			NAME "FUR31"
			Tags{"LightMode" = "FUR31"}
			Cull Off
			HLSLPROGRAM
			#define SHELLINDEX 31
			#include "FurPBRInclude.hlsl"
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		USEPASS "Hidden/ShadowCaster/MAIN"
	}
}