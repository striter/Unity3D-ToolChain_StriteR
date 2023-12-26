using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Validation
{
    public static partial class UGeometry
    {
        public static bool Clip(this GTriangle _triangle,GPlane _plane, out IShape3D _outputShape,bool _directed = true)
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
        
        
        public static bool Clip(this G2Triangle _triangle,G2Plane _plane, out I2Shape _clippedShape)
        {
            _clippedShape = null;
            for (int i = 0; i < 3; i++)
            {
                var curPoint = _triangle[(i + 1) % 3];
                
                var curDot = _plane.dot(curPoint);
                if (curDot > 0)
                    continue;
                
                var prePoint = _triangle[i];
                var preDot = _plane.dot(prePoint);
                var nextPoint = _triangle[(i + 2) % 3];
                var nextDot = _plane.dot(nextPoint);

                if (preDot < 0 && nextDot < 0)     //No intersections
                    break;
                
                if (nextDot < 0)
                {
                    _clippedShape = new G2Triangle(
                        prePoint,
                        math.lerp(curPoint, prePoint, curDot / math.dot(_plane.normal,curPoint-prePoint)),
                        math.lerp(nextPoint, prePoint, nextDot / math.dot(_plane.normal,nextPoint-prePoint)));
                }
                else if (preDot < 0)
                {
                    _clippedShape = new G2Triangle(
                        math.lerp(prePoint, nextPoint, preDot / math.dot(_plane.normal,prePoint - nextPoint)),
                        math.lerp(curPoint, nextPoint, curDot / math.dot(_plane.normal,curPoint - nextPoint)),
                        nextPoint);
                }
                else
                {
                    _clippedShape = new G2Quad(prePoint
                        , math.lerp(curPoint, prePoint, curDot / math.dot(_plane.normal,curPoint-prePoint))
                        , math.lerp(curPoint, nextPoint, curDot / math.dot(_plane.normal,curPoint-nextPoint))
                        , nextPoint );
                }
                break;
            }

            return _clippedShape != null; 
        }
    }
}