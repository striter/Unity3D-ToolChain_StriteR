using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;
using static kint2;

//https://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/index.html

public struct ContourTracingData
{
    public int2 resolution;
    public bool[] m_ContourTessellation;
    public static ContourTracingData FromColorAlpha(int _width,Color[] _colors,float _threshold = 0.5f)
    {
        var length = _colors.Length;
        var height = length / _width;
        var contourTessellation = new bool[_width * height];
        for (int i = 0; i < length; i++)
            contourTessellation[i] = _colors[i].a > _threshold;
        return new ContourTracingData {
            resolution = new int2(_width, height),
            m_ContourTessellation = contourTessellation,
        };
    }
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
        
        _startPixel = UCoordinates.Tile.ToIndex(index, _data.resolution.x);
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
                var outOfBounds = (nextPixel < int2.zero).any() || (nextPixel >= _data.resolution).any();
                if (!outOfBounds)
                    break;
                directionIndex = (directionIndex + indexMovement + 4) % 4;
                nextPixel =  _curPixel + kSquareTracingIndexer[directionIndex];
            }
            return nextPixel;
        };

        var curIteration = _data.m_ContourTessellation.Length;
        curPixel = MoveToNext(curPixel,true);
        while (curIteration-- > 0 )
        {
            var sampled = _data.m_ContourTessellation[curPixel.x + curPixel.y * _data.resolution.x];
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

        var curIteration = _data.m_ContourTessellation.Length;
        while (curIteration-- > 0 )
        {
            var iteration = kMooreNeighbotIteration[curDirection];
            var sampled = false;
            
            for (var i = 0; i < iteration.Count; i++)
            {
                curDirection = iteration[i];
                curPixel += curDirection;
                
                var outOfBounds = (curPixel < int2.zero).any() || (curPixel >= _data.resolution).any();
                sampled = !outOfBounds && _data.m_ContourTessellation[curPixel.x + curPixel.y * _data.resolution.x];

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

        var curIteration = _data.m_ContourTessellation.Length;
        while (curIteration-- > 0 )
        {
            var swept = false;
            
            for(var i=0;i<8;i++)
            {
                var sweepPixel = curPixel + kNeighbors8[(sweepDirection + i)%8];
                var outOfBounds = (sweepPixel < int2.zero).any() || (sweepPixel >= _data.resolution).any();
                swept = !outOfBounds && _data.m_ContourTessellation[sweepPixel.x + sweepPixel.y * _data.resolution.x];
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
    
    
}
