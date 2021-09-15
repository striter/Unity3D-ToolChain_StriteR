using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class UVector
{

    public static float GetAngle(Vector3 first, Vector3 second, Vector3 up)
    {
        float angle = Vector3.Angle(first, second);
        angle *= Mathf.Sign(Vector3.Dot(up, Vector3.Cross(first, second)));
        return angle;
    }
    public static float GetAngleY(Vector3 first, Vector3 second, Vector3 up)
    {
        Vector3 newFirst = new Vector3(first.x, 0, first.z);
        Vector3 newSecond = new Vector3(second.x, 0, second.z);
        return GetAngle(newFirst, newSecond, up);
    }
    public static float GetXZDistance(Vector3 start, Vector3 end) => new Vector2(start.x - end.x, start.z - end.z).magnitude;
    public static float GetXZSqrDistance(Vector3 start, Vector3 end) => new Vector2(start.x - end.x, start.z - end.z).sqrMagnitude;
    public static Vector3 GetXZLookDirection(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 lookDirection = endPoint - startPoint;
        lookDirection.y = 0;
        lookDirection.Normalize();
        return lookDirection;
    }
    public static Vector3 RotateDirectionClockwise(this Vector3 _src, Vector3 axis, float angle) => (Quaternion.AngleAxis(angle, axis) * _src).normalized;
    
    public static float SqrMagnitude(Vector3 _src) => _src.x * _src.x + _src.y * _src.y + _src.z * _src.z;
    public static float Dot(Vector3 _src, Vector3 _dst) => _src.x * _dst.x + _src.y * _dst.y + _src.z * _dst.z;
    public static Vector3 Project(Vector3 _src, Vector3 _dst) => (Dot(_src, _dst) / SqrMagnitude(_dst)) * _dst;
    
    public static Vector3 Cross(Vector3 _src, Vector3 _dst) => new Vector3(_src.y * _dst.z - _src.z * _dst.y, _src.z * _dst.x - _src.x * _dst.z, _src.x * _dst.y - _src.y * _dst.x);
    public static float Cross2(dynamic _src, dynamic _dst) => _src.x * _dst.y - _src.y * _dst.x;
    
    public static float Max(this Vector3 _src) => Mathf.Max(Mathf.Max(_src.x, _src.y), _src.z);
    public static float Min(this Vector3 _src) => Mathf.Min(Mathf.Min(_src.x, _src.y), _src.z);
    public static float Max(this Vector4 _src) => Mathf.Max(Mathf.Max(Mathf.Max(_src.x, _src.y), _src.z), _src.w);
    public static float Min(this Vector4 _src) => Mathf.Min(Mathf.Min(Mathf.Min(_src.x, _src.y), _src.z), _src.w);
    public static Vector3 Multiply(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z);
    public static Vector4 Multiply(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z, _src.w * _tar.w);
    public static Vector3 Divide(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z);
    public static Vector4 Divide(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z, _src.w / _tar.w);
    public static Vector2 SetX(this Vector2 _vector, float _x) => new Vector2(_x, _vector.y);
    public static Vector2 SetY(this Vector2 _vector, float _y) => new Vector2(_vector.x, _y);
    public static Vector3 SetX(this Vector3 _vector, float _x) => new Vector3(_x, _vector.y, _vector.z);
    public static Vector3 SetY(this Vector3 _vector, float _y) => new Vector3(_vector.x, _y, _vector.z);
    public static Vector3 SetZ(this Vector3 _vector, float _z) => new Vector3(_vector.x, _vector.y, _z);
    public static Vector4 SetX(this Vector4 _vector, float _x) => new Vector4(_x, _vector.y, _vector.z, _vector.w);
    public static Vector4 SetY(this Vector4 _vector, float _y) => new Vector4(_vector.x, _y, _vector.z, _vector.w);
    public static Vector4 SetZ(this Vector4 _vector, float _z) => new Vector4(_vector.x, _vector.y, _z, _vector.w);
    public static Vector4 SetW(this Vector4 _vector, float _w) => new Vector4(_vector.x, _vector.y, _vector.z, _w);
    public static Vector2 ToVector2(this Vector3 _vector) => new Vector2(_vector.x, _vector.y);
    public static Vector4 ToVector4(this Vector3 _vector, float _fill = 0) => new Vector4(_vector.x, _vector.y, _vector.z, _fill);
    public static Vector3 ToVector3(this Vector4 _vector) => new Vector3(_vector.x, _vector.y, _vector.z);
}
