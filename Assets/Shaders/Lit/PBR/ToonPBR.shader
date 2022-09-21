Shader "Game/Lit/ToonPBR"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="black"{}
		
		[Header(Tooness)]
    	[MinMaxRange]_GeometryShadow("Geometry Shadow",Range(0,1))=0
    	[HideInInspector]_GeometryShadowEnd("",float)=0.1
		_IndirectSpecularOffset("Indirect Specular Offset",Range(-7,7))=0
		
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
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

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
				INSTANCING_PROP(float,_GeometryShadow)
				INSTANCING_PROP(float,_GeometryShadowEnd)
				INSTANCING_PROP(float,_IndirectSpecularOffset)
			INSTANCING_BUFFER_END
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
			
			float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
			    return saturate(invlerp(_GeometryShadow,_GeometryShadowEnd, lightSurface.NDL));
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
			    half sqrRoughness=surface.roughness2;
			    half NDH=lightSurface.NDH;

			    NDH = saturate(NDH);
			    float d = NDH * NDH * (sqrRoughness-1.f) +1.00001f;
							
			    float specular= sqrRoughness / (d * d);
			    specular = clamp(specular,0,100);
			    
			    half steps = max(((1 - surface.smoothness) * 2), 0.01);
			    float toonSpecular = round(specular * steps) / steps;
			    return toonSpecular;
			}
					
			#define GET_GEOMETRYSHADOW(surface,lightSurface) GetGeometryShadow(surface,lightSurface)
	        #define GET_NORMALDISTRIBUTION(surface,input) GetNormalDistribution(surface,input)
			#define GET_INDIRECTSPECULAR(surface) IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,INSTANCE(_IndirectSpecularOffset));
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
