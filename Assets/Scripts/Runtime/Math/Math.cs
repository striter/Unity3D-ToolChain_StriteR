using System;
using Unity.Mathematics;
using UnityEngine;
using static kmath;
using static Unity.Mathematics.math;
public static partial class umath
{
    public static bool IsPrime(ushort _value)
    {
        for (ushort i = 2; i < _value; i++)
        {
            if (_value % i == 0)
                return false;
        }
        return true;
    }
    
    public static ushort[] ComputePrimes(int _count)
    {
        ushort[] primes = new ushort[_count];
        ushort currentNum = 1;
        ushort curIndex = 0;
        while (curIndex < _count)
        {
            currentNum++;
            if (!IsPrime(currentNum))
                continue;
            primes[curIndex++] = currentNum;
        }
        return primes;
    }
    
    public static float GetRadClockWise(Vector2 _axis,Vector2 _vector)
    {
        float sin = _vector.x * _axis.y - _axis.x * _vector.y;
        float cos = _vector.x * _axis.x + _vector.y * _axis.y;
        
        return Mathf.Atan2(sin,cos);
    }

    public static int pow(int _src, int _pow)
    {
        if (_pow == 0) return 1;
        if (_pow == 1) return _src;
        int dst = _src;
        for (int i = 0; i < _pow - 1; i++)
            dst *= _src;
        return dst;
    }

    public static uint pow(uint _src, uint _pow)
    {
        if (_pow == 0) return 1;
        if (_pow == 1) return _src;
        var dst = _src;
        for (int i = 0; i < _pow - 1; i++)
            dst *= _src;
        return dst;
    }

    public static int sqr(int _src) => _src * _src;
    public static float sqr(float _src) => _src * _src;
    public static float2 sqr(float2 _src) => _src * _src;
    public static float3 sqr(float3 _src) => _src * _src;
    
    public static float pow2(float _src) => _src * _src;
    public static float pow3(float _src) => _src * _src* _src;
    public static float pow4(float _src) => _src * _src* _src* _src;
    public static float mod(float _src, float _dst) => _src - _dst * Mathf.Floor(_src/_dst);

    public static float lerp(float _a, float _b, float _value) => Mathf.Lerp(_a, _b, _value);
    
    public static float invLerp(float _a, float _b, float _value)=> (_value - _a) / (_b - _a);
    
    public static float3 bilinearLerp(float3 tl, float3 tr, float3 br, float3 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float2 bilinearLerp(float2 tl, float2 tr, float2 br, float2 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float bilinearLerp(float tl, float tr, float br, float bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float2 invBilinearLerp(float2 tl, float2 tr, float2 br, float2 bl, float2 p)
    {
        var e = tr - tl;
        var f = bl - tl;
        var g = tl - tr + br - bl;
        var h = p - tl;
        var k2 = umath.cross(g,f);
        var k1 = umath.cross(e, f);
        var k0 = umath.cross(h, e);
        if (Mathf.Abs(k2) > float.Epsilon)
        {
            float w = k1 * k1 - 4f * k0 * k2;
            if (w < 0f)
                return -Vector2.one;
            w = Mathf.Sqrt(w);
            float ik2 = .5f / k2;
            float v = (-k1 - w) * ik2;
            float u = (h.x - f.x * v) / (e.x + g.x * v);
            if (!RangeFloat.k01.Contains(u) || !RangeFloat.k01.Contains(v))
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
    
    public static int lerp(int _src, int _dst, float _interpolate)=> (int)math.lerp(_src, _dst, _interpolate);
    public static bool lerp(bool _src, bool _dst, float _interpolate)
    {
        if (Math.Abs(_interpolate - 1) < float.Epsilon)
            return _dst;
        if (_interpolate == 0)
            return _src;
        return _src || _dst;
    }

    
    public static float cosH(float _src) => (Mathf.Exp(_src) + Mathf.Exp(_src)) / 2;
    public static float copySign(float _a, float _b)
    {
        var signA = Mathf.Sign(_a);
        var signB = Mathf.Sign(_b);
        return Math.Abs(signA - signB) < float.Epsilon ? _a : _a * signB;
    }
    
    //Fast
    public static float negExp_Fast(float _x)
    {
        return 1.0f / (1.0f + _x + 0.48f * _x * _x + 0.235f * _x * _x * _x);
    }

    public static float atan_Fast(float _x)
    {
        float z = Mathf.Abs(_x);
        float w = z > 1f ? 1f / z : z;
        float y = (kPI / 4.0f) * w - w * (w - 1) * (0.2447f + 0.0663f * w);
        return copySign(z > 1 ? kPID2 - y : y,_x);
    }

    static float sin_kinda(float _x)
    {
        float x2 = sqr(_x);
        float x3 = x2*_x;
        float x5 = x3 * x2;
        _x = _x - x3 / 6.0f + x5 / 120f;
        return _x;
    }

    public static float sin_basic_approximation(float _x)
    {
        int k = (int)math.floor(_x / kPID2);
        float y = _x - k * kPID2;
        switch (( k % 4+4) % 4)
        {
            default: throw new ArgumentNullException();
            case 0: return sin_kinda(y);
            case 1: return sin_kinda(kPID2 - y);
            case 2: return -sin_kinda(y);
            case 3: return -sin_kinda(kPID2 - y);
        }
    }
}
