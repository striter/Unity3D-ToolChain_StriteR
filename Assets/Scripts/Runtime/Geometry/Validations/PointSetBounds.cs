using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using TObjectPool;
using Unity.Mathematics;

namespace Runtime.Geometry.Validation
{
    public static partial class UBounds
    {
        #region 3D
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

        public static GSphere GetBoundingSphere(IEnumerable<GSphere> _spheres)
        {
            var boundingSphere = _spheres.First();
            foreach (var sphere in _spheres)
                boundingSphere = GSphere.Minmax(boundingSphere, sphere);
            return boundingSphere;
        }
        
        private static readonly List<float3> kBoundaryPoints = new List<float3>(4);
        private static readonly List<float3> kContainedPoints = new List<float3>();
        public static GSphere GetBoundingSphere(IEnumerable<float3> _positions)
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
        private static List<float3> kEnclosingPoints = new List<float3>();
        
        public static GSphere MinimumEnclosingSphere(params float3[] _points) => MinimumEnclosingSphere(_points.AsEnumerable());
        public static GSphere MinimumEnclosingSphere(IEnumerable<float3> _points)
        {
            kEnclosingPoints.Clear();
            kEnclosingPoints.AddRange(_points);
            URandom.Shuffle(kContainedPoints,kContainedPoints.Count,1);
            
            if(kEnclosingPoints.Count==0)
                return GSphere.kZero;

            var box = GetBoundingBox(kEnclosingPoints);
            return new GSphere(box.center,box.extent.magnitude());
        }


        #endregion
        
        #region 2D
        static void Minmax(IEnumerable<float2> _positions,out float2 _min,out float2 _max)
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            foreach (var position in _positions)
            {
                _min = math.min(position, _min);
                _max = math.max(position, _max);
            }
        }
        
        public static G2Box GetBoundingBox(IEnumerable<float2> _positions)
        {
            Minmax(_positions,out var min,out var max);
            return G2Box.Minmax(min,max);
        }
        
        
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
        
        public static G2Circle GetBoundingCircle(IList<float2> _positions)
        {
            kBoundaryCirclePoints.Clear();
            kContainedCirclePoints.Clear();
            kContainedCirclePoints.AddRange(_positions);
            URandom.Shuffle(kContainedCirclePoints,kContainedCirclePoints.Count,1);
            return GetBoundingCircleWelzl(kContainedCirclePoints);
        }
        
        static G2Circle GetBoundingCircleWelzl(IList<float2> _positions)            //Welzl Algorithm
        {
            if (_positions.Count == 0 || kBoundaryCirclePoints.Count == G2Circle.kMaxBoundsCount)
                return G2Circle.Create(kBoundaryCirclePoints);

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


        private static readonly List<float2> kBoundingPolygonPoints = new List<float2>();
        public static G2Polygon GetBoundingPolygon(IList<float2> _positions,float _bias = float.Epsilon)
        {
            if(_positions.Count<=0)
                return G2Polygon.kZero;
            
            kBoundingPolygonPoints.Clear();
            var direction = kfloat2.right + kfloat2.up;
            var initialPoint = _positions.MinElement(p=>p.sum());
            kBoundingPolygonPoints.Add(initialPoint);
            
            while (kBoundingPolygonPoints.Count <= _positions.Count)
            {
                var previousPoint = kBoundingPolygonPoints[^1];
                var nextPoint = _positions.Collect(p=>(p-previousPoint).sqrmagnitude()> 0.001f).MinElement(p =>  umath.getRadClockwise(p-previousPoint,direction));
                if ((nextPoint - initialPoint).sqrmagnitude() < _bias)
                    break;
                direction = nextPoint - previousPoint;
                kBoundingPolygonPoints.Add(nextPoint);
            }

            return new G2Polygon(kBoundingPolygonPoints);
        }
        #endregion
    }
}