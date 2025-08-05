using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Polygon
    {
        public static implicit operator G2Polygon(List<float2> _positions) => new(_positions);
        public static implicit operator List<float2>(G2Polygon _polygon) => _polygon.positions;
        
        #region Convex
        public static G2Polygon ConvexHull(IList<float2> _positions,float _bias = float.Epsilon) 
        {
            var polygonPoints = PoolList<float2>.Empty(nameof(ConvexHull));
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

        //https://www.dinocajic.com/grahams-scan-visually-explained/
        public static G2Polygon GrahamScan(IEnumerable<float2> _positions)
        {
            var sortedPoints = PoolList<float2>.Empty(nameof(GrahamScan));
            sortedPoints.AddRange(_positions);
            if(sortedPoints.Count<=3)
                return new G2Polygon(sortedPoints);
            var initialPoint = _positions.MinElement(p => p.y);

            sortedPoints.SortDescending(p=>umath.getRadClockwise(kfloat2.down,p-initialPoint));
            
            var hull = PoolList<float2>.Empty(nameof(GrahamScan) + "_Output");
            hull.Add(sortedPoints[0]);
            hull.Add(sortedPoints[1]);
            for (var i = 2; i < sortedPoints.Count; i++)
            {
                while (hull.Count > 1 && umath.getRadClockwise(hull[^2]-hull[^1],sortedPoints[i]-hull[^1]) > kmath.kPI)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(sortedPoints[i]);
            }
            return new G2Polygon(hull);
        }
        
        //https://algs4.cs.princeton.edu/99hull/quickhull/Algorithm.html
        public static G2Polygon QuickHull(IList<float2> _positions)
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
        #endregion
        
        #region Concave
        //&https://www.researchgate.net/publication/220868874_Concave_hull_A_k-nearest_neighbours_approach_for_the_computation_of_the_region_occupied_by_a_set_of_points
        public static G2Polygon ConcaveHull(IEnumerable<float2> _positions,int k = 3,float _bias = float.Epsilon)
        {
            Debug.Assert(k >= 3,$"[ConcaveHull] k must be >= 3, k = {k}.");
            var pointsSet = PoolList<float2>.Empty(nameof(ConcaveHull) + "_PointsHelper" + k);
            pointsSet.AddRange(_positions.Distinct());

            switch (pointsSet.Count)
            {
                case < 3:
                    Debug.Assert(false,$"[ConcaveHull] pointsSet Count must be >= 3, _positions.Count = {pointsSet.Count}.");
                    return kZero;
                case 3:
                    return new G2Polygon(pointsSet);
            }

            k = math.min(k, pointsSet.Count - 1);
            var firstPoint = pointsSet.MinElement(p => p.y);
            var hull = new List<float2> { firstPoint };
            pointsSet.Remove(firstPoint);
            
            var currentPoint = firstPoint;
            var currentDirection = kfloat2.down;
            var step = 1;

            while ((math.distancesq(currentPoint ,firstPoint) > _bias || step == 1) && pointsSet.Count > 0)
            {
                if(step == 4)
                    pointsSet.Add(firstPoint);
                
                var cPoints = pointsSet.MinElements(k,p=>math.distancesq(p, currentPoint)).FillList(PoolList<float2>.Empty(nameof(ConcaveHull) + "_kNeighbors"));
                cPoints.Sort(p=>umath.getRadClockwise(currentDirection,p - currentPoint));

                // #region Debug
                // const int debugIndex = 1;
                // if (step == debugIndex)
                // {
                //     Debug.DrawLine(currentPoint.to3xz(.2f),(currentPoint + currentDirection).to3xz(.2f),Color.white);
                //     Debug.DrawLine(currentPoint.to3xz(), currentPoint.to3xz(1f), Color.yellow);
                //     for (var i = 0; i < cPoints.Count; i++)
                //         Debug.DrawLine(cPoints[i].to3xz(), cPoints[i].to3xz(1f), UColor.IndexToColor(i));
                // }                    
                // #endregion
                
                var intersect = true;
                for(var i = 0; i < cPoints.Count; i++)
                {
                    currentPoint = cPoints[i];
                    var lastPoint = math.lengthsq(currentPoint - firstPoint) < _bias ? 1 : 0;
                    intersect = false;
                    for(var j = 2 ; j < hull.Count - lastPoint; j++)
                    {
                        intersect |= new G2Line(hull[step - 1], currentPoint).Intersect(new G2Line(hull[step - 1 - j], hull[step - j]));
                        if (intersect)
                            break;
                    }

                    if (intersect)
                        break;
                }


                if (intersect)
                    return ConcaveHull(_positions,k+1);
                        
                hull.Add(currentPoint);
                pointsSet.Remove(currentPoint);
                currentDirection = -(hull[step] - hull[step - 1]);
                step += 1;
            }

            var allInside = true;
            for (var i = 0; i < pointsSet.Count; i++)
            {
                var point = pointsSet[i];
                allInside &= ((G2Polygon)hull).Contains(point);
                if(!allInside)
                    break;
            }
            return allInside ? hull : ConcaveHull(_positions,k+1);;
        }
    
        public static G2Polygon AlphaShape(IList<float2> _positions,float _threshold)
        {
            var triangles = PoolList<PTriangle>.Empty(nameof(AlphaShape));
            UTriangulation.Triangulation(_positions,ref triangles);
            
            var alphaValues = PoolList<float>.Empty(nameof(AlphaShape));
            alphaValues.AddRange(triangles.Select(p=>new G2Triangle(_positions,p).GetCircumradius()));
            alphaValues.MinmaxElement(p=>p,out var min,out var max);
            alphaValues.Remake(p=>umath.invLerp(min,max,p));
            for (var i = triangles.Count - 1; i >= 0; i--)
            {
                var alpha = alphaValues[i];
                if (alpha > _threshold || alpha <= 0)
                    triangles.RemoveAt(i);
            }
            
            var graph = G2Graph.FromTriangles(_positions,triangles);
            return ((G2Polygon)graph.ContourTracing(p=>p.position));
        }
        #endregion
    }
}