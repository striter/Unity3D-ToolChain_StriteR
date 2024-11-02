Shader "Game/Lit/PBR/Anisotropic"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		
		[Header(PBR)]
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[NoScaleOffset]_PBRTex("PBR Tex(Smoothness.Metallic.AO)",2D)="black"{}
		
		[Header(Anisotropic)]
        [KeywordEnum(Anisotropic_TrowbridgeReitz,Anisotropic_Ward,Anisotropic_Beckmann,Anisotropic_GGX)]_NDF("Normal Distribution:",float) = 1
		_AnisotropicValue("Anisotropic Value:",Range(0,1))=1
		
		[Header(Thin Film Coating)]
		_Thickness("Thickness",Range(0,3000)) = 250
		_ExternalIOR("External IOR",Range(0.2,3)) = 1
		_ThinFilmIOR("Thin Film IOR",Range(0.2,3)) = 1.5
		_InternalIOR("Internal IOR",Range(0.2,3)) = 1.25
		
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
				INSTANCING_PROP(float,_AnisotropicValue)

				INSTANCING_PROP(float,_Thickness)
				INSTANCING_PROP(float,_ExternalIOR)
				INSTANCING_PROP(float,_ThinFilmIOR)
				INSTANCING_PROP(float,_InternalIOR)
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
			
            //https://www.gamedev.net/tutorials/_/technical/graphics-programming-and-theory/thin-film-interference-for-computer-graphics-r2962/
			float rs(float n1, float n2, float cosI, float cosT) 
		    { 
		        return (n1 * cosI - n2 * cosT) / (n1 * cosI + n2 * cosT); 
		    } 
		    
		    /* Amplitude reflection coefficient (p-polarized) */ 
		    float rp(float n1, float n2, float cosI, float cosT) 
		    { 
		        return (n2 * cosI - n1 * cosT) / (n1 * cosT + n2 * cosI); 
		    } 
		    
		    /* Amplitude transmission coefficient (s-polarized) */ 
		    float ts(float n1, float n2, float cosI, float cosT) 
		    { 
		        return 2 * n1 * cosI / (n1 * cosI + n2 * cosT); 
		    } 
		    
		    /* Amplitude transmission coefficient (p-polarized) */ 
		    float tp(float n1, float n2, float cosI, float cosT) 
		    { 
		        return 2 * n1 * cosI / (n1 * cosT + n2 * cosI); 
		    } 
		    
		    /* Pass the incident cosine. */ 
		    float3 F_Coating(float cos0) 
		    {
				float thinfilmIOR = INSTANCE(_ThinFilmIOR);
				float externalIOR = INSTANCE(_ExternalIOR);
				float internalIOR = INSTANCE(_InternalIOR);
				float thickness = INSTANCE(_Thickness);
		        /* Precompute the reflection phase changes (depends on IOR) */ 
		        float delta10 = (thinfilmIOR < externalIOR) ? PI : 0.0f; 
		        float delta12 = (thinfilmIOR < internalIOR) ? PI : 0.0f; 
		        float delta = delta10 + delta12; 
		        
		        /* Calculate the thin film layer (and transmitted) angle cosines. */ 
		        float sin1 = pow(externalIOR / thinfilmIOR, 2) * (1 - pow(cos0, 2)); 
		        float sin2 = pow(externalIOR / internalIOR, 2) * (1 - pow(cos0, 2)); 
		        
		        if ((sin1 > 1) || (sin2 > 1)) return 1; 
		        
		        /* Account for TIR. */ 
		        float cos1 = sqrt(1 - sin1), cos2 = sqrt(1 - sin2); 
		        
		        /* Calculate the interference phase change. */ 
		        float3 phi = 2 * thinfilmIOR * thickness * cos1; 
		        phi *= 2 * PI / float3(650, 510, 475); phi += delta; 
		        
		        /* Obtain the various Fresnel amplitude coefficients. */ 
		        float alpha_s = rs(thinfilmIOR, externalIOR, cos1, cos0) * rs(thinfilmIOR, internalIOR, cos1, cos2); 
		        float alpha_p = rp(thinfilmIOR, externalIOR, cos1, cos0) * rp(thinfilmIOR, internalIOR, cos1, cos2); 
		        float beta_s = ts(externalIOR, thinfilmIOR, cos0, cos1) * ts(thinfilmIOR, internalIOR, cos1, cos2); 
		        float beta_p = tp(externalIOR, thinfilmIOR, cos0, cos1) * tp(thinfilmIOR, internalIOR, cos1, cos2); 
		        
		        /* Calculate the s- and p-polarized intensity transmission coefficient. */ 
		        float3 ts = pow(beta_s, 2) / (pow(alpha_s, 2) - 2 * alpha_s * cos(phi) + 1); 
		        float3 tp = pow(beta_p, 2) / (pow(alpha_p, 2) - 2 * alpha_p * cos(phi) + 1); 
		        
		        /* Calculate the transmitted power ratio for medium change. */ 
		        float beamRatio = (internalIOR * cos2) / (externalIOR * cos0); 
		        
		        /* Calculate the average reflectance. */ 
		        return 1 - beamRatio * (ts + tp) * 0.5f; 
		    }

            half3 GetNormalizationTerm(BRDFSurface surface,BRDFLightInput input)
			{
				half V = V_SmithJointGGXAniso(surface.TDV,surface.BDV,surface.NDV,input.TDL,input.BDL,input.NDL,surface.roughnessT,surface.roughnessB);
				float3 F = F_Coating(input.VDH);
				return V * F * 4;
			}

            #define GET_NORMALDISTRIBUTION(surface,input) GetNormalDistribution(surface,input);
            #define GET_NORMALIZATIONTERM(surface,input) GetNormalizationTerm(surface,input);
			
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
