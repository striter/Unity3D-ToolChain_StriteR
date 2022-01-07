#define DIELETRIC_SPEC half4(0.04h,0.04h,0.04h,0.96h)
struct BRDFSurface
{
    half3 diffuse;
    half3 specular;
    half3 emission;
    
    half metallic;
    half smoothness;
    half ao;
    
    half3 normal;
    half3 tangent;
    half3 viewDir;
    half3 reflectDir;
    half NDV;
    
    half grazingTerm;
    half fresnelTerm;
    half perceptualRoughness;
    half roughness;
    half roughness2;
    half anisotropic;
};

BRDFSurface BRDFSurface_Ctor(half3 albedo,half3 emission, half smoothness, half metallic,half ao, half3 normal, half3 tangent, half3 viewDir,half anisotropic)
{
    BRDFSurface surface;
    
    half oneMinusReflectivity = DIELETRIC_SPEC.a - metallic * DIELETRIC_SPEC.a;
    half reflectivity = 1.0h - oneMinusReflectivity;
				
    surface.diffuse = albedo * oneMinusReflectivity;
    surface.emission=emission;
    surface.specular = lerp(DIELETRIC_SPEC.rgb, albedo, metallic);
    surface.metallic = metallic;
    surface.smoothness = smoothness;
    surface.ao = ao;
    surface.anisotropic=anisotropic;
    
    surface.normal = normal;
    surface.tangent = tangent;
    surface.viewDir = viewDir;
    surface.reflectDir = normalize(reflect(-surface.viewDir, surface.normal));
    surface.NDV = max(0.h, dot(surface.normal,surface.viewDir));
    
    surface.grazingTerm = saturate(smoothness + reflectivity);
    surface.fresnelTerm=F_Schlick(surface.NDV);
    surface.perceptualRoughness = 1.0h - surface.smoothness;
    surface.roughness = max(HALF_MIN_SQRT, surface.perceptualRoughness * surface.perceptualRoughness);
    surface.roughness2 = max(HALF_MIN, surface.roughness * surface.roughness);
    return surface;
}

struct BRDFLightInput
{
    half3 halfDir;
    half3 lightDirection;
    half3 lightColor;
    half shadowAttenuation;
    half distanceAttenuation;
    half NDL;
    half VDH;
    half LDV;
    half NDH;
    half LDH;
};

BRDFLightInput BRDFLightInput_Ctor(BRDFSurface surface,half3 lightDir,half3 lightColor,half shadowAttenuation,half distanceAttenuation)
{
    half3 viewDir = surface.viewDir;
    half3 normal = surface.normal;
    float3 halfDir = SafeNormalize(float3(viewDir) + float3(lightDir));

    BRDFLightInput input;
    input.lightDirection=lightDir;
    input.lightColor=lightColor;
    input.shadowAttenuation=shadowAttenuation;
    input.distanceAttenuation=distanceAttenuation;
    input.halfDir=halfDir;
    input.NDL = max(0., dot(normal, lightDir));
    input.VDH = max(0., dot(viewDir, halfDir));
    input.LDV = max(0., dot(lightDir, viewDir));
    input.NDH = max(0., dot(normal, halfDir));
    input.LDH = max(0., dot(lightDir, halfDir));
    return input;
}

BRDFLightInput BRDFLightInput_Ctor(BRDFSurface surface,Light light)
{
    return BRDFLightInput_Ctor(surface,light.direction,light.color,light.shadowAttenuation,light.distanceAttenuation);
}