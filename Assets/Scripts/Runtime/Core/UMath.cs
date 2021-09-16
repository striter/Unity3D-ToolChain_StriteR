using System;
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


    public static dynamic BilinearLerp(dynamic tl, dynamic tr, dynamic br, dynamic bl,dynamic _uv)
    {
        float u = _uv.x;
        float v = _uv.y;
        return tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    }
    public static dynamic InvBilinearLerp(dynamic tl, dynamic tr, dynamic br, dynamic bl, dynamic p)
    {
        var e = tr - tl;
        var f = bl - tl;
        var g = tl - tr + br - bl;
        var h = p - tl;
        var k2 = UVector.Cross2(g,f);
        var k1 = UVector.Cross2(e, f);
        var k0 = UVector.Cross2(h, e);
        if (Mathf.Abs(k2) > float.Epsilon)
        {
            float w = k1 * k1 - 4f * k0 * k2;
            if (w < 0f)
                return -Vector2.one;
            w = Mathf.Sqrt(w);
            float ik2 = .5f / k2;
            float v = (-k1 - w) * ik2;
            float u = (h.x - f.x * v) / (e.x + g.x * v);
            if (!UCommon.m_Range01.InRange(u) || !UCommon.m_Range01.InRange(v))
            {
                v = (-k1 + w) * ik2;
                u = (h.x - f.x * v) / (e.x + g.x * v);
            }
            return new Vector2(u,v);
        }
        else
        {
            float u=(h.x*k1+f.x*k0)/(e.x*k1-g.x*k0);
            float v = -k0 / k1;
            return new Vector2(u,v);
        }
    }
}
