
float2x2 Rotate2x2(float _angle)
{
    float sinAngle, cosAngle;
    sincos(_angle, sinAngle, cosAngle);
    return float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
}

float3x3 Rotate3x3(float _angle, float3 _axis)
{
    float s, c;
    sincos(_angle, s, c);

    float t = 1 - c;
    float x = _axis.x;
    float y = _axis.y;
    float z = _axis.z;

    return float3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c);
}

float sqrDistance(float3 _offset)
{
    return dot(_offset, _offset);
}
float sqrDistance(float3 _pA, float3 _pB)
{
    return sqrDistance(_pA - _pB);
}

half Blend_Overlay(half _src,half _dst)
{
    return _src<.5h?2.h*_src*_dst:1.h-2.h*(1.h-_src)*(1.h-_dst);
}
half3 Blend_Overlay(half3 _src,half3 _dst)
{
    return half3(Blend_Overlay(_src.x,_dst.x),Blend_Overlay(_src.y,_dst.y),Blend_Overlay(_src.z,_dst.z));
}
float4 Blend_Screen(float4 _src, float4 _dst)
{
    return 1 - (1 - _src) * (1 - _dst);
}
float3 Blend_Screen(float3 _src, float3 _dst)
{
    return 1 - (1 - _src) * (1 - _dst);
}

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
half min(half2 target)
{
    return min(target.x, target.y);
}
half max(half2 target)
{
    return max(target.x, target.y);
}

half min(half3 target)
{
    return min(min(target.x, target.y), target.z);
}
half max(half3 target)
{
    return max(max(target.x, target.y), target.z);
}
