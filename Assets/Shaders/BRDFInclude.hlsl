#include "BRDFInput.hlsl"
#include "BRDFMethods.hlsl"
#define kDieletricSpec half4(0.04,0.04,0.04,1.0-0.04)

BRDFSurface InitializeBRDFSurface(half3 albedo, half smoothness, half metallic,half ao,half anisotropic, half3 normal, half3 tangent, half3 viewDir)
{
    BRDFSurface surface;
    
    half oneMinusReflectivity = kDieletricSpec.a - metallic * kDieletricSpec.a;
    half reflectivity = 1.0 - oneMinusReflectivity;
    half3 brdfDiffuse = albedo * oneMinusReflectivity;
    half3 brdfSpecular = lerp(kDieletricSpec.rgb, albedo, metallic);
				
    surface.diffuse = albedo * oneMinusReflectivity;
    surface.specular = lerp(kDieletricSpec.rgb, albedo, metallic);
    surface.metallic = metallic;
    surface.smoothness = smoothness;
    surface.ao = ao;
    surface.anisotropic = anisotropic;
    surface.grazingTerm = saturate(smoothness + reflectivity);
    
    surface.perceptualRoughness = 1 - surface.smoothness;
    surface.roughness = max(HALF_MIN_SQRT, surface.perceptualRoughness * surface.perceptualRoughness);
    surface.roughness2 = max(HALF_MIN, surface.roughness * surface.roughness);
    
    surface.normal = normal;
    surface.tangent = tangent;
    surface.viewDir = viewDir;
    return surface;
}

BRDFLight InitializeBRDFLight(BRDFSurface surface, half3 lightDir, half3 lightCol, half3 lightAtten)
{
    half3 viewDir = surface.viewDir;
    half3 normal = surface.normal;
    half3 tangent = surface.tangent;
    float anisotropic = surface.anisotropic;
    half3 halfDir = normalize(viewDir + lightDir);
    half glossiness = surface.smoothness;
    half roughness = surface.roughness;
    half sqrRoughness = surface.roughness2;
        
    float NDL = max(0., dot(normal, lightDir));
    float NDH = max(0., dot(normal, halfDir));
    float NDV = max(0., dot(normal, viewDir));
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
    
    light.fresnel =
#if _FRESNEL_SCHLICK
                F_Schlick(NDV);
#elif _FRESNEL_SCHLICK_IOR
				F_SchlickIOR(NDV,_Ior);
#elif _FRESNEL_SPHERICALGAUSSIAN
                F_SphericalGaussian(NDV);
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

half3 BRDFGlobalIllumination(BRDFSurface surface,half3 bakedGI)
{
    half3 reflectDir = normalize(reflect(-surface.viewDir, surface.normal));
    half NDR = saturate(dot(surface.normal,reflectDir));
    half fresnelTerm = Pow4(1.-NDR);
    
    half3 indirectDiffuse = bakedGI * surface.ao;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectDir,surface.perceptualRoughness,surface.ao);
    
    half3 giDiffuse = indirectDiffuse * surface.diffuse;
    
    float3 surfaceReduction = 1.0 / (surface.roughness2 + 1.) * lerp(surface.specular, surface.grazingTerm, fresnelTerm);
    half3 giSpecular = indirectSpecular * surfaceReduction;
    
    return giDiffuse + giSpecular;
}