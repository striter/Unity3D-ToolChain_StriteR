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
        public static readonly GTetrahedron kZero= new GTetrahedron(0,0,0,0);
        public static readonly GTetrahedron kDefault = new GTetrahedron(new float3(0.33f,-.25f,0.33f),new float3(-0.33f,-.25f,0.33f),new float3(0,-.25f,-0.33f),new float3(0,0.5f,0));
        public float3 Origin => center;
        public float3 this[int _index] => _index switch {
                0 => v0, 1 => v1, 2 => v2, 3 => v3,
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

        public void DrawGizmos()
        {
            Gizmos.color = Color.white.SetA(.5f);
            foreach (var triangle in GetTriangles())
            {
                triangle.DrawGizmos();
                UGizmos.DrawArrow(triangle.baryCentre,triangle.normal,.3f,.05f);
            }
        }

        public GTetrahedron GetSuperTetrahedron(IEnumerable<float3> _points)
        {
            UGeometry.Minmax(_points, out var min, out var max);
            var mid = (min + max) / 2f;
            
            var deltaMax = mid.max();
            var p1 = new float3(mid.x - 3f * deltaMax, mid.y - 3f * deltaMax, mid.z - 3f * deltaMax);
            var p2 = new float3(mid.x, mid.y + 20 * deltaMax, mid.z - 3f * deltaMax);
            var p3 = new float3(mid.x + 3f * deltaMax, mid.y - 3f * deltaMax, mid.z - 3f * deltaMax);
            var p4 = new float3(mid.x, mid.y, mid.z + 3f * deltaMax);
            return new GTetrahedron(p1, p2, p3, p4);
        }
    }
    
}