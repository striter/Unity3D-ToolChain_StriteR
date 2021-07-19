
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

