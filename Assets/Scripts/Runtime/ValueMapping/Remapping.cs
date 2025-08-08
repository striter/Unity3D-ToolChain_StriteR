using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

using static kmath;
public partial class umath
{
    public static EAxis maxAxis(this float2 _value) => _value.x > _value.y ? EAxis.X : EAxis.Y;

    public static EAxis maxAxis(this float3 _value)
    {
        if (_value.x > _value.y && _value.x > _value.z)
            return EAxis.X;
        return _value.y > _value.z ? EAxis.Y : EAxis.Z;
    }
    
    public static float max(this float2 _value) => math.max(_value.x, _value.y);
    public static float max(this float3 _value) => math.max(_value.x, math.max(_value.y, _value.z));
    public static float max(this float4 _value) => math.max(_value.x, math.max(_value.y, math.max(_value.z, _value.w)));
    
    public static int max(int a, int b, int c) => math.max(a, math.max(b, c));
    
    public static int min(int a, int b, int c) => math.min(a, math.min(b, c));
    
    public static float min(this float2 _value) => math.min(_value.x, _value.y);
    public static float min(this float3 _value) => math.min(_value.x, math.min(_value.y, _value.z));
    public static float min(this float4 _value) => math.min(_value.x, math.min(_value.y, math.min(_value.z, _value.w)));
    public static float bilinearLerp(float tl, float tr, float br, float bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float2 bilinearLerp(float2 tl, float2 tr, float2 br, float2 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float3 bilinearLerp(float3 tl, float3 tr, float3 br, float3 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float2 bilinearLerp(float2 tl, float2 tr, float2 br, float2 bl,float2 p) => bilinearLerp(tl, tr, br, bl, p.x, p.y);
    public static float3 bilinearLerp(float3 tl, float3 tr, float3 br, float3 bl,float2 p) => bilinearLerp(tl, tr, br, bl, p.x, p.y);
    public static float3 trilerp(float3 _a, float3 _b, float3 _c, float _value)
    {
        if (_value < .5f)
            return math.lerp(_a, _b, _value * 2);
        return math.lerp(_b, _c, _value * 2 - 1f);
    }
    public static int signNonZero(float _a) => _a < 0 ? -1 : 1;

    public static float flipSign(float a, int s)
    {
        var newSign = signNonZero(signNonZero(a) ^ s);
        return abs(a) * newSign;
    }
    
    public static float2 invBilinearLerp(float2 tl, float2 tr, float2 br, float2 bl, float2 p)
    {
        var e = tr - tl;
        var f = bl - tl;
        var g = tl - tr + br - bl;
        var h = p - tl;
        var k2 = cross(g,f);
        var k1 = cross(e, f);
        var k0 = cross(h, e);
        if (abs(k2) > float.Epsilon)
        {
            var w = k1 * k1 - 4f * k0 * k2;
            if (w < 0f)
                return -kfloat2.one;
            w = math.sqrt(w);
            var ik2 = .5f / k2;
            var v = (-k1 - w) * ik2;
            var u = (h.x - f.x * v) / (e.x + g.x * v);
            if (RangeFloat.k01.Contains(u) && RangeFloat.k01.Contains(v)) 
                return new float2(u, v);
            v = (-k1 + w) * ik2;
            u = (h.x - f.x * v) / (e.x + g.x * v);
            return new float2(u,v);
        }
        else
        {
            var u=(h.x*k1+f.x*k0)/(e.x*k1-g.x*k0);
            var v = -k0 / k1;
            return new float2(u,v);
        }
    }

    public static float smoothLerp(float from,float to,float t)
    {
        t = -2.0f * t * t * t + 3.0f * t * t;
        return to * t + from * (1.0f - t);
    }
    
    public static int lerp(int _src, int _dst, float _interpolate)=> (int)math.lerp(_src, _dst, _interpolate);
    public static bool lerp(bool _src, bool _dst, float _interpolate)
    {
        if (math.abs(_interpolate - 1) < float.Epsilon)
            return _dst;
        if (_interpolate == 0)
            return _src;
        return _src || _dst;
    }

    public static int repeat(int _t,int _length) => math.clamp(_t - _t / _length * _length, 0, _length);
    public static float repeat(float _t,float _length) => clamp(_t - floor(_t / _length) * _length, 0.0f, _length);
    public static float2 repeat(float2 _t,float2 _length) => clamp(_t - floor(_t / _length) * _length, 0.0f, _length);
    public static float3 repeat(float3 _t,float3 _length) => clamp(_t - floor(_t / _length) * _length, 0.0f, _length);
    public static float4 repeat(float4 _t,float4 _length) => clamp(_t - floor(_t / _length) * _length, 0.0f, _length);
    public static bool IsPrime(ushort _value)
    {
        for (ushort i = 2; i < _value; i++)
        {
            if (_value % i == 0)
                return false;
        }
        return true;
    }

    public static bool IsOdd(int _value) => _value % 2 == 1;
    public static bool IsEven(int _value) => _value % 2 == 0;
    
    public static int Factorial(int n)
    {
        var result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
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
    

    public static int pow(int _src, int _pow)
    {
        switch (_pow)
        {
            case 0:
                return 1;
            case 1:
                return _src;
            default:
            {
                var dst = _src;
                for (var i = 0; i < _pow - 1; i++)
                    dst *= _src;
                return dst;
            }
        }
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
    public static double sqr(double _src) => _src * _src;
    public static float2 sqr(float2 _src) => _src * _src;
    public static float3 sqr(float3 _src) => _src * _src; 
    public static float4 sqr(float4 _src) => _src * _src;
    public static float pow2(float _src) => _src * _src; 
    public static float2 pow2(float2 _src) => _src * _src; 
    public static float3 pow2(float3 _src) => _src * _src; 
    public static float4 pow2(float4 _src) => _src * _src;
    
    public static float dot2(float _src) => _src * _src;    
    public static float dot2(float2 _src) => _src.x * _src.x + _src.y * _src.y;
    public static float dot2(float3 _src) => _src.x * _src.x + _src.y * _src.y + _src.z * _src.z;
    public static float dot2(float4 _src) => _src.x * _src.x + _src.y * _src.y + _src.z * _src.z + _src.w * _src.w;

    public static float pow3(float _src) => _src * _src* _src;
    public static float2 pow3(float2 _src) => _src * _src* _src; 
    public static float3 pow3(float3 _src) => _src * _src* _src;
    public static float4 pow3(float4 _src) => _src * _src* _src;
    
    public static float pow4(float _src) => _src * _src* _src* _src;
    public static float mod(float _src, float _dst) => _src - _dst * floor(_src/_dst);
    public static float lerp(float _a, float _b, float _value) => _a + (_b - _a) * _value;
    public static float invLerp(float _a, float _b, float _value)=> (_value - _a) / (_b - _a);
    public static float invLerp(this RangeFloat _range, float _value)=> (_value - _range.start) / (_range.length);
    public static float2 invLerp(float2 _a, float2 _b, float2 _value)=> float2(invLerp(_a.x,_b.x, _value.x),invLerp(_a.y,_b.y, _value.y));
    public static float3 invLerp(float3 _a, float3 _b, float3 _value)=> float3(invLerp(_a.x,_b.x, _value.x),invLerp(_a.y,_b.y, _value.y),invLerp(_a.z,_b.z, _value.z));
    
    public static float2 sincos(float _rad)
    {
        math.sincos(_rad, out var s, out var c);
        return new float2(s, c);
    }
    //&https://iquilezles.org/articles/functions/
    public static float almostIdentity(float _x)
    {
        return _x * _x * (2.0f - _x);
    }

    public static float almostIdentity(float _x, float _n)
    {
        return sqrt(_x * _x + _n * _n);
    }
    
    public static float almostIdentity(float _x,float _m,float _n)
    {
        if (_x > _m) return _x;
        var a = 2 * _n - _m;
        var b = 2 * _m - 3 * _n;
        var t = _x / _m;
        return (a * t + b) * t * t + _n;
    }

    public static float smoothStepIntegral(float _x, float _t)
    {
        if (_x > _t) return _x - _t / 2.0f;
        return _x * _x * _x * (1.0f - _x * .5f / _t) / _t / _t;
    }

    public static float expImpulse(float _x,float _k)
    {
        var h = _k * _x;
        return h * exp(1.0f - h);
    }
    
    public static float quadraticImpulse(float _x,float _k)
    {
        return 2.0f * sqrt(_k) * _x / (1.0f + _k * _x * _x);
    }

    public static float polynomialImpulse(float _x, float _k, float _n)
    {
        return (_n/(_n-1f))*math.pow((_n-1f)*_k,1.0f/_n)*_x/(1.0f+_k*math.pow(_x,_n));
    }
    
    public static float expSustainedImpulse(float x, float f, float k)
    {
        var s = math.max(x-f,0.0f);
        return math.min( x*x/(f*f), 1.0f+(2.0f/f)*s*exp(-k*s));
    }

    public static float cubicImpulse(float _x,float _c, float _w)
    {
        _x = abs(_x - _c);
        if (_x > _w)
            return 0;
        _x /= _w;
        return 1.0f - _x * _x * (3.0f - 2.0f * _x);
    }
    
    public static float sincImpulse(float _x, float _k)
    {
        var a = kmath.kPI * ((_k * _x - 1));
        return sin(a) / a;
    }
    
    public static float expStep(float _x, float _k, float _n) => exp(-_k * math.pow(_x, _n));
    
    public static float almostUnitIdentity( float _x ) => _x*_x*(2.0f-_x);
    
    public static float gain(float _x, float _k)
    {
        var a = 0.5f * math.pow( 2.0f*(_x<.5f?_x:1.0f-_x),_k);
        return _x < .5f ? a : 1.0f - a;
    }
    
    public static float parabola(float _x, float _k) => math.pow(4.0f*_x*(1.0f-_x),_k);

    public static float triangle(float _x) => 1.0f - 2.0f * abs(_x - 0.5f);
    
    public static float powerCurve(float _x, float _a, float _b)
    {
        var k = math.pow(_a + _b, _a + _b) / math.pow(_a,_a) * math.pow(_b, _b);
        return k * math.pow(_x, _a) * math.pow(1.0f - _x, _b);
    }

    public static float tonemap(float _x, float _k) => (_k + 1f) / (1f + _k * _x);
}