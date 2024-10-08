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
}
