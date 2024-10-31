Shader "Game/Lit/PBR/Anisotropic"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		
		[Header(PBR)]
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="black"{}
		
		[Header(Anisotropic)]
        [KeywordEnum(Anisotropic_TrowbridgeReitz,Anisotropic_Ward,Anisotropic_Beckmann,Anisotropic_GGX)]_NDF("Normal Distribution:",float) = 1
		_AnisotropicValue("Anisotropic Value:",Range(0,1))=1
//		_Distance ("Grating distance", Range(0,10000)) = 1600
		
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
			INSTANCING_BUFFER_END
    	ENDHLSL
    	
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
            #include "Assets/Shaders/Library/Additional/Algorithms/Spectral.hlsl"
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            
            #pragma multi_compile_fragment _ _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD _NDF_ANISOTROPIC_BECKMANN _NDF_ANISOTROPIC_GGX
		    #define BRDF_SURFACE_ADDITIONAL  half roughnessT; half roughnessB;
			void AnisotropicSurface(float roughness,out float roughnessT, out float roughnessB)
			{
			    float anisotropic = INSTANCE(_AnisotropicValue);
			    float anisotropicAspect = sqrt(1.0h - anisotropic * 0.9h);
			    roughnessT = max(.001, roughness / anisotropicAspect) * 5;
			    roughnessB = max(.001, roughness * anisotropicAspect) * 5;
			}
			#define BRDF_SURFACE_ADDITIONAL_TRANSFER(input,surface) AnisotropicSurface(surface.roughness,surface.roughnessT, surface.roughnessB)
            
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half roughness=surface.roughness;
				half sqrRoughness=surface.roughness2;
				half3 normal=surface.normal;
				half NDV=max(0., surface.NDV);
				half NDH=max(0., lightSurface.NDH);
				half NDL=max(0., lightSurface.NDL);

				half smoothness=surface.smoothness;
				half roughnessT=surface.roughnessT;
				half roughnessB=surface.roughnessB;
				half TDH= lightSurface.TDH;
				half BDH= lightSurface.BDH;

				float normalDistribution = 
				#if _NDF_ANISOTROPIC_TROWBRIDGEREITZ
					NDFA_TrowbridgeReitz(NDH, TDH, BDH, roughnessT, roughnessB);
				#elif _NDF_ANISOTROPIC_WARD
				    NDFA_Ward(NDL, NDV, NDH,TDH, BDH, roughnessT, roughnessB);
				#elif  _NDF_ANISOTROPIC_BECKMANN
					NDFA_Beckmann(NDH,TDH,BDH,roughnessT,roughnessB);
				#elif _NDF_ANISOTROPIC_GGX
					NDFA_GGX(NDH,TDH,BDH,roughnessT,roughnessB);
				#else
					0;
				#endif
				normalDistribution = clamp(normalDistribution,0,10000);
				return normalDistribution;
			}
			#define GET_NORMALDISTRIBUTION(surface,lightSurface) GetNormalDistribution(surface,lightSurface)
			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
            
			// float3 GetDiffractionGrating(BRDFSurface surface,Light light)
			// {
		 //        BRDFLightInput input=BRDFLightInput_Ctor(surface,light.direction,light.color,light.shadowAttenuation,light.distanceAttenuation);
		 //        BRDFLight brdfLight=BRDFLight_Ctor(surface,input);
		 //        float3 pbr = BRDFLighting(surface,brdfLight);
			// 	
			// 	float3 L = light.direction;
			//     float3 V = surface.viewDir;
			//     float3 T = surface.tangent;
			//     float d = _Distance;
			//     float cos_ThetaL = dot(L, T);
			//     float cos_ThetaV = dot(V, T);
			//     float u = abs(cos_ThetaL - cos_ThetaV);
			//     if (u == 0)
			//         return pbr;
			// 	
			//     float3 color = 0;
			//     for (int n = 1; n <= 8; n++)
			//     {
			//         float wavelength = u * d / n;
			//         color += spectral_zucconi6(wavelength);
			//     }
			//     color = saturate(color);
			// 	return color + pbr;
			// }
   //          
			// #define GET_LIGHTING_OUTPUT(surface,light) GetDiffractionGrating(surface,light)
			
            #pragma target 3.5
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
