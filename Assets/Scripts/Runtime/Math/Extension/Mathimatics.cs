using System;
using UnityEngine;
using Unity.Mathematics;

public static class kfloat3
{
    public static readonly float3 one = 1f;
    public static readonly float3 up = new float3(0, 1, 0);
    public static readonly float3 down = new float3(0, -1, 0);
    public static readonly float3 left = new float3(-1, 0, 0);
    public static readonly float3 right = new float3(1, 0, 0);
    public static readonly float3 forward = new float3(0, 0, 1);
    public static readonly float3 back = new float3(0, 0, -1);
    
    public static readonly float3 leftDownBack = new float3(-1, -1, -1);
    public static readonly float3 rightUpForward = new float3(1, 1, 1);
    public static readonly float3 min = (float3)float.MinValue ;
    public static readonly float3 max = (float3)float.MaxValue ;
}

public static class kfloat2
{
    public static readonly float2 one = new float2(1, 1);
    public static readonly float2 up = new float2(0, 1);
    public static readonly float2 down = new float2(0, -1);
    public static readonly float2 left = new float2(-1, 0);
    public static readonly float2 right = new float2(1, 0);
}

public static class kint2
{
    public static readonly int2 one = new(1, 1);
    public static readonly int2 k00 = new(0, 0); public static readonly int2 k01 = new(0, 1); public static readonly int2 k02 = new(0, 2); public static readonly int2 k03 = new(0, 3);
    public static readonly int2 k10 = new(1, 0); public static readonly int2 k11 = new(1, 1); public static readonly int2 k12 = new(1, 2); public static readonly int2 k13 = new(1, 3);
    public static readonly int2 k20 = new(2, 0); public static readonly int2 k21 = new(2, 1); public static readonly int2 k22 = new(2, 2); public static readonly int2 k23 = new(2, 3);
    public static readonly int2 k30 = new(3, 0); public static readonly int2 k31 = new(3, 1); public static readonly int2 k32 = new(3, 2); public static readonly int2 k33 = new(3, 3);
}


public static class umath_swizzlling       //Swizzling
{
    public static float3 to3xy(this float2 _value, float _z = 0) => new float3(_value, _z);
    public static float3 to3xz(this float2 _value, float _y = 0) => new float3(_value.x, _y,_value.y);
    
    public static float3 to3xyz(this float4 _value) => new float3(_value.x, _value.y,_value.z);
    public static float4 to4(this float2 _value, float _z=0,float _w=0) => new float4(_value, _z,_w);
    public static float4 to4(this float3 _value, float _w=0) => new float4(_value, _w);
    public static float4 to4(this float _value) => new float4(_value, _value,_value,_value);

    public static float3 setY(this float3 _value, float _y) => new float3(_value.x, _y, _value.z);
    
    public static float magnitude(this float2 _value) => math.length(_value);
    public static float magnitude(this float3 _value) => math.length(_value);
    public static float magnitude(this float4 _value) => math.length(_value);
    
    public static float sqrmagnitude(this float2 _value) => math.lengthsq(_value);
    public static float sqrmagnitude(this float3 _value) => math.lengthsq(_value);
    public static float sqrmagnitude(this float4 _value) => math.lengthsq(_value);

    public static float sum(this float2 _value) => _value.x + _value.y;
    public static float sum(this float3 _value) => _value.x + _value.y + _value.z;
    public static float sum(this float4 _value) => _value.x + _value.y + _value.z + _value.w;

    public static bool isZero(this float2 _value) => _value is { x: 0, y: 0 };
    public static bool isZero(this float3 _value) => _value is { x: 0, y: 0, z: 0 };
    public static bool isZero(this float4 _value) => _value is { x: 0, y: 0, z: 0, w: 0 };
    
    public static float2 normalize(this float2 _value) => math.normalize(_value);
    public static float3 normalize(this float3 _value) => math.normalize(_value);
    public static float4 normalize(this float4 _value) => math.normalize(_value);
    
    public static float saturate(this float _value)=> math.min(math.max(_value,0f) ,1f);
    public static float2 saturate(this float2 _value)=> math.min(math.max(_value,0f) ,1f);
    public static float3 saturate(this float3 _value)=> math.min(math.max(_value,0f) ,1f);
    public static float4 saturate(this float4 _value) => math.min(math.max(_value,0f) ,1f);
    
    public static float clamp(this float _value,float _min,float _max)=> math.min(math.max(_value,_min) ,_max);
    public static float2 clamp(this float2 _value,float2 _min,float2 _max)=> math.min(math.max(_value,_min) ,_max);
    public static float3 clamp(this float3 _value,float3 _min,float3 _max)=> math.min(math.max(_value,_min) ,_max);
    public static float4 clamp(this float4 _value,float4 _min,float4 _max)=> math.min(math.max(_value,_min) ,_max);

    public static float dot(this float3 _src) => math.dot(_src, _src);
    public static float dot(this float3 _src,float3 _dst) => math.dot(_src, _dst);
    
    public static bool anyGreater(this float2 _value, float _comparer) => _value.x > _comparer || _value.y > _comparer;
    public static bool anyGreater(this float3 _value, float _comparer) => _value.x > _comparer || _value.y > _comparer || _value.z > _comparer;
    public static bool anyGreater(this float4 _value, float _comparer) => _value.x > _comparer || _value.y > _comparer || _value.z > _comparer || _value.w > _comparer;
    
    public static float minElement(this float2 _src) => Mathf.Min(_src.x, _src.y);
    public static float minElement(this float3 _src) => Mathf.Min(_src.x, _src.y, _src.z);
    public static float minElement(this float4 _src) => Mathf.Min(_src.x, _src.y, _src.z, _src.w);
    
    public static float maxElement(this float2 _src) => Mathf.Max(_src.x, _src.y);
    public static float maxElement(this float3 _src) => Mathf.Max(_src.x, _src.y, _src.z);
    public static float maxElement(this float4 _src) => Mathf.Max(_src.x, _src.y, _src.z, _src.w);

    public static float convert(this float _src, Func<float, float> _func) => _func(_src);
    public static float2 convert(this float2 _src, Func<float, float> _func) => new float2(_func(_src.x),_func(_src.y));
    public static float3 convert(this float3 _src, Func<float, float> _func) => new float3(_func(_src.x),_func(_src.y),_func(_src.z));
    public static float4 convert(this float4 _src, Func<float, float> _func) => new float4(_func(_src.x),_func(_src.y),_func(_src.z),_func(_src.w));
    
    public static float3 convert(this float3 _src, Func<int,float, float> _func) => new float3(_func(0,_src.x),_func(1,_src.y),_func(2,_src.z));
    public static float4 convert(this float4 _src, Func<int,float, float> _func) => new float4(_func(0,_src.x),_func(1,_src.y),_func(2,_src.z),_func(3,_src.z));


    public static float2 cross(this float2 _src) => new float2(_src.y,-_src.x);
}
