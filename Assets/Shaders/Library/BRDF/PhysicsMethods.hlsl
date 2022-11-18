
#define PI_SQRT2 4.442882938158

//Unused
float F0(float NDL, float NDV, float LDH, float roughness)
{
    float fresnelLight = F_Schlick(NDL);
    float fresnelView = F_Schlick(NDV);
    float fresnelDiffuse90 = .5 + 2 * pow2(LDH) * roughness;
    return lerp(1, fresnelDiffuse90, fresnelLight) * lerp(1, fresnelDiffuse90, fresnelView);
}

//Fresnel
float F_SchlickIOR(float NDV, float ior)
{
    float f0 = pow2((ior - 1.) / (ior + 1.));
    return f0 + (1. - f0) * F_Schlick(NDV);
}

float F_SphericalGaussian(float NDV)
{
    float power = (-5.55473 * NDV - 6.98316) * NDV;
    return pow(2, power);
}

//GSF Geometric Shadowing Function
float GSF_Burley(float NDL, float NDV, float LDH, float roughness)
{
    half FD90MinusOne = -.5 + 2.0 * LDH * LDH * roughness;
    float NDLPow5 = pow5(1. - NDL);
    float NDVPow5 = pow5(1. - NDV);
    return (1. + FD90MinusOne * NDLPow5) * (1. - FD90MinusOne * NDVPow5) * NDL;
}

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

float MieScattering(float lightCos, float g1, float g2, float g3)  
{
    return g1 / pow(g2 - g3 * lightCos, 1.5);
}

float MieScattering(float3 lightDir,float3 rayDir,float g)      //Fast Test, Precalculate G1 G2 G3 Pls
{
    float powG = g * g;
    float g1 = (1 - powG)/4*PI;
    float g2 = 1 + powG;
    float g3 = g * 2;
    float lightCos = dot(lightDir, rayDir);
    return MieScattering(lightCos, g1, g2, g3);
}

float BeerLambert(float stepSize,float density,float scatterFactor,float extinctionFactor,inout float extinction)
{
    float scattering = scatterFactor * stepSize * density;
    extinction += extinctionFactor * stepSize * density;
    return scattering * exp(-extinction);
}

