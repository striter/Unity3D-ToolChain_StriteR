using System;
using System.Collections.Generic;
using UnityEngine;
public static class UVector
{

#region Common Methods
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
    
    public static IEnumerable<bool> Greater(this Vector3 _vector, float _comparer)
    {
        yield return _vector.x > _comparer;
        yield return _vector.y > _comparer;
        yield return _vector.z > _comparer;
    }
#endregion   

#region Convertions
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
    public static Vector2 ToVector2(this float _value) => new Vector2(_value, _value);
    public static Vector3 ToVector3(this float _value) => new Vector3(_value, _value, _value);
    public static Vector4 ToVector4(this float _value) => new Vector4(_value, _value, _value);
    public static Vector3 ToVector3_XY(this Vector2 _value,float _preset=0) => new Vector3(_value.x, _value.y, _preset);
    public static Vector3 ToVector3_XZ(this Vector2 _value,float _preset=0) => new Vector3(_value.x,  _preset,_value.y);
    public static Vector4 ToVector4(this Vector2 _value,float _preset=0) => new Vector4(_value.x, _value.y, _preset, _preset);
    
    public static Vector2 Convert(this Vector2 _vec, Func<float, float> _conversion) => new Vector2(_conversion(_vec.x),_conversion(_vec.y));
    public static Vector3 Convert(this Vector3 _vec, Func<float, float> _conversion) => new Vector3(_conversion(_vec.x),_conversion(_vec.y),_conversion(_vec.z));
    public static Vector4 Convert(this Vector4 _vec, Func<float, float> _conversion) => new Vector4(_conversion(_vec.x),_conversion(_vec.y),_conversion(_vec.z),_conversion(_vec.w));
    public static Vector3 Convert(this Vector3 _vec, Func<int,float, float> _conversion) => new Vector3(_conversion(0,_vec.x),_conversion(1,_vec.y),_conversion(2,_vec.z));
    #endregion
    
#region Swizzling
    public static Vector2 mod(Vector2 _src,float _value) => new Vector2(UMath.Mod(_src.x,_value), UMath.Mod(_src.y, _value));
    public static Vector3 mod(Vector3 _src,float _value) => new Vector3(UMath.Mod(_src.x, _value), UMath.Mod(_src.y, _value), UMath.Mod(_src.z, _value));
    public static Vector4 mod(Vector4 _src,float _value) => new Vector4(UMath.Mod(_src.x, _value), UMath.Mod(_src.y, _value), UMath.Mod(_src.z, _value), UMath.Mod(_src.w, _value));
    public static Vector2 frac(Vector2 _src) => new Vector2(UMath.Frac(_src.x), UMath.Frac(_src.y));
    public static Vector3 frac(Vector3 _src) => new Vector3(UMath.Frac(_src.x), UMath.Frac(_src.y), UMath.Frac(_src.z));
    public static Vector4 frac(Vector4 _src) => new Vector4(UMath.Frac(_src.x), UMath.Frac(_src.y), UMath.Frac(_src.z), UMath.Frac(_src.w));
    public static float dot(Vector2 _vec, float _value) => Vector2.Dot(_vec, _value.ToVector2());
    public static float dot(Vector3 _vec, float _value) => Vector3.Dot(_vec, _value.ToVector3());
    public static float dot(Vector4 _vec, float _value) => Vector4.Dot(_vec, _value.ToVector4());
    
    public static Vector3 clamp(this Vector3 _value,RangeFloat _range)=> Vector3.Min(Vector3.Max(_value,_range.start.ToVector3()) ,_range.end.ToVector3());
    public static Vector3 clamp(this Vector3 _value,Vector3 _min,Vector3 _max)=> Vector3.Min(Vector3.Max(_value,_min) ,_max);
    
    public static float max(this Vector3 _src) => Mathf.Max(_src.x, _src.y, _src.z);
    public static float min(this Vector3 _src) => Mathf.Min(_src.x, _src.y, _src.z);
    public static float max(this Vector4 _src) => Mathf.Max(_src.x, _src.y, _src.z, _src.w);
    public static float min(this Vector4 _src) => Mathf.Min(_src.x, _src.y, _src.z, _src.w);
    
    public static Vector2 abs(this Vector2 _src) => new Vector2(Mathf.Abs(_src.x), Mathf.Abs(_src.y));
    public static Vector3 abs(this Vector3 _src)=>new Vector3(Mathf.Abs(_src.x),Mathf.Abs(_src.y),Mathf.Abs(_src.z));
    public static Vector4 abs(this Vector4 _src) => new Vector4(Mathf.Abs(_src.x), Mathf.Abs(_src.y), Mathf.Abs(_src.z), Mathf.Abs(_src.w));
    
    public static Vector2 floor(Vector2 _src) => new Vector2(Mathf.Floor( _src.x),Mathf.Floor(_src.y));
    public static Vector3 floor(Vector3 _src) => new Vector3(Mathf.Floor(_src.x), Mathf.Floor(_src.y), Mathf.Floor(_src.z));
    public static Vector4 floor(Vector4 _src) => new Vector4(Mathf.Floor(_src.x), Mathf.Floor(_src.y), Mathf.Floor(_src.z), Mathf.Floor(_src.w));
    
    public static Vector3 sqrt(this Vector3 _src) => new Vector3(Mathf.Sqrt(_src.x),Mathf.Sqrt(_src.y),Mathf.Sqrt(_src.z));
    public static Vector3 square(this Vector3 _src) => new Vector3(UMath.Square(_src.x),UMath.Square(_src.y),UMath.Square(_src.z));

    public static Vector2 mul(this Vector2 _src, Vector2 _tar) => new Vector2(_src.x * _tar.x, _src.y * _tar.y);
    public static Vector3 mul(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z);
    public static Vector4 mul(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z, _src.w * _tar.w);
    public static Vector3 div(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z);
    public static Vector3 div(this Vector2 _src, float _tar) => new Vector2(_src.x / _tar, _src.y / _tar);
    public static Vector3 div(this Vector2 _src, float _tarX,float _tarY) => new Vector2(_src.x / _tarX, _src.y / _tarY);
    public static Vector3 div(this Vector2 _src, Vector2 _tar) => new Vector2(_src.x / _tar.x, _src.y / _tar.x);
    public static Vector3 div(this Vector3 _src, float _tar) => new Vector3(_src.x / _tar, _src.y / _tar, _src.z / _tar);
    public static Vector4 div(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z, _src.w / _tar.w);
#endregion

}

public static class KVector
{
    public static Vector2 kHalf2 = Vector2.one * .5f;
}