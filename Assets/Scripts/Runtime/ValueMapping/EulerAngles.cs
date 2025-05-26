﻿using System;
using Unity.Mathematics;
using static umath.KEulerAngle;
using static Unity.Mathematics.math;
using static kmath;
using quaternion = Unity.Mathematics.quaternion;
public enum EEulerOrder
{
    kOrderXYZ,
    kOrderXZY,
    kOrderYZX,
    kOrderYXZ,
    kOrderZXY,
    kOrderZYX,
};

public static partial class umath
{
    public static class KEulerAngle
    {
        public const EEulerOrder DEFAULT_ORDER = EEulerOrder.kOrderZXY;
        public const float SINGULARITY_CUTOFF = 0.499999f;
        public static float qAtan2(float _a, float _b) => atan2(_a, _b);
        public static float qNull(float _a, float _b) => 0f;
        public static float qAsin(float _a, float _b) => _a * asin(clamp(_b, -1.0f, 1.0f));

        public static readonly Func<float, float, float>[][] qFuncs =
        {
            new Func<float, float, float>[] { qAtan2, qAsin, qAtan2 }, //OrderXYZ
            new Func<float, float, float>[] { qAtan2, qAtan2, qAsin }, //OrderXZY
            new Func<float, float, float>[] { qAtan2, qAtan2, qAsin }, //OrderYZX,
            new Func<float, float, float>[] { qAsin, qAtan2, qAtan2 }, //OrderYXZ,
            new Func<float, float, float>[] { qAsin, qAtan2, qAtan2 }, //OrderZXY,
            new Func<float, float, float>[] { qAtan2, qAsin, qAtan2 } //OrderZYX,
        };
    }

    private static Func<float, float, float>[] kQuaternionHelper = {qNull, qNull, qNull};
    public static float3 toEulerAngles(this quaternion _q,EEulerOrder order = DEFAULT_ORDER)
    {
        var q = _q.value;
        var yz = q.y * q.z;
        var xz = q.x * q.z;
        var xw = q.x * q.w;
        var xy = q.x * q.y;
        var zw = q.z * q.w;
        var xx = q.x * q.x;
        var yy = q.y * q.y;
        var zz = q.z * q.z;
        var ww = q.w * q.w;
        var yw = q.y * q.w;
        var singularity_test = yz - xw;
        float X1, X2, Y1, Y2, Z1, Z2;
        var eulerFunctions = qFuncs[(int)order];
        kQuaternionHelper[0] = eulerFunctions[0];
        kQuaternionHelper[1] = eulerFunctions[1];
        kQuaternionHelper[2] = eulerFunctions[2];
        switch (order)
        {
            default: throw new ArgumentOutOfRangeException(nameof(order), order, null);
            case EEulerOrder.kOrderZYX:
                singularity_test = xz + yw;
                Z1 = 2.0f * (-xy + zw);
                Z2 = xx - zz - yy + ww;
                Y1 = 1.0f;
                Y2 = 2.0f * singularity_test;
                if (abs(singularity_test) < SINGULARITY_CUTOFF)
                {
                    X1 = 2.0f * (-yz + xw);
                    X2 = zz - yy - xx + ww;
                }
                else //x == xzx z == 0
                {
                    var a = xz + yw;
                    var b = -xy + zw;
                    var c = xz - yw;
                    var e = xy + zw;

                    X1 = a * e + b * c;
                    X2 = b * e - a * c;
                    kQuaternionHelper[2] = qNull;
                }
                break;
            case EEulerOrder.kOrderXZY:
                singularity_test = xy + zw;
                X1 = 2.0f * (-yz + xw);
                X2 = yy - zz - xx + ww;
                Z1 = 1.0f;
                Z2 = 2.0f * singularity_test;

                if (abs(singularity_test) < SINGULARITY_CUTOFF)
                {
                    Y1 = 2.0f * (-xz + yw);
                    Y2 = xx - zz - yy + ww;
                }
                else //y == yxy x == 0
                {
                    var a = xy + zw;
                    var b = -yz + xw;
                    var c = xy - zw;
                    var e = yz + xw;

                    Y1 = a * e + b * c;
                    Y2 = b * e - a * c;
                    kQuaternionHelper[0] = qNull;
                }
                break;

            case EEulerOrder.kOrderYZX:
                singularity_test = xy - zw;
                Y1 = 2.0f * (xz + yw);
                Y2 = xx - zz - yy + ww;
                Z1 = -1.0f;
                Z2 = 2.0f * singularity_test;

                if (abs(singularity_test) < SINGULARITY_CUTOFF)
                {
                    X1 = 2.0f * (yz + xw);
                    X2 = yy - xx - zz + ww;
                }
                else //x == xyx y == 0
                {
                    var a = xy - zw;
                    var b = xz + yw;
                    var c = xy + zw;
                    var e = -xz + yw;

                    X1 = a * e + b * c;
                    X2 = b * e - a * c;
                    kQuaternionHelper[1] = qNull;
                }
                break;
            case EEulerOrder.kOrderZXY:
            {
                singularity_test = yz - xw;
                Z1 = 2.0f * (xy + zw);
                Z2 = yy - zz - xx + ww;
                X1 = -1.0f;
                X2 = 2.0f * singularity_test;

                if (abs(singularity_test) < SINGULARITY_CUTOFF)
                {
                    Y1 = 2.0f * (xz + yw);
                    Y2 = zz - xx - yy + ww;
                }
                else //x == yzy z == 0
                {
                    var a = xy + zw;
                    var b = -yz + xw;
                    var c = xy - zw;
                    var e = yz + xw;

                    Y1 = a * e + b * c;
                    Y2 = b * e - a * c;
                    kQuaternionHelper[2] = qNull;
                }
            }
            break;
            case EEulerOrder.kOrderYXZ:
                singularity_test = yz + xw;
                Y1 = 2.0f * (-xz + yw);
                Y2 = zz - yy - xx + ww;
                X1 = 1.0f;
                X2 = 2.0f * singularity_test;

                if (abs(singularity_test) < SINGULARITY_CUTOFF)
                {
                    Z1 = 2.0f * (-xy + zw);
                    Z2 = yy - zz - xx + ww;
                }
                else //x == zyz y == 0
                {
                    var a = yz + xw;
                    var b = -xz + yw;
                    var c = yz - xw;
                    var e = xz + yw;

                    Z1 = a * e + b * c;
                    Z2 = b * e - a * c;
                    kQuaternionHelper[1] = qNull;
                }
                break;
            case EEulerOrder.kOrderXYZ:
                singularity_test = xz - yw;
                X1 = 2.0f * (yz + xw);
                X2 = zz - yy - xx + ww;
                Y1 = -1.0f;
                Y2 = 2.0f * singularity_test;

                if (abs(singularity_test) < SINGULARITY_CUTOFF)
                {
                    Z1 = 2.0f * (xy + zw);
                    Z2 = xx - zz - yy + ww;
                }
                else //x == zxz x == 0
                {
                    var a = xz - yw;
                    var b = yz + xw;
                    var c = xz + yw;
                    var e = -yz + xw;

                    Z1 = a * e + b * c;
                    Z2 = b * e - a * c;
                    kQuaternionHelper[0] = qNull;
                }
                break;
        }
        return new float3(kQuaternionHelper[0](X1, X2), kQuaternionHelper[1](Y1, Y2), kQuaternionHelper[2](Z1, Z2)) * kRad2Deg;
    }
    
    
    public static quaternion EulerToQuaternion(float3 _euler, EEulerOrder order = DEFAULT_ORDER)
    {
        
        _euler *= kDeg2Rad;
        #if _0      //XYZ
            var radinHX = _euler.x / 2f;
            var radinHY = _euler.y / 2f;
            var radinHZ = _euler.z / 2f;
            var sinHX = sin(radinHX);
            var cosHX = cos(radinHX);
            var sinHY = sin(radinHY);
            var cosHY = cos(radinHY);
            var sinHZ = sin(radinHZ);
            var cosHZ = cos(radinHZ);
            var qX = cosHX * sinHY * sinHZ + sinHX * cosHY * cosHZ;
            var qY = cosHX * sinHY * cosHZ + sinHX * cosHY * sinHZ;
            var qZ = cosHX * cosHY * sinHZ - sinHX * sinHY * cosHZ;
            var qW = cosHX * cosHY * cosHZ - sinHX * sinHY * sinHZ;
            return new Quaternion(qX, qY, qZ, qW);
        #endif
        
        
        var cX = cos(_euler.x / 2.0f);
        var sX = sin(_euler.x / 2.0f);
        
        var cY = cos(_euler.y / 2.0f);
        var sY = sin(_euler.y / 2.0f);
        
        var cZ = cos(_euler.z / 2.0f);
        var sZ = sin(_euler.z / 2.0f);
        
        var qX = new quaternion(sX, 0.0F, 0.0F, cX);
        var qY = new quaternion(0.0F, sY, 0.0F, cY);
        var qZ = new quaternion(0.0F, 0.0F, sZ, cZ);
        
        return order switch
        {
            EEulerOrder.kOrderZYX => mul(qX, qY, qZ),
            EEulerOrder.kOrderYZX => mul(qX, qZ, qY),
            EEulerOrder.kOrderXZY => mul(qY, qZ, qX),
            EEulerOrder.kOrderZXY => mul(qY, qX, qZ),
            EEulerOrder.kOrderYXZ => mul(qZ, qX, qY),
            EEulerOrder.kOrderXYZ => mul(qZ, qY, qX),
            _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
        };
    }


    public static quaternion EulerToQuaternion(float _angleX, float _angleY, float _angleZ) => EulerToQuaternion(new float3(_angleX, _angleY, _angleZ));

}