using System;
using System.Numerics;
using UnityEngine;
using Unity.Mathematics;

public static class kmath
{
    public const float kSQRT3 = 1.7320508075689f;
    public static readonly float kSQRT3Half = kSQRT3 / 2f;
    public static readonly float kInvSQRT3 = 1f / kSQRT3;

    public const float kSin0d = 0, kSin30d = 0.5f,     kSin45d=kSQRT2/2f, kSin60d = kSQRT3/2f, kSin90d = 1f,             kSin120d = kSQRT3/2;
    public const float kCos0d = 1, kCos30d = kSQRT3/2, kCos45d=kSQRT2/2f, kCos60d = 0.5f,      kCos90d = 0f,             kCos120d = -1/2f;
    public const float kTan0d = 0, kTan30d = kSQRT3/3, kTan45d = 1,       kTan60d = kSQRT3,    kTan90d = float.MaxValue, kTan120d =-kSQRT3;
    
    public const float kSQRT2 = 1.4142135623731f;
    public const float kPI = 3.141593f;
    public const float kPI2 = kPI * 2;
    public const float kPID2 = 1.5707963267948966f;
    public const float kPID4 = 0.7853981633974483f;
    public const float kDeg2Rad = 0.017453292519943f;//PI / 180
    public const float kRad2Deg = 57.295779513082f ;//180f / PI;

    public const float kOneMinusEpsilon = 1f - float.Epsilon;

    public static readonly ushort[] kPrimes128 = new ushort[] {
        2,3   ,5   ,7  ,11 ,13 ,17 ,19 ,23 ,29 ,
        31,37  ,41  ,43 ,47 ,53 ,59 ,61 ,67 ,71 ,
        73,79  ,83  ,89 ,97 ,101,103,107,109,113,
        127,131 ,137,139,149,151,157,163,167,173,
        179,181 ,191,193,197,199,211,223,227,229,
        233,239 ,241,251,257,263,269,271,277,281,
        283,293 ,307,311,313,317,331,337,347,349,
        353,359 ,367,373,379,383,389,397,401,409,
        419,421 ,431,433,439,443,449,457,461,463,
        467,479 ,487,491,499,503,509,521,523,541,
        547,557 ,563,569,571,577,587,593,599,601,
        607,613 ,617,619,631,641,643,647,653,659,
        661,673 ,677,683,691,701,709,719
    };

    public static readonly ushort[] kPolys128 = new ushort[]
    {
        1,    3,    7,   11,   13,   19,   25,   37,   59,   47,
        61,   55,   41,   67,   97,   91,  109,  103,  115,  131,
        193,  137,  145,  143,  241,  157,  185,  167,  229,  171,
        213,  191,  253,  203,  211,  239,  247,  285,  369,  299,
        301,  333,  351,  355,  357,  361,  391,  397,  425,  451,
        463,  487,  501,  529,  539,  545,  557,  563,  601,  607,
        617,  623,  631,  637,  647,  661,  675,  677,  687,  695, 
        701,  719,  721,  731,  757,  761,  787,  789,  799,  803,
        817,  827,  847,  859,  865,  875,  877,  883,  895,  901,
        911,  949,  953,  967,  971,  973,  981,  985,  995, 1001,
        1019, 1033, 1051, 1063, 1069, 1125, 1135, 1153, 1163, 1221,
        1239, 1255, 1267, 1279, 1293, 1305, 1315, 1329, 1341, 1347,
        1367, 1387, 1413, 1423, 1431, 1441, 1479, 1509,
    };
}

public static partial class umath
{
    public static float3 to3xy(this float2 _value, float _z = 0) => new float3(_value, _z);
    public static float3 to3xz(this float2 _value, float _y = 0) => new float3(_value.x, _y,_value.y);

    public static float2 to2xy(this float3 _value) => new float2(_value.x, _value.y);
    public static float2 to2xz(this float3 _value) => new float2(_value.x, _value.z);
    
    public static float4 to4(this float2 _value, float _z=0,float _w=0) => new float4(_value, _z,_w);
    public static float4 to4(this float3 _value, float _w=0) => new float4(_value, _w);
    
    public static float magnitude(this float2 _value) => math.length(_value);
    public static float magnitude(this float3 _value) => math.length(_value);
    public static float magnitude(this float4 _value) => math.length(_value);
    
    public static float sqrmagnitude(this float2 _value) => math.lengthsq(_value);
    public static float sqrmagnitude(this float3 _value) => math.lengthsq(_value);
    public static float sqrmagnitude(this float4 _value) => math.lengthsq(_value);

    public static float sum(this float2 _value) => _value.x + _value.y;
    public static float sum(this float3 _value) => _value.x + _value.y + _value.z;
    public static float sum(this float4 _value) => _value.x + _value.y + _value.z + _value.w;

    public static float2 normalized(this float2 _value) => math.normalize(_value);
    public static float3 normalized(this float3 _value) => math.normalize(_value);
    public static float4 normalized(this float4 _value) => math.normalize(_value);
    
    public static float2 saturate(this float2 _value)=> math.min(math.max(_value,0f) ,1f);
    public static float3 saturate(this float3 _value)=> math.min(math.max(_value,0f) ,1f);
    public static float4 saturate(this float4 _value) => math.min(math.max(_value,0f) ,1f);
    
    public static float2 clamp(this float2 _value,float2 _min,float2 _max)=> math.min(math.max(_value,_min) ,_max);
    public static float3 clamp(this float3 _value,float3 _min,float3 _max)=> math.min(math.max(_value,_min) ,_max);
    public static float4 clamp(this float4 _value,float4 _min,float4 _max)=> math.min(math.max(_value,_min) ,_max);

    public static float dot(this float3 _value) => math.dot(_value, _value);
    
    public static bool anyGreater(this float2 _value, float _comparer) => _value.x > _comparer || _value.y > _comparer;
    public static bool anyGreater(this float3 _value, float _comparer) => _value.x > _comparer || _value.y > _comparer || _value.z > _comparer;
    public static bool anyGreater(this float4 _value, float _comparer) => _value.x > _comparer || _value.y > _comparer || _value.z > _comparer || _value.w > _comparer;
    
    public static float minElement(this float2 _src) => Mathf.Min(_src.x, _src.y);
    public static float minElement(this float3 _src) => Mathf.Min(_src.x, _src.y, _src.z);
    public static float minElement(this float4 _src) => Mathf.Min(_src.x, _src.y, _src.z, _src.w);
    
    public static float maxElement(this float2 _src) => Mathf.Max(_src.x, _src.y);
    public static float maxElement(this float3 _src) => Mathf.Max(_src.x, _src.y, _src.z);
    public static float maxElement(this float4 _src) => Mathf.Max(_src.x, _src.y, _src.z, _src.w);

    public static float convert(this float _src, Func<float, float> _func) => _func(_src);
    public static float2 convert(this float2 _src, Func<float, float> _func) => new float2(_func(_src.x),_func(_src.y));
    public static float3 convert(this float3 _src, Func<float, float> _func) => new float3(_func(_src.x),_func(_src.y),_func(_src.z));
    public static float3 convert(this float3 _src, Func<int,float, float> _func) => new float3(_func(0,_src.x),_func(1,_src.y),_func(2,_src.z));

}

public static class ucomplex
{
    public static float2 mul(float2 _c1, float2 _c2)
    {
        return new float2(_c1.x * _c2.x - _c1.y * _c2.y, _c1.x * _c2.y + _c1.y * _c2.x );
    }
    
    public static float2 divide(float2 _complex1, float2 _complex2)
    {
        var real1 = _complex1.x;
        var imaginary1 = _complex1.y;
        var real2 = _complex2.x;
        var imaginary2 = _complex2.y;
        if (math.abs(imaginary2) < math.abs(real2))
        {
            var num = imaginary2 / real2;
            return new float2((real1 + imaginary1 * num) / (real2 + imaginary2 * num), (imaginary1 - real1 * num) / (real2 + imaginary2 * num));
        }
        var num1 = real2 / imaginary2;
        return new float2((imaginary1 + real1 * num1) / (imaginary2 + real2 * num1), (-real1 + imaginary1 * num1) / (imaginary2 + real2 * num1));
    }

    public static float2 pow(float2 _value, float _power)
    {
        if (_power == 0f)
            return 0f;
        if (_value.sum() == 0f)
            return 0f;
        var real1 = _value.x;
        var imaginary1 = _value.y;
        var num1 = abs(_value);
        var num2 = math.atan2(imaginary1, real1);
        var num3 = _power * num2;
        var num4 = math.pow(num1, _power) ;
        return new float2(num4 * math.cos(num3), num4 * math.sin(num3));
    }

    public static float abs(float2 _value)
    {
        if (float.IsInfinity(_value.x) || float.IsInfinity(_value.y))
            return float.PositiveInfinity;
        var num1 = Math.Abs(_value.x);
        var num2 = Math.Abs(_value.y);
        if (num1 > num2)
        {
            var num3 = num2 / num1;
            return num1 * math.sqrt(1.0f + num3 * num3);
        }
        if (num2 == 0.0)
            return num1;
        var num4 = num1 / num2;
        return num2 * math.sqrt(1.0f + num4 * num4);
    }
}

public static class kfloat3
{
    public static readonly float3 one = 1f;
    public static readonly float3 up = new float3(0, 1, 0);
    public static readonly float3 down = new float3(0, -1, 0);
    public static readonly float3 left = new float3(-1, 0, 0);
    public static readonly float3 right = new float3(1, 0, 0);
    public static readonly float3 forward = new float3(0, 0, 1);
    public static readonly float3 back = new float3(0, 0, -1);
}

public static class kfloat2
{
    public static readonly float2 up = new float2(0, 1);
    public static readonly float2 down = new float2(0, -1);
    public static readonly float2 left = new float2(-1, 0);
    public static readonly float2 right = new float2(1, 0);
}