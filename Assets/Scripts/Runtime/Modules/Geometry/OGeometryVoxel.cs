using System;
using System.Collections.Generic;
using System.Linq;
using OSwizzling;
using UnityEngine;

namespace Geometry.Voxel
{
    #region Enums
    [Flags]
    public enum EQubeCorner
    {
        DB=1,
        DL=2,
        DF=8,
        DR=16,
        
        TB=32,
        TL=64,
        TF=128,
        TR=256,
    }
    
    [Flags]
    public enum ECubeFacing
    {
        LF=1,
        FR=2,
        BL=4,
        RB=8,
        T=16,
        D=32,
    }
    #endregion
    
    #region Interface
    public interface IQube<T> where T : struct
    {
        T vDB { get; set; }
        T vDL { get; set; }
        T vDF { get; set; }
        T vDR { get; set; }
        T vTB { get; set; }
        T vTL { get; set; }
        T vTF { get; set; }
        T vTR { get; set; }
        T this[int _index] { get; }
        T this[EQubeCorner _corner] { get; }
    }
    public interface ICubeFace<T> where T : struct
    {
        public T fBL { get; set; }
        public T fLF { get; set; }
        public T fFR { get; set; }
        public T fRB { get; set; }
        public T fT { get; set; }
        public T fD { get; set; }
        T this[int _index] { get; }
        T this[ECubeFacing _facing] { get; }
    }
    #endregion
    
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
        public Vector3 GetElement(int _index)
        {
            switch (_index)
            {
                default: Debug.LogError("Invalid Index:" + _index); return vertex0;
                case 0: return vertex0;
                case 1: return vertex1;
                case 2: return vertex2;
            }
        }

        public GTriangle((Vector3 v0,Vector3 v1,Vector3 v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2)
        {
        }

        public GTriangle(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            uOffset = vertex1-vertex0;
            vOffset = vertex2-vertex0;
            normal= Vector3.Cross(uOffset,vOffset);
        }
        public Vector3 GetUVPoint(float u,float v)=>(1f - u - v) * vertex0 + u * uOffset + v * vOffset;
    }

    [Serializable]
    public struct GQuad:IQuad<Vector3>, IIterate<Vector3>
    {
        public Vector3 vB { get; set; }
        public Vector3 vL { get; set; }
        public Vector3 vF { get; set; }
        public Vector3 vR { get; set; }
        public Vector3 normal;

        public GQuad(Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr)
        {
            vB = _vb;
            vL = _vl;
            vF = _vf;
            vR = _vr;
            normal= Vector3.Cross(vL-vB,vF-vB);
        }

        public GQuad((Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr) _tuple) : this(_tuple._vb,_tuple._vl,_tuple._vf,_tuple._vr)
        {
        }

        public int Length => 4;
        public Vector3 this[int _index]=>this.GetVertex<GQuad,Vector3>(_index); 
        public Vector3 this[EQuadCorner _corner] =>this.GetVertex<GQuad,Vector3>(_corner);
        public Vector3 GetElement(int _index) => this[_index];
        public static GQuad operator +(GQuad _src, Vector3 _offset) => new GQuad(_src.vB+_offset,_src.vL+_offset,_src.vF+_offset,_src.vR+_offset);
    }

    [Serializable]
    public struct GQube:IQube<Vector3>,IIterate<Vector3>
    {
        public Vector3 vDB { get; set; }
        public Vector3 vDL { get; set; }
        public Vector3 vDF { get; set; }
        public Vector3 vDR { get; set; }
        public Vector3 vTB { get; set; }
        public Vector3 vTL { get; set; }
        public Vector3 vTF { get; set; }
        public Vector3 vTR { get; set; }
        
        public GQube(Vector3 _vertDB, Vector3 _vertDL, Vector3 _vertDF, Vector3 _vertDR,
            Vector3 _vertTB, Vector3 _vertTL, Vector3 _vertTF, Vector3 _vertTR)
        {
            vDB = _vertDB;
            vDL = _vertDL;
            vDF = _vertDF;
            vDR = _vertDR;
            vTB = _vertTB;
            vTL = _vertTL;
            vTF = _vertTF;
            vTR = _vertTR;
        }

        public Vector3 this[int _index] => this.GetVertex<GQube,Vector3>(_index);
        public Vector3 this[EQubeCorner _corner] => this.GetVertex<GQube,Vector3>(_corner);
        public Vector3 GetElement(int _index) => this[_index];
        public int Length => 8;
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
        public GPlane(Vector3 _normal, Vector3 _position) : this(_normal, UGeometryIntersect.PointPlaneDistance(_position, new GPlane(_normal, 0))) { }
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
    #endregion
    
}