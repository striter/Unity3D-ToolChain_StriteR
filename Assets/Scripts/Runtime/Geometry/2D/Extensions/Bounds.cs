using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension.BoundingSphere;
using Runtime.Pool;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
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
        
        public static G2Box GetBoundingBox<T>(IList<T> _elements, Func<T, float2> _convert)
        {
            var min = kfloat2.max;
            var max = kfloat2.min;
            for(var i = _elements.Count - 1; i>=0;i--)
            {
                var position = _convert( _elements[i]);
                min = math.min(position, min);
                max = math.max(position, max);
            }

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
        
        public static G2Circle GetBoundingCircle(IList<float2> _positions) => EPOS._2D.Evaluate(_positions, EPOS._2D.EMode.EPOS8,Welzl<G2Circle, float2>.Evaluate);

        public static List<float2> GetBoundingPolygon(IList<float2> _positions,float _bias = float.Epsilon)        //Graham Scan
        {
            var polygonPoints = PoolList<float2>.Empty(nameof(QuickHull));
            if(_positions.Count<=0)
                return polygonPoints;
            
            var minimumY = _positions.MinElement(p => p.y).y;
            var initialPoint = _positions.Collect(p=>Math.Abs(p.y - minimumY) < _bias).MinElement(p => p.x);
            var curDirection = kfloat2.right + kfloat2.down;
            polygonPoints.Add(initialPoint);

            while (polygonPoints.Count <= _positions.Count)
            {
                var previousPoint = polygonPoints[^1];
                var direction = curDirection;
                var nextPoint = _positions.Collect(p=>(p-previousPoint).sqrmagnitude() > _bias).MinElement(p =>
                {
                    var radCW = umath.getRadClockwise(p - previousPoint, direction);
                    return radCW <= 0 ? float.MaxValue : radCW;
                });
                if ((nextPoint - initialPoint).sqrmagnitude() < _bias)
                    break;
                curDirection = nextPoint - previousPoint;
                polygonPoints.Add(nextPoint);
            }

            return polygonPoints;
        }

        //https://algs4.cs.princeton.edu/99hull/quickhull/Algorithm.html
        public static List<float2> QuickHull(IList<float2> _positions)
        {
            var convexHull = PoolList<float2>.Empty(nameof(QuickHull));
            if(_positions.Count<=0)
                return convexHull;

            var comparer = kfloat2.up;
            _positions.MinmaxIndex(p=>math.dot(p,comparer),out var minIndex,out var maxIndex);
            var A = _positions[minIndex];
            var B = _positions[maxIndex];
            
            if(maxIndex < minIndex)
                (maxIndex,minIndex) = (minIndex,maxIndex);
            
            convexHull.Add(A);
            convexHull.Add(B);

            var ray = G2Ray.StartEnd(A,B);
            var s1 = new List<float2>();
            var s2 = new List<float2>();
            for (var i = 0; i < _positions.Count; i++)
            {
                if (i == minIndex || i == maxIndex) 
                    continue;
                
                var p = _positions[i];
                if(ray.SideSign(p))
                    s1.Add(p);
                else
                    s2.Add(p);
            }
            FindHull(convexHull,1,s1,A,B);
            FindHull(convexHull,convexHull.Count,s2,B,A);
            return convexHull;
        }
        
        static void FindHull(List<float2> _convexHull,int _insertIndex, IList<float2> _positions, float2 _P,float2 _Q)
        {
            if (_positions.Count == 0)
                return;

            var ray = G2Ray.StartEnd(_P, _Q);
            var maxIndex = _positions.MaxIndex(p=>ray.Distance(p));
            var C = _positions[maxIndex];
            _convexHull.Insert(_insertIndex,C);

            var s1 = new List<float2>();
            var s2 = new List<float2>();

            var linePC = G2Ray.StartEnd(_P, C);
            var lineCQ = G2Ray.StartEnd(C, _Q);

            foreach (var (index,point) in _positions.LoopIndex())
            {
                if(index == maxIndex)
                    continue;
                
                if (linePC.SideSign(point))
                {
                    s1.Add(point);
                    continue;                    
                }

                if (lineCQ.SideSign(point))
                    s2.Add(point);
            }
            
            FindHull(_convexHull,_insertIndex+1,s2,C,_Q);
            FindHull(_convexHull,_insertIndex,s1,_P,C);
        }
    }
}