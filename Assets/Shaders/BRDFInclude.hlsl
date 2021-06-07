#include "BRDFInput.hlsl"
#include "BRDFMethods.hlsl"
#define DIELETRIC_SPEC half4(0.04,0.04,0.04,1.0-0.04)

half GetFresnel(BRDFSurface surface,float ior)
{
    return
#if _F_SCHLICK
                F_Schlick(surface.NDV);
#elif _F_SCHLICK_IOR
				F_SchlickIOR(surface.NDV,ior);
#elif _F_SPHERICALGAUSSIAN
                F_SphericalGaussian(surface.NDV);
#else
				0;
#endif
}

BRDFSurface InitializeBRDFSurface(half3 albedo, half smoothness, half metallic,half ao,half ior, half3 normal, half3 tangent, half3 viewDir)
{
    BRDFSurface surface;
    
    half oneMinusReflectivity = DIELETRIC_SPEC.a - metallic * DIELETRIC_SPEC.a;
    half reflectivity = 1.0 - oneMinusReflectivity;
				
    surface.diffuse = albedo * oneMinusReflectivity;
    surface.specular = lerp(DIELETRIC_SPEC.rgb, albedo, metallic);
    surface.metallic = metallic;
    surface.smoothness = smoothness;
    surface.ao = ao;
    
    surface.normal = normal;
    surface.tangent = tangent;
    surface.viewDir = viewDir;
    surface.reflectDir = normalize(reflect(-surface.viewDir, surface.normal));
    surface.NDV = max(0., dot(surface.normal,surface.viewDir));
    
    surface.grazingTerm = saturate(smoothness + reflectivity);
    surface.fresnelTerm = GetFresnel(surface,ior);
    surface.perceptualRoughness = 1. - surface.smoothness;
    surface.roughness = max(HALF_MIN_SQRT, surface.perceptualRoughness * surface.perceptualRoughness);
    surface.roughness2 = max(HALF_MIN, surface.roughness * surface.roughness);
    return surface;
}

BRDFLight InitializeBRDFLight(BRDFSurface surface, half3 lightDir, half3 lightCol, half3 lightAtten, half anisotropic)
{
    half3 viewDir = surface.viewDir;
    half3 normal = surface.normal;
    half3 tangent = surface.tangent;
    half3 halfDir = normalize(viewDir + lightDir);
    half glossiness = surface.smoothness;
    half roughness = surface.roughness;
    half sqrRoughness = surface.roughness2;
        
    float NDV = surface.NDV;
    float NDL = max(0., dot(normal, lightDir));
    float NDH = max(0., dot(normal, halfDir));
    float VDH = max(0., dot(viewDir, halfDir));
    float LDH = max(0., dot(lightDir, halfDir));
    float LDV = max(0., dot(lightDir, viewDir));
    
    BRDFLight light;
    light.lightDir = lightDir;
    light.color = lightCol;
    light.radiance = lightCol * lightAtten * NDL;
    light.normalDistribution =
#if _NDF_BLINNPHONG
                NDF_BlinnPhong(NDH, glossiness,max(1, glossiness *40));
#elif _NDF_COOKTORRANCE
                NDF_CookTorrance(NDH,LDH,roughness,sqrRoughness);
#elif _NDF_BECKMANN
                NDF_Beckmann(NDH,sqrRoughness);
#elif _NDF_GAUSSIAN
                NDF_Gaussian(NDH,sqrRoughness);
#elif _NDF_GGX
                NDF_GGX(NDH,roughness,sqrRoughness);
#elif _NDF_TROWBRIDGEREITZ
                NDF_TrowbridgeReitz(NDH,roughness,sqrRoughness);
#elif _NDF_ANISOTROPIC_TROWBRIDGEREITZ
                NDFA_TrowbridgeReitz(NDH, dot(halfDir, tangent), dot(halfDir, cross(normal,tangent)), anisotropic, glossiness);
#elif _NDF_ANISOTROPIC_WARD
                NDFA_Ward(NDL, NDV, NDH, dot(halfDir, tangent), dot(halfDir,  cross(normal,tangent)), anisotropic, glossiness);
#else
				0;
#endif
    
    light.geometricShadow =
#if _GSF_IMPLICIT
                GSF_Implicit(NDL, NDV);
#elif _GSF_ASHIKHMINSHIRLEY
                GSF_AshikhminShirley(NDL, NDV, LDH);
#elif _GSF_ASHIKHMINPREMOZE
                GSF_AshikhminPremoze(NDL, NDV);
#elif _GSF_DUER
                GSF_Duer(NDL,NDV, lightDir,viewDir,normal);
#elif _GSF_NEUMANN
                GSF_Neumann(NDL,NDV);
#elif _GSF_KELEMEN
                GSF_Kelemen(NDL,NDV,VDH);
#elif _GSF_COOKTORRANCE
                GSF_CookTorrance(NDL,NDV,VDH,NDH);
#elif _GSF_WARD
                GSF_Ward(NDL,NDV);
#elif _GSF_R_KELEMEN_MODIFIED
                GSFR_Kelemen_Modifed(NDL,NDV,roughness);
#elif _GSF_R_KURT
                GSFR_Kurt(NDL,NDV,VDH,roughness);
#elif _GSF_R_WALTERETAL
                GSFR_WalterEtAl(NDL,NDV,roughness);
#elif _GSF_R_SMITHBECKMANN
                GSFR_SmithBeckmann(NDL,NDV,roughness);
#elif _GSF_R_GGX
                GSFR_GGX(NDL,NDV,roughness);
#elif _GSF_R_SCHLICK
                GSFR_Schlick(NDL,NDV,roughness);
#elif _GSF_R_SCHLICK_BECKMANN
                GSFR_SchlickBeckmann(NDL,NDV,roughness);
#elif _GSF_R_SCHLICK_GGX
                GSFR_SchlickGGX(NDL,NDV,roughness);
#else
				0;
#endif
    
    return light;
}
half3 BRDFLighting(BRDFSurface surface,BRDFLight light)
{
    half3 brdf = surface.diffuse;
    brdf += surface.specular * light.normalDistribution;
    return brdf*light.radiance;
}

half3 IndirectBRDFDiffuse(half3 normal)
{
    float3 res = SHEvalLinearL0L1(normal, unity_SHAr, unity_SHAg, unity_SHAb);
    res += SHEvalLinearL2(normal, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
#ifdef UNITY_COLORSPACE_GAMMA
	res = LinearToSRGB(res);
#endif
    return res;
}

half3 IndirectBRDFSpecular(half3 reflectDir,float perceptualRoughness)
{
    half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * UNITY_SPECCUBE_LOD_STEPS;
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDir, mip);
    half3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    return irradiance;
}

half3 BRDFGlobalIllumination(BRDFSurface surface)
{
    half3 indirectDiffuse = IndirectBRDFDiffuse(surface.normal) * surface.ao;
    half3 indirectSpecular = IndirectBRDFSpecular(surface.reflectDir, surface.perceptualRoughness)*surface.ao;
    
    half3 giDiffuse = indirectDiffuse * surface.diffuse;
    
    float3 surfaceReduction = 1.0 / (surface.roughness2 + 1.0);
    surfaceReduction *= lerp(surface.specular, surface.grazingTerm, surface.fresnelTerm);
    half3 giSpecular = indirectSpecular * surfaceReduction;
    
    return giDiffuse + giSpecular;
}
