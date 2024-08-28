using System;
using Unity.Mathematics;
using UnityEngine;
using static kmath;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
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

    public static float radBetween(float3 _from,float3 _to)      //Radin
    {
        var num =  math.sqrt( _from.sqrmagnitude() *  _to.sqrmagnitude());
        return num < 1.0000000036274937E-15f ? 0.0f
            : math.acos(math.clamp(math.dot(_from, _to) / num, -1f, 1f));
    }

    public static float closestAngle(float3 _first, float3 _second, float3 _up)
    {
        var nonCloseAngle = radBetween(_first, _second) * kRad2Deg;
        nonCloseAngle *= sign(dot(_up, math.cross(_first, _second)));
        return nonCloseAngle;
    }

    public static float angle(float3 _first, float3 _second, float3 _up) => rad(_first, _second, _up) * kRad2Deg;
    
    public static float rad(float3 _first, float3 _second, float3 _up)
    {
        var nonCloseAngle = radBetween(_first, _second) ;
        nonCloseAngle *= sign(dot(_up, math.cross(_first, _second)));
        return nonCloseAngle;
    }

    public static float getRadClockwise(float2 _axis,float2 _vector)
    {
        var sin = _vector.x * _axis.y - _axis.x * _vector.y;
        var cos = _vector.x * _axis.x + _vector.y * _axis.y;
        return atan2(sin,cos);
    }
    public static float2 closestPitchYaw(quaternion _quaternion) => closestPitchYaw(math.mul(_quaternion,kfloat3.forward));
    
    public static float3 rotateCW(this float3 _src, float3 _axis, float _angle) => math.mul(quaternion.AxisAngle( _axis,_angle) , _src).normalize();
    
    public static float2 toPitchYaw(this quaternion _rotation)
    {
        var direction = math.mul(_rotation, kfloat3.forward);
        var pitch = atan2(-direction.y, sqrt(direction.x * direction.x + direction.z * direction.z));
        var yaw = atan2(direction.x, direction.z);
        return new float2(pitch, yaw) * kRad2Deg;
    }
    
    public static float closestYaw(float3 _direction) => closestAngle(kfloat3.forward,_direction.setY(0).normalize(),kfloat3.up);
    public static float2 closestPitchYaw(float3 _direction)
    {
        var xzDirection = _direction.setY(0).normalize();
        
        var desiredPitch = closestAngle(_direction,xzDirection, math.cross(xzDirection,_direction));
        var desiredYaw = closestAngle(kfloat3.forward,xzDirection,kfloat3.up);
        return new float2(desiredPitch, desiredYaw);
    }

    public static float2 toPitchYaw(float3 _direction)
    {
        var pitch = atan2(-_direction.y, sqrt(_direction.x * _direction.x + _direction.z * _direction.z));
        var yaw = atan2(_direction.x, _direction.z);
        return new float2(pitch, yaw) * kRad2Deg;
    }
}
