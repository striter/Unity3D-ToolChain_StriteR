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
BRDFLight BRDFLight_Ctor(BRDFSurface surface,BRDFLightInput input)
{
    BRDFLight light;
    light.lightDir = input.lightDirection;
    light.color = input.lightColor;
    light.radiance = input.lightColor * (input.distanceAttenuation*input.shadowAttenuation * GEOMETRY_SHADOW(surface,input));
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

half3 BRDFLighting(BRDFSurface surface, Light light)
{
    BRDFLightInput input=BRDFLightInput_Ctor(surface,light.direction,light.color,light.shadowAttenuation,light.distanceAttenuation);
    BRDFLight brdfLight=BRDFLight_Ctor(surface,input);
    return BRDFLighting(surface,brdfLight);
}

half3 BRDFGlobalIllumination(BRDFSurface surface,half3 indirectDiffuse,half3 indirectSpecular)
{
    indirectDiffuse *= surface.ao;
    indirectSpecular *= surface.ao;
    
    half3 giDiffuse = indirectDiffuse * surface.diffuse;
    
    float fresnelTerm =F_Schlick(max(0,surface.NDV));
    float3 surfaceReduction = 1.0 / (surface.roughness2 + 1.0) * lerp(surface.specular, surface.grazingTerm, fresnelTerm);
    half3 giSpecular = indirectSpecular * surfaceReduction;
    return giDiffuse + giSpecular;
}
