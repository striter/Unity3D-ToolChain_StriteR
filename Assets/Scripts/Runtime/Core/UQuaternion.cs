using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UQuaternion
{
    public static Quaternion EulerToQuaternion(Vector3 euler) => EulerToQuaternion(euler.x, euler.y, euler.z);
    public static Quaternion EulerToQuaternion(float _angleX, float _angleY, float _angleZ)     //Euler Axis XYZ
    {
        float radinHX = UMath.Deg2Rad*_angleX;
        float radinHY = UMath.Deg2Rad*_angleY;
        float radinHZ = UMath.Deg2Rad*_angleZ;
        float sinHX = Mathf.Sin(radinHX); float cosHX = Mathf.Cos(radinHX);
        float sinHY = Mathf.Sin(radinHY); float cosHY = Mathf.Cos(radinHY);
        float sinHZ = Mathf.Sin(radinHZ); float cosHZ = Mathf.Cos(radinHZ);
        float qX = cosHX * sinHY * sinHZ + sinHX * cosHY * cosHZ;
        float qY = cosHX * sinHY * cosHZ + sinHX * cosHY * sinHZ;
        float qZ = cosHX * cosHY * sinHZ - sinHX * sinHY * cosHZ;
        float qW = cosHX * cosHY * cosHZ - sinHX * sinHY * sinHZ;
        return new Quaternion(qX, qY, qZ, qW);
    }
    public static Quaternion AngleAxisToQuaternion(float _angle, Vector3 _axis)
    {
        float radinH = UMath.Deg2Rad*_angle / 2;
        float sinH = Mathf.Sin(radinH);
        float cosH = Mathf.Cos(radinH);
        return new Quaternion(_axis.x * sinH, _axis.y * sinH, _axis.z * sinH, cosH);
    }
    public static Matrix3x3 AngleAxisToRotateMatrix(float _angle,Vector3 _axis)
    {
        float radin =  UMath.Deg2Rad*_angle;
        float s = Mathf.Sin(radin);
        float c = Mathf.Cos(radin);

        float t = 1 - c;
        float x = _axis.x;
        float y = _axis.y;
        float z = _axis.z;

        return new Matrix3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
            t * x * y + s * z, t * y * y + c, t * y * z - s * x,
            t * x * z - s * y, t * y * z + s * x, t * z * z + c);
    }
}
