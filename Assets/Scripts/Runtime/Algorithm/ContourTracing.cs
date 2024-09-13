using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using static kint2;

//https://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/index.html

public struct ContourTracingData : IGraph<int2>
{
    public int2 resolution;
    public bool[] m_ContourTessellation;

    public int Count => m_ContourTessellation.Length;
    public static ContourTracingData FromColor(int _width,Color[] _colors,Func<Color,bool> _predicate = null)
    {
        _predicate ??= color => color.a > 0.1f;
        
        var length = _colors.Length;
        var height = length / _width;
        var contourTessellation = new bool[_width * height];
        for (var i = 0; i < length; i++)
            contourTessellation[i] = _predicate(_colors[i]) ;
        return new ContourTracingData {
            resolution = new int2(_width, height),
            m_ContourTessellation = contourTessellation,
        };
    }

    public bool OutOfBounds(int2 _pixel) => (_pixel < int2.zero).any() || (_pixel >= resolution).any();
    public bool Sample(int2 _pixel) => !OutOfBounds(_pixel) && m_ContourTessellation[UCoordinates.Tile.ToIndex(_pixel,resolution.x)];

    public IEnumerable<int2> GetAdjacentNodes(int2 _src)
    {
        if(Sample(_src + kDown)) yield return _src + kDown;
        if(Sample(_src + kLeft)) yield return _src + kLeft;
        if(Sample(_src + kUp)) yield return _src + kUp;
        if(Sample(_src + kRight)) yield return _src + kRight;
    }

    public IEnumerator<int2> GetEnumerator()
    {
        for(var i=0;i<m_ContourTessellation.Length;i++)
            yield return UCoordinates.Tile.ToTile(i,resolution.x);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class ContourTracingData_Extension   //Transform ContourTracingData to integer positions which represent contour edges
{
    public static bool ContourAble(this ContourTracingData _data,out int2 _startPixel)
    {
        _startPixel = int2.zero;
        if (_data.m_ContourTessellation== null || _data.m_ContourTessellation.Length == 0)
            return false;
        
        var index = _data.m_ContourTessellation.IndexOf(true);
        if (index == -1)
            return false;
        
        _startPixel = UCoordinates.Tile.ToTile(index, _data.resolution.x);
        return true;
    }

    private static List<int2> kSquareTracingIndexer = new () { kLeft, kDown, kRight, kUp };
    public static List<float2> SquareTracing(this ContourTracingData _data)
    {
        if (!_data.ContourAble(out var startPixel))
            return null;
        
        var curPixel = startPixel;
        var directionIndex = -1;
        var contourEdges = new List<float2> { startPixel };
        Func<int2,bool,int2> MoveToNext = (_curPixel,_curSampled) =>
        {
            var nextPixel = _curPixel;
            var indexMovement = _curSampled ? 1 : -1;
            directionIndex = (directionIndex + indexMovement + 4) % 4;
            nextPixel += kSquareTracingIndexer[directionIndex];
            while (true)
            {
                if (!_data.OutOfBounds(nextPixel))
                    break;
                directionIndex = (directionIndex + indexMovement + 4) % 4;
                nextPixel =  _curPixel + kSquareTracingIndexer[directionIndex];
            }
            return nextPixel;
        };

        var curIteration = _data.Count;
        curPixel = MoveToNext(curPixel,true);
        while (curIteration-- > 0 )
        {
            var sampled = _data.Sample(curPixel);
            if(sampled)
                contourEdges.Add(curPixel);
            curPixel = MoveToNext(curPixel,sampled);
            if ((curPixel == startPixel).all())
                break;
        }
        
        return contourEdges;
    }

    private static readonly Dictionary<int2, List<int2>> kMooreNeighbotIteration = new() {
        { kRight, new() {kLeft,kUp,kRight,kRight,kDown,kDown,kLeft,kLeft } },
        { kLeft, new() {kRight,kDown,kLeft,kLeft,kUp,kUp,kRight,kRight } },
        { kDown, new (){kUp,kRight,kDown,kDown,kLeft,kLeft,kUp,kUp}},
        { kUp, new (){kDown,kLeft,kUp,kUp,kRight,kRight,kDown,kDown}}
    };
    
    public static List<float2> MooreNeighborTracing(this ContourTracingData _data)
    {
        if (!_data.ContourAble(out var startPixel))
            return null;

        var curDirection = kUp;
        var curPixel = startPixel;
        var contourEdges = new List<float2> { startPixel };

        var curIteration = _data.Count;
        while (curIteration-- > 0 )
        {
            var iteration = kMooreNeighbotIteration[curDirection];
            var sampled = false;
            
            for (var i = 0; i < iteration.Count; i++)
            {
                curDirection = iteration[i];
                curPixel += curDirection;
                sampled = _data.Sample(curPixel);
                if (!sampled) 
                    continue;
                contourEdges.Add(curPixel);
                break;
            }

            var startPixelMarched = (startPixel == curPixel).all();
            if (!sampled || startPixelMarched)
                break;
        }
        return contourEdges;
    }
    
    private static readonly List<int2> kNeighbors8 = new() { kLeft + kDown,kLeft, kLeft + kUp, kUp, kRight + kUp, kRight, kRight + kDown, kDown };
    public static List<float2> RadialSweep(this ContourTracingData _data)
    {
        if (!_data.ContourAble(out var startPixel))
            return null;

        var sweepDirection = 0;
        var curPixel = startPixel;
        var contourEdges = new List<float2> { startPixel };

        var curIteration = _data.Count;
        while (curIteration-- > 0 )
        {
            var swept = false;
            
            for(var i=0;i<8;i++)
            {
                var sweepPixel = curPixel + kNeighbors8[(sweepDirection + i)%8];
                swept = _data.Sample(sweepPixel);
                if (!swept) 
                    continue;
                sweepDirection = (sweepDirection + i + 5) % 8;
                curPixel = sweepPixel;
                contourEdges.Add(curPixel);
                break;
            }

            var startPixelMarched = (startPixel == curPixel).all();
            if (!swept || startPixelMarched)
                break;
        }
        return contourEdges;
    }

    private static readonly Dictionary<EQuadCorner, List<int2>> kFacingPixels = new Dictionary<EQuadCorner, List<int2>>() {
        {EQuadCorner.L,new (){kLeft + kDown,kLeft,kLeft + kUp}},
        {EQuadCorner.F,new (){kLeft + kUp,kUp,kRight + kUp}},
        {EQuadCorner.R,new (){kRight + kUp,kRight,kRight + kDown}},
        {EQuadCorner.B,new (){kRight + kDown,kDown,kLeft + kDown}}
    };

    
    public static List<float2> TheoPavlidis(this ContourTracingData _data)
    {
        if (!_data.ContourAble(out var startPixel))
            return null;
    
        var contourEdges = new List<float2> { startPixel };
        
        var step = EQuadCorner.L;

        var curPixel = startPixel;
        var curIteration = _data.Count;
        while (curIteration-- > 0 )
        {
            var sampled = false;
            
            for(var i=0;i<4;i++)
            {
                var p0 = curPixel + kFacingPixels[step][0];
                var p1 = curPixel + kFacingPixels[step][1];
                var p2 = curPixel + kFacingPixels[step][2];
                
                if (_data.Sample(p0))
                {
                    sampled = true;
                    curPixel = p0;
                    step = step.Prev();
                }
                else if (_data.Sample(p1))
                {
                    sampled = true;
                    curPixel = p1;

                }
                else if(_data.Sample(p2))
                {
                    sampled = true;
                    curPixel = p2;
                }
                else
                {
                    step = step.Next();
                }

                if (sampled)
                {
                    contourEdges.Add(curPixel);
                    break;
                }
            }
    
            var startPixelMarched = (startPixel == curPixel).all();
            if (!sampled || startPixelMarched)
                break;
        }

        return contourEdges;
    }
    
}
