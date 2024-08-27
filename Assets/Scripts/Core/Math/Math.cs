using System;
using Unity.Mathematics;
using UnityEngine;
using static kmath;
using static Unity.Mathematics.math;
public enum EAxis
{
    X = 0,
    Y = 1,
    Z = 2,
}

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
    public static float mod(float _src, float _dst) => _src - _dst * Mathf.Floor(_src/_dst);

    public static float lerp(float _a, float _b, float _value) => Mathf.Lerp(_a, _b, _value);
    
    public static float invLerp(float _a, float _b, float _value)=> (_value - _a) / (_b - _a);
    public static float invLerp(this RangeFloat _range, float _value)=> (_value - _range.start) / (_range.length);

    public static float3 invLerp(float3 _a, float3 _b, float3 _value)=> float3(invLerp(_a.x,_b.x, _value.x),invLerp(_a.y,_b.y, _value.y),invLerp(_a.z,_b.z, _value.z));
    public static float3 trilerp(float3 _a, float3 _b, float3 _c, float _value)
    {
        if (_value < .5f)
            return math.lerp(_a, _b, _value * 2);
        return math.lerp(_b, _c, _value * 2 - 1f);
    }
    
    public static float angle(float3 a, float3 b)
    {
        var sqMagA = a.sqrmagnitude();
        var sqMagB = b.sqrmagnitude();
        if (sqMagB == 0 || sqMagA == 0)
            return 0;
            
        var dot = math.dot(a, b);
        if (abs(1 - sqMagA) < EPSILON && abs(1 - sqMagB) < EPSILON) {
            return acos(dot);
        }
 
        float length = sqrt(sqMagA) * sqrt(sqMagB);
        return acos(dot / length);
    }
    public static float3 slerp(float3 from, float3 to, float t,float3 up)
    {
        float theta = angle(from, to);
        float sin_theta = sin(theta);
        var dotValue = dot(from.normalize(), to.normalize());
        Debug.Log(dotValue);
        if (dotValue > .999f)
            return to;
        if(dotValue < -.999f)
            return trilerp(from, up,to, t);

        float a = sin((1 - t) * theta) / sin_theta;
        float b = sin(t * theta) / sin_theta;
        return from * a + to * b;
    }

    public static float3 nlerp(float3 _from, float3 _to, float t)
    {
        return normalize(math.lerp(_from,_to,t));
    }
    
    public static EAxis maxAxis(this float2 _value)
    {
        if (_value.x > _value.y)
            return EAxis.X;
        return EAxis.Y;
    }
    public static EAxis maxAxis(this float3 _value)
    {
        if (_value.x > _value.y && _value.x > _value.z)
            return EAxis.X;
        return _value.y > _value.z ? EAxis.Y : EAxis.Z;
    }
    
    public static Matrix4x4 add(this Matrix4x4 _src, Matrix4x4 _dst)
    {
        Matrix4x4 dst = Matrix4x4.identity;
        for(int i=0;i<4;i++)
            dst.SetRow(i,_src.GetRow(i)+_dst.GetRow(i));
        return dst;
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

    public static float2 tripleProduct(float2 _a, float2 _b, float2 _c) => _b *math.dot(_a, _c)  - _a * math.dot(_c, _b);
    public static float3 tripleProduct(float3 _a, float3 _b, float3 _c) => _b *math.dot(_a, _c)  - _a * math.dot(_c, _b);
    public static float repeat(float _t,float _length) => math.clamp(_t - math.floor(_t / _length) * _length, 0.0f, _length);
    public static float2 repeat(float2 _t,float2 _length) => math.clamp(_t - math.floor(_t / _length) * _length, 0.0f, _length);
    
    public static float lerpAngle(float _a,float _b,float _t)
    {
        var num = repeat( _b - _a, 360);
        if (num > 180)
            num -= 360;
        return _a + num * _t;
    }

    public static float3 lerpAngle(float3 _a, float3 _b, float _t) => new(
        lerpAngle(_a.x, _b.x, _t), 
        lerpAngle(_a.y, _b.y, _t), 
        lerpAngle(_a.z, _b.z, _t));

    public static float deltaAngle(float _x,float _xd)
    {
        float num = repeat(_xd - _x, 360f);
        if (num > 180.0)
            num -= 360f;
        return num;
    }
    public static float2 deltaAngle(float2 _x, float2 _xd)
    {
        return new float2(
            deltaAngle(_x.x, _xd.x),
            deltaAngle(_x.y, _xd.y)
        );
    }

    public static float4 deltaAngle(float4 _x, float4 _xd)
    {
        return new float4(
            deltaAngle(_x.x, _xd.x),
            deltaAngle(_x.y, _xd.y),
            deltaAngle(_x.z, _xd.z),
            deltaAngle(_x.w, _xd.w)
        );
    }
    public static int signNonZero(float _a) => _a < 0 ? -1 : 1;

    public static float flipSign(float a, int s)
    {
        var newSign = signNonZero(signNonZero(a) ^ s);
        return abs(a) * newSign;
    }

}
