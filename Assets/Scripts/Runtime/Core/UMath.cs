using System;
using UnityEngine;

public static class UMath
{
    public const float kSQRT2 = 1.4142135623731f;
    public const float kSQRT3 = 1.7320508075689f;
    public const float kPI = 3.141593f;
    public const float kPIM2 = kPI * 2;
    public const float kPID2 = 1.5707963267948966f;
    public const float kPID4 = 0.7853981633974483f;
    public const float kDeg2Rad = 0.017453292519943f;//PI / 180
    public const float kRad2Deg = 57.295779513082f ;//180f / PI;
    
    public static readonly Matrix2x2 kRotateCW90 = URotation.Rotate2D(90*kDeg2Rad,true);
    public static readonly Matrix2x2 kRotateCW180 = URotation.Rotate2D(180*kDeg2Rad,true);
    public static readonly Matrix2x2 kRotateCW270 = URotation.Rotate2D(270*kDeg2Rad,true);
    public static readonly Matrix2x2[] kRotate2DCW = { Matrix2x2.Identity,kRotateCW90,kRotateCW180,kRotateCW270};
    public static readonly Quaternion[] kRotate3DCW = {Quaternion.Euler(0f,0f,0f),Quaternion.Euler(0f,90f,0f),Quaternion.Euler(0f,180f,0f),Quaternion.Euler(0f,270f,0f)};
    
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
    public static float Square(float _src) => _src * _src;
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
            if (!KRuntime.kRange01.Contains(u) || !KRuntime.kRange01.Contains(v))
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


    public static Matrix4x4 add(this Matrix4x4 _src, Matrix4x4 _dst)
    {
        Matrix4x4 dst = Matrix4x4.identity;
        for(int i=0;i<4;i++)
            dst.SetRow(i,_src.GetRow(i)+_dst.GetRow(i));
        return dst;
    }
    
    public static int IntLerp(int _src, int _dst, float _interpolate)=> (int)Mathf.Lerp(_src, _dst, _interpolate);
    public static bool BoolLerp(bool _src, bool _dst, float _interpolate)
    {
        if (Math.Abs(_interpolate - 1) < float.Epsilon)
            return _dst;
        if (_interpolate == 0)
            return _src;
        return _src || _dst;
    }

    public static float Cos(float _src) => Mathf.Cos(_src);
    public static float Sin(float _src) => Mathf.Sin(_src);
    public static float CosH(float _src) => (Mathf.Exp(_src) + Mathf.Exp(_src)) / 2;
    public static float CopySign(float _a, float _b)
    {
        var signA = Mathf.Sign(_a);
        var signB = Mathf.Sign(_b);
        return Math.Abs(signA - signB) < float.Epsilon ? _a : _a * signB;
    }

    //Shortcuts
    public static float NegExp_Fast(float _x)
    {
        return 1.0f / (1.0f + _x + 0.48f * _x * _x + 0.235f * _x * _x * _x);
    }

    public static float Atan_Fast(float _x)
    {
        float z = Mathf.Abs(_x);
        float w = z > 1f ? 1f / z : z;
        float y = (kPI / 4.0f) * w - w * (w - 1) * (0.2447f + 0.0663f * w);
        return CopySign(z > 1 ? kPID2 - y : y,_x);
    }

}
