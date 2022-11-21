Shader "PCG/Structure"
{
    Properties
    {
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
    	_Progress("_Progress",Range(0,1))=0
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="black"{}
		
		[Header(Detail Tex)]
		_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)
		
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
    }
    SubShader
    {
        Pass
        {
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			#include "PCGInclude.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)
				INSTANCING_PROP(float,_Progress)
			INSTANCING_BUFFER_END
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"

			float3 GetPositionWSOverride(float3 _positionOS,float4 _color)
			{
				float3 positionWS = TransformObjectToWorld(_positionOS);
				return positionWS - normalize(positionWS) * INSTANCE(_Progress) * .3f  ;
			}
			
			float3 GetAlbedoOverride(float2 uv,float4 color)
			{
				float4 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, uv);
				clip(albedo.a-.1f);
				return albedo.rgb * color * _Color;
			}

			float3 GetEmissionOverride(float3 color)
			{
				return step(max(color),.01)*_EmissionColor+INSTANCE(_Progress) * .8f;
			}

			#define GET_POSITION_WS(v,o) GetPositionWSOverride(v.positionOS,v.color)
			#define GET_ALBEDO(i) GetAlbedoOverride(i.uv,i.color);
			#define GET_EMISSION(i) GetEmissionOverride(i.color.rgb);
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
