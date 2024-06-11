using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension.BoundingSphere;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
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

        public static GBox GetBoundingBox(IEnumerable<GBox> _boxes)
        {
            var min = kfloat3.max;
            var max = kfloat3.min;
            foreach (var box in _boxes)
            {
                min = math.min(box.min, min);
                max = math.max(box.max, max);
            }
            return new GBox(min, max);
        }
        
        public static GSphere GetBoundingSphere(IEnumerable<GSphere> _spheres)
        {
            var boundingSphere = _spheres.First();
            foreach (var sphere in _spheres)
                boundingSphere = GSphere.Minmax(boundingSphere, sphere);
            return boundingSphere;
        }

        public static GSphere GetBoundingSphere(IEnumerable<float3> _positions) => EPOS.Evaluate(_positions,EPOS.EMode.EPOS26,Welzl<GSphere,float3>.Evaluate);
        public static GEllipsoid GetBoundingEllipsoid(IEnumerable<float3> _positions)
        {
            var box = GetBoundingBox(_positions);
            var m = math.mul(float3x3.identity,box.size);
            var sphere = GetBoundingSphere(_positions.Select(p=>m * p));
            return new GEllipsoid(sphere.center,sphere.radius*2*box.size);
        }
        
        public static GSphere GetSuperSphere(params float3[] _points) => GetSuperSphere(_points.AsEnumerable());
        public static GSphere GetSuperSphere(IEnumerable<float3> _points) => GetBoundingBox(_points).GetBoundingSphere();
        
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
        
        
        public static G2Triangle GetSuperTriangle(params float2[] _positions)     //always includes,but not minimum
        {
            Minmax(_positions,out var min,out var max);
            var delta = (max - min);
            return new G2Triangle(
                new float2(min.x - delta.x,min.y - delta.y * 3f),
                new float2(min.x - delta.x,max.y + delta.y),
                new float2(max.x + delta.x*3f,max.y + delta.y)
            );
        }
        
        public static G2Circle GetBoundingCircle(IList<float2> _positions) => Welzl<G2Circle, float2>.Evaluate(_positions);
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