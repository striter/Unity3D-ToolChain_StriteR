#define _AtmosphereHeight 80000
#define _PlanetRadius 6371000
#define _PlanetCenter float3(0, -_PlanetRadius, 0)
#define _DensityScaleHeight float2(7994,1200)

float3 ComputeOpticalDepth(float2 _density,float3 _extinctionR,float3 _extinctionM)
{
    float3 Tr = _density.x * _extinctionR;
    float3 Tm = _density.y * _extinctionM;
    return _MainLightColor.rgb *exp(-(Tr+Tm));
}

float RayleighScattering(float _lightCos)
{
    return 3.0/(16*PI)*(1+sqr(_lightCos));
}

float MieScattering(float _lightCos,float _g)
{
    float g2 = _g * _g;
    return INV_FOUR_PI * (3.0 * (1.0 - g2) / (2.0 * (2.0 + g2))) * ((1 + sqr(_lightCos)) / (pow((1 + g2 - 2 * _g*_lightCos), 1.5)));;
}

float BeerLambert(float stepSize,float density,float scatterFactor,float extinctionFactor,inout float extinction)
{
    float scattering = scatterFactor * stepSize * density;
    extinction += extinctionFactor * stepSize * density;
    return scattering * exp(-extinction);
}

float Sun(float _lightCos,float g)
{
    float powG = g * g;
    float g1 = (1 - powG)/4*PI;
    float g2 = 1 + powG;
    float g3 = g * 2;
    float sun = g1 / pow(g2 - g3 * _lightCos, 1.5);
    return sun * 0.003;
}

float3 RenderSun(float3 scatterM,float _lightCos,float g = 0.98)
{
    return scatterM * Sun(_lightCos,g);
}