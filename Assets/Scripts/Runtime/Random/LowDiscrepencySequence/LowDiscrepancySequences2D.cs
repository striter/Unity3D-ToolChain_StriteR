﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Procedural.Tile;
using Runtime.DataStructure;
using Runtime.Random;
using Unity.Mathematics;
using UnityEngine;
using static kmath;
using static Unity.Mathematics.math;

public static partial class ULowDiscrepancySequences   // 0 - 1
{
    public static float2 Hammersley2D(uint _index, uint _size)=>new float2(Hammersley(_index,0,_size),Hammersley(_index,1,_size));
    public static float2 Halton2D(uint _index) => new float2( Halton(_index,0),Halton(_index,kPrimes128[1]));
    public static float2[] Grid2D(int _width,int _height)
    {
        float2[] grid = new float2[_width * _height];
        float2 uvOffset = new float2(1f/(_width ),1f / (_height));

        float2 start = uvOffset * .5f;
        for(int y = 0; y < _height; y++)
        for(int x = 0; x < _width; x++)
            grid[y * _width + x] = start +  new float2(x, y) * uvOffset;
        return grid;
    }
    public static float2[] Stratified2D(int _width, int _height, bool _jitter = false, IRandomGenerator _random = null)
    {
        float2 uvOffset = 1f / new float2(_width,_height);
        float2[] grid = new float2[_width*_height];
        for(int x = 0; x < _width; x++)
        for(int y = 0; y < _height; y++)
        {
            var jx = _jitter ? URandom.Random01(_random) : .5f;
            var jy = _jitter ? URandom.Random01(_random) : .5f;
            grid[y * _width + x] = new float2(x+jx,y+jy)*uvOffset;
        } 
        UShuffle.LatinHypercube(grid,grid.Length,_width,_random);
        return grid;
    }
    struct SobelMatrix
    {
        public uint a;
        public uint[] m;

        public SobelMatrix(uint _a, uint[] _m)
        {
            a = _a;
            m = _m;
        }
    }
    
    static readonly SobelMatrix[] kSobelMatrices = {
        new (0,new uint[]{0,0}),new (0,new uint[]{0,1}),          new (1,new uint[]{0,1,3}),       new (1,new uint[]{0,1,3,1}),  
        new (2,new uint[]{0,1,1,1}),new (1,new uint[]{0,1,1,3,3}),     new (4,new uint[]{0,1,3,5,13}),  new (2,new uint[]{0,1,1,5,5,17}),
        new (4,new uint[]{0,1,1,5,5,5}),  new (4,new uint[]{0,1,1,7,11,19}), new (7,new uint[]{0,1,1,5,1,1}), new (11,new uint[]{0,1,1,1,3,11}),
        new (13,new uint[]{0,1,3,5,5,31}), new (14,new uint[]{0,1,3,3,9,7,49}), new (1,new uint[]{0,1,1,1,15,21,21}), new (13,new uint[]{0,1,3,1,13,27,49}),
    };
   
    public static float2[] Sobol2D(uint _size)
    {
        var N = _size;
        float2[] points = new float2[N];
        var C = new uint[N];
        for (int i = 0; i < N; i++)
        {
            C[i] = 1;
            var value = i;
            while ((value & 1) > 0)
            {
                value >>= 1;
                C[i]++;
            }
        }

        var L = (uint) math.ceil(math.log(N) / math.log(2.0f));
        var V = new uint[L + 1];
        for (int i = 1; i <= L; i++) V[i] = 1u << (32 - i);

        var X = new uint[N];
        X[0] = 0;
        for (uint i = 1u; i < N; i++)
        {
            X[i] = X[i - 1] ^ V[C[i - 1]];
            points[i].x = X[i] / kSobolMaxValue;
        }

        var matrix = kSobelMatrices[1];
        var a = matrix.a;
        var m = matrix.m;
        var s = m.Length - 1;
        if (L <= s) {
            for (int i=1;i<=L;i++) V[i] = m[i] << (32-i); 
        }
        else {
            for (int i=1;i<=s;i++) V[i] = m[i] << (32-i); 
            for (int i = s+1; i <= L; i++)
            {
                V[i] = V[i-s] ^ (V[i-s] >> s); 
                for (int k=1;k<=s-1;k++) 
                    V[i] ^= (((a >> (s-1-k)) & 1) * V[i-k]); 
            }
        }

        for (uint i = 1; i < N; i++)
        {
            X[i] = X[i-1] ^ V[C[i-1]];
            points[i].y = X[i] /kSobolMaxValue;
        }
        
        return points;
    }

    private static List<float2> kCheckList =  new List<float2>();
    private static MultiHashMap<int2,float2> kSamplePoints = new MultiHashMap<int2,float2>();
    public static IList<float2> PoissonDisk2D(int _maxCount,int _k = 30,IRandomGenerator _seed = null,Func<float2,float> _getRadiusNormalized = null) => PoissonDisk2D(1f,sqrt(_maxCount),_k,_seed,_getRadiusNormalized);
    public static IList<float2> PoissonDisk2D(float _radius,float2 _gridSize,int _k = 30,IRandomGenerator _seed = null,Func<float2,float> _getRadiusNormalized = null)
    {
        if (_gridSize.anyLesser(0f))
        {
            Debug.LogError($"[LDS PoissonDisk] Invalid spacing:{_gridSize}");
            return Array.Empty<float2>();
        }
        var r = _radius;
        if (r < 1f)
        {
            _gridSize *= 1f / r;
            r = 1f;
        }
        var k = _k;
        
        var initialPoint = new float2(URandom.Random01(_seed) , URandom.Random01(_seed) ) * _gridSize;
        
        kCheckList.Clear();
        kSamplePoints.Clear();
        
        kCheckList.Add(initialPoint);
        kSamplePoints.Add((int2)floor(initialPoint), initialPoint);
        
        while (kCheckList.Count > 0)     //Optimize with spatial hashmap
        {
            var activeIndex = URandom.RandomInt(kCheckList.Count - 1,_seed);
            var activePoint = kCheckList[activeIndex];

            var found = false;
            for (var i = 0; i < k; i++)
            {
                var angle = URandom.Random01(_seed)* PI * 2;
                var direction = new float2(cos(angle), sin(angle));
                var radius = _getRadiusNormalized?.Invoke(activePoint/_gridSize) * r ?? r;
                var distance = URandom.Random01(_seed) * (2 * radius - radius) + radius;
                var newPoint = activePoint + direction * distance;

                if (newPoint.x < 0 || newPoint.x >= _gridSize.x || newPoint.y < 0 || newPoint.y >= _gridSize.y)
                    continue;

                var gridPosition = (int2)floor(newPoint);
                if (!kSamplePoints.GetValues(UTile.GetAxisRange(gridPosition, 2)
                                 .Select(p => new int2(p.x, p.y)))
                                 .All(p => (newPoint - p).sqrmagnitude() > radius * radius))
                    continue;
                found = true;
                kCheckList.Add(newPoint);
                kSamplePoints.Add(gridPosition, newPoint);
                break;
            }

            if (!found)
                kCheckList.RemoveAt(activeIndex);
        }

        return kSamplePoints.Values.Remake(p=>p/_gridSize);
    }

    private static List<float2> kPositionHelper = new();
    public static float2[] BCCLattice2D(float2 _spacing,float _bias = float.Epsilon) // normalized spacing
    {
        if (_spacing.anyLesser(0f))
        {
            Debug.LogError($"[LDS BCCLattice] Invalid spacing:{_spacing}");
            return Array.Empty<float2>();
        }
        kPositionHelper.Clear();
        var halfSpacing = _spacing.x / 2.0f;
        var hasOffset = false;
        var position = kfloat2.zero;
        for (var j = 0; j * _spacing.y <= 1f + _bias; ++j) {
            position.y = j * _spacing.y;

            var offset = hasOffset ? halfSpacing : 0.0f;

            for (var i = 0; i * _spacing.x + offset <= 1f + _bias; ++i) {
                position.x = i * _spacing.x + offset;
                kPositionHelper.Add(position);
            }
            hasOffset = !hasOffset;
        }

        return kPositionHelper.ToArray();
    }
}
