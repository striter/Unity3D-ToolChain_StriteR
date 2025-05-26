using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using static Unity.Mathematics.math;

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

    
    //https://zingl.github.io/bresenham.html
    public static class Bresenham
    {
        public static void Line(int2 _start, int2 _end,Action<int2,float> _lineOpacity)
        {
            var x0 = _start.x;
            var x1 = _end.x;
            var y0 = _start.y;
            var y1 = _end.y;
                
            var dx = abs(x1 - x0);
            var dy = abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = (dx > dy ? dx : -dy) / 2;
            for(;;)
            {
                _lineOpacity(new int2(x0, y0), 1);
                if (x0 == x1 && y0 == y1) break;
                var e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 >= dy) continue;
                err += dx; y0 += sy;
            }
        }

        public static void LineWidth(int2 _start, int2 _end, float wd, Action<int2, float> _lineOpacity)
        {
            var x0 = _start.x;
            var x1 = _end.x;
            var y0 = _start.y;
            var y1 = _end.y;

            int dx = abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx - dy, e2, x2, y2; /* error value e_xy */
            float ed = dx + dy == 0 ? 1 : sqrt((float)dx * dx + (float)dy * dy);

            for (wd = (wd + 1) / 2;;)
            {
                /* pixel loop */
                _lineOpacity(new int2(x0, y0), 1f - max(0, abs(err - dx + dy) / ed - wd + 1));
                e2 = err;
                x2 = x0;
                if (2 * e2 >= -dx)
                {
                    /* x step */
                    for (e2 += dy, y2 = y0; e2 < ed * wd && (y1 != y2 || dx > dy); e2 += dx)
                        _lineOpacity(new int2(x0, y2 += sy), 1f - max(0, abs(e2) / ed - wd + 1));
                    if (x0 == x1) break;
                    e2 = err;
                    err -= dy;
                    x0 += sx;
                }

                if (2 * e2 <= dy)
                {
                    /* y step */
                    for (e2 = dx - e2; e2 < ed * wd && (x1 != x2 || dx < dy); e2 += dy)
                        _lineOpacity(new int2(x2 += sx, y0), 1f - max(0, abs(e2) / ed - wd + 1));
                    if (y0 == y1) break;
                    err += dx;
                    y0 += sy;
                }
            }
        }
        
        
        static void plot1(int x,int y,int2 _centre,Action<int2, float> _color) => _color(new int2(_centre.x + x, _centre.y + y), 1f);
        static void plot8(int x,int y,int2 _centre, Action<int2, float> _color){
            plot1(x,y,_centre,_color);plot1(y,x,_centre,_color);
            plot1(x,-y,_centre,_color);plot1(y,-x,_centre,_color);
            plot1(-x,-y,_centre,_color);plot1(-y,-x,_centre,_color);
            plot1(-x,y,_centre,_color);plot1(-y,x,_centre,_color);
        }

        public static void Circle(int2 _centre, int _radius, Action<int2, float> _lineOpacity)
        {
            var x = 0;
            var y = _radius;
            var d = 1 - _radius;
            while(x < y)
            {
                if(d < 0)
                {
                    d += 2 * x + 3;
                }
                else
                {
                    d += 2 * (x-y) + 5;
                    y--;
                }
                plot8(x,y, _centre,_lineOpacity);
                x++;
            }
        }
    }
}
