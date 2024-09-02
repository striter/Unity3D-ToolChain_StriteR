using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Procedural.Tile;
using Unity.Mathematics;
using UnityEngine;
using static kint2;

//https://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/index.html

public struct ContourTracing
{
    public int2 resolution;
    public bool[] m_ContourTessellation;
    public static ContourTracing FromColorAlpha(int _width,Color[] _colors,float _threshold = 0.5f)
    {
        var length = _colors.Length;
        var height = length / _width;
        var contourTessellation = new bool[_width * height];
        for (int i = 0; i < length; i++)
            contourTessellation[i] = _colors[i].a > _threshold;
        return new ContourTracing {
            resolution = new int2(_width, height),
            m_ContourTessellation = contourTessellation,
        };
    }
}

public static class ContourTracing_Extension
{
    public static bool ContourAble(this ContourTracing _data,out int2 _startPixel)
    {
        _startPixel = int2.zero;
        if (_data.m_ContourTessellation== null || _data.m_ContourTessellation.Length == 0)
            return false;
        
        var index = _data.m_ContourTessellation.IndexOf(true);
        if (index == -1)
            return false;
        
        _startPixel = UTile.GetAxisByIndex(index, _data.resolution.x);
        return true;
    }

    private static readonly Dictionary<int2, List<int2>> kMooreNeighbotIteration = new() {
        { kRight, new() {kLeft,kUp,kRight,kRight,kDown,kDown,kLeft,kLeft } },
        { kLeft, new() {kRight,kDown,kLeft,kLeft,kUp,kUp,kRight,kRight } },
        { kDown, new (){kUp,kRight,kDown,kDown,kLeft,kLeft,kUp,kUp}},
        { kUp, new (){kDown,kLeft,kUp,kUp,kRight,kRight,kDown,kDown}}
    };
    
    public static List<float2> MooreNeighborTracing(this ContourTracing _data)      // integer positions
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
                
                var outOfBounds = curPixel < int2.zero | curPixel >= _data.resolution;
                sampled = outOfBounds is { x: false, y: false } && _data.m_ContourTessellation[curPixel.x + curPixel.y * _data.resolution.x];

                if (!sampled) 
                    continue;
                contourEdges.Add(curPixel);
                break;
            }

            var startPixelMarched = startPixel == curPixel;
            if (!sampled || startPixelMarched is { x: true, y: true })
                break;
        }
        
        return contourEdges;
    }
    
}
