using System;
using UnityEngine;

public static class KVector
{
    public static Vector2 kHalf2 = Vector2.one * .5f;
}

public static class Vector_Extension
{
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

}
    