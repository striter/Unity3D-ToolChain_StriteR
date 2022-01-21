using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UMath
{
    public const float SQRT2 = 1.4142135623731f;
    public const float SQRT3 = 1.7320508075689f;
    public const float PI = 3.141593f;
    public const float Deg2Rad = 0.017453292519943f;//PI / 180
    public const float Rad2Deg = 57.295779513082f ;//180f / PI;
    
    public static readonly Matrix2x2 m_RotateCW90 = URotation.Rotate2D(90*Deg2Rad,true);
    public static readonly Matrix2x2 m_RotateCW180 = URotation.Rotate2D(180*Deg2Rad,true);
    public static readonly Matrix2x2 m_RotateCW270 = URotation.Rotate2D(270*Deg2Rad,true);
    public static readonly Matrix2x2[] m_Rotate2DCW = { Matrix2x2.Identity,m_RotateCW90,m_RotateCW180,m_RotateCW270};
    public static readonly Quaternion[] m_Rotate3DCW = {Quaternion.Euler(0f,0f,0f),Quaternion.Euler(0f,90f,0f),Quaternion.Euler(0f,180f,0f),Quaternion.Euler(0f,270f,0f)};
    
    public static float GetRadClockWise(Vector2 _axis,Vector2 _vector)
    {
        float sin = _vector.x * _axis.y - _axis.x * _vector.y;
        float cos = _vector.x * _axis.x + _vector.y * _axis.y;
        
        return Mathf.Atan2(sin,cos);
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

    public static float Pow2(float _src) => _src * _src;
    public static float Pow3(float _src) => _src * _src* _src;
    public static float Pow4(float _src) => _src * _src* _src* _src;
    public static float Frac(float _src) => _src - Mathf.Floor(_src);
    public static float Mod(float _src, float _dst) => _src - _dst * Mathf.Floor(_src/_dst);

    public static float InvLerp(float _a, float _b, float _value)=> (_value - _a) / (_b - _a);
    public static Vector3 BilinearLerp(Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static Vector2 BilinearLerp(Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float BilinearLerp(float tl, float tr, float br, float bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static Vector2 InvBilinearLerp(Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl, Vector2 p)
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

    public static bool BoolLerp(bool _src, bool _dst, float _interpolate)
    {
        if (Math.Abs(_interpolate - 1) < float.Epsilon)
            return _dst;
        if (_interpolate == 0)
            return _src;
        return _src || _dst;
    }

    public static Vector3 QuadraticBezierCurve(Vector3 _src,Vector3 _dst,Vector3 _control,float _interpolation)
    {
        float value = _interpolation;
        float oneMinusValue = 1 - value;
        return Pow2(oneMinusValue) * _src + 2 * (oneMinusValue) * value * _control + Pow2(value) * _dst;
    }

    public static Vector3 CubicBezierCurve(Vector3 _src, Vector3 _dst, Vector3 _controlSrc, Vector3 _controlDst, float _interpolation)
    {
        float value = _interpolation;
        float oneMinusValue = 1 - value;
        return Pow3(oneMinusValue) * _src +  3 * Pow2(oneMinusValue) * value * _controlSrc +  3 * oneMinusValue * Pow2(value) * _controlDst + Pow3(value) * _dst;
    }
}
