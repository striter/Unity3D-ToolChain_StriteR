using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;
    public Matrix3x3(float _00, float _01, float _02, float _10, float _11, float _12, float _20, float _21, float _22) { m00 = _00; m01 = _01; m02 = _02; m10 = _10; m11 = _11; m12 = _12; m20 = _20; m21 = _21; m22 = _22; }
    public Vector3 InvMultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m10 + _srcVector.z * m20,
        _srcVector.x * m01 + _srcVector.y * m11 + _srcVector.z * m21,
        _srcVector.x * m02 + _srcVector.y * m12 + _srcVector.z * m22);
    public Vector3 MultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m01 + _srcVector.z * m02,
        _srcVector.x * m10 + _srcVector.y * m11 + _srcVector.z * m12,
        _srcVector.x * m20 + _srcVector.y * m21 + _srcVector.z * m22);
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
    public void SetColumn(int _index, Vector3 column)
    {
        switch (_index)
        {
            default: throw new Exception("Invalid Column For Matrix3x3:" + _index.ToString());
            case 0: m00 = column.x; m10 = column.y; m20 = column.z; break;
            case 1: m01 = column.x; m11 = column.y; m21 = column.z; break;
            case 2: m02 = column.x; m12 = column.y; m22 = column.z; break;
        }
    }
    public static readonly Matrix3x3 identity = new Matrix3x3() { m00 = 0, m01 = 0, m02 = 0, m10 = 0, m11 = 0, m12 = 0, m20 = 0, m21 = 0, m22 = 0 };
    public static explicit operator Matrix3x3(Matrix4x4 _srcMatrix) => new Matrix3x3(_srcMatrix.m00, _srcMatrix.m01, _srcMatrix.m02, _srcMatrix.m10, _srcMatrix.m11, _srcMatrix.m12, _srcMatrix.m20, _srcMatrix.m21, _srcMatrix.m22);
}

[Serializable]
public struct Triangle
{
    public Vector3 m_Vertex1;
    public Vector3 m_Vertex2;
    public Vector3 m_Vertex3;
    public Vector3[] m_Verticies { get; private set; }
    public Vector3[] GetDrawLinesVerticies() => new Vector3[] { m_Vertex1, m_Vertex2, m_Vertex3, m_Vertex1 };
    public Vector3 this[int index]
    {
        get
        {
            switch (index)
            {
                default: Debug.LogError("Invalid Index:" + index); return m_Vertex1;
                case 0: return m_Vertex1;
                case 1: return m_Vertex2;
                case 2: return m_Vertex3;
            }
        }
    }
    public Triangle(Vector3[] _verticies) : this(_verticies[0], _verticies[1], _verticies[2])
    {
        Debug.Assert(_verticies.Length != 3, "Triangles' Vertices Count Must Equals 3!");
    }
    public Triangle(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3)
    {
        m_Vertex1 = _vertex1;
        m_Vertex2 = _vertex2;
        m_Vertex3 = _vertex3;
        m_Verticies = new Vector3[] { _vertex1, _vertex2, _vertex3 };
    }
}

[Serializable]
public struct DirectedTriangle
{
    public Triangle m_Triangle;
    public Vector3 m_UOffset => m_Triangle.m_Vertex2 - m_Triangle.m_Vertex1;
    public Vector3 m_VOffset => m_Triangle.m_Vertex3 - m_Triangle.m_Vertex1;
    public Vector3 m_Normal => Vector3.Cross(m_UOffset, m_VOffset);
    public Vector3 this[int index] => m_Triangle[index];
    public DirectedTriangle(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3)
    {
        m_Triangle = new Triangle(_vertex1, _vertex2, _vertex3);
    }
    public Vector3 GetUVPoint(Vector2 uv) => (1f - uv.x - uv.y) * m_Triangle.m_Vertex1 + uv.x * m_UOffset + uv.y * m_VOffset;
}
[Serializable]
public struct MeshPolygon
{
    public int m_Indice0 => m_Indices[0];
    public int m_Indice1 => m_Indices[1];
    public int m_Indice2 => m_Indices[2];
    public int[] m_Indices;
    public MeshPolygon(int _indice0, int _indice1, int _indice2)
    {
        m_Indices = new int[3] { _indice0, _indice1, _indice2 };
    }
    public Triangle GetTriangle(Vector3[] verticies) => new Triangle(verticies[m_Indice0], verticies[m_Indice1], verticies[m_Indice2]);
    public DirectedTriangle GetDirectedTriangle(Vector3[] verticies) => new DirectedTriangle(verticies[m_Indice0], verticies[m_Indice1], verticies[m_Indice2]);
}

[Serializable]
public struct DistancePlane
{
    public Vector3 m_Normal;
    public float m_Distance;
    public DistancePlane(Vector3 _normal, float _distance) { m_Normal = _normal; m_Distance = _distance; }
    public Matrix4x4 GetMirrorMatrix()
    {
        Matrix4x4 mirrorMatrix = Matrix4x4.identity;
        mirrorMatrix.m00 = 1 - 2 * m_Normal.x * m_Normal.x;
        mirrorMatrix.m01 = -2 * m_Normal.x * m_Normal.y;
        mirrorMatrix.m02 = -2 * m_Normal.x * m_Normal.z;
        mirrorMatrix.m03 = 2 * m_Normal.x * m_Distance;
        mirrorMatrix.m10 = -2 * m_Normal.x * m_Normal.y;
        mirrorMatrix.m11 = 1 - 2 * m_Normal.y * m_Normal.y;
        mirrorMatrix.m12 = -2 * m_Normal.y * m_Normal.z;
        mirrorMatrix.m13 = 2 * m_Normal.y * m_Distance;
        mirrorMatrix.m20 = -2 * m_Normal.x * m_Normal.z;
        mirrorMatrix.m21 = -2 * m_Normal.y * m_Normal.z;
        mirrorMatrix.m22 = 1 - 2 * m_Normal.z * m_Normal.z;
        mirrorMatrix.m23 = 2 * m_Normal.z * m_Distance;
        mirrorMatrix.m30 = 0;
        mirrorMatrix.m31 = 0;
        mirrorMatrix.m32 = 0;
        mirrorMatrix.m33 = 1;
        return mirrorMatrix;
    }
}
