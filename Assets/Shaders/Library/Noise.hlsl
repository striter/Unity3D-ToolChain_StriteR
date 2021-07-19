
float random01(float value)
{
    return frac(sin(value * 12.9898) * 43758.543123);
}
float random01(float2 value)
{
    return frac(sin(dot(value, float2(12.9898, 78.233))) * 43758.543123);
}
float random01(float3 value)
{
    return frac(sin(dot(value, float3(12.9898, 78.233, 53.539))) * 43758.543123);
}

float randomUnit(float value)
{
    return random01(value) * 2 - 1;
}
float randomUnit(float2 value)
{
    return random01(value) * 2 - 1;
}
float randomUnit(float3 value)
{
    return random01(value) * 2 - 1;
}
float2 randomUnitQuad(float2 value)
{
    return float2(randomUnit(value.xy), randomUnit(value.yx));
}
float2 randomUnitCircle(float2 value)
{
    float theta = 2 * PI * random01(value);
    return float2(cos(theta), sin(theta));
}

float quinterp(float _f)
{
    return _f * _f * _f * (_f * (_f * 6 - 15) + 10);
}
float random01Perlin(float2 value)
{
    float2 pos00 = floor(value);
    float2 pos10 = pos00 + float2(1.0f, 0.0f);
    float2 pos01 = pos00 + float2(0.0f, 1.0f);
    float2 pos11 = pos00 + float2(1.0f, 1.0f);

    float2 rand00 = randomUnitCircle(pos00);
    float2 rand10 = randomUnitCircle(pos10);
    float2 rand01 = randomUnitCircle(pos01);
    float2 rand11 = randomUnitCircle(pos11);
    
    float dot00 = dot(rand00, pos00 - value);
    float dot01 = dot(rand01, pos01 - value);
    float dot10 = dot(rand10, pos10 - value);
    float dot11 = dot(rand11, pos11 - value);
    
    float2 d = frac(value);
    float interpolate = quinterp(d.x);
    float x1 = lerp(dot00, dot10, interpolate);
    float x2 = lerp(dot01, dot11, interpolate);
    return lerp(x1, x2, quinterp(d.y));
}

float randomUnitPerlin(float2 value)
{
    return random01Perlin(value) * 2 - 1;
}
