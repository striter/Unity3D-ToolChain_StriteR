using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GSpaceData
{
    public Vector3 m_Position;
    public Vector3 m_Direction;
    public GSpaceData(Vector3 _position, Vector3 _direction) { m_Position = _position; m_Direction = _direction; }
}
[Serializable]
public struct GMeshPolygon
{
    public int m_Indice0 => m_Indices[0];
    public int m_Indice1 => m_Indices[1];
    public int m_Indice2 => m_Indices[2];
    public int[] m_Indices;
    public GMeshPolygon(int _indice0, int _indice1, int _indice2)
    {
        m_Indices = new int[3] { _indice0, _indice1, _indice2 };
    }
    public GTriangle GetTriangle(Vector3[] verticies) => new GTriangle(verticies[m_Indice0], verticies[m_Indice1], verticies[m_Indice2]);
    public GDirectedTriangle GetDirectedTriangle(Vector3[] verticies) => new GDirectedTriangle(verticies[m_Indice0], verticies[m_Indice1], verticies[m_Indice2]);
}

[Serializable]
public struct GTriangle
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
    public GTriangle(Vector3[] _verticies) : this(_verticies[0], _verticies[1], _verticies[2])
    {
        Debug.Assert(_verticies.Length != 3, "Triangles' Vertices Count Must Equals 3!");
    }
    public GTriangle(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3)
    {
        m_Vertex1 = _vertex1;
        m_Vertex2 = _vertex2;
        m_Vertex3 = _vertex3;
        m_Verticies = new Vector3[] { _vertex1, _vertex2, _vertex3 };
    }
}
[Serializable]
public struct GDirectedTriangle
{
    public GTriangle m_Triangle;
    public Vector3 m_UOffset => m_Triangle.m_Vertex2 - m_Triangle.m_Vertex1;
    public Vector3 m_VOffset => m_Triangle.m_Vertex3 - m_Triangle.m_Vertex1;
    public Vector3 m_Normal => Vector3.Cross(m_UOffset, m_VOffset);
    public Vector3 this[int index] => m_Triangle[index];
    public GDirectedTriangle(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3) { m_Triangle = new GTriangle(_vertex1, _vertex2, _vertex3); }
    public Vector3 GetUVPoint(Vector2 uv) => (1f - uv.x - uv.y) * m_Triangle.m_Vertex1 + uv.x * m_UOffset + uv.y * m_VOffset;
}
[Serializable]
public struct GPlane
{
    public Vector3 m_Normal;
    public float m_Distance;
    public GPlane(Vector3 _normal, float _distance) { m_Normal = _normal; m_Distance = _distance; }
}
[Serializable]
public struct GCone
{
    public GSpaceData m_DirectedPosition;
    [Range(0,180)]public float m_Angle;
    public Vector3 m_Origin => m_DirectedPosition.m_Position;
    public Vector3 m_Normal => m_DirectedPosition.m_Direction;
    public GCone(GSpaceData _directedPosition, float _angle) { m_DirectedPosition = _directedPosition; m_Angle = _angle; }
    public GCone(Vector3 _origin, Vector3 _direction, float _angle) : this(new GSpaceData(_origin, _direction), _angle) { }
    public float GetRadius(float _height) => _height * Mathf.Tan(UMath.AngleToRadin(m_Angle));
}

[Serializable]
public struct GHeightCone
{
    public GCone m_Cone;
    public float m_Height;
    public float m_Angle => m_Cone.m_Angle;
    public Vector3 m_Origin => m_Cone.m_Origin;
    public Vector3 m_Normal => m_Cone.m_Normal;
    public float m_Radius =>  m_Cone.GetRadius(m_Height);
    public GHeightCone(GCone _cone,float _height) { m_Cone = _cone;m_Height = _height; }
    public GHeightCone(Vector3 _origin, Vector3 _direction, float _radin, float _height) : this(new GCone(_origin, _direction, _radin), _height) { }
}