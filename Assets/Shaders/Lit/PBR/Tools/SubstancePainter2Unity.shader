Shader "Hidden/SubstancePainterPBRtoUnity"
{
	Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="white"{}

		[Header(Detail Tex)]
		[NoScaleOffset]_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)

		[Header(IndirectSpecular)]
		[NoScaleOffset]_SpecularCube("Specular",Cube)="black"{}
		
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		
		HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			#include "Assets/Shaders/Library/PBR.hlsl"
			// #pragma multi_compile_instancing
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURECUBE(_SpecularCube);SAMPLER(sampler_SpecularCube);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)

				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float4,_EmissionColor)
			INSTANCING_BUFFER_END
		ENDHLSL
		
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
			
			Light OverrideLighting()
			{
				Light mainLight;
				mainLight.direction = normalize(float3(0,1,1));
				mainLight.color = .5;
				mainLight.shadowAttenuation = 1;
				mainLight.distanceAttenuation = 1;
				return mainLight;
			}

			half4 _SpecularCube_HDR;
			void OverrideGI(out half3 indirectDiffuse, out half3 indirectSpecular, v2ff i, BRDFSurface surface, Light mainLight)
			{
				indirectDiffuse = SampleSHL2(surface.normal,
			    	half4(0.0109134,0.1585592,0.0129353,0.7254902),
			    	half4(0.0109134,0.1585592,0.0129353,0.7254902),
			    	half4(0.0109134,0.1585592,0.0129353,0.7254902),
			    	half4(0.06141909,0.02248548,0.01431291,0.007843138),
			    	half4(0.06141909,0.02248548,0.01431291,0.007843138),
			    	half4(0.06141909,0.02248548,0.01431291,0.007843138),
			    	half4(0.06627715,0.06627715,0.06627715,0));
				indirectSpecular = SampleCubeSpecular(TEXTURECUBE_ARGS(_SpecularCube,sampler_SpecularCube),_SpecularCube_HDR,surface.reflectDir,surface.perceptualRoughness);
			}
			
			#define GET_MAINLIGHT(i) OverrideLighting();
			#define GET_PBRPARAM(i,smoothness,metallic,ao)
			#define GET_GI(indirectDiffuse,indirectSpecular,i,surface,mainLight) OverrideGI(indirectDiffuse,indirectSpecular,i,surface,mainLight);
			
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
		}
		
		Pass
		{
			NAME "DEPTH"
			Tags{"LightMode" = "DepthOnly"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			
			HLSLPROGRAM
			#include "Assets/Shaders/Library/Passes/DepthOnly.hlsl"
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
			ENDHLSL
		}
		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			
			HLSLPROGRAM
			#include "Assets/Shaders/Library/Passes/ShadowCaster.hlsl"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			ENDHLSL
		}
	}
	
}