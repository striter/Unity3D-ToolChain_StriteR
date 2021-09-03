using System;
using System.Collections.Generic;
using System.Linq;
using OSwizzling;
using UnityEngine;

namespace Geometry.Voxel
{
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
        public Vector3 end;
        public Vector3 GetPoint(float _distance) => origin + direction * _distance;
        public GLine(Vector3 _position, Vector3 _direction, float _length) { 
            origin = _position; 
            direction = _direction; 
            length = _length;
            end = origin + direction * length;
        }
        public static implicit operator GRay(GLine _line)=>new GRay(_line.origin,_line.direction);
    }

    [Serializable]
    public struct GTriangle:ITriangle<Vector3>,IIterate<Vector3>
    {
        public Vector3 vertex0 { get; set; }
        public Vector3 vertex1 { get; set; }
        public Vector3 vertex2 { get; set; }
        public Vector3 normal;
        public Vector3 uOffset;
        public Vector3 vOffset;
        public int Length => 3;
        public Vector3[] GetDrawLineVertices() => new Vector3[] { vertex0, vertex1, vertex2, vertex0 };
        public Vector3 this[int index] => GetElement(index);
        public Vector3 GetElement(int index)
        {
            switch (index)
            {
                default: Debug.LogError("Invalid Index:" + index); return vertex0;
                case 0: return vertex0;
                case 1: return vertex1;
                case 2: return vertex2;
            }
        }

        public GTriangle((Vector3 v0,Vector3 v1,Vector3 v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2)
        {
        }

        public GTriangle(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2):this()
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            var uv = this.GetUVDirection();
            uOffset = uv.u;
            vOffset = uv.v;
            normal= Vector3.Cross(uOffset,vOffset);
        }
        public Vector3 GetUVPoint(float u,float v)=>(1f - u - v) * vertex0 + u * uOffset + v * vOffset;
    }

    [Serializable]
    public struct GQuad
    {
        public Vector3[] vertices { get; private set; }
        public Vector3 vertex1 => vertices[0];
        public Vector3 vertex2=> vertices[1];
        public Vector3 vertex3=> vertices[2];
        public Vector3 vertex4=> vertices[3];
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
        public GQuad(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3,Vector3 _vertex4) : this(new[] { _vertex1, _vertex2, _vertex3,_vertex4 }) { }
        public GQuad(Vector3[] _vertices)
        {
            Debug.Assert(_vertices.Length == 4, "Quads' Vertices Count Must Equals 4!");
            vertices = _vertices;
        }
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
    public struct GPlane
    {
        public Vector3 normal;
        public float distance;
        public Vector3 Position => normal * distance;
        public GPlane(Vector3 _normal, float _distance) { normal = _normal; distance = _distance; }
        public GPlane(Vector3 _normal, Vector3 _position) : this(_normal, UGeometryVoxel.PointPlaneDistance(_position, new GPlane(_normal, 0))) { }
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
}