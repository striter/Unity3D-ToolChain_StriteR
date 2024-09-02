using Unity.Mathematics;
namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        public static bool Clip(this G2Triangle _triangle,G2Plane _plane, out IGeometry2 _clippedShape)
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