#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"

#ifndef _BRDFLIGHTING
#define _BRDFLIGHTING

struct BRDFLight
{
    half3 color;
    half3 radiance;
    half3 lightDir;
    
    half normalDistribution;
    half3 normalizationTerm;
};

BRDFLight BRDFLight_Ctor(BRDFSurface surface,BRDFLightInput input)
{
    BRDFLight light;
    light.lightDir = input.lightDirection;
    light.color = input.lightColor;
   
#if defined(GET_GEOMETRYSHADOW)
    float geometryShadow = GET_GEOMETRYSHADOW(surface,input);
#else
    float geometryShadow = input.NDL;
#endif
    light.radiance = input.lightColor * (input.distanceAttenuation*input.shadowAttenuation * geometryShadow);
    
#if defined(GET_NORMALDISTRIBUTION)
    light.normalDistribution = GET_NORMALDISTRIBUTION(surface,input);
#else
    half sqrRoughness=surface.roughness2;
    half NDH=input.NDH;

    NDH = saturate(NDH);
    float d = NDH * NDH * (sqrRoughness-1.f) +1.00001f;
							
    light.normalDistribution = clamp(sqrRoughness / (d * d),0,100);
#endif

#if defined(GET_NORMALIZATIONTERM)
    light.normalizationTerm = GET_NORMALIZATIONTERM(surface,input);
#else
    float sqrLDH = pow2(input.LDH);
    light.normalizationTerm = max(0.1h, sqrLDH) * (surface.roughness*4 + 2);
#endif
    return light;
}

half3 BRDFLighting(BRDFSurface surface,BRDFLight light)
{
    half3 brdf = surface.diffuse;
    half D = light.normalDistribution;
    half3 VF = light.normalizationTerm;

    brdf += surface.specular * D * rcp(VF);
    return brdf*light.radiance;
}

half3 BRDFGlobalIllumination(BRDFSurface surface,half3 indirectDiffuse,half3 indirectSpecular)
{
    indirectDiffuse *= surface.ao;
    indirectSpecular *= surface.ao;
    
    half3 giDiffuse = indirectDiffuse * surface.diffuse;
    
    float fresnelTerm = F_Schlick(max(0,surface.NDV));
    float3 surfaceReduction = 1.0 / (surface.roughness2 + 1.0) * lerp(surface.specular, surface.grazingTerm, fresnelTerm);
    half3 giSpecular = indirectSpecular * surfaceReduction;
    return giDiffuse + giSpecular;
}

#endif
