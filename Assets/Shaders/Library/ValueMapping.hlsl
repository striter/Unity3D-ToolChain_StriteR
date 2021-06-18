
float invlerp(float _a, float _b, float _value)
{
    return (_value - _a) / (_b - _a);
}
float remap(float _value, float _from1, float _to1, float _from2, float _to2)
{
    return lerp(_from2, _to2, invlerp(_from1, _to1, _value));
}

float max(float _max1, float _max2, float _max3, float _max4)
{
    float final = max(_max1, _max2);
    final = max(final, _max3);
    final = max(final, _max4);
    return final;
}

float bilinearLerp(float tl, float tr, float bl, float br, float2 uv)
{
    float lerpT = lerp(tl, tr, uv.x);
    float lerpB = lerp(bl, br, uv.x);
    return lerp(lerpB, lerpT, uv.y);
}
float2 bilinearLerp(float2 tl, float2 tr, float2 bl, float2 br, float2 uv)
{
    float2 lerpT = lerp(tl, tr, uv.x);
    float2 lerpB = lerp(bl, br, uv.x);
    return lerp(lerpB, lerpT, uv.y);
}

float3 bilinearLerp(float3 tl, float3 tr, float3 bl, float3 br, float2 uv)
{
    float3 lerpT = lerp(tl, tr, uv.x);
    float3 lerpB = lerp(bl, br, uv.x);
    return lerp(lerpB, lerpT, uv.y);
}
float min(float3 target)
{
    return min(min(target.x, target.y), target.z);
}
float max(float3 target)
{
    return max(max(target.x, target.y), target.z);
}
half min(half3 target)
{
    return min(min(target.x, target.y), target.z);
}
half max(half3 target)
{
    return max(max(target.x, target.y), target.z);
}
