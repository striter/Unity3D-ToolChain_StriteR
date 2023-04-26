using Unity.Mathematics;
using UnityEngine;
using static kmath;
using static Unity.Mathematics.math;

public static class KRotation
{
    public static readonly Matrix2x2 kRotateCW90 = URotation.Rotate2D(90 * kDeg2Rad, true);
    public static readonly Matrix2x2 kRotateCW180 = URotation.Rotate2D(180 * kDeg2Rad, true);
    public static readonly Matrix2x2 kRotateCW270 = URotation.Rotate2D(270 * kDeg2Rad, true);
    public static readonly Matrix2x2[] kRotate2DCW = {Matrix2x2.Identity, kRotateCW90, kRotateCW180, kRotateCW270};

    public static readonly Quaternion[] kRotate3DCW =
    {
        URotation.EulerToQuaternion(0f, 0f, 0f),
        URotation.EulerToQuaternion(0f, 90f, 0f),
        URotation.EulerToQuaternion(0f, 180f, 0f),
        URotation.EulerToQuaternion(0f, 270f, 0f)
    };
}


public static class URotation
{
    public static Quaternion EulerToQuaternion(Vector3 euler)
    {
        return EulerToQuaternion(euler.x, euler.y, euler.z);
    }

    public static Quaternion EulerToQuaternion(float _angleX, float _angleY, float _angleZ) //Euler Axis XYZ
    {
        var radinHX = kDeg2Rad * _angleX / 2f;
        var radinHY = kDeg2Rad * _angleY / 2f;
        var radinHZ = kDeg2Rad * _angleZ / 2f;
        var sinHX = Mathf.Sin(radinHX);
        var cosHX = Mathf.Cos(radinHX);
        var sinHY = Mathf.Sin(radinHY);
        var cosHY = Mathf.Cos(radinHY);
        var sinHZ = Mathf.Sin(radinHZ);
        var cosHZ = Mathf.Cos(radinHZ);
        var qX = cosHX * sinHY * sinHZ + sinHX * cosHY * cosHZ;
        var qY = cosHX * sinHY * cosHZ + sinHX * cosHY * sinHZ;
        var qZ = cosHX * cosHY * sinHZ - sinHX * sinHY * cosHZ;
        var qW = cosHX * cosHY * cosHZ - sinHX * sinHY * sinHZ;
        return new Quaternion(qX, qY, qZ, qW);
    }

    public static Quaternion AngleAxisToQuaternion(float _radin, Vector3 _axis)
    {
        var radinH = _radin / 2;
        var sinH = Mathf.Sin(radinH);
        var cosH = Mathf.Cos(radinH);
        return new Quaternion(_axis.x * sinH, _axis.y * sinH, _axis.z * sinH, cosH);
    }

    public static Matrix2x2 Rotate2D(float _rad, bool _clockWise = false)
    {
        var sinA = Mathf.Sin(_rad);
        var cosA = Mathf.Cos(_rad);
        if (_clockWise)
            return new Matrix2x2(cosA, sinA, -sinA, cosA);
        return new Matrix2x2(cosA, -sinA, sinA, cosA);
    }

    public static Matrix3x3 AngleAxis3x3(float _radin, Vector3 _axis)
    {
        var s = Mathf.Sin(_radin);
        var c = Mathf.Cos(_radin);

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
        float dt = dot(_q1, _q2);
        if (dt < 0.0f)
            dt = -dt;
        return acos(dt);
    }

    public static quaternion Slerp(quaternion _q1, quaternion _q2,float _t)
    {
        float dt = dot(_q1, _q2);
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

        return normalize(quaternion(_q1.value * (1.0f - _t) + _q2.value * _t));
    }
    
    public static Quaternion FromToQuaternion(Vector3 _from, Vector3 _to)
    {
        var e = Vector3.Dot(_from, _to);
        var v = Vector3.Cross(_from, _to);
        var sqrt1Pe = Mathf.Sqrt(2 * (1 + e));
        var Qv = v * (1f / sqrt1Pe);
        var Qw = sqrt1Pe / 2f;
        return new Quaternion(Qv.x, Qv.y, Qv.z, Qw);
    }

    public static Matrix3x3 FromTo3x3(Vector3 _from, Vector3 _to)
    {
        var v = Vector3.Cross(_from, _to);
        var e = Vector3.Dot(_from, _to);
        var h = 1 / (1 + e);
        return new Matrix3x3(e + h * v.x * v.x, h * v.x * v.y - v.z, h * v.x * v.z + v.y,
            h * v.x * v.y + v.z, e + h * v.y * v.y, h * v.y * v.z - v.x,
            h * v.x * v.z - v.y, h * v.y * v.z + v.x, e * h * v.z * v.z
        );
    }
}