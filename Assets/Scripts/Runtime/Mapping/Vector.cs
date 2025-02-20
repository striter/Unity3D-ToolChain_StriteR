using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
public enum EAxis
{
    X = 0,
    Y = 1,
    Z = 2,
}

public static partial class umath
{
    public static EAxis maxAxis(this float2 _value)
    {
        if (_value.x > _value.y)
            return EAxis.X;
        return EAxis.Y;
    }
    public static EAxis maxAxis(this float3 _value)
    {
        if (_value.x > _value.y && _value.x > _value.z)
            return EAxis.X;
        return _value.y > _value.z ? EAxis.Y : EAxis.Z;
    }


    public static float3 GetXZLookDirection(float3 _startPoint, float3 _endPoint)
    {
        float3 lookDirection = _endPoint - _startPoint;
        lookDirection.y = 0;
        lookDirection.normalize();
        return lookDirection;
    }
    
    // public static float dot(float3 _src, float3 _dst) => _src.x * _dst.x + _src.y * _dst.y + _src.z * _dst.z;
    // public static float3 cross(float3 _src, float3 _dst) => new float3(_src.y * _dst.z - _src.z * _dst.y, _src.z * _dst.x - _src.x * _dst.z, _src.x * _dst.y - _src.y * _dst.x);
    
    // public static float dot(float2 _src, float2 _dst) => _src.x * _dst.x + _src.y * _dst.y;
    public static float angle(float3 a, float3 b)
    {
        var sqMagA = a.sqrmagnitude();
        var sqMagB = b.sqrmagnitude();
        if (sqMagB == 0 || sqMagA == 0)
            return 0;
            
        var dot = math.dot(a, b);
        if (abs(1 - sqMagA) < EPSILON && abs(1 - sqMagB) < EPSILON) {
            return acos(dot);
        }
 
        float length = sqrt(sqMagA) * sqrt(sqMagB);
        return acos(dot / length);
    }
    
    public static float3 slerp(float3 from, float3 to, float t,float3 up)
    {
        float theta = angle(from, to);
        float sin_theta = sin(theta);
        var dotValue = dot(from.normalize(), to.normalize());
        if (dotValue > .999f)
            return to;
        if(dotValue < -.999f)
            return trilerp(from, up,to, t);

        float a = sin((1 - t) * theta) / sin_theta;
        float b = sin(t * theta) / sin_theta;
        return from * a + to * b;
    }
    
    public static float2 tripleProduct(float2 _a, float2 _b, float2 _c) => _b *math.dot(_a, _c)  - _a * math.dot(_c, _b);
    public static float3 tripleProduct(float3 _a, float3 _b, float3 _c) => _b *math.dot(_a, _c)  - _a * math.dot(_c, _b);

    public static float3 nlerp(float3 _from, float3 _to, float t) => normalize(math.lerp(_from,_to,t));
    public static float cross(float2 _src, float2 _dst) => _src.x * _dst.y - _src.y * _dst.x;
    public static bool isParallel(float3 a, float3 b,float _tolerence = 0.99f) => math.abs(a.x * b.x + a.y * b.y + a.z * b.z) > _tolerence; // Tolerance to consider parallel
    public static float3 calculatePerpendicular(float3 a, float3 b) => a - b * math.dot(a, b) / math.dot(b, b);
}
