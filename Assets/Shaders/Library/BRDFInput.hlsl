struct BRDFSurface
{
    half3 diffuse;
    half3 specular;
    
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
};

struct BRDFLight
{
    half3 color;
    half3 radiance;
    half3 lightDir;
    
    half normalDistribution;
    half invNormalizationTerm;
};
