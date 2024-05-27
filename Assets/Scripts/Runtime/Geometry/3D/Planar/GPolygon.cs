using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GPolygon
    {
        public float3[] positions;
        [NonSerialized] public float3 center;
        public GPolygon(IEnumerable<float3> _positions):this(_positions.ToArray()){}
        public GPolygon(params float3[] _positions)
        {
            positions = _positions;
            center = _positions.Average();
        }

        public float3 GetSupportPoint(float3 _direction)=>positions.MaxElement(_p => math.dot(_direction, _p));
        public float3 Center => center;

        public static readonly GPolygon kZero = new GPolygon();
        public static readonly GPolygon kDefault = new GPolygon(kfloat3.forward,kfloat3.right,kfloat3.back,kfloat3.left);
        public IEnumerator<float3> GetEnumerator() => positions.Cast<float3>().GetEnumerator();

        public IEnumerable<GLine> GetEdges()
        {
            for (int i = 0; i < positions.Length - 1; i++)
                yield return new GLine(positions[i], positions[i + 1]);
        }

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
        
        public int Count => positions.Length;
    }

}