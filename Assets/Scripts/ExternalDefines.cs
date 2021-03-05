using System;
using System.Collections.Generic;
using UnityEngine;
public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;
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
            default: throw new Exception("Invalid Row For Matrix3x3:"+ _index.ToString());
            case 0: m00 = _row.x; m01 = _row.y; m02 = _row.z; break;
            case 1: m10 = _row.x; m11 = _row.y; m12 = _row.z; break;
            case 2: m20 = _row.x; m21 = _row.y; m22 = _row.z; break;
        }
    }
    public void SetColumn(int _index,Vector3 column)
    {
        switch(_index)
        {
            default: throw new Exception("Invalid Column For Matrix3x3:" + _index.ToString());
            case 0: m00 = column.x; m10 = column.y; m20 = column.z; break;
            case 1: m01 = column.x; m11 = column.y; m21 = column.z; break;
            case 2: m02 = column.x; m12 = column.y; m22 = column.z; break;
        }
    }
    public static readonly Matrix3x3 identity = new Matrix3x3() { m00=0,m01=0,m02=0,m10=0,m11=0,m12=0,m20=0,m21=0,m22=0};
}

[Serializable]
public struct RangeFloat
{
    public float start;
    public float length;
    public float end => start + length;
    public RangeFloat(float _start, float _length)
    {
        start = _start;
        length = _length;
    }
}

[Serializable]
public struct RangeInt
{

    public int start;
    public int length;
    public int end => start + length;
    public RangeInt(int _start, int _length)
    {
        start = _start;
        length = _length;
    }
}

[Serializable]
public struct Polygon
{
    public Vector3 m_Point1;
    public Vector3 m_Point2;
    public Vector3 m_Point3;
    public Polygon(Vector3 _point1, Vector3 _point2, Vector3 _point3)
    {
        m_Point1 = _point1;
        m_Point2 = _point2;
        m_Point3 = _point3;
    }
    public Polygon(Vector3[] _points) : this(_points[0], _points[1], _points[2])
    {
        if (_points.Length != 3)
            throw new Exception("Invalid Array Length To Construct Polygon!");
    }

    public Polygon(List<Vector3> _points) : this(_points[0], _points[1], _points[2])
    {
        if (_points.Count != 3)
            throw new Exception("Invalid Array Length To Construct Polygon!");
    }
}