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
    public partial struct GQuad
    {
        public Quad<float3> quad;
        // public Vector3 normal;
        // public float area;
        public GQuad(Quad<float3> _quad)
        {
            quad = _quad;
        }

        public static readonly GQuad kDefault = (GQuad)KQuad.k3SquareCenteredUpward;
    }
    
    [Serializable]
    public partial struct GQuad : IQuad<float3>,IIterate<float3>,IVolume  , IConvex , IRayIntersection
    {
        public GQuad(float3 _vb, float3 _vl, float3 _vf, float3 _vr):this(new Quad<float3>(_vb,_vl,_vf,_vr)){}
        public GQuad((float3 _vb, float3 _vl, float3 _vf, float3 _vr) _tuple) : this(_tuple._vb, _tuple._vl, _tuple._vf, _tuple._vr) { }
        public GQuad(IList<float3> _points) : this(new Quad<float3>(_points[0], _points[1], _points[2], _points[3])) { }
        public static explicit operator GQuad(Quad<float3> _src) => new GQuad(_src);
        
        public float3 this[int _index] => quad[_index];
        public float3 this[EQuadCorner _corner] => quad[_corner];
        public float3 B => quad.B;
        public float3 L => quad.L;
        public float3 F => quad.F;
        public float3 R => quad.R;
        public int Length => quad.Length;

        public static GQuad operator +(GQuad _src, float3 _dst)=> new GQuad(_src.B + _dst, _src.L + _dst, _src.F + _dst,_src.R+_dst);
        public static GQuad operator -(GQuad _src, float3 _dst)=> new GQuad(_src.B - _dst, _src.L - _dst, _src.F - _dst,_src.R-_dst);
        public float3 GetPoint(float2 _uv)=>umath.bilinearLerp(B, L, F, R, _uv);
        public float3 GetSupportPoint(float3 _direction) => quad.MaxElement(p => math.dot(p, _direction));
        public GBox GetBoundingBox() => UGeometry.GetBoundingBox(this);
        public GSphere GetBoundingSphere() => UGeometry.GetBoundingSphere(this);
        public float3 Origin => quad.Average();

        public IEnumerator<float3> GetEnumerator()
        {
            yield return quad.B;
            yield return quad.L;
            yield return quad.F;
            yield return quad.R;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public void GetTriangles(out GTriangle _triangle1, out GTriangle _triangle2)
        {
            _triangle1 = new GTriangle(B, L, F);
            _triangle2 = new GTriangle(B, F, R);
        }

        public IEnumerable<GTriangle> GetTriangles()
        {
            yield return new GTriangle(B, L, F);
            yield return new GTriangle(B, F, R);
        }

        public IEnumerable<GLine> GetEdges()
        {
            yield return new GLine(quad.B, quad.L);
            yield return new GLine(quad.L, quad.F);
            yield return new GLine(quad.F, quad.R);
            yield return new GLine(quad.R, quad.B);
        }

        public IEnumerable<float3> GetAxes() => GetTriangles().Select(p => p.normal);

        public IEnumerable<float3> GetNormals()
        {
            GetTriangles(out var triangle1, out var triangle2);
            var initialNormal = triangle1.normal;
            var midNormal = (triangle1.normal + triangle2.normal)/2;
            var finalNormal = triangle2.normal;
            yield return initialNormal;
            yield return midNormal;
            yield return finalNormal;
            yield return midNormal;
        }

        public float GetArea()
        {
            GetTriangles(out var triangle1, out var triangle2);
            return triangle1.GetArea() + triangle2.GetArea();
        }

        public float4 GetWeightsToPoint(float3 _point)
        {
            GetTriangles(out var triangle1, out var triangle2);
            
            var weights = float4.zero;
            
            var weight1 = triangle1.GetWeightsToPoint(_point);
            var weight2 = triangle2.GetWeightsToPoint(_point);

            if (!weight1.anyLesser(0))
                weights.xyz = weight1;

            if (!weight2.anyLesser(0))
                weights.xzw = weight2;
                
            return weights;
        }

        public bool RayIntersection(GRay _ray, out float distance)
        {
            GetTriangles(out var triangle1, out var triangle2);
            return triangle1.RayIntersection(_ray,out distance) || triangle2.RayIntersection(_ray,out distance);
        }

        public void DrawGizmos()  => UGizmos.DrawLinesConcat(quad);
    }

}