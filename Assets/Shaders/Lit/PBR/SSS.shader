Shader "Game/Lit/PBR/SSS"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		
		[Header(PBR)]
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[NoScaleOffset]_PBRTex("PBR Tex(Smoothness.Metallic.AO)",2D)="black"{}
		
		[NoScaleOffset]_ThicknessMap("SSS Thickness",2D)="black"{}
		_SSSNormalInfluence("SSS Normal Influence",Range(0,1))=1
		_SSSIntensity("SSS Intensity",Range(0.1,10))=1
		
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
		Tags { "Queue" = "Geometry" }
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		
    	HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D( _NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)
				INSTANCING_PROP(float,_Distance)
				INSTANCING_PROP(float,_AnisotropicValue)
				INSTANCING_PROP(float,_SSSIntensity)
				INSTANCING_PROP(float,_SSSNormalInfluence)
			INSTANCING_BUFFER_END
    	ENDHLSL
    	
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
            #include "Assets/Shaders/Library/Additional/Algorithms/Spectral.hlsl"
			TEXTURE2D(_ThicknessMap);SAMPLER(sampler_ThicknessMap);
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

		    #define BRDF_SURFACE_ADDITIONAL  half thickness; half sssInfluence; half sssIntensity;
            #define BRDF_SURFACE_ADDITIONAL_TRANSFER(input,surface) surface.thickness = SAMPLE_TEXTURE2D(_ThicknessMap,sampler_ThicknessMap,input.uv);surface.sssInfluence = INSTANCE(_SSSNormalInfluence);surface.sssIntensity = INSTANCE(_SSSIntensity);

			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
            
			half3 LightingWithSSS(BRDFSurface surface,Light light)
			{
				BRDFLightInput input=BRDFLightInput_Ctor(surface,light.direction,light.color,light.shadowAttenuation,light.distanceAttenuation);
				BRDFLight brdfLight=BRDFLight_Ctor(surface,input);
				half3 output =  BRDFLighting(surface,brdfLight);
				output += SSSLighting(surface.thickness,surface.sssInfluence,surface.sssIntensity,light,surface.normal,surface.viewDir)*surface.diffuse;
				return output;
			}
            
			#define GET_LIGHTING_OUTPUT(surface,light) LightingWithSSS(surface,light);
            
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			ENDHLSL
		}
        
		Pass
		{
            Name "META"
            Tags{"LightMode" = "Meta"}
			Cull Off

            HLSLPROGRAM
            #pragma vertex VertexMeta
            #pragma fragment FragmentMeta
            #include "Assets/Shaders/Library/Passes/MetaPBR.hlsl"
            ENDHLSL
		}
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
