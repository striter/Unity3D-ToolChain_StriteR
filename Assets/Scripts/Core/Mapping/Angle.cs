using System;
using Unity.Mathematics;
using UnityEngine;
using static kmath;
using static Unity.Mathematics.math;
public static partial class umath
{
    public static float lerpAngle(float _a,float _b,float _t)
    {
        var num = repeat( _b - _a, 360);
        if (num > 180)
            num -= 360;
        return _a + num * _t;
    }

    public static float3 lerpAngle(float3 _a, float3 _b, float _t) => new(
        lerpAngle(_a.x, _b.x, _t), 
        lerpAngle(_a.y, _b.y, _t), 
        lerpAngle(_a.z, _b.z, _t));

    public static float deltaAngle(float _x,float _xd)
    {
        float num = repeat(_xd - _x, 360f);
        if (num > 180.0)
            num -= 360f;
        return num;
    }
    public static float2 deltaAngle(float2 _x, float2 _xd)
    {
        return new float2(
            deltaAngle(_x.x, _xd.x),
            deltaAngle(_x.y, _xd.y)
        );
    }

    public static float4 deltaAngle(float4 _x, float4 _xd)
    {
        return new float4(
            deltaAngle(_x.x, _xd.x),
            deltaAngle(_x.y, _xd.y),
            deltaAngle(_x.z, _xd.z),
            deltaAngle(_x.w, _xd.w)
        );
    }

}
