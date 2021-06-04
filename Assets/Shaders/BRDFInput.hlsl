struct BRDFSurface
{
    half3 diffuse;
    half3 specular;
    half alpha;
    
    half metallic;
    half smoothness;
    half ao;
    
    half anisotropic;
    half grazingTerm;
    
    half perceptualRoughness;
    half roughness;
    half roughness2;
    
    half3 normal;
    half3 tangent;
    half3 viewDir;
};

struct BRDFLight
{
    half3 color;
    half3 radiance;
    
    half3 lightDir;
    half3 halfDir;
    half3 viewReflectDir;
    half3 lightReflectDir;
    
    float geometricShadow;
    float normalDistribution;
    float fresnel;
};
