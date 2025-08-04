using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    using static math;
    public partial struct GTetrahedron 
    {
        public float3 v0, v1, v2, v3;
        [HideInInspector] public float3 center;
        [HideInInspector] public GTriangle t0, t1, t2, t3;
        public GTetrahedron(float3 _v0, float3 _v1, float3 _v2, float3 _v3)
        {
            this = default;
            v0 = _v0;
            v1 = _v1;
            v2 = _v2;
            v3 = _v3;
            Ctor();
        }
        GTetrahedron Ctor()
        {
            center = GetBoundingSphere().center;
            t0 = new GTriangle(v0, v1, v2);
            t1 = new GTriangle(v2, v1, v3);
            t2 = new GTriangle(v2, v3, v0);
            t3 = new GTriangle(v3, v1, v0);
            return this;
        }
    }

    public partial struct GTetrahedron : IComplex , IEnumerable<float3> , ISerializationCallbackReceiver , ISDF
    {
        public static readonly GTetrahedron kDefault = new GTetrahedron(new float3(0.33f,-.25f,0.33f),new float3(-0.33f,-.25f,0.33f),new float3(0,-.25f,-0.33f),new float3(0,0.5f,0));

        public GTetrahedron(IList<float3> _vertices, PTetrahedron _tetrahedron):this(_vertices[_tetrahedron.v0],_vertices[_tetrahedron.v1],_vertices[_tetrahedron.v2],_vertices[_tetrahedron.v3]){}
        
        public float3 Origin => center;
        public float3 this[int _index] => _index switch {
                0 => v0, 1 => v1, 2 => v2, 3 => v3,
                _ => throw new IndexOutOfRangeException()
            };

        public GTriangle GetTriangle(int _index) => _index switch {
            0 => t0, 1 => t1, 2 => t2, 3 => t3,
            _ => throw new IndexOutOfRangeException()
        };

        public float3 GetSupportPoint(float3 _direction) => this.MaxElement(_p => math.dot(_p, _direction));

        public GBox GetBoundingBox() => GBox.GetBoundingBox(this);

        public GSphere GetBoundingSphere() => GSphere.Tetrahedron(v0,v1,v2,v3);

        public void OnBeforeSerialize(){}

        public void OnAfterDeserialize() => Ctor();

        public IEnumerable<GTriangle> GetTriangles()
        {
            yield return t0;
            yield return t1;
            yield return t2;
            yield return t3;
        }
        
        public IEnumerable<GPlane> GetPlanes() => GetTriangles().Select(t => t.GetPlane());
        public IEnumerator<float3> GetEnumerator()
        {
            yield return v0;
            yield return v1;
            yield return v2;
            yield return v3;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public GTetrahedron Expand(float _distance)
        {
            var v0 = this.v0;
            var v1 = this.v1;
            var v2 = this.v2;
            var v3 = this.v3;
            var center = this.center;
            v0 += (v0 - center).normalize() * _distance;
            v1 += (v1 - center).normalize() * _distance;
            v2 += (v2 - center).normalize() * _distance;
            v3 += (v3 - center).normalize() * _distance;
            return new GTetrahedron(v0, v1, v2, v3).Ctor();
        }
        public float SDF(float3 _position)
        {
            var sdfs = new float4(t0.GetPlane().SDF(_position), t1.GetPlane().SDF(_position), t2.GetPlane().SDF(_position), t3.GetPlane().SDF(_position));
            var inside = (sdfs < 0f).all();
            if (inside)
                return sdfs.min();

            var max = float.MinValue;
            for(var i = 0; i < 4; i++)
                if(sdfs[i] > 0)
                    max = math.max(max, sdfs[i]);
            return max;
        }

        public void DrawGizmos() => DrawGizmos(false);

        public void DrawGizmos(bool _arrow)
        {
            Gizmos.color = Color.white.SetA(.5f);
            foreach (var triangle in GetTriangles())
            {
                triangle.DrawGizmos();
                if (_arrow)
                    UGizmos.DrawArrow(triangle.baryCentre,triangle.normal,.3f,.05f);
            }
        }

        public static GTetrahedron GetSuperTetrahedron(IEnumerable<float3> _points,float _extension = .1f)
        {
            UGeometry.Minmax(_points, out var min, out var max);
            var delta = max - min;
            var v0 = min;
            delta *= 3f;
            var v1 = new float3(min.x, min.x + delta.y, min.z);
            var v2 = new float3(min.x + delta.x, min.y, min.z);
            var v3 = new float3(min.x, min.y, min.x + delta.z);
            return new GTetrahedron(v0, v1, v2, v3).Expand(_extension);
        }
    }
    
}