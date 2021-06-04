#define PI_ONE 3.1415926535
#define PI_TWO 6.2831853071796
#define PI_FOUR 12.566370614359
#define PI_SQRT2 0.797884560802865
#define PI_ONEMINUS 0.31830988618379

float sqr(float value)
{
    return value * value;
}
float pow5(float value)
{
    return value * value * value * value * value;
}

//float GSF_Burley(float NDL,float NDV,float LDH,float roughness)
//{
//    half FD90MinusOne = -.5 + 2.0 * LDH * LDH * roughness;
//    float NDLPow5 = pow5(1. - NDL);
//    float NDVPow5 = pow5(1. - NDV);
//    return (1. + FD90MinusOne * NDLPow5) * (1. - FD90MinusOne * NDVPow5) * NDL;
//}

//NDF,Normal Distribution Function

float NDF_BlinnPhong(float NDH, float specularPower, float specularGloss)
{
    float distribution = pow(NDH, specularGloss) * specularPower;
    distribution *= (2 + specularPower) / PI_TWO;
    return distribution;
}
float NDF_Phong(float RDV, float specularPower, float specularGloss)
{
    float Distribution = pow(RDV, specularGloss) * specularPower;
    Distribution *= (2 + specularPower) / PI_TWO;
    return Distribution;
}
float NDF_Beckmann(float NDH, float sqrRoughness)
{
    float sqrNDH = dot(NDH, NDH);
    return max(0.000001, (1.0 / (PI_ONE * sqrRoughness * sqrNDH * sqrNDH)) * exp((sqrNDH - 1) / (sqrNDH * sqrRoughness)));
}
float NDF_Gaussian(float NDH, float sqrRoughness)
{
    float thetaH = acos(NDH);
    return exp(-thetaH * thetaH / sqrRoughness);
}
float NDF_GGX(float NDH,float roughness, float sqrRoughness)
{
    float sqrNDH = dot(NDH, NDH);
    float tanSqrNDH = (1 - sqrNDH) / sqrNDH;
    return max ( 0.00001, PI_ONEMINUS * sqr(roughness / (sqrNDH * (sqrRoughness + tanSqrNDH))));
}
float NDF_CookTorrance(float NDH,float LDH,float roughness,float roughness2)
{
    float d = NDH * NDH *( roughness2-1.) +1.00001f;
    float sqrLDH = sqr(LDH);
    float normalizationTerm = roughness * 4. + 2.;
    return roughness2 / ((d * d) * max(0.1h, sqrLDH) * normalizationTerm);
}
float NDF_TrowbridgeReitz(float NDH, float roughness,float sqrRoughness)
{
    float sqrNDH = dot(NDH, NDH);
    float distribution = sqrNDH * (sqrRoughness - 1.0) + 1.0;
    return sqrRoughness / (PI_ONE * distribution * distribution+1e-5f);
}
//Anisotropic NDF
float NDFA_TrowbridgeReitz(float NDH, float HDX, float HDY, float anisotropic, float glossiness)
{
    float aspect = sqrt(1.0h - anisotropic * 0.9h);
    glossiness = sqr(1.0 - glossiness);
    float X = max(.001, glossiness / aspect) * 5;
    float Y = max(.001, glossiness * aspect) * 5;
    return 1.0 / (PI_ONE * X * Y * sqr(sqr(HDX / X) + sqr(HDY / Y) + sqr(NDH)));
}
float NDFA_Ward(float NDL, float NDV, float NDH, float HDX, float HDY, float anisotropic, float glossiness)
{
    float aspect = sqrt(1.0h - anisotropic * 0.9h);
    glossiness = sqr(1.0 - glossiness);
    float X = max(.001, glossiness / aspect) * 5;
    float Y = max(.001, glossiness * aspect) * 5;
    float exponent = -(sqr(HDX / X) + sqr(HDY / Y)) / sqr(NDH);
    float distribution = 1. / (PI_FOUR * X * Y) * sqrt(NDL*NDV);
    distribution *= exp(exponent);
    return distribution;
}

//GSF Geometric Shadowing Function
float GSF_Implicit(float NDL, float NDV)
{
    return NDL * NDV;
}
float GSF_AshikhminShirley(float NDL, float NDV, float LDH)
{
    return NDL * NDV / (LDH * max(NDL, NDV));
}
float GSF_AshikhminPremoze(float NDL, float NDV)
{
    return NDL * NDV / (NDL + NDV - NDL * NDV);
}
float GSF_Duer(float NDL, float NDV, float3 lightDir, float3 viewDir, float3 normal)
{
    return dot(NDL, NDV) * pow(dot(lightDir + viewDir, normal), -.4);
}
float GSF_Neumann(float NDL, float NDV)
{
    return NDL * NDV / max(NDL, NDV);
}
float GSF_Kelemen(float NDL, float NDV, float VDH)
{
    return NDL * NDV / (VDH * VDH);
}
float GSF_CookTorrance(float NDL, float NDV, float VDH, float NDH)
{
    return min(1.0, min(2 * NDH * NDV / VDH, 2 * NDH * NDL / VDH));
}
float GSF_Ward(float NDL, float NDV)
{
    return pow(NDL * NDV, .5);
}

//roughness based GSF
float GSFR_Kelemen_Modifed(float NDL, float NDV, float roughness)
{
    float k = sqr(roughness) * PI_SQRT2;
    float gH = NDV * k + (1 - k);
    return gH * gH * NDL;
}
float GSFR_Kurt(float NDL, float NDV, float VDH, float roughness)
{
    return NDL * NDV / (VDH * pow(NDL * NDV, 1 - roughness));
}
//Smith-Based GSF
float GSFR_WalterEtAl(float NDL, float NDV, float roughness)
{
    float sqrRoughness = sqr(roughness);
    float sqrNDL = sqr(NDL);
    float sqrNDV = sqr(NDV);
    float smithL = 2 / (1 + sqrt(1 + sqrRoughness * (1 - sqrNDL) / sqrNDL));
    float smithV = 2 / (1 + sqrt(1 + sqrRoughness * (1 - sqrNDV) / sqrNDV));
    return smithL * smithV;
}
float GSFR_SmithBeckmann(float NDL, float NDV, float roughness)
{
    float sqrRoughness = max(0.00001, sqr(roughness));
    float sqrNDL = sqr(NDL);
    float sqrNDV = sqr(NDV);
    float calculationL = NDL / (sqrRoughness * sqrt(1 - sqrNDL));
    float calculationV = NDV / (sqrRoughness * sqrt(1 - sqrNDV));
    float sqrCalculationL = sqr(calculationL);
    float sqrCalculationV = sqr(calculationV);
    float smithL = calculationL < 1.6 ? ((3.535 * calculationL + 2.181 * sqrCalculationL) / (1 + 2.276 * calculationL + 2.577 * sqrCalculationL)) : 1.0;
    float smithV = calculationV < 1.6 ? ((3.535 * calculationV + 2.181 * sqrCalculationV) / (1 + 2.276 * calculationV + 2.577 * sqrCalculationV)) : 1.0;
    return smithL * smithV;
}
float GSFR_GGX(float NDL, float NDV, float roughness)
{
    float sqrRoughness = sqr(roughness);
    float sqrNDL = sqr(NDL);
    float sqrNDV = sqr(NDV);
    float smithL = 2 * NDL / (NDL + sqrt(sqrRoughness + (1 - sqrRoughness) * sqrNDL));
    float smithV = 2 * NDV / (NDV + sqrt(sqrRoughness + (1 - sqrRoughness) * sqrNDV));
    return smithL * smithV;
}
float GSFR_Schlick(float NDL, float NDV, float roughness)
{
    float sqrRoughness = sqr(roughness);
    float smithL = NDL / (sqrRoughness + (1 - sqrRoughness) * NDL);
    float smithV = NDV / (sqrRoughness + (1 - sqrRoughness) * NDV);
    return smithL * smithV;
}
float GSFR_SchlickBeckmann(float NDL, float NDV, float roughness)
{
    float k = sqr(roughness) * PI_SQRT2;
    float smithL = NDL / (k + (1 - k) * NDL);
    float smithV = NDV / (k + (1 - k) * NDV);
    return smithL * smithV;
}
float GSFR_SchlickGGX(float NDL, float NDV, float roughness)
{
    float k = roughness / 2;
    float smithL = NDL / (k + NDL * (1 - k));
    float smithV = NDV / (k + NDV * (1 - k));
    return smithL * smithV;
}

//Fresnel
float F_Schlick(float NDV)
{ 
    float x = saturate(1. - NDV);
    return pow5(x);
}
float F_SchlickIOR(float NDV, float ior)
{
    float f0 = sqr((ior - 1.) / (ior + 1.));
    return f0 + (1. - f0) * F_Schlick(NDV);
}
float F_SphericalGaussian(float NDV)
{
    float power = (-5.55473 * NDV - 6.98316) * NDV;
    return pow(2, power);
}

//normal incidence reflection
float F0(float NDL, float NDV, float LDH, float roughness)
{
    float fresnelLight = F_Schlick(NDL);
    float fresnelView = F_Schlick(NDV);
    float fresnelDiffuse90 = .5 + 2 * sqr(LDH) * roughness;
    return lerp(1, fresnelDiffuse90, fresnelLight) * lerp(1, fresnelDiffuse90, fresnelView);
}