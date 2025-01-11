using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

public static class CartographicGeneralization
{
    public static float2[] DouglasPeucker(float2[] _pointList, float _epslion = 0.1f)
    {
        var dmax = 0f;
        var index = 0;
        var end = _pointList.Length;
        var line = new G2Line(_pointList[0],_pointList[^1]);
        for (var i = 1; i < end - 1;i++)
        {
            var d = line.Distance(_pointList[i]);
            if (d > dmax) {
                index = i;
                dmax = d;
            }
        }

        if (dmax <= _epslion)
            return new[] {line.start,line.end};
        
        var recResults1 = DouglasPeucker(_pointList.Cut(0,index), _epslion);
        var recResults2 = DouglasPeucker(_pointList.Cut(index,end), _epslion);
        return recResults1.Concat(recResults2).ToArray();
    }

    public static List<float2> VisvalingamWhyatt(List<float2> _pointList,int _desireCount,bool _minimumBoundingPolygon = false)
    {
        var finalCount = math.max(_desireCount,3);
        while (_pointList.Count > finalCount)
        {
            var minArea = float.MaxValue;
            PTriangle minTriangleIndex = default;
            for(var i = 0; i< _pointList.Count; i++)
            {
                var triangleIndexes = new PTriangle((i-1 + _pointList.Count)%_pointList.Count,i,(i+1)%_pointList.Count);
                var area = new G2Triangle(_pointList,triangleIndexes).GetArea();
                if (area < minArea)
                {
                    minArea = area;
                    minTriangleIndex = triangleIndexes;
                }
            }

            if (_minimumBoundingPolygon)
            {
                var newLine = new G2Line(_pointList[minTriangleIndex.V0], _pointList[minTriangleIndex.V2]);
                var distance = newLine.Distance(_pointList[minTriangleIndex.V1]);
                var offset = math.mul(umath.Rotate2D(-90) , newLine.direction) * distance;
                _pointList[minTriangleIndex.V0] += offset;
                _pointList[minTriangleIndex.V2] += offset;
            }
            
            _pointList.RemoveAt(minTriangleIndex.V1);
        }

        return _pointList;
    }

    private static List<int2> kResults = new List<int2>();
    public static List<int2> BresenhamLine(int2 _start, int2 _end)
    {
        kResults.Clear();

        var x0 = _start.x;
        var x1 = _end.x;
        var y0 = _start.y;
        var y1 = _end.y;
            
        var dx = math.abs(x1 - x0);
        var dy = math.abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = (dx > dy ? dx : -dy) / 2;
        for(;;) {
            kResults.Add(new int2(x0, y0));
            if (x0 == x1 && y0 == y1) break;
            var e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }

            if (e2 >= dy) continue;
            err += dx; y0 += sy;
        }

        return kResults;
    }
}
