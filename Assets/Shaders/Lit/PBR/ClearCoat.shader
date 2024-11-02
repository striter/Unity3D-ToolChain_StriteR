Shader "Game/Lit/PBR/ClearCoat"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		
		[Header(PBR)]
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[NoScaleOffset]_PBRTex("PBR Tex(Smoothness.Metallic.AO)",2D)="black"{}
		
		[Header(Clear Coat)]
        [NoScaleOffset]_ClearcoatMaskMap("Coat Tex (Mask,Smoothness)",2D)="white"{}
		_ClearcoatMask("Mask Multiplier",Range(0,1))=1
		_ClearcoatSmoothness("Smoothness Multiplier",Range(0,1))=.5
        _ClearcoatIOR("Clearcoat IOR",Range(1,3))=1.5

		
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
			TEXTURE2D(_ClearcoatMaskMap);SAMPLER(sampler_ClearcoatMaskMap);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)

				INSTANCING_PROP(float, _ClearcoatSmoothness)
				INSTANCING_PROP(float, _ClearcoatIOR)
				INSTANCING_PROP(float, _ClearcoatMask)
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

            #pragma multi_compile _ _CLEARCOAT_NORMALMAP
			struct ClearCoatData
			{
				float diffuse;
				float specular;
			    float perceptualRoughness;
			    float smoothness;
				float roughness;
				float roughness2;
				float grazingTerm;
			    half IOR;
			    half mask;
			};

		    #define BRDF_SURFACE_ADDITIONAL ClearCoatData clearCoat;
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
            void GetClearCoatData(BRDFInitializeInput input,inout BRDFSurface _surface)
            {
            	float4 mask = SAMPLE_TEXTURE2D(_ClearcoatMaskMap, sampler_ClearcoatMaskMap, input.uv);
				ClearCoatData clearcoatData;
            	clearcoatData.diffuse = kDieletricSpec.aaa;
				clearcoatData.specular = kDielectricSpec.r;
			    clearcoatData.smoothness = INSTANCE(_ClearcoatSmoothness) * mask.g;
			    clearcoatData.roughness = PerceptualSmoothnessToRoughness(clearcoatData.smoothness);
			    clearcoatData.roughness2 = clearcoatData.roughness * clearcoatData.roughness;
            	clearcoatData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(clearcoatData.smoothness);
			    clearcoatData.IOR = (_ClearcoatIOR);
			    clearcoatData.mask = mask.r * INSTANCE(_ClearcoatMask);
            	clearcoatData.grazingTerm = saturate(clearcoatData.smoothness + kDielectricSpec.x);
            	_surface.clearCoat = clearcoatData;
            	
            	float ieta = lerp(1.0, 1.0 / clearcoatData.IOR, clearcoatData.mask);
				    half coatRoughnessScale          = Sq(ieta);
				    half sigma                       = RoughnessToVariance(PerceptualRoughnessToRoughness(_surface.perceptualRoughness));

			    _surface.perceptualRoughness = RoughnessToPerceptualRoughness(VarianceToRoughness(sigma * coatRoughnessScale));

			    // Recompute base material for new roughness, previous computation should be eliminated by the compiler (as it's unused)
			    _surface.roughness          = max(PerceptualRoughnessToRoughness(_surface.perceptualRoughness), HALF_MIN_SQRT);
			    _surface.roughness2         = max(_surface.roughness * _surface.roughness, HALF_MIN);
			    _surface.specular = lerp(_surface.specular, ConvertF0ForAirInterfaceToF0ForClearCoat15(_surface.specular), clearcoatData.mask);
            }

            #define BRDF_SURFACE_ADDITIONAL_TRANSFER(input,surface) GetClearCoatData(input,surface)
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"


			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
			float3 BRDFLightingClearCoat(BRDFSurface surface,Light light)
            {
				BRDFLightInput input=BRDFLightInput_Ctor(surface,light.direction,light.color,light.shadowAttenuation,light.distanceAttenuation);
				BRDFLight brdfLight=BRDFLight_Ctor(surface,input);

			    half3 diffTerm = surface.diffuse;
            	half3 specTerm = surface.specular * brdfLight.normalDistribution * rcp(brdfLight.normalizationTerm);

				half brdf = (diffTerm + specTerm);
            	
			    ClearCoatData clearcoatData = surface.clearCoat;
            	half coatDiffTerm = clearcoatData.diffuse;
            	half coatSpecTerm = clearcoatData.specular *NDF_CookTorrance(input.LDH,clearcoatData.roughness) * rcp(InvVF_GGX(input.LDH, clearcoatData.roughness)); ;
				half brdfCoat = kDielectricSpec.r * (coatDiffTerm + coatSpecTerm);
	            half NoV = saturate(surface.NDV);
	            half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);
				brdf = brdf * (1.0 - clearcoatData.mask *  coatFresnel) + brdfCoat * clearcoatData.mask;
            	
				 return brdf * brdfLight.radiance;
            }
            
			float3 GetGlobalIllumination(v2ff i,BRDFSurface surface,Light mainLight)
			{
				half3 indirectDiffuse = IndirectDiffuse(mainLight,i,surface.normal);
				half3 indirectSpecular = IndirectSpecularWithSSR(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,surface.normalTS);
			    float fresnelTerm = F_Schlick(max(0,surface.NDV));
				float3 surfaceReduction = 1.0 / (surface.roughness2 + 1.0) * lerp(surface.specular, surface.grazingTerm, fresnelTerm);
			    half3 giDiffuse = indirectDiffuse * surface.diffuse;
				half3 giSpecular = indirectSpecular * surfaceReduction;
            	half3 baseColor = giDiffuse + giSpecular;
	  
			    half3 coatIndirectSpecular = IndirectSpecularWithSSR(surface.reflectDir, surface.clearCoat.perceptualRoughness, i.positionHCS,surface.normalTS);
			    float coatSurfaceReduction = 1.0 / (Sq(surface.clearCoat.smoothness) + 1.0);
			    half3 coatColor = coatIndirectSpecular * coatSurfaceReduction * lerp(surface.clearCoat.specular, surface.clearCoat.grazingTerm, fresnelTerm);
            	half clearCoatMask = surface.clearCoat.mask;
	  
			    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
			    return (baseColor * (1.0 - coatFresnel * clearCoatMask) + coatColor * clearCoatMask) * surface.ao;
			}
			#define GET_GI(i,surface,mainLight) GetGlobalIllumination(i,surface,mainLight);

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
