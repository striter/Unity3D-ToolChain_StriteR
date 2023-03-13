
float quinterp(float _f)
{
    return _f * _f * _f * (_f * (_f * 6 - 15) + 10);
}
float perlin(float2 _v)
{
    float2 pos00 = floor(_v);
    float2 pos10 = pos00 + float2(1.0f, 0.0f);
    float2 pos01 = pos00 + float2(0.0f, 1.0f);
    float2 pos11 = pos00 + float2(1.0f, 1.0f);

    float2 rand00 = randomUnitCircle(pos00);
    float2 rand10 = randomUnitCircle(pos10);
    float2 rand01 = randomUnitCircle(pos01);
    float2 rand11 = randomUnitCircle(pos11);
    
    float dot00 = dot(rand00, pos00 - _v);
    float dot01 = dot(rand01, pos01 - _v);
    float dot10 = dot(rand10, pos10 - _v);
    float dot11 = dot(rand11, pos11 - _v);
    
    float2 d = frac(_v);
    float interpolate = quinterp(d.x);
    float x1 = lerp(dot00, dot10, interpolate);
    float x2 = lerp(dot01, dot11, interpolate);
    return lerp(x1, x2, quinterp(d.y));
}

float perlinUnit(float2 _v)
{
    return perlin(_v) * 2 - 1;
}