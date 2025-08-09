using System.Collections;
using System.Collections.Generic;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        public static bool Clip(this G2Polygon _polygon,G2Plane _plane,out G2Polygon _clippedPolygon)
        {
            var cliped = false;
            var positions = new List<float2>();
            for (var i = 0; i < _polygon.Count; i++)
            {
                var curPoint = _polygon[i];
                var curDot = _plane.dot(curPoint);
                var curForward =  curDot > 0;

                if(curForward)
                    positions.Add(curPoint);
                
                var line = new G2Line(_polygon[i],_polygon[(i + 1) % _polygon.Count]);
                if (line.Intersect(_plane, out var projection))
                {
                    positions.Add(line.GetPoint(projection));
                    cliped = true;
                }
            }

            _clippedPolygon = new G2Polygon(positions);
            return cliped;
        }

        public static G2Polygon Clip(this G2Polygon _polygon, G2Plane _plane)
        {
            _polygon.Clip(_plane, out var clippedPolygon);
            return clippedPolygon;
        }
        
        public static bool Clip(this GTriangle _triangle,GPlane _plane, out IVolume _outputShape,bool _directed = true)
        {
            _outputShape = null;
            if (_directed && math.dot(_triangle.normal, _plane.normal) < 0)
                return false;

            int forwardCount = 0;
            for (int i = 0; i < 3; i++)
            {
                var curPoint = _triangle[(i + 1) % 3];
                
                var curDot = _plane.dot(curPoint);
                if (curDot > 0)
                {
                    forwardCount++;
                    continue;
                }
                
                var prePoint = _triangle[i];
                var preDot = _plane.dot(prePoint);
                var nextPoint = _triangle[(i + 2) % 3];
                var nextDot = _plane.dot(nextPoint);

                if (preDot < 0 && nextDot < 0)     //No intersections
                    break;
                
                if (nextDot < 0)
                {
                    _outputShape = new GTriangle(
                        math.lerp(nextPoint, prePoint, nextDot / math.dot(_plane.normal,nextPoint-prePoint)),
                        prePoint,
                        math.lerp(curPoint, prePoint, curDot / math.dot(_plane.normal,curPoint-prePoint)));
                }
                else if (preDot < 0)
                {
                    _outputShape = new GTriangle(
                        math.lerp(prePoint, nextPoint, preDot / math.dot(_plane.normal,prePoint - nextPoint)),
                        math.lerp(curPoint, nextPoint, curDot / math.dot(_plane.normal,curPoint - nextPoint)),
                        nextPoint);
                }
                else
                {
                    _outputShape = new GQuad(prePoint
                        , math.lerp(curPoint, prePoint, curDot / math.dot(_plane.normal,curPoint-prePoint))
                        , math.lerp(curPoint, nextPoint, curDot / math.dot(_plane.normal,curPoint-nextPoint))
                        , nextPoint );
                }
                break;
            }

            if (forwardCount == 3)
                _outputShape = _triangle;
            
            return _outputShape != null; 
        }
    }
}