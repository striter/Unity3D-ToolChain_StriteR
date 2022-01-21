using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class URotation
{
    public static Quaternion EulerToQuaternion(Vector3 euler) => EulerToQuaternion(euler.x, euler.y, euler.z);
    public static Quaternion EulerToQuaternion(float _radin, float _angleY, float _angleZ)     //Euler Axis XYZ
    {
        float radinHX = UMath.Deg2Rad*_radin;
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
    public static Quaternion AngleAxisToQuaternion(float _radin, Vector3 _axis)
    {
        float radinH = _radin / 2;
        float sinH = Mathf.Sin(radinH);
        float cosH = Mathf.Cos(radinH);
        return new Quaternion(_axis.x * sinH, _axis.y * sinH, _axis.z * sinH, cosH);
    }
    
    public static Matrix2x2 Rotate2D(float _rad,bool _clockWise=false)
    {
        float sinA = Mathf.Sin(_rad);
        float cosA = Mathf.Cos(_rad);
        if (_clockWise)
            return new Matrix2x2(cosA,sinA,-sinA,cosA);
        return new Matrix2x2(cosA,-sinA,sinA,cosA);
    }
    
    public static Matrix3x3 AngleAxis3x3(float _radin, Vector3 _axis)
    {
        float s = Mathf.Sin(_radin);
        float c = Mathf.Cos(_radin);
            
        float t = 1 - c;
        float x = _axis.x;
        float y = _axis.y;
        float z = _axis.z;

        return new Matrix3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
            t * x * y + s * z, t * y * y + c, t * y * z - s * x,
            t * x * z - s * y, t * y * z + s * x, t * z * z + c);
    }

    public static Quaternion FromToQuaternion(Vector3 _from,Vector3 _to)
    {
        float e = Vector3.Dot(_from, _to);
        Vector3 v = Vector3.Cross(_from, _to);
        float sqrt1Pe = Mathf.Sqrt(2 * (1 + e));
        Vector3 Qv = v * (1f / sqrt1Pe);
        float Qw = sqrt1Pe / 2f;
        return new Quaternion(Qv.x,Qv.y,Qv.z,Qw);
    }

    public static Matrix3x3 FromTo3x3(Vector3 _from, Vector3 _to)
    {
        Vector3 v = Vector3.Cross(_from, _to);
        float e = Vector3.Dot(_from, _to);
        float h = 1 / (1 + e);
        return new Matrix3x3(e+h*v.x*v.x,h*v.x*v.y-v.z,h*v.x*v.z+v.y,
            h*v.x*v.y+v.z,e+h*v.y*v.y,h*v.y*v.z-v.x,
            h*v.x*v.z-v.y,h*v.y*v.z+v.x,e*h*v.z*v.z
        );
    }
}
