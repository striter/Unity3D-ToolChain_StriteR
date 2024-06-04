using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mathf;

public partial class umath
{
    public static float2 bilinearLerp(float2 tl, float2 tr, float2 br, float2 bl,float2 p)=> tl + (tr - tl) * p.x + (bl - tl) * p.y + (tl - tr + br - bl) * (p.x * p.y);
    public static float3 bilinearLerp(float3 tl, float3 tr, float3 br, float3 bl,float2 p)=> tl + (tr - tl) * p.x + (bl - tl) * p.y + (tl - tr + br - bl) * (p.x * p.y);

    public static float3 bilinearLerp(float3 tl, float3 tr, float3 br, float3 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float2 bilinearLerp(float2 tl, float2 tr, float2 br, float2 bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
    public static float bilinearLerp(float tl, float tr, float br, float bl,float u,float v)=> tl + (tr - tl) * u + (bl - tl) * v + (tl - tr + br - bl) * (u * v);
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
            float w = k1 * k1 - 4f * k0 * k2;
            if (w < 0f)
                return -Vector2.one;
            w = math.sqrt(w);
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
        if (Math.Abs(_interpolate - 1) < float.Epsilon)
            return _dst;
        if (_interpolate == 0)
            return _src;
        return _src || _dst;
    }

    public static class valueMapping
    {
        public static float Lerp(float _a, float _b, float _t)=> (1.0f - _t) * _a + _b * _t;
        public static float InvLerp(float _a, float _b, float _v)=>(_v - _a) / (_b - _a);
        public static float Remap(float _a, float _b, float _v, float _ta, float _tb) => Lerp(_ta, _tb, InvLerp(_a,_b,_v));
        
        //&https://iquilezles.org/articles/functions/
        public static float AlmostIdentity(float _x)
        {
            return _x * _x * (2.0f - _x);
        }

        public static float AlmostIdentity(float _x, float _n)
        {
            return Sqrt(_x * _x + _n);
        }
        
        public static float AlmostIdentity(float _x,float _m,float _n)
        {
            if (_x > _m) return _x;
            float a = 2 * _n - _m;
            float b = 2 * _m - 3 * _n;
            float t = _x / _m;
            return (a * t + b) * t * t + _n;
        }

        public static float SmoothStepIntegral(float _x, float _t)
        {
            if (_x > _t) return _x - _t / 2.0f;
            return _x * _x * _x * (1.0f - _x * .5f / _t) / _t / _t;
        }

        public static float ExpImpulse(float _x,float _k)
        {
            float h = _k * _x;
            return h * Exp(1.0f - h);
        }
        
        public static float QuadraticImpulse(float _x,float _k)
        {
            return 2.0f * Sqrt(_k) * _x / (1.0f + _k * _x * _x);
        }

        public static float PolyNormalImpulse(float _x, float _k, float _n)
        {
            return (_n/(_n-1f))*Pow((_n-1f)*_k,1.0f/_n)*_x/(1.0f+_k*Pow(_x,_n));
        }
        
        public static float ExpSustainedImpulse(float _x, float _f, float _k)
        {
            float s = Max(_x - _f, 0);
            return Min(_x * _x / (_f * _f), 1 +(2.0f/_f)*s*Exp(-_k*s));
        }

        public static float CubicPulse(float _x,float _c, float _w)
        {
            _x = Abs(_x - _c);
            if (_x > _w)
                return 0;
            _x /= _w;
            return 1.0f - _x * _x * (3.0f - 2.0f * _x);
        }
        
        public static float ExpStep(float _x, float _k, float _n)
        {
            return Exp(-_k * Pow(_x, _n));
        }

        public static float Gain(float _x, float _k)
        {
            float a = 0.5f * Pow( 2.0f*(_x<.5f?_x:1.0f-_x),_k);
            return _x < .5f ? a : 1.0f - a;
        }
        
        public static float Parabola(float _x, float _k)
        {
            return Pow(4.0f*_x*(1.0f-_x),_k);
        }

        public static float Triangle(float _x)
        {
            return 1.0f - 2.0f * Abs(_x - 0.5f);
        }
        
        public static float PowerCurve(float _x, float _a, float _b)
        {
            float k = Pow(_a + _b, _a + _b) / Pow(_a,_a) * Mathf.Pow(_b, _b);
            return k * Pow(_x, _a) * Pow(1.0f - _x, _b);
        }
        
        public static float Sinc(float _x, float _k)
        {
            float a = kmath.kPI * ((_k * _x - 1));
            return Sin(a) / a;
        }
    }
}