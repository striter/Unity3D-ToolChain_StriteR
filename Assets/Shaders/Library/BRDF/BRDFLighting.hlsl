#define GEOMETRY_SHADOW(surface,lightSurface) GetGeometryShadow(surface,lightSurface)
#define NORMALIZATION_TERM(surface,lightSurface) GetNormalizationTerm(surface,lightSurface)
#define NORMAL_DISTRIBUTION(surface,lightSurface) GetNormalDistribution(surface,lightSurface)

struct BRDFLight
{
    half3 color;
    half3 radiance;
    half3 lightDir;
    
    half normalDistribution;
    half normalizationTerm;
};
BRDFLight BRDFLight_Ctor(BRDFSurface surface, half3 lightDir, half3 lightCol, half3 lightAtten, half anisotropic)
{
    BRDFLightSurface input=BRDFLightInput_Ctor(surface,lightDir);
    BRDFLight light;
    light.lightDir = lightDir;
    light.color = lightCol;
    light.radiance = lightCol * (lightAtten * GEOMETRY_SHADOW(surface,input));
    light.normalDistribution = NORMAL_DISTRIBUTION(surface,input);
    light.normalizationTerm = NORMALIZATION_TERM(surface,input);
    return light;
}

half3 BRDFLighting(BRDFSurface surface,BRDFLight light)
{
    half3 brdf = surface.diffuse;
    
    half D = light.normalDistribution;
    half VF = light.normalizationTerm;
    
    brdf += surface.specular * D * rcp(VF);
    return brdf*light.radiance;
}

half3 BRDFLighting(BRDFSurface surface, Light light,half anisotropic)
{
    BRDFLight brdfLight=BRDFLight_Ctor(surface,light.direction,light.color,light.shadowAttenuation*light.distanceAttenuation,anisotropic);
    return BRDFLighting(surface,brdfLight);
}

half3 BRDFGlobalIllumination(BRDFSurface surface,half3 indirectDiffuse,half3 indirectSpecular)
{
    indirectDiffuse *= surface.ao;
    indirectSpecular *= surface.ao;
    
    half3 giDiffuse = indirectDiffuse * surface.diffuse;
    
    float fresnelTerm = surface.fresnelTerm;
    float3 surfaceReduction = 1.0 / (surface.roughness2 + 1.0) * lerp(surface.specular, surface.grazingTerm, fresnelTerm);
    half3 giSpecular = indirectSpecular * surfaceReduction;
    return giDiffuse + giSpecular;
}
