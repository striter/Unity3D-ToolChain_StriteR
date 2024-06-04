using Unity.Mathematics;
using static kmath;
using static Unity.Mathematics.math;
using float2x2 = Unity.Mathematics.float2x2;
using quaternion = Unity.Mathematics.quaternion;

public static class KRotation
{
    public static readonly float2x2 kRotateCW90 = umath.Rotate2D(90 * kDeg2Rad, true);
    public static readonly float2x2 kRotateCW180 = umath.Rotate2D(180 * kDeg2Rad, true);
    public static readonly float2x2 kRotateCW270 = umath.Rotate2D(270 * kDeg2Rad, true);
    public static readonly float2x2[] kRotate2DCW = { float2x2.identity , kRotateCW90, kRotateCW180, kRotateCW270};
    public static readonly float2x2[] kRotate2DCCW = { float2x2.identity, kRotateCW270, kRotateCW180, kRotateCW90 };

    public static readonly quaternion[] kRotate3DCW =
    {
        umath.EulerToQuaternion(0f, 0f, 0f),
        umath.EulerToQuaternion(0f, 90f, 0f),
        umath.EulerToQuaternion(0f, 180f, 0f),
        umath.EulerToQuaternion(0f, 270f, 0f)
    };
}


public static partial class umath
{
    public static quaternion mul(this quaternion _q, quaternion _q2, quaternion _q3) => math.mul(_q, math.mul(_q2, _q3));
    public static float3 mul(this quaternion _q, float3 _direction) => math.mul(_q, _direction);
    public static quaternion AngleAxisToQuaternion(float _radin, float3 _axis)
    {
        var radinH = _radin / 2;
        var sinH = sin(radinH);
        var cosH = cos(radinH);
        return new quaternion(_axis.x * sinH, _axis.y * sinH, _axis.z * sinH, cosH);
    }

    public static float2x2 Rotate2D(float _rad, bool _clockWise = false)
    {
        var sinA = sin(_rad);
        var cosA = cos(_rad);
        if(_clockWise)
            return new float2x2(cosA, sinA, -sinA, cosA);
        return new float2x2(cosA, -sinA, sinA, cosA);
    }

    public static Matrix3x3 AngleAxis3x3(float _radin, float3 _axis)
    {
        var s = sin(_radin);
        var c = cos(_radin);

        var t = 1 - c;
        var x = _axis.x;
        var y = _axis.y;
        var z = _axis.z;

        return new Matrix3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
            t * x * y + s * z, t * y * y + c, t * y * z - s * x,
            t * x * z - s * y, t * y * z + s * x, t * z * z + c);
    }

    public static float3x3 QuaternionToMatrix3x3(quaternion _quaternion)
    {
        var x = _quaternion.value.x;
        var y = _quaternion.value.y;
        var z = _quaternion.value.z;
        var w = _quaternion.value.w;
        return new float3x3(
            1f - 2 * y * y - 2 * x * x,     2f * x * y - 2 * w * z,         2 * x * z + 2 * w * y,
            2 * x * y + 2 * w * z,         1f - 2 * x * x - 2 * z * z,     2 * y * z - 2 * w * x,
            2 * x * z - 2 * w * y,         2 * y * z + 2 * w * x,         1f - 2 * x * x - 2 * y * y
        );
    }

    public static float GetAngle(quaternion _q1, quaternion _q2)
    {
        float dt = math.dot(_q1, _q2);
        if (dt < 0.0f)
            dt = -dt;
        return acos(dt);
    }

    public static quaternion Slerp(quaternion _q1, quaternion _q2,float _t)
    {
        float dt = math.dot(_q1.value, _q2.value);
        if (dt < 0.0f)
            dt = -dt;

        if (dt < 0.9995f)
        {
            float angle = acos(dt);
            float s = rsqrt(1.0f - dt * dt);    // 1.0f / sin(angle)
            float w1 = sin(angle* (1.0f - _t))  * s;
            float w2 = sin(angle * _t)  * s;
            return quaternion(_q1.value * w1 + _q2.value * w2);
        }

        return normalize(quaternion(_q1.value * (1.0f - _t) + _q2.value * _t).value);
    }
    
    public static quaternion FromToQuaternion(float3 _from, float3 _to)
    {
        var e = dot(_from, _to);
        var v = math.cross(_from, _to);
        var sqrt1Pe = sqrt(2 * (1 + e));
        var Qv = v * (1f / sqrt1Pe);
        var Qw = sqrt1Pe / 2f;
        return new quaternion(Qv.x, Qv.y, Qv.z, Qw);
    }

    public static Matrix3x3 FromTo3x3(float3 _from, float3 _to)
    {
        var v = math.cross(_from, _to);
        var e = math.dot(_from, _to);
        var h = 1 / (1 + e);
        return new Matrix3x3(e + h * v.x * v.x, h * v.x * v.y - v.z, h * v.x * v.z + v.y,
            h * v.x * v.y + v.z, e + h * v.y * v.y, h * v.y * v.z - v.x,
            h * v.x * v.z - v.y, h * v.y * v.z + v.x, e * h * v.z * v.z
        );
    }

}