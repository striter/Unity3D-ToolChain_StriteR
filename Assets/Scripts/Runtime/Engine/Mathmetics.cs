using System;
using System.Numerics;
using UnityEngine;
using Unity.Mathematics;

public static class kmath
{
    public static readonly float kInv2 = 1f / 2;
    public static readonly float kInv3 = 1f / 3;
    public static readonly float kInv6 = 1f / 6;
    public static readonly float kInv9 = 1f / 9;
    
    public const float kSQRT3 = 1.7320508075689f;
    public static readonly float kSQRT3Half = kSQRT3 / 2f;
    public static readonly float kInvSQRT3 = 1f / kSQRT3;

    public const float kSin0d = 0, kSin30d = 0.5f,     kSin45d=kSQRT2/2f, kSin60d = kSQRT3/2f, kSin90d = 1f,             kSin120d = kSQRT3/2;
    public const float kCos0d = 1, kCos30d = kSQRT3/2, kCos45d=kSQRT2/2f, kCos60d = 0.5f,      kCos90d = 0f,             kCos120d = -1/2f;
    public const float kTan0d = 0, kTan30d = kSQRT3/3, kTan45d = 1,       kTan60d = kSQRT3,    kTan90d = float.MaxValue, kTan120d =-kSQRT3;
    
    public const float kDeg2Rad = 0.017453292519943f;//PI / 180
    public const float kRad2Deg = 57.295779513082f ;//180f / PI;
    public const float kSQRT2 = 1.4142135623731f;
    
    //PI
    public const float kPI = 3.14159265359f;
    public const float kPIMulHalf = 1.57079632679f;
    public const float kPIMul2 = 6.28318530718f;
    public const float kPIMul4 = 12.5663706144f;
    //Division
    public const float kPIDiv2 = 1.5707963267948966f;
    public const float kPIDiv4 = 0.7853981633974483f;
    public const float kPIDiv8 = 0.3926990817f;
    public const float kPIDiv16 = 0.19634954085f;
    //Invert
    public const float kInvPI = 0.31830988618f;

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

public static partial class umath       //Swizzling
{
    public static float3 to3xy(this float2 _value, float _z = 0) => new float3(_value, _z);
    public static float3 to3xz(this float2 _value, float _y = 0) => new float3(_value.x, _y,_value.y);
    
    public static float3 to3xyz(this float4 _value) => new float3(_value.x, _value.y,_value.z);
    public static float4 to4(this float2 _value, float _z=0,float _w=0) => new float4(_value, _z,_w);
    public static float4 to4(this float3 _value, float _w=0) => new float4(_value, _w);
    public static float4 to4(this float _value) => new float4(_value, _value,_value,_value);

    public static float3 setY(this float3 _value, float _y) => new float3(_value.x, _y, _value.z);
    
    public static float magnitude(this float2 _value) => math.length(_value);
    public static float magnitude(this float3 _value) => math.length(_value);
    public static float magnitude(this float4 _value) => math.length(_value);
    
    public static float sqrmagnitude(this float2 _value) => math.lengthsq(_value);
    public static float sqrmagnitude(this float3 _value) => math.lengthsq(_value);
    public static float sqrmagnitude(this float4 _value) => math.lengthsq(_value);

    public static float sum(this float2 _value) => _value.x + _value.y;
    public static float sum(this float3 _value) => _value.x + _value.y + _value.z;
    public static float sum(this float4 _value) => _value.x + _value.y + _value.z + _value.w;

    public static bool isZero(this float2 _value) => _value is { x: 0, y: 0 };
    public static bool isZero(this float3 _value) => _value is { x: 0, y: 0, z: 0 };
    public static bool isZero(this float4 _value) => _value is { x: 0, y: 0, z: 0, w: 0 };
    
    public static float2 normalize(this float2 _value) => math.normalize(_value);
    public static float3 normalize(this float3 _value) => math.normalize(_value);
    public static float4 normalize(this float4 _value) => math.normalize(_value);
    
    public static float saturate(this float _value)=> math.min(math.max(_value,0f) ,1f);
    public static float2 saturate(this float2 _value)=> math.min(math.max(_value,0f) ,1f);
    public static float3 saturate(this float3 _value)=> math.min(math.max(_value,0f) ,1f);
    public static float4 saturate(this float4 _value) => math.min(math.max(_value,0f) ,1f);
    
    public static float2 clamp(this float2 _value,float2 _min,float2 _max)=> math.min(math.max(_value,_min) ,_max);
    public static float3 clamp(this float3 _value,float3 _min,float3 _max)=> math.min(math.max(_value,_min) ,_max);
    public static float4 clamp(this float4 _value,float4 _min,float4 _max)=> math.min(math.max(_value,_min) ,_max);

    public static float dot(this float3 _src) => math.dot(_src, _src);
    public static float dot(this float3 _src,float3 _dst) => math.dot(_src, _dst);
    
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
    public static float4 convert(this float4 _src, Func<float, float> _func) => new float4(_func(_src.x),_func(_src.y),_func(_src.z),_func(_src.w));
    
    public static float3 convert(this float3 _src, Func<int,float, float> _func) => new float3(_func(0,_src.x),_func(1,_src.y),_func(2,_src.z));
    public static float4 convert(this float4 _src, Func<int,float, float> _func) => new float4(_func(0,_src.x),_func(1,_src.y),_func(2,_src.z),_func(3,_src.z));


    public static float2 cross(this float2 _src) => new float2(_src.y,-_src.x);
}

public static class umatrix
{
    
    public static float3 GetEigenValues(this float3x3 _C)
    {
        var c0 = _C.c0; var c00 = c0.x; var c01 = c0.y; var c02 = c0.z;
        var c1 = _C.c1; var c10 = c1.x; var c11 = c1.y; var c12 = c1.z;
        var c2 = _C.c2; var c20 = c2.x; var c21 = c2.y; var c22 = c2.z;
            
        var polynomial = new CubicPolynomial(-1,
            c00 + c11 + c22,
            -c00*c11 -c00*c22 + c12*c21 -c11*c22 +c10*c01 +c20*c02,
            -c00*c12*c21 + c00*c11*c22-c10*c01*c22 +c10*c02*c21+c20*c01*c12-c20*c02*c11);
        var root = polynomial.GetRoots(out var _roots);
        Debug.Assert(root == 3 , $"Invalid Root Length Find:{root}");
        Array.Sort(_roots,(a,b)=>a<b?1:-1);
        return new float3(_roots[0], _roots[1], _roots[2]);
    }
    
    public static void GetEigenVectors(this float3x3 _C,out float3 _R,out float3 _S,out float3 _T)
    {
        var eigenValues = _C.GetEigenValues();
        _R = _C.GetEigenVector( eigenValues.x);
        _S = _C.GetEigenVector( eigenValues.y);
        _T = _C.GetEigenVector( eigenValues.z);
    }
    
    public static float3 GetEigenVector(this float3x3 _matrix,float _eigenValue)
    {
        _matrix -= _eigenValue * float3x3.identity;
        float3 equation0 = new float3(_matrix.c0.x, _matrix.c1.x, _matrix.c2.x);
        float3 equation1 = new float3(_matrix.c0.y, _matrix.c1.y, _matrix.c2.y);
        var yzEquation= equation1.x!=0? (equation1 - equation0 * (equation1.x/equation0.x)):equation1;
        var xzEquation = equation1.y!=0? (equation1 - equation0 * (equation1.y/equation0.y)):equation1;
        return new float3(xzEquation.z/xzEquation.x, yzEquation.z/yzEquation.y, -1).normalize();
    }

    public static float2 GetEigenValues(this float2x2 _C)
    {
        var c00 = _C.c0.x; var c01 = _C.c0.y;
        var c10 = _C.c1.x; var c11 = _C.c1.y;
        var polynomial = new QuadraticPolynomial(1, - c00 - c11 , c00*c11 - c10*c01);
        polynomial.GetRoots(out var roots);
        Array.Sort(roots,(_a,_b)=>_a<_b?1:-1);
        return new float2(roots[0], roots[1]);
    }

    public static float2 GetEigenVector(this float2x2 _matrix, float _eigenValue)
    {
        _matrix -= _eigenValue * float2x2.identity;
        var equation0 = new float2(_matrix.c0.x, _matrix.c1.x);
        var equation1 = new float2(_matrix.c0.y, _matrix.c1.y);
        var yzEquation= equation1.x!=0? (equation1 - equation0 * (equation1.x/equation0.x)):equation1;
        if (yzEquation.sqrmagnitude() <= 0.01f)
            yzEquation = equation0;
        
        return new float2(yzEquation.x, yzEquation.y).normalize();
    }

    public static void GetEigenVectors(this float2x2 _C,out float2 _R,out float2 _S)
    {
        var eigenValues = _C.GetEigenValues();
        _R = _C.GetEigenVector( eigenValues.x);
        _S = _C.GetEigenVector( eigenValues.y);
    }
    #region Notes
    // internal static class Notes
    // {
    //     public static float OutputPolynomial(float3x3 C,float λ)       //Jezz
    //     {         
    //         var c0 = C.c0; var c00 = c0.x; var c01 = c0.y; var c02 = c0.z;
    //         var c1 = C.c1; var c10 = c1.x; var c11 = c1.y; var c12 = c1.z;
    //         var c2 = C.c2; var c20 = c2.x; var c21 = c2.y; var c22 = c2.z;
    //         
    //         //Resolve determination parts
    //     var part1 = c00 * (c11 * c22 - c12 * c21);      //   c0.x * (c1.y * c2.z - c1.z * c2.y) 
    //         part1 = (c00 - λ) * ((c11-λ) * (c22-λ) - c12*c21);
    //         part1 = (c00 - λ) * (+c11*c22 -c11*λ - c22*λ     + λ*λ -c12*c21 );
    //         part1 = (c00 - λ) * (λ*λ      -c11*λ - c22*λ     -c12*c21 +c11*c22);
    //         part1 =        c00* (λ*λ      -c11*λ - c22*λ     -c12*c21 +c11*c22)
    //                        - λ* (λ*λ      -c11*λ - c22*λ     -c12*c21 +c11*c22);
    //         part1 =        c00*λ*λ   -c00*c11*λ - c00*c22*λ  -c00*c12*c21 + c00*c11*c22 
    //                         - λ*λ*λ      +c11*λ*λ + c22*λ*λ   +c12*c21*λ -c11*c22*λ;
    //         part1 = -λ*λ*λ 
    //             + c00*λ*λ +c11*λ*λ + c22*λ*λ
    //             -c00*c11*λ - c00*c22*λ +c12*c21*λ -c11*c22*λ
    //             -c00*c12*c21 + c00*c11*c22  ;
    //         part1 = -1*λ*λ*λ 
    //             + (c00 + c11 +c22)*λ*λ
    //             -c00*c11*λ -c00*c22*λ + c12*c21*λ -c11*c22*λ
    //             -c00*c12*c21 + c00*c11*c22;
    //         
    //     var part2 = -c10 * (c01 * c22 - c02 * c21);      // - c1.x * (c0.y * c2.z - c0.z * c2.y) 
    //         part2 = -c10 * (c01 * (c22-λ) - c02 * c21); 
    //         part2 = -c10 * (c01*c22  -c01*λ      -c02*c21); 
    //         part2 = -c10*c01*c22  +c10*c01*λ   +c10*c02*c21; 
    //         part2 = +c10*c01*λ    -c10*c01*c22 +c10*c02*c21; 
    //         
    //     var part3 =  + c20 * (c01 * c12 - c02 * c11);   // + c2.x * (c0.y * c1.z - c0.z * c1.y);
    //         part3 = c20 * (c01*c12 - c02 * (c11-λ));
    //         part3 = c20 * (c01*c12 - c02*c11   +c02*λ);
    //         part3 = +c20*c01*c12 -c20*c02*c11  +c20*c02*λ;
    //         part3 = +c20*c02*λ   +c20*c01*c12 -c20*c02*c11;
    //
    //         var cubic = -1;
    //         var quadratic = c00 + c11 + c22;
    //         var linear = -c00*c11 -c00*c22 + c12*c21 -c11*c22 +c10*c01 +c20*c02;
    //         var constant = -c00*c12*c21 + c00*c11*c22-c10*c01*c22 +c10*c02*c21+c20*c01*c12-c20*c02*c11;
    //         
    //         return part1 + part2 + part3;
    //     }
    // }
    #endregion
}

public static class kint2
{
    public static readonly int2 one = new(1, 1);
    public static readonly int2 k00 = new(0, 0); public static readonly int2 k01 = new(0, 1); public static readonly int2 k02 = new(0, 2); public static readonly int2 k03 = new(0, 3);
    public static readonly int2 k10 = new(1, 0); public static readonly int2 k11 = new(1, 1); public static readonly int2 k12 = new(1, 2); public static readonly int2 k13 = new(1, 3);
    public static readonly int2 k20 = new(2, 0); public static readonly int2 k21 = new(2, 1); public static readonly int2 k22 = new(2, 2); public static readonly int2 k23 = new(2, 3);
    public static readonly int2 k30 = new(3, 0); public static readonly int2 k31 = new(3, 1); public static readonly int2 k32 = new(3, 2); public static readonly int2 k33 = new(3, 3);
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
    
    public static readonly float3 leftDownBack = new float3(-1, -1, -1);
    public static readonly float3 rightUpForward = new float3(1, 1, 1);
    public static readonly float3 min = (float3)float.MinValue ;
    public static readonly float3 max = (float3)float.MaxValue ;
}

public static class kfloat2
{
    public static readonly float2 one = new float2(1, 1);
    public static readonly float2 up = new float2(0, 1);
    public static readonly float2 down = new float2(0, -1);
    public static readonly float2 left = new float2(-1, 0);
    public static readonly float2 right = new float2(1, 0);
}