using Unity.Mathematics;
using System.Collections.Generic;
using TPoolStatic;
using UnityEngine;

namespace Geometry.PointSet
{
    public static class UBounds
    {
        #region 2D
        static void Minmax(IEnumerable<float3> _positions,out float3 _min,out float3 _max)
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            foreach (var position in _positions)
            {
                _min = math.min(position, _min);
                _max = math.max(position, _max);
            }
        }
        
        public static GBox GetBoundingBox(IEnumerable<float3> _positions)
        {
            Minmax(_positions,out var min,out var max);
            return GBox.Minmax(min,max);
        }

        
        public static GBox GetBoundingBoxOriented(float3 _right,float3 _up,float3 _forward,IEnumerable<float3> _points)
        {
            float3 min = float.MaxValue;
            float3 max = float.MinValue;
            foreach (var point in _points)
            {
                var cur = new float3(math.dot(point,_right),math.dot(point,_up),math.dot(point,_forward));
                min = math.min(cur, min);
                max = math.max(cur, max);
            }

            return GBox.Minmax(min, max);
        }
        
        private static readonly List<float3> kBoundaryPoints = new List<float3>(4);
        private static readonly List<float3> kContainedPoints = new List<float3>();
        
        public static GSphere GetBoundingSphere(IList<float3> _positions)
        {
            kBoundaryPoints.Clear();
            kContainedPoints.Clear();
            kContainedPoints.AddRange(_positions);
            URandom.Shuffle(kContainedPoints,kContainedPoints.Count,1);
            return GetBoundingSphereWelzl(kContainedPoints);
        }
        
        static GSphere GetBoundingSphereWelzl(IList<float3> _positions)            //Welzl Algorithm
        {
            if (_positions.Count == 0 || kBoundaryPoints.Count == GSphere.kMaxBoundsCount)
                return GSphere.Create(kBoundaryPoints);

            var lastIndex = _positions.Count - 1;
            var removed = _positions[lastIndex];
            _positions.RemoveAt(lastIndex);

            var sphere = GetBoundingSphereWelzl(_positions);
            if (!sphere.Contains(removed))
            {
                kBoundaryPoints.Add(removed);
                sphere = GetBoundingSphereWelzl(_positions);
                kBoundaryPoints.RemoveAt(kBoundaryPoints.Count-1);
            }
            
            _positions.Add(removed);
            return sphere;
        }

        public static GEllipsoid GetBoundingEllipsoid(IList<float3> _positions)
        {
            var box = GetBoundingBox(_positions);
            var m = math.mul(float3x3.identity,box.size);

            TSPoolList<float3>.Spawn(out var transformedPositions);
            foreach (var point in _positions)
            {
                var ePoint = m * point;
                transformedPositions.Add(ePoint);
            }
            var sphere = GetBoundingSphere(transformedPositions);
            TSPoolList<float3>.Recycle(transformedPositions);
            return new GEllipsoid(sphere.center,sphere.radius*2*box.size);
        }
        #endregion
        
        #region 2D
        static void Minmax2(float2[] _positions,out float2 _min,out float2 _max)
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            foreach (var position in _positions)
            {
                _min = math.min(position, _min);
                _max = math.max(position, _max);
            }
        }
        
        public static G2Triangle GetSuperTriangle(params float2[] _positions)     //always includes,but not minimum
        {
            Minmax2(_positions,out var min,out var max);
            var delta = (max - min);
            return new G2Triangle(
                new float2(min.x - delta.x,min.y - delta.y * 3f),
                new float2(min.x - delta.x,max.y + delta.y),
                new float2(max.x + delta.x*3f,max.y + delta.y)
            );
        }
        
        private static readonly List<float2> kBoundaryCirclePoints = new List<float2>(4);
        private static readonly List<float2> kContainedCirclePoints = new List<float2>();
        
        public static GCircle GetBoundingCircle(IList<float2> _positions)
        {
            kBoundaryCirclePoints.Clear();
            kContainedCirclePoints.Clear();
            kContainedCirclePoints.AddRange(_positions);
            URandom.Shuffle(kContainedCirclePoints,kContainedCirclePoints.Count,1);
            return GetBoundingCircleWelzl(kContainedCirclePoints);
        }
        
        static GCircle GetBoundingCircleWelzl(IList<float2> _positions)            //Welzl Algorithm
        {
            if (_positions.Count == 0 || kBoundaryCirclePoints.Count == GCircle.kMaxBoundsCount)
                return GCircle.Create(kBoundaryCirclePoints);

            var lastIndex = _positions.Count - 1;
            var removed = _positions[lastIndex];
            _positions.RemoveAt(lastIndex);

            var sphere = GetBoundingCircleWelzl(_positions);
            if (!sphere.Contains(removed))
            {
                kBoundaryCirclePoints.Add(removed);
                sphere = GetBoundingCircleWelzl(_positions);
                kBoundaryCirclePoints.RemoveAt(kBoundaryCirclePoints.Count-1);
            }
            
            _positions.Add(removed);
            return sphere;
        }
        #endregion
    }
}