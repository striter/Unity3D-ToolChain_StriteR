using System;
using System.Collections.Generic;
using System.Numerics;

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