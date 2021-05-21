using System;
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
    }
}

[Serializable]
public struct GMeshPolygon
{
    public int indice0 => indices[0];
    public int indice1 => indices[1];
    public int indice2 => indices[2];
    public int[] indices;
    public GMeshPolygon(int _indice0, int _indice1, int _indice2) { indices = new int[3] { _indice0, _indice1, _indice2 }; }
    public GTriangle GetTriangle(Vector3[] verticies) => new GTriangle(verticies[indice0], verticies[indice1], verticies[indice2]);
    public GDirectedTriangle GetDirectedTriangle(Vector3[] verticies) => new GDirectedTriangle(verticies[indice0], verticies[indice1], verticies[indice2]);
}

[Serializable]
public struct GDirectedTriangle
{
    public GTriangle triangle;
    public Vector3 UOffset => triangle.vertex2 - triangle.vertex1;
    public Vector3 VOffset => triangle.vertex3 - triangle.vertex1;
    public Vector3 normal => Vector3.Cross(UOffset, VOffset);
    public Vector3 this[int index] => triangle[index];
    public GDirectedTriangle(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3) { triangle = new GTriangle(_vertex1, _vertex2, _vertex3); }
    public Vector3 GetUVPoint(Vector2 uv) => (1f - uv.x - uv.y) * triangle.vertex1 + uv.x * UOffset + uv.y * VOffset;
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
    public GRay m_Ray;
    [Range(0, 180)] public float angle;
    public Vector3 origin => m_Ray.origin;
    public Vector3 normal => m_Ray.direction;
    public GCone(GRay _directedPosition, float _angle) { m_Ray = _directedPosition; angle = _angle; }
    public GCone(Vector3 _origin, Vector3 _direction, float _angle) : this(new GRay(_origin, _direction), _angle) { }
    public float GetRadius(float _height) => _height * Mathf.Tan(UMath.AngleToRadin(angle));
}

[Serializable]
public struct GHeightCone
{
    public GCone cone;
    public float height;
    public float angle => cone.angle;
    public Vector3 origin => cone.origin;
    public Vector3 normal => cone.normal;
    public float Radius => cone.GetRadius(height);
    public Vector3 Bottom => origin + normal * height;
    public GHeightCone(GCone _cone, float _height) { cone = _cone; height = _height; }
    public GHeightCone(Vector3 _origin, Vector3 _direction, float _radin, float _height) : this(new GCone(_origin, _direction, _radin), _height) { }
}