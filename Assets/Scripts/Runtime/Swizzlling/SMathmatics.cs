using System;
using Unity.Mathematics;
using UnityEngine;

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
    
    public static bool anyLesser(this float2 _value, float _comparer) => _value.x < _comparer || _value.y < _comparer;
    public static bool anyLesser(this float3 _value, float _comparer) => _value.x < _comparer || _value.y < _comparer || _value.z < _comparer;
    public static bool anyLesser(this float4 _value, float _comparer) => _value.x < _comparer || _value.y < _comparer || _value.z < _comparer || _value.w < _comparer;
    
    public static float minElement(this float2 _src) => math.min(_src.x, _src.y);
    public static float minElement(this float3 _src) => math.min(_src.x, math.min(_src.y, _src.z));
    public static float minElement(this float4 _src) => math.min(_src.x, math.min(_src.y, math.min(_src.z, _src.w)));
    
    public static float maxElement(this float2 _src) => math.max(_src.x, _src.y);
    public static float maxElement(this float3 _src) => math.max(_src.x,math.max( _src.y, _src.z));
    public static float maxElement(this float4 _src) => math.max(_src.x, math.max(_src.y,math.max( _src.z, _src.w)));

    public static float convert(this float _src, Func<float, float> _func) => _func(_src);
    public static float2 convert(this float2 _src, Func<float, float> _func) => new float2(_func(_src.x),_func(_src.y));
    public static float3 convert(this float3 _src, Func<float, float> _func) => new float3(_func(_src.x),_func(_src.y),_func(_src.z));
    public static float4 convert(this float4 _src, Func<float, float> _func) => new float4(_func(_src.x),_func(_src.y),_func(_src.z),_func(_src.w));
    
    public static float3 convert(this float3 _src, Func<int,float, float> _func) => new float3(_func(0,_src.x),_func(1,_src.y),_func(2,_src.z));
    public static float4 convert(this float4 _src, Func<int,float, float> _func) => new float4(_func(0,_src.x),_func(1,_src.y),_func(2,_src.z),_func(3,_src.z));


    public static float2 cross(this float2 _src) => new float2(_src.y,-_src.x);
}