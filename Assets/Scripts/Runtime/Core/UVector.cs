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
    
    public static float Dot2(Vector2 _src, Vector2 _dst) => _src.x * _dst.x + _src.y * _dst.y;
    public static float Cross2(Vector2 _src, Vector2 _dst) => _src.x * _dst.y - _src.y * _dst.x;
    
    public static float Max(this Vector3 _src) => Mathf.Max(Mathf.Max(_src.x, _src.y), _src.z);
    public static float Min(this Vector3 _src) => Mathf.Min(Mathf.Min(_src.x, _src.y), _src.z);
    public static float Max(this Vector4 _src) => Mathf.Max(Mathf.Max(Mathf.Max(_src.x, _src.y), _src.z), _src.w);
    public static float Min(this Vector4 _src) => Mathf.Min(Mathf.Min(Mathf.Min(_src.x, _src.y), _src.z), _src.w);
    
    public static Vector3 abs(this Vector3 _src)=>new Vector3(Mathf.Abs(_src.x),Mathf.Abs(_src.y),Mathf.Abs(_src.z));
    public static Vector3 sqrt(this Vector3 _src) => new Vector3(Mathf.Sqrt(_src.x),Mathf.Sqrt(_src.y),Mathf.Sqrt(_src.z));
    public static Vector2 mul(this Vector2 _src, Vector2 _tar) => new Vector2(_src.x * _tar.x, _src.y * _tar.y);
    public static Vector3 mul(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z);
    public static Vector4 mul(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z, _src.w * _tar.w);
    public static Vector3 div(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z);
    public static Vector3 div(this Vector2 _src, float _tar) => new Vector2(_src.x / _tar, _src.y / _tar);
    public static Vector3 div(this Vector2 _src, float _tarX,float _tarY) => new Vector2(_src.x / _tarX, _src.y / _tarY);
    public static Vector3 div(this Vector2 _src, Vector2 _tar) => new Vector2(_src.x / _tar.x, _src.y / _tar.x);
    public static Vector3 div(this Vector3 _src, float _tar) => new Vector3(_src.x / _tar, _src.y / _tar, _src.z / _tar);
    public static Vector4 div(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z, _src.w / _tar.w);
    public static Vector2 SetX(this Vector2 _vector, float _x) => new Vector2(_x, _vector.y);
    public static Vector2 SetY(this Vector2 _vector, float _y) => new Vector2(_vector.x, _y);
    public static Vector3 SetX(this Vector3 _vector, float _x) => new Vector3(_x, _vector.y, _vector.z);
    public static Vector3 SetY(this Vector3 _vector, float _y) => new Vector3(_vector.x, _y, _vector.z);
    public static Vector3 SetZ(this Vector3 _vector, float _z) => new Vector3(_vector.x, _vector.y, _z);
    public static Vector4 SetX(this Vector4 _vector, float _x) => new Vector4(_x, _vector.y, _vector.z, _vector.w);
    public static Vector4 SetY(this Vector4 _vector, float _y) => new Vector4(_vector.x, _y, _vector.z, _vector.w);
    public static Vector4 SetZ(this Vector4 _vector, float _z) => new Vector4(_vector.x, _vector.y, _z, _vector.w);
    public static Vector4 SetW(this Vector4 _vector, float _w) => new Vector4(_vector.x, _vector.y, _vector.z, _w);
    public static Vector2 XY(this Vector3 _vector) => new Vector2(_vector.x, _vector.y);
    public static Vector2 XZ(this Vector3 _vector) => new Vector2(_vector.x, _vector.z);
    public static Vector4 ToVector4(this Vector3 _vector, float _fill = 0) => new Vector4(_vector.x, _vector.y, _vector.z, _fill);
    public static Vector3 XYZ(this Vector4 _vector) => new Vector3(_vector.x, _vector.y, _vector.z);

    public static IEnumerable<bool> Greater(this Vector3 _vector, float _comparer)
    {
        yield return _vector.x > _comparer;
        yield return _vector.y > _comparer;
        yield return _vector.z > _comparer;
    }

    public static Vector3 Clamp(this Vector3 _value,Vector3 _min,Vector3 _max)
    {
        return Vector3.Min(Vector3.Max(_value,_min) ,_max);
    }
    
}
