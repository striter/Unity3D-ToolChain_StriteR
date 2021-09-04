using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UMath
{
    public const float SQRT2 = 1.14142135623731f;
    public const float PI = 3.141593f;
    public const float Deg2Rad = PI  / 180f;
    public const float Rad2Deg = 180f / PI;
    
    public static readonly Matrix2x2 m_Rotate90CW = GetRotateMatrix(90*Deg2Rad,true);
    public static readonly Matrix2x2 m_Rotate180CW = GetRotateMatrix(180*Deg2Rad,true);
    public static readonly Matrix2x2 m_Rotate270CW = GetRotateMatrix(270*Deg2Rad,true);
    
    public static float GetRadClockWise(Vector2 _axis,Vector2 _vector)
    {
        float sin = _vector.x * _axis.y - _axis.x * _vector.y;
        float cos = _vector.x * _axis.x + _vector.y * _axis.y;
        
        return Mathf.Atan2(sin,cos);
    }

    public static Matrix2x2 GetRotateMatrix(float _rad,bool _clockWise=false)
    {
        float sinA = Mathf.Sin(_rad);
        float cosA = Mathf.Cos(_rad);
        if (_clockWise)
            return new Matrix2x2(cosA,sinA,-sinA,cosA);
        return new Matrix2x2(cosA,-sinA,sinA,cosA);
    }
    
    public static int Pow(int _src, int _pow)
    {
        if (_pow == 0) return 1;
        if (_pow == 1) return _src;
        int dst = _src;
        for (int i = 0; i < _pow - 1; i++)
            dst *= _src;
        return dst;
    }
    public static float Frac(float _src) => _src - Mathf.Floor(_src);
    public static float Mod(float _src, float _dst) => _src - _dst * Mathf.Floor(_src/_dst);
    
    public static float BilinearLerp(float tl, float tr, float bl, float br, float u,float v)
    {
        float lerpB = Mathf.Lerp(bl, br, u);
        float lerpT = Mathf.Lerp(tl, tr, u);
        return Mathf.Lerp(lerpB, lerpT, v);
    }
    public static Vector2 BilinearLerp(Vector2 tl, Vector2 tr, Vector2 bl, Vector2 br, float u,float v)
    {
        Vector2 lerpB = Vector2.Lerp(bl, br, u);
        Vector2 lerpT = Vector2.Lerp(tl, tr, u);
        return Vector2.Lerp(lerpB, lerpT, v);
    }
    public static Vector3 BilinearLerp(Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, float u,float v)
    {
        Vector3 lerpB = Vector3.Lerp(bl, br, u);
        Vector3 lerpT = Vector3.Lerp(tl, tr, u);
        return Vector3.Lerp(lerpB, lerpT, v);
    }
}
