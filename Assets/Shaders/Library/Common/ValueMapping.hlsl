#define PI_ONE 3.1415926535
#define PI_TWO 6.2831853071796
#define PI_FOUR 12.566370614359
#define PI_SQRT2 0.797884560802865
#define PI_ONEMINUS 0.31830988618379

//Pow
half pow2(half value){ return value * value; }
half pow3(half value) { return value*value*value; }
half pow4(half value) { return value * value * value * value ;}
half pow5(half value) { return value * value * value * value * value;}

float pow2(float value){ return value * value; }
float pow3(float value) { return value*value*value; }
float pow4(float value) { return value * value * value * value ;}
float pow5(float value) { return value * value * value * value * value;}

float sqrDistance(float2 _offset)
{
    return dot(_offset,_offset);
}

float sqrDistance(float3 _offset)
{
    return dot(_offset, _offset);
}

float sqrDistance(float3 _pA, float3 _pB)
{
    return sqrDistance(_pA - _pB);
}

//Max Float
float max(float _max1,float _max2,float _max3)
{
    return max(_max1,max(_max2,_max3));
}

float max(float _max1, float _max2, float _max3, float _max4)
{
    return max(_max1,max(_max2,max(_max3,_max4)));
}

float max(float _max1, float _max2, float _max3, float _max4,float _max5)
{
    return max(_max1,max(_max2,max(_max3,max(_max4,_max5))));
}

float min(float2 _target)
{
    return min(_target.x,_target.y);
}

float min(float3 _target)
{
    return min(min(_target.x, _target.y), _target.z);
}

//Max Half
half max(half _max1,half _max2,half _max3)
{
    return max(_max1,max(_max2,_max3));
}

half max(half _max1,half _max2,half _max3,half _max4)
{
    return max(_max1,max(_max2,max(_max3,_max4)));
}

half max(half _max1,half _max2,half _max3,half _max4,half _max5)
{
    return max(_max1,max(_max2,max(_max3,max(_max4,_max5))));
}

half max(half2 _target)
{
    return max(_target.x, _target.y);
}

half max(half3 _target)
{
    return max(_target.x,_target.y,_target.z);
}

//Min Float
float min(float _min1,float _min2,float _min3)
{
    return min(_min1,min(_min2,_min3));
}

float min(float _min1, float _min2, float _min3, float _min4)
{
    return min(_min1,min(_min2,min(_min3,_min4)));
}

float min(float _min1, float _min2, float _min3, float _min4,float _min5)
{
    return min(_min1,min(_min2,min(_min3,min(_min4,_min5))));
}

float max(float2 _target)
{
    return max(_target.x,_target.y);
}

float max(float3 _target)
{
    return max(_target.x, _target.y, _target.z);
}

//Min Half
half min(half _min1,half _min2,half _min3)
{
    return min(_min1,min(_min2,_min3));
}

half min(half _min1,half _min2,half _min3,half _min4)
{
    return min(_min1,min(_min2,min(_min3,_min4)));
}

half min(half _min1,half _min2,half _min3,half _min4,half _min5)
{
    return min(_min1,min(_min2,min(_min3,min(_min4,_min5))));
}

half min(half2 _target)
{
    return min(_target.x, _target.y);
}

half min(half3 _Target)
{
    return min(_Target.x,_Target.y,_Target.z);
}

//Bilinear Lerp
float bilinearLerp(float tl, float tr, float bl, float br, float2 uv)
{
    float lerpB = lerp(bl, br, uv.x);
    float lerpT = lerp(tl, tr, uv.x);
    return lerp(lerpB, lerpT, uv.y);
}

float2 bilinearLerp(float2 tl, float2 tr, float2 bl, float2 br, float2 uv)
{
    float2 lerpB = lerp(bl, br, uv.x);
    float2 lerpT = lerp(tl, tr, uv.x);
    return lerp(lerpB, lerpT, uv.y);
}

float3 bilinearLerp(float3 tl, float3 tr, float3 bl, float3 br, float2 uv)
{
    float3 lerpB = lerp(bl, br, uv.x);
    float3 lerpT = lerp(tl, tr, uv.x);
    return lerp(lerpB, lerpT, uv.y);
}


//Blend
half Blend_Overlay(half _src,half _dst){
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

//Value Remap
float invlerp(float _a, float _b, float _value)
{
    return (_value - _a) / (_b - _a);
}

float remap(float _value, float _from1, float _to1, float _from2, float _to2)
{
    return lerp(_from2, _to2, invlerp(_from1, _to1, _value));
}
