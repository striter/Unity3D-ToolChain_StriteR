using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct Matrix2x2
{
    public float m00, m01;
    public float m10, m11;
    public float determinant;
    public Matrix2x2(float _00,float _01,float _10,float _11)
    {
        m00 = _00;
        m01 = _01;
        m10 = _10;
        m11 = _11;
        determinant = m00 * m11 - m01 * m10;
    }

    public readonly (float x,float y) Multiply(float x, float y) => (
        x * m00 + y * m01,
        x * m10 + y * m11
    );

    public readonly (float x,float y) InvMultiply(float x, float y) => (
        x * m00 + y * m10,
        x * m01 + y * m11
    );

    public readonly (float x, float y) Multiply((float x, float y) float2) => Multiply(float2.x, float2.y);
    public readonly (float x, float y) InvMultiply((float x, float y) float2) => InvMultiply(float2.x, float2.y);

    public readonly Vector2 MultiplyVector(Vector2 _srcVector)
    {
        var float2=Multiply(_srcVector.x,_srcVector.y);
        return new Vector2(float2.x, float2.y);
    }

    public override string ToString()=>$"{m00} {m01}\n{m10} {m11}";
    public static Matrix2x2 Identity = new Matrix2x2(1f, 0f, 0f, 1f);
    public static implicit operator Matrix2x2(float2x2 _matrix) => new Matrix2x2(_matrix.c0.x, _matrix.c0.y, 
                                                                                _matrix.c1.x, _matrix.c1.y);
    public static implicit operator float2x2(Matrix2x2 _matrix) => new float2x2(_matrix.m00, _matrix.m01, 
                                                                                _matrix.m10, _matrix.m11);
}

[Serializable]
public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;
    public float determinant;
    public Matrix3x3(float _00, float _01, float _02,
        float _10, float _11, float _12,
        float _20, float _21, float _22)
    {
        m00 = _00; m01 = _01; m02 = _02; 
        m10 = _10; m11 = _11; m12 = _12; 
        m20 = _20; m21 = _21; m22 = _22;
        determinant = m00 * (m11 * m22 - m12 * m21) 
                    - m01 * (m10 * m22 - m12 * m20) 
                    + m02 * (m10 * m21 - m20 * m21);
    }

    public Vector3 MultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m01 + _srcVector.z * m02,
        _srcVector.x * m10 + _srcVector.y * m11 + _srcVector.z * m12,
        _srcVector.x * m20 + _srcVector.y * m21 + _srcVector.z * m22);    
    public Vector3 InvMultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m10 + _srcVector.z * m20,
        _srcVector.x * m01 + _srcVector.y * m11 + _srcVector.z * m21,
        _srcVector.x * m02 + _srcVector.y * m12 + _srcVector.z * m22);
    public static Vector3 operator *(Matrix3x3 _matrix, Vector3 _vector) => _matrix.MultiplyVector(_vector);
    public static Vector3 operator *(Vector3 _vector, Matrix3x3 matrix) => matrix.InvMultiplyVector(_vector);
    public void SetRow(int _index, Vector3 _row)
    {
        switch (_index)
        {
            default: throw new Exception("Invalid Row For Matrix3x3:" + _index.ToString());
            case 0: m00 = _row.x; m01 = _row.y; m02 = _row.z; break;
            case 1: m10 = _row.x; m11 = _row.y; m12 = _row.z; break;
            case 2: m20 = _row.x; m21 = _row.y; m22 = _row.z; break;
        }
    }
    public void SetColumn(int _index, Vector3 _column)
    {
        switch (_index)
        {
            default: throw new Exception("Invalid Column For Matrix3x3:" + _index.ToString());
            case 0: m00 = _column.x; m10 = _column.y; m20 = _column.z; break;
            case 1: m01 = _column.x; m11 = _column.y; m21 = _column.z; break;
            case 2: m02 = _column.x; m12 = _column.y; m22 = _column.z; break;
        }
    }
    public static readonly Matrix3x3 kZero = new Matrix3x3() { m00 = 0, m01 = 0, m02 = 0, m10 = 0, m11 = 0, m12 = 0, m20 = 0, m21 = 0, m22 = 0 };
    public static readonly Matrix3x3 kIdentity = new Matrix3x3() { m00 = 1, m01 = 0, m02 = 0, m10 = 0, m11 = 1, m12 = 0, m20 = 0, m21 = 0, m22 = 1 };
    public static explicit operator Matrix3x3(Matrix4x4 _srcMatrix) => new Matrix3x3(_srcMatrix.m00, _srcMatrix.m01, _srcMatrix.m02, _srcMatrix.m10, _srcMatrix.m11, _srcMatrix.m12, _srcMatrix.m20, _srcMatrix.m21, _srcMatrix.m22);
}

[Serializable]
public struct float3x2_homogenous
{
    public float2 c0, c1, c2;
    // 0,0,1
    public float3 Row0 => new float3(c0.x, c1.x, c2.x);
    public float3 Row1 => new float3(c0.y, c1.y, c2.y);
    //r2 [0 , 0 , 1]
    private float3x2_homogenous(float2 _c0,float2 _c1,float2 _c2) { c0 = _c0;  c1 = _c1; c2 = _c2;  }
    private float3x2_homogenous(float _00, float _01, float _02,
                                 float _10, float _11, float _12 ): this(
                                 new float2(_00, _10),
                                 new float2(_01, _11),
                                 new float2(_02, _12) ) { }


    public static float3x2_homogenous Translate(float2 _translate)
    {
        return new float3x2_homogenous(
           1,0 ,_translate.x,
           0,1,_translate.y
        );
    }
    
    public static float3x2_homogenous Scale(float2 _scale)
    {
        return new float3x2_homogenous(
           _scale.x,0 ,0,
           0,_scale.y,0
        );
    }

    public static float3x2_homogenous Rotate(float _angle)
    {
        var r = umath.Rotate2D(_angle);
        return new float3x2_homogenous(
            r.c0.x, r.c0.y ,0,
            r.c1.x, r.c1.y ,0
        );
    }
    
    public static float3x2_homogenous TRS(float2 _translate, float _angle, float2 _scale)=> Translate(_translate) * Rotate(_angle) * Scale(_scale);
    public float2 mulDirection(float2 _srcVector) => c0*_srcVector.x + c1*_srcVector.y;
    public float2 mulPosition(float2 _position) =>  c0*_position.x + c1*_position.y + c2;
    public static float3x2_homogenous operator *(float3x2_homogenous a, float3x2_homogenous b) => new(
        a.c0 * b.c0.x + a.c1 * b.c0.y ,
        a.c0 * b.c1.x + a.c1 * b.c1.y ,
        a.c0 * b.c2.x + a.c1 * b.c2.y + a.c2 );
    
    public static implicit operator float2x3(float3x2_homogenous _matrix) => new float2x3(_matrix.c0,_matrix.c1,_matrix.c2);
    public static implicit operator float3x2_homogenous(float3x3 _srcMatrix)=>new float3x2_homogenous(_srcMatrix.c0.xy,_srcMatrix.c1.xy,_srcMatrix.c2.xy);
    public static implicit operator float3x3(float3x2_homogenous _matrix) => new float3x3(_matrix.c0.to3xy(0),_matrix.c1.to3xy(0),_matrix.c2.to3xy(1));
    
    public float4x4 ToMatrix4x4XZ() => new float4x4(
            c0.x,0,c0.y,c2.x,
            0,1,0,0,
            c1.x,0,c1.y,c2.y,
            0,0,0,1 );
    
    public static implicit operator float3x2(float3x2_homogenous _homogenous)=> new float3x2(_homogenous.Row0,_homogenous.Row1);
}

[Serializable]
public struct float4x3_homogenous  //float3x4
{
    public float3 c0, c1, c2, c3;
    //r3 [0 , 0 , 0 , 1]
    
    private float4x3_homogenous(float3 _c0,float3 _c1,float3 _c2,float3 _c3) { c0 = _c0;  c1 = _c1; c2 = _c2; c3 = _c3; }
    private float4x3_homogenous( 
        float _00, float _01, float _02,float _03,
        float _10, float _11, float _12,float _13,
        float _20, float _21, float _22,float _23): this(
            new float3(_00, _10, _20),
            new float3(_01, _11, _21),
            new float3(_02, _12, _22), 
            new float3(_03, _13, _23)) { }

    public static float4x3_homogenous TS(float3 _t, float3 _s) => new float4x3_homogenous(
        _s.x,0f,0f,_t.x,
        0f,_s.y,0f,_t.y,
        0f,0f,_s.z,_t.z
    );

    public static float4x3_homogenous TRS(float3 _t, quaternion _q, float3 _s)
    {
        var r = new float3x3(_q);
        return new float4x3_homogenous(
                r.c0.x*_s.x, r.c0.y*_s.x ,r.c0.z*_s.x,_t.x,
                r.c1.x*_s.y, r.c1.y*_s.y ,r.c1.z*_s.y,_t.y,
                r.c2.x*_s.z, r.c2.y*_s.z ,r.c2.z*_s.z,_t.z
            );
    }

    public float3 mulDirection(float3 _srcVector) => c0*_srcVector.x + c1*_srcVector.y + c2*_srcVector.z;
    public float3 mulPosition(float3 _position) =>  c0*_position.x + c1*_position.y + c2*_position.z + c3;
    
    public static implicit operator float3x4(float4x3_homogenous _matrix) => new float3x4(_matrix.c0,_matrix.c1,_matrix.c2,_matrix.c3);
    public static implicit operator float4x3_homogenous(float4x4 _srcMatrix)=>new float4x3_homogenous(_srcMatrix.c0.xyz,_srcMatrix.c1.xyz,_srcMatrix.c2.xyz,_srcMatrix.c3.xyz);
}

[Serializable]
public struct float4x4_symmetric
{
    public float c0; public float2 c1; public float3 c2; public float4 c3;
    public float x00 => c0; public float x01 => c1.x; public float x02 => c2.x; public float x03 => c3.x;
    public float x10 => x01; public float x11 => c1.y; public float x12 => c2.y; public float x13 => c3.y;
    public float x20 => x02; public float x21 => x12; public float x22 => c2.z; public float x23 => c3.z;
    public float x30 => x03; public float x31 => x13; public float x32 => x23; public float x33 => c3.w;
    
    public float4x4_symmetric( float _m00, float _m01, float _m02, float _m03
                                         , float _m11, float _m12, float _m13
                                                     , float _m22, float _m23
                                                                 , float _m33)
    {
                             c0 = _m00; c1.x = _m01; c2.x = _m02; c3.x = _m03;
                                        c1.y = _m11; c2.y = _m12; c3.y = _m13;
                                                     c2.z = _m22; c3.z = _m23;
                                                                  c3.w = _m33;
    }

    public float this[int2 _index] => this[_index.x + _index.y * 4];
    public float this[int _index]
    {
        get
        {
            switch (_index)
            {
                default: throw new IndexOutOfRangeException();
                case 0 : return c0;   case 1 : return c1.x; case 2 : return c2.x; case 3 : return c3.x;
                case 4 : return c1.x; case 5 : return c2.y; case 6 : return c2.y; case 7 : return c3.y;
                case 8 : return c2.x; case 9 : return c2.y; case 10: return c2.z; case 11: return c3.z;
                case 12: return c3.x; case 13: return c3.y; case 14: return c3.z; case 15: return c3.w;
            }
        }
    }

    public float4 GetRow(int _index)
    {
        return _index switch
        {
            0 => new float4(c0, c1.x, c2.x, c3.x),
            1 => new float4(c1.x, c1.y, c2.y, c3.y),
            2 => new float4(c2.x, c2.y, c2.z, c3.z),
            3 => new float4(c3.x, c3.y, c3.z, c3.w),
            _ => throw new IndexOutOfRangeException()
        };
    }

    public float4 GetColumn(int _index) => GetRow(_index);
    
    public static float4x4_symmetric operator +(float4x4_symmetric _src, float4x4_symmetric _dst)=>new() {
            c0 = _src.c0 + _dst.c0,
            c1 = _src.c1 + _dst.c1,
            c2 = _src.c2 + _dst.c2,
            c3 = _src.c3 + _dst.c3,
        };

    public static float4 operator *(float4x4_symmetric _src, float4 _value) =>  
        _src.GetColumn(0) * _value.x + 
        _src.GetColumn(1) * _value.y + 
        _src.GetColumn(2) * _value.z + 
        _src.GetColumn(3) * _value.w;
    
    public static float4x4_symmetric operator *(float4x4_symmetric _src,float _value) => new float4x4_symmetric(_src.x00 * _value, _src.x01 * _value, _src.x02 * _value, _src.x03 * _value,
                                                                                                                _src.x11 * _value, _src.x12 * _value, _src.x13 * _value,
                                                                                                                _src.x22 * _value, _src.x23 * _value,
                                                                                                                _src.x33 * _value);
    
    public static implicit operator float4x4(float4x4_symmetric _src) => new float4x4(_src.GetColumn(0), _src.GetColumn(1), _src.GetColumn(2), _src.GetColumn(3));
    public static readonly float4x4_symmetric zero = default;
    
}

public static class matrix_extension
{
    public static float2 mul(this float2x2 _matrix, float2 _point) => math.mul(_matrix, _point);

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