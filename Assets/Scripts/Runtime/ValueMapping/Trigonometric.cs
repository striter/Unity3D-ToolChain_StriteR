using System;
using Unity.Mathematics;
using static kmath;
using static Unity.Mathematics.math;
using static umath;

public static partial class umath
{
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
    // const float s1 =  0.7853975892066955566406250000000000f;
    // const float s2 = -0.0807407423853874206542968750000000f;
    // const float s3 =  0.0024843954015523195266723632812500f;
    // const float s4 = -0.0000341485538228880614042282104492f;
    // public static float sin_fast(float _x)
    // {
    //     var x2 = _x * _x;
    //     var sp = s3 + s4 * x2;
    //     sp = s2 + sp * x2;
    //     sp = s1 + sp * x2;
    //     return sp * _x;
    // }
		
    // Coefficients for minimax approximation of cos(x*pi/4), x=[0,2].
    // const float c1 =  0.9999932952821962577665326692990000f;
    // const float c2 = -0.3083711259464511647371969120320000f;
    // const float c3 =  0.0157862649459062213825197189573000f;
    // const float c4 = -0.0002983708648233575495551227373110f;
    // public static float cos_fast(float _x)
    // {
    //     var x2 = _x * _x;
    //     var cp = c3 + c4 * _x;
    //     cp = c2 + cp * x2;
    //     cp = c1 + cp * x2;
    //     return cp;
    // }

    // public static void sincos_fast(float _x, out float sinX, out float cosX)
    // {
    //     var x2 = _x * _x;
    //     var sp = s3 + s4 * x2;
    //     sp = s2 + sp * x2;
    //     sp = s1 + sp * x2;
    //     var cp = c3 + c4 * _x;
    //     cp = c2 + cp * x2;
    //     cp = c1 + cp * x2;
    //     
    //     sinX =  sp * _x;
    //     cosX =  cp;
    // }

    private static readonly float2[] kAlphaSinCos = GenerateAlphaCosSin();
    static float2[] GenerateAlphaCosSin()
    {
        var alphaSinCos = new float2[256];
        for (int i = 0; i < 256; i++)
        {
            var angle = i * kPiDiv128;
            alphaSinCos[i] = new float2( math.cos(angle),math.sin(angle));
        }
        return alphaSinCos;
    }
    
    public static void sincos_fast(float _f, out float _s, out float _c)
    {
        var a =abs(_f) * k128InvPi;
        var i = (int)floor(a);
        var b = (a - i) * kPiDiv128;
        var alphaCosSin = kAlphaSinCos[i&255];
        var b2 = b * b;
        var sine_beta = b - b * b2 * (0.1666666667F - b2 * 0.00833333333F);
        var cosine_beta = 1.0f - b2 * (0.5f - b2 * 0.04166666667F);
    
        var sine = alphaCosSin.y * cosine_beta + alphaCosSin.x * sine_beta;
        var cosine = alphaCosSin.x * cosine_beta - alphaCosSin.y * sine_beta;
    
        _s = _f < 0f ? -sine : sine;
        _c = cosine;
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
        int k = (int)floor(_x / kPIDiv2);
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
}
