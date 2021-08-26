using System;
using System.Collections.Generic;
using System.Linq;
using OSwizzling;
using UnityEngine;
[Serializable]
public struct GRay
{
    public Vector3 origin;
    public Vector3 direction;
    public GRay(Vector3 _position, Vector3 _direction) { origin = _position; direction = _direction; }
    public Vector3 GetPoint(float _distance) => origin + direction * _distance;
    public static implicit operator Ray(GRay _ray)=>new Ray(_ray.origin,_ray.direction);
    public static implicit operator GRay(Ray _ray)=>new GRay(_ray.origin,_ray.direction);
    public GLine ToLine(float _length)=>new GLine(origin,direction,_length);
}
[Serializable]
public struct GLine
{
    public Vector3 origin;
    public Vector3 direction;
    public float length;
    public Vector3 End => origin + direction * length;
    public Vector3 GetPoint(float _distance) => origin + direction * _distance;
    public GLine(Vector3 _position, Vector3 _direction, float _length) { origin = _position; direction = _direction; length = _length; }
    public static implicit operator GRay(GLine _line)=>new GRay(_line.origin,_line.direction);
}

[Serializable]
public struct GSphere
{
    public Vector3 center;
    public float radius;
    public GSphere(Vector3 _center,float _radius) { center = _center;radius = _radius; }
}
[Serializable]
public struct GBox
{
    public Vector3 center;
    public Vector3 extend;
    public Vector3 Size => extend * 2f;
    public Vector3 Min => center - extend;
    public Vector3 Max => center + extend;
    public GBox(Vector3 _center,Vector3 _extend) { center = _center;extend = _extend; }
}

[Serializable]
public struct GTriangle
{
    public Vector3 vertex1;
    public Vector3 vertex2;
    public Vector3 vertex3;
    public Vector3 normal;
    public Vector3 uOffset;
    public Vector3 vOffset;
    public Vector3[] verticies { get; private set; }
    public Vector3[] GetDrawLinesVerticies() => new Vector3[] { vertex1, vertex2, vertex3, vertex1 };
    public Vector3 this[int index]
    {
        get
        {
            switch (index)
            {
                default: Debug.LogError("Invalid Index:" + index); return vertex1;
                case 0: return vertex1;
                case 1: return vertex2;
                case 2: return vertex3;
            }
        }
    }
    public GTriangle(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3) : this(new Vector3[] { _vertex1, _vertex2, _vertex3 }) { }
    public GTriangle(Vector3[] _verticies)
    {
        Debug.Assert(_verticies.Length == 3, "Triangles' Vertices Count Must Equals 3!");
        vertex1 = _verticies[0];
        vertex2 = _verticies[1];
        vertex3 = _verticies[2];
        verticies = _verticies;
        uOffset = vertex2 - vertex1;
        vOffset = vertex3 - vertex1;
        normal= Vector3.Cross(uOffset,vOffset);
    }

    public Vector3 GetUVPoint(float u,float v)=>(1f - u - v) * vertex1 + u * uOffset + v * vOffset;
}
[Serializable]
public struct GMeshTriangle
{
    public int index0 => indices[0];
    public int index1 => indices[1];
    public int index2 => indices[2];
    public int[] indices;
    public GMeshTriangle(int _index0, int _index1, int _index2) { indices = new int[3] { _index0, _index1, _index2 }; }

    public Vector3[] GetVertices(Vector3[] container) => new[] {  container[index0], container[index1], container[index2]};
    public Vector3[] GetVertices(List<Vector3> container) => new[] {  container[index0], container[index1], container[index2]};
    public Vector3[] GetVertices<T>(List<T> container, Func<T, Vector3> _getVertex) => new[] { _getVertex( container[index0]), _getVertex(container[index1]),_getVertex( container[index2])};
    public GTriangle GetTriangle(List<Vector3> _vertices) => new GTriangle(_vertices[index0],_vertices[index1],_vertices[index2]);
    public GTriangle GetTriangle(Vector3[] _vertices) => new GTriangle(_vertices[index0],_vertices[index1],_vertices[index2]);
}

[Serializable]
public struct GQuad
{
    public Vector3 vertex1 => vertices[0];
    public Vector3 vertex2=> vertices[1];
    public Vector3 vertex3=> vertices[2];
    public Vector3 vertex4=> vertices[3];
    public Vector3[] vertices { get; private set; }
    public Vector3 this[int index]
    {
        get
        {
            switch (index)
            {
                default: Debug.LogError("Invalid Index:" + index); return vertex1;
                case 0: return vertex1;
                case 1: return vertex2;
                case 2: return vertex3;
                case 3: return vertex4;
            }
        }
    }
    public GQuad(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3,Vector3 _vertex4) : this(new Vector3[] { _vertex1, _vertex2, _vertex3,_vertex4 }) { }
    public GQuad(Vector3[] _vertices)
    {
        Debug.Assert(_vertices.Length == 4, "Quads' Vertices Count Must Equals 4!");
        vertices = _vertices;
    }
}

[Serializable]
public struct GMeshQuad
{
    public int index0 => indices[0];
    public int index1 => indices[1];
    public int index2 => indices[2];
    public int index3 => indices[3];
    public int[] indices;
    public GMeshQuad(int _index0, int _index1, int _index2,int _index3) { indices = new int[4] { _index0, _index1, _index2 ,_index3}; }

    public Vector3[] GetVertices(Vector3[] container) => new[] {  container[index0], container[index1], container[index2],container[index3]};
    public Vector3[] GetVertices(List<Vector3> container) => new[] {  container[index0], container[index1], container[index2],container[index3]};
    public Vector3[] GetVertices<T>(List<T> container, Func<T, Vector3> _getVertex) => new[] { _getVertex( container[index0]), _getVertex(container[index1]),_getVertex( container[index2]),_getVertex( container[index3])};
    
    public GQuad GetQuad(List<Vector3> _vertices) => new GQuad(_vertices[index0],_vertices[index1],_vertices[index2],_vertices[index3]);
    public GQuad GetQuad(Vector3[] _vertices) => new GQuad(_vertices[index0],_vertices[index1],_vertices[index2],_vertices[index3]);
}

[Serializable]
public struct GPlane
{
    public Vector3 normal;
    public float distance;
    public Vector3 Position => normal * distance;
    public GPlane(Vector3 _normal, float _distance) { normal = _normal; distance = _distance; }
    public GPlane(Vector3 _normal, Vector3 _position) : this(_normal, UGeometry.PointPlaneDistance(_position, new GPlane(_normal, 0))) { }
}
[Serializable]
public struct GCone
{
    public Vector3 origin;
    public Vector3 normal;
    [Range(0, 180)] public float angle;
    public GCone(Vector3 _origin, Vector3 _normal, float _angle) { origin = _origin;normal = _normal;angle = _angle; }
    public float GetRadius(float _height) => _height * Mathf.Tan(angle*UMath.Rad2Deg);
}

[Serializable]
public struct GHeightCone
{
    public Vector3 origin;
    public Vector3 normal;
    [Range(0,180)]public float angle;
    public float height;
    public float Radius => ((GCone)this).GetRadius(height);
    public Vector3 Bottom => origin + normal * height;
    public GHeightCone(Vector3 _origin, Vector3 _normal, float _radin, float _height)
    {
        origin = _origin;
        normal =_normal;
        angle = _radin;
        height = _height;
    }
    public static implicit operator GCone(GHeightCone _cone)=>new GCone(_cone.origin,_cone.normal,_cone.angle);
}