using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

using static kmath;
public partial class umath
{
    public static float saturate(float _src) => clamp(_src, 0f, 1f);
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
        if (math.abs(k2) > float.Epsilon)
        {
            var w = k1 * k1 - 4f * k0 * k2;
            if (w < 0f)
                return -kfloat2.one;
            w = math.sqrt(w);
            var ik2 = .5f / k2;
            var v = (-k1 - w) * ik2;
            var u = (h.x - f.x * v) / (e.x + g.x * v);
            if (!RangeFloat.k01.Contains(u) || !RangeFloat.k01.Contains(v))
            {
                v = (-k1 + w) * ik2;
                u = (h.x - f.x * v) / (e.x + g.x * v);
            }
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

    
    public static float cosH(float _src) => (math.exp(_src) + math.exp(_src)) / 2;
    public static float copySign(float _a, float _b)
    {
        var signA = sign(_a);
        var signB = sign(_b);
        return abs(signA - signB) < float.Epsilon ? _a : _a * signB;
    }
    
    //Fast
    public static float negExp_Fast(float _x)
    {
        return 1.0f / (1.0f + _x + 0.48f * _x * _x + 0.235f * _x * _x * _x);
    }

    public static float atan_Fast(float _x)
    {
        float z = abs(_x);
        float w = z > 1f ? 1f / z : z;
        float y = (kPI / 4.0f) * w - w * (w - 1) * (0.2447f + 0.0663f * w);
        return copySign(z > 1 ? kPIDiv2 - y : y,_x);
    }
    
    // Coefficients for 6th degree minimax approximation of atan(x)*2/pi, x=[0,1].
    const float t1 =  0.406758566246788489601959989e-5f;
    const float t2 =  0.636226545274016134946890922156f;
    const float t3 =  0.61572017898280213493197203466e-2f;
    const float t4 = -0.247333733281268944196501420480f;
    const float t5 =  0.881770664775316294736387951347e-1f;
    const float t6 =  0.419038818029165735901852432784e-1f;
    const float t7 = -0.251390972343483509333252996350e-1f;
    public static float atan_Fast_2DivPI(float _x) 
    {
        float phi = t6 + t7*_x;
        phi = t5 + phi*_x;
        phi = t4 + phi*_x;
        phi = t3 + phi*_x;
        phi = t2 + phi*_x;
        phi = t1 + phi*_x;
        return phi;
    }
    
    // Coefficients for minimax approximation of sin(x*pi/4), x=[0,2].
    const float s1 =  0.7853975892066955566406250000000000f;
    const float s2 = -0.0807407423853874206542968750000000f;
    const float s3 =  0.0024843954015523195266723632812500f;
    const float s4 = -0.0000341485538228880614042282104492f;
    public static float sin_fast(float _x)
    {
        var x2 = _x * _x;
        var sp = s3 + s4 * x2;
        sp = s2 + sp * x2;
        sp = s1 + sp * x2;
        return sp * _x;
    }
		
    // Coefficients for minimax approximation of cos(x*pi/4), x=[0,2].
    const float c1 =  0.9999932952821962577665326692990000f;
    const float c2 = -0.3083711259464511647371969120320000f;
    const float c3 =  0.0157862649459062213825197189573000f;
    const float c4 = -0.0002983708648233575495551227373110f;
    public static float cos_fast(float _x)
    {
        var x2 = _x * _x;
        var cp = c3 + c4 * _x;
        cp = c2 + cp * x2;
        cp = c1 + cp * x2;
        return cp;
    }

    public static void sincos_fast(float _x, out float sinX, out float cosX)
    {
        var x2 = _x * _x;
        var sp = s3 + s4 * x2;
        sp = s2 + sp * x2;
        sp = s1 + sp * x2;
        var cp = c3 + c4 * _x;
        cp = c2 + cp * x2;
        cp = c1 + cp * x2;
        
        sinX =  sp * _x;
        cosX =  cp;
    }

    // private static readonly float2[] kAlphaSinCos = GenerateAlphaCosSin();
    // static float2[] GenerateAlphaCosSin()
    // {
    //     var alphaSinCos = new float2[256];
    //     for (int i = 0; i < 256; i++)
    //     {
    //         var angle = i * kPiDiv128;
    //         alphaSinCos[i] = new float2( math.cos(angle),math.sin(angle));
    //     }
    //     return alphaSinCos;
    // }
    //
    // public static void sincos_fast(float _f, out float _s, out float _c)
    // {
    //     var a =abs(_f) * k128InvPi;
    //     var i = (int)floor(a);
    //     var b = (a - i) * kPiDiv128;
    //     var alphaCosSin = kAlphaSinCos[i&255];
    //     var b2 = b * b;
    //     var sine_beta = b - b * b2 * (0.1666666667F - b2 * 0.00833333333F);
    //     var cosine_beta = 1.0f - b2 * (0.5f - b2 * 0.04166666667F);
    //
    //     var sine = alphaCosSin.y * cosine_beta + alphaCosSin.x * sine_beta;
    //     var cosine = alphaCosSin.x * cosine_beta - alphaCosSin.y * sine_beta;
    //
    //     _s = _f < 0f ? -sine : sine;
    //     _c = cosine;
    // }

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
        int k = (int)math.floor(_x / kPIDiv2);
        float y = _x - k * kPIDiv2;
        switch (( k % 4+4) % 4)
        {
            default: throw new ArgumentNullException();
            case 0: return sin_kinda(y);
            case 1: return sin_kinda(kPIDiv2 - y);
            case 2: return -sin_kinda(y);
            case 3: return -sin_kinda(kPIDiv2 - y);
        }
    }
    public static float repeat(float _t,float _length) => math.clamp(_t - math.floor(_t / _length) * _length, 0.0f, _length);
    public static float2 repeat(float2 _t,float2 _length) => math.clamp(_t - math.floor(_t / _length) * _length, 0.0f, _length);
    
    public static bool IsPrime(ushort _value)
    {
        for (ushort i = 2; i < _value; i++)
        {
            if (_value % i == 0)
                return false;
        }
        return true;
    }

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
    public static float sqr(float _src) => _src * _src; public static float2 sqr(float2 _src) => _src * _src; public static float3 sqr(float3 _src) => _src * _src; public static float4 sqr(float4 _src) => _src * _src;
    public static float pow2(float _src) => _src * _src; public static float2 pow2(float2 _src) => _src * _src; public static float3 pow2(float3 _src) => _src * _src; public static float4 pow2(float4 _src) => _src * _src;
    
    public static float pow3(float _src) => _src * _src* _src; public static float2 pow3(float2 _src) => _src * _src* _src; public static float3 pow3(float3 _src) => _src * _src* _src; public static float4 pow3(float4 _src) => _src * _src* _src;
    
    public static float pow4(float _src) => _src * _src* _src* _src;
    public static float mod(float _src, float _dst) => _src - _dst * floor(_src/_dst);
    public static float lerp(float _a, float _b, float _value) => _a + (_b - _a) * _value;
    public static float invLerp(float _a, float _b, float _value)=> (_value - _a) / (_b - _a);
    public static float invLerp(this RangeFloat _range, float _value)=> (_value - _range.start) / (_range.length);

    public static float3 invLerp(float3 _a, float3 _b, float3 _value)=> float3(invLerp(_a.x,_b.x, _value.x),invLerp(_a.y,_b.y, _value.y),invLerp(_a.z,_b.z, _value.z));
    
    //&https://iquilezles.org/articles/functions/
    public static float almostIdentity(float _x)
    {
        return _x * _x * (2.0f - _x);
    }

    public static float almostIdentity(float _x, float _n)
    {
        return sqrt(_x * _x + _n);
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

    public static float polyNormalImpulse(float _x, float _k, float _n)
    {
        return (_n/(_n-1f))*math.pow((_n-1f)*_k,1.0f/_n)*_x/(1.0f+_k*math.pow(_x,_n));
    }
    
    public static float expSustainedImpulse(float _x, float _f, float _k)
    {
        float s = max(_x - _f, 0);
        return min(_x * _x / (_f * _f), 1 +(2.0f/_f)*s*exp(-_k*s));
    }

    public static float cubicPulse(float _x,float _c, float _w)
    {
        _x = abs(_x - _c);
        if (_x > _w)
            return 0;
        _x /= _w;
        return 1.0f - _x * _x * (3.0f - 2.0f * _x);
    }
    
    public static float expStep(float _x, float _k, float _n)
    {
        return exp(-_k * math.pow(_x, _n));
    }

    public static float gain(float _x, float _k)
    {
        float a = 0.5f * math.pow( 2.0f*(_x<.5f?_x:1.0f-_x),_k);
        return _x < .5f ? a : 1.0f - a;
    }
    
    public static float parabola(float _x, float _k)
    {
        return math.pow(4.0f*_x*(1.0f-_x),_k);
    }

    public static float triangle(float _x)
    {
        return 1.0f - 2.0f * abs(_x - 0.5f);
    }
    
    public static float powerCurve(float _x, float _a, float _b)
    {
        var k = math.pow(_a + _b, _a + _b) / math.pow(_a,_a) * math.pow(_b, _b);
        return k * math.pow(_x, _a) * math.pow(1.0f - _x, _b);
    }
    
    public static float sinc(float _x, float _k)
    {
        var a = kmath.kPI * ((_k * _x - 1));
        return sin(a) / a;
    }
}