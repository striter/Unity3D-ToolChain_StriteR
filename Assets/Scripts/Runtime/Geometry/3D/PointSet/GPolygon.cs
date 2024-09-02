using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GPolygon : IVolume
    {
        public List<float3> positions;
        [NonSerialized] public float3 center;
        public GPolygon(IEnumerable<float3> _positions) : this(_positions.ToList()) { }
        public GPolygon(params float3[] _positions) : this(_positions.ToList()) { }
        public GPolygon(List<float3> _positions)
        {
            positions = _positions;
            center = _positions.Average();
        }
        
        public float3 GetSupportPoint(float3 _direction)=>positions.MaxElement(_p => math.dot(_direction, _p));
        public GBox GetBoundingBox() => UGeometry.GetBoundingBox(positions);
        public GSphere GetBoundingSphere() => UGeometry.GetBoundingSphere(positions);

        public float3 Origin => center;

        public static readonly GPolygon kZero = new GPolygon();
        public static readonly GPolygon kDefault = new GPolygon(kfloat3.forward,kfloat3.right,kfloat3.back,kfloat3.left);
        public static GPolygon kBunny = new GPolygon(G2Polygon.kBunny.Select(p=>p.to3xz()));
        
        public IEnumerator<float3> GetEnumerator() => positions.Cast<float3>().GetEnumerator();

        public float3 this[int index] => positions[index];

        public static GPolygon operator +(GPolygon _polygon, float3 _dst)
        {
            _polygon.positions.Remake(p=> p + _dst);
            _polygon.center += _dst;
            return _polygon;
        }
        public static GPolygon operator -(GPolygon _src, float3 _dst) => _src + -_dst;

        public static GPolygon operator *(Matrix4x4 _matrix, GPolygon _polygon)
        {
            _polygon.positions.Remake(p=>_matrix.MultiplyPoint(p));
            _polygon.center = _matrix.MultiplyPoint(_polygon.center);
            return _polygon;
        }
        
        public int Count => positions.Count;
        public void DrawGizmos() => UGizmos.DrawLinesConcat(positions);
    }

}