using Unity.Mathematics;
using static Unity.Mathematics.math;

public partial class umath
{
    public static float bilinearLerp(float tl, float tr, float br, float bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float2 bilinearLerp(float2 tl, float2 tr, float2 br, float2 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float3 bilinearLerp(float3 tl, float3 tr, float3 br, float3 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float2 bilinearLerp(float2 tl, float2 tr, float2 br, float2 bl,float2 p) => bilinearLerp(tl, tr, br, bl, p.x, p.y);
    public static float3 bilinearLerp(float3 tl, float3 tr, float3 br, float3 bl,float2 p) => bilinearLerp(tl, tr, br, bl, p.x, p.y);
    
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