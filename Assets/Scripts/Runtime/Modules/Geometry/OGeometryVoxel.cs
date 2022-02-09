using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OSwizzling;
using UnityEngine;

namespace Geometry.Voxel
{
    #region Defines
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
    public struct GQuad : IQuad<Vector3>, IIterate<Vector3>
    {
        public Quad<Vector3> quad;
        public Vector3 normal;
        public GQuad(Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr)
        {
            quad = new Quad<Vector3>(_vb, _vl, _vf, _vr);
            normal = Vector3.Cross(_vl - _vb, _vr - _vb);
        }

        public GQuad((Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr) _tuple) : this(_tuple._vb, _tuple._vl,
            _tuple._vf, _tuple._vr)
        {
        }

        public Vector3 this[int _index] => quad[_index];
        public Vector3 this[EQuadCorner _corner] => quad[_corner];
        public Vector3 B => quad.B;
        public Vector3 L => quad.L;
        public Vector3 F => quad.F;
        public Vector3 R => quad.R;
        public int Length => quad.Length;

        public static GQuad operator +(GQuad _src, Vector3 _dst)=> new GQuad(_src.B + _dst, _src.L + _dst, _src.F + _dst,_src.R+_dst);
        public static GQuad operator -(GQuad _src, Vector3 _dst)=> new GQuad(_src.B - _dst, _src.L - _dst, _src.F - _dst,_src.R-_dst);
    }

    [Serializable]
    public struct GTriangle:ITriangle<Vector3>, IIterate<Vector3>
    {
        public Triangle<Vector3> triangle;
        public Vector3 normal;
        public Vector3 uOffset;
        public Vector3 vOffset;
        public int Length => 3;
        public Vector3 this[int index] => triangle[index];
        public Vector3 V0 => triangle.v0;
        public Vector3 V1 => triangle.v1;
        public Vector3 V2 => triangle.v2;
        public GTriangle((Vector3 v0,Vector3 v1,Vector3 v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2)
        {
        }

        public GTriangle(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2)
        {
            triangle = new Triangle<Vector3>(_vertex0, _vertex1, _vertex2);
            uOffset = _vertex1-_vertex0;
            vOffset = _vertex2-_vertex0;
            normal= Vector3.Cross(uOffset,vOffset);
        }
        public Vector3 GetUVPoint(float u,float v)=>(1f - u - v) * this[0] + u * uOffset + v * vOffset;

        public static GTriangle operator +(GTriangle _src, Vector3 _dst)=> new GTriangle(_src.V0 + _dst, _src.V1 + _dst, _src.V2 + _dst);
        public static GTriangle operator -(GTriangle _src, Vector3 _dst)=> new GTriangle(_src.V0 - _dst, _src.V1 - _dst, _src.V2 - _dst);
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
        public static GBox GBoxCtor_MinMax(Vector3 _min, Vector3 _max)
        {
            Vector3 size = _max - _min;
            Vector3 extend = size / 2;
            return new GBox(_min+extend,extend);
        }
        public void DrawGizmos()=>Gizmos.DrawWireCube(center,Size);
    }


    [Serializable]
    public struct GPlane
    {
        public Vector3 normal;
        public float distance;
        public Vector3 position;
        public GPlane(Vector3 _normal, float _distance) 
        { 
            normal = _normal;
            distance = _distance;
            position = _normal * distance;
        }

        public GPlane(Vector3 _normal, Vector3 _position)
        {
            normal = _normal;
            position = _position;
            distance = UGeometryIntersect.PointPlaneDistance(_position, new GPlane(_normal, 0));
        }
        
        public static implicit operator Vector4(GPlane _plane)=>_plane.normal.ToVector4(_plane.distance);
    }
    [Serializable]
    public struct GCone
    {
        public Vector3 origin;
        public Vector3 normal;
        [Range(0, 90f)] public float angle;
        public GCone(Vector3 _origin, Vector3 _normal, float _angle) { origin = _origin;normal = _normal;angle = _angle; }
        public float GetRadius(float _height) => _height * Mathf.Tan(angle*UMath.Deg2Rad);
    }

    [Serializable]
    public struct GHeightCone
    {
        public Vector3 origin;
        public Vector3 normal;
        [Range(0,90f)]public float angle;
        public float height;
        public float Radius => ((GCone)this).GetRadius(height);
        public Vector3 Bottom => origin + normal * height;
        public GHeightCone(Vector3 _origin, Vector3 _normal, float _angle, float _height)
        {
            origin = _origin;
            normal =_normal;
            angle = _angle;
            height = _height;
        }
        public static implicit operator GCone(GHeightCone _cone)=>new GCone(_cone.origin,_cone.normal,_cone.angle);
    }


    [Serializable]
    public struct GFrustum
    {
        [Clamp(0)]public float fov;
        public float aspect;
        [Clamp(0)]public float zNear;
        [Clamp(0)]public float zFar;
        public GFrustum(Camera _camera)
        {
            fov = _camera.fieldOfView;
            aspect = _camera.aspect;
            zNear = _camera.nearClipPlane;
            zFar = _camera.farClipPlane;
        }
        
        public GFrustumPlanes GetFrustumPlanes()
        {
            float an = fov * .5f  * UMath.Deg2Rad;
            float s = Mathf.Sin(an);
            float c = Mathf.Cos(an);
            float aspectC = c / aspect;

            Vector3 forward = new Vector3(0f, 0f, 1f);
            float centerDistance = zNear + (zFar-zNear)/2f;
            return new GFrustumPlanes
            {
                left = new GPlane( new Vector3(-aspectC , 0f,-s  ), new Vector3(-s,0f,aspectC).normalized*centerDistance),
                right = new GPlane( new Vector3(aspectC, 0f, -s ), new Vector3(s,0f,aspectC).normalized*centerDistance),
                top = new GPlane( new Vector3(0f, c, -s), new Vector3(0f,s,c).normalized*centerDistance),
                bottom = new GPlane( new Vector3(0f, -c, -s), new Vector3(0f,-s,c).normalized*centerDistance),
                near = new GPlane(-forward, -zNear),
                far = new GPlane(forward, zFar),
            };
        }
        public GFrustumRays GetFrustumRays(Vector3 _translate=default, Quaternion rotation=default)
        {
            float halfHeight = zNear * Mathf.Tan(fov * .5f * Mathf.Deg2Rad);
            Vector3 forward = rotation*Vector3.forward;
            Vector3 toRight = rotation*Vector3.right * halfHeight * aspect;
            Vector3 toTop = rotation*Vector3.up * halfHeight;

            Vector3 tl = forward * zNear + toTop - toRight;
            float scale = tl.magnitude / zNear;
            tl.Normalize();
            tl *= scale;
            Vector3 tr = forward * zNear + toTop + toRight;
            tr.Normalize();
            tr *= scale;
            Vector3 bl = forward * zNear - toTop - toRight;
            bl.Normalize();
            bl *= scale;
            Vector3 br = forward * zNear - toTop + toRight;
            br.Normalize();
            br *= scale;

            return new GFrustumRays()
            {
                topLeft = new GRay(_translate+tl*zNear,tl),
                topRight = new GRay(_translate+tr*zNear,tr),
                bottomLeft =  new GRay(_translate+bl*zNear,bl) ,
                bottomRight = new GRay(_translate+br*zNear,br),
                farDistance=zFar-zNear
            };
        }
    }
    public struct GFrustumPlanes:IEnumerable<GPlane>
    {
        public GPlane left;
        public GPlane right;
        public GPlane top;
        public GPlane bottom;
        public GPlane near;
        public GPlane far;

        public IEnumerator<GPlane> GetEnumerator()
        {
            yield return left;
            yield return right;
            yield return top;
            yield return bottom;
            yield return near;
            yield return far;
        }
        
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    }

    public struct GFrustumRays:IEnumerable<GRay>
    {
        public GRay bottomLeft;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        public GRay bottomRight;
        public GRay topRight;
        public GRay topLeft;
        public float farDistance;

        public IEnumerator<GRay> GetEnumerator()
        {
            yield return bottomLeft;
            yield return bottomRight;
            yield return topRight;
            yield return topLeft;
        }

        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }
    
    #endregion
}