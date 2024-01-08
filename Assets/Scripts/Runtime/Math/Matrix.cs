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
    public float c0;
    public float2 c1;
    public float3 c2;
    public float4 c3;

    public float4x4_symmetric( float _m01, float _m02, float _m03, float _m04
                                         , float _m12, float _m13, float _m14
                                                     , float _m23, float _m24
                                                                 , float _m34
    )
    {
                             c0 = _m01; c1.x = _m02; c2.x = _m03; c3.x = _m04;
                                        c1.y = _m12; c2.y = _m13; c3.y = _m14;
                                                     c2.z = _m23; c3.z = _m24;
                                                                  c3.w = _m34;
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

    public float Index(int _value)
    {
        return _value switch
        {
            0 => c0, 1 => c1.x, 2 => c2.x, 3 => c3.x,
                    4 => c1.y, 5 => c2.y, 6 => c3.y,
                                7 => c2.z, 8 => c3.z,
                                            9 => c3.w,
            _ => throw new IndexOutOfRangeException()
        };
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

    public float determinant(int _a00, int _a01, int _a02, 
                             int _a10, int _a11, int _a12, 
                             int _a20, int _a21, int _a22)
    {
        return Index(_a00) * Index(_a11) * Index(_a22) + Index(_a01) * Index(_a12) * Index(_a20) + Index(_a02) * Index(_a10) * Index(_a21) 
               - Index(_a02) * Index(_a11) * Index(_a20) - Index(_a01) * Index(_a10) * Index(_a22) - Index(_a00) * Index(_a12) * Index(_a21);
    }

    public static float4 operator *(float4x4_symmetric _src, float4 _value) =>  
        _src.GetColumn(0) * _value.x + 
        _src.GetColumn(1) * _value.y + 
        _src.GetColumn(2) * _value.z + 
        _src.GetColumn(3) * _value.w;

    public static readonly float4x4_symmetric zero = default;
}

public static class matrix_extension
{
    public static float2 mul(this float2x2 _matrix, float2 _point) => math.mul(_matrix, _point);
}