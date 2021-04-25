#define SQRT2DPI 0.797884560802865

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


float sqr(float value)
{
    return value * value;
}
float UnpackRoughness(float glossiness)
{
    float roughness = 1 - glossiness * glossiness;
    return roughness * roughness;
}

//NDF,Normal Distribution Function
float NDF_BlinnPhong(float NDH, float specularPower, float specularGloss)
{
    float distribution = pow(NDH, specularGloss) * specularPower;
    distribution *= (2 + specularPower) / (2 * PI);
    return distribution;
}
float NDF_Beckmann(float NDH, float roughness)
{
    float sqrRoughness = dot(roughness, roughness);
    float sqrNDH = dot(NDH, NDH);
    return max(0.000001, (1.0 / (PI * sqrRoughness * sqrNDH * sqrNDH)) * exp((sqrNDH - 1) / (sqrNDH * sqrRoughness)));
}
float NDF_Gaussian(float NDH, float roughness)
{
    float sqrRoughness = dot(roughness, roughness);
    float thetaH = acos(NDH);
    return exp(-thetaH * thetaH / sqrRoughness);
}
float NDF_GGX(float NDH, float roughness)
{
    float sqrRoughness = dot(roughness, roughness);
    float sqrNDH = dot(NDH, NDH);
    float tanSqrNDH = (1 - sqrNDH) / sqrNDH;
    return (1.0 / PI) * sqr(roughness / (sqrNDH * (sqrRoughness + tanSqrNDH)));
}
float NDF_TrowbridgeReitz(float NDH, float roughness)
{
    float sqrRoughness = dot(roughness, roughness);
    float sqrNDH = dot(NDH, NDH);
    float distribution = sqrNDH * (sqrRoughness - 1.0) + 1.0;
    return sqrRoughness / (PI * distribution * distribution);
}
//Anisotropic NDF
float NDFA_TrowbridgeReitz(float NDH, float HDX, float HDY, float anisotropic, float glossiness)
{
    float aspect = sqrt(1.0h - anisotropic * 0.9h);
    glossiness = sqr(1.0 - glossiness);
    float X = max(.001, glossiness / aspect) * 5;
    float Y = max(.001, glossiness * aspect) * 5;
    return 1.0 / (PI * X * Y * sqr(sqr(HDX / X) + sqr(HDY / Y) + sqr(NDH)));
}
float NDFA_Ward(float NDL, float NDV, float NDH, float HDX, float HDY, float anisotropic, float glossiness)
{
    float aspect = sqrt(1.0h - anisotropic * 0.9h);
    glossiness = sqr(1.0 - glossiness);
    float X = max(.001, glossiness / aspect) * 5;
    float Y = max(.001, glossiness * aspect) * 5;
    float distribution = 1.0 / (4.0 * PI * X * Y * sqrt(NDL * NDV));
    distribution *= exp(-(sqr(HDX / X) + sqr(HDY / Y)) / sqr(NDH));
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
float GSF_CookTorrence(float NDL, float NDV, float VDH, float NDH)
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
    float k = sqr(roughness) * SQRT2DPI;
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
    float sqrRoughness = sqr(roughness);
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
    float k = sqr(roughness) * SQRT2DPI;
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
float Fresnel_Schlick(float i)
{
    float x = saturate(1 - i);
    float x2 = x * x;
    return x2 * x2 * x;
}

float Fresnel_SphericalGaussian(float i)
{
    float power = (-5.55473 * i - 6.98316) * i;
    return pow(2, power);
}

//normal incidence reflection
float F0(float NDL, float NDV, float LDH, float roughness)
{
    float fresnelLight = Fresnel_Schlick(NDL);
    float fresnelView = Fresnel_Schlick(NDV);
    float fresnelDiffuse90 = .5 + 2 * sqr(LDH) * roughness;
    return lerp(1, fresnelDiffuse90, fresnelLight) * lerp(1, fresnelDiffuse90, fresnelView);
}