using System;
using System.Collections.Generic;
using Procedural.Tile;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Explicit
{
    using static math;
    using static kmath;
    using static umath;
    public static class USphereExplicit
    {
        public static float3 CubeToSpherePosition(float3 _point)
        {
            var sqrP = _point * _point;
            return _point * sqrt(1f - (sqrP.yxx + sqrP.zzy) / 2f + sqrP.yxx * sqrP.zzy / 3f);
        }
        
        public static float2 SpherePositionToUV(float3 _point,bool _poleValidation=false)
        {
            var texCoord=new float2(atan2(_point.x, -_point.z) / -kPI2, math.asin(_point.y) / kPI)+.5f;
            if (_poleValidation&&texCoord.x<1e-6f)
                texCoord.x = 1f;
            return texCoord;
        }
        
        public static class Cube
        {
            public static int GetQuadCount(int _resolution) =>  _resolution * _resolution * UCubeExplicit.kCubeFacingAxisCount;
            
            public static int GetVertexCount(int _resolution)=> (_resolution + 1) * (_resolution + 1) +
                    (_resolution + 1) * _resolution +
                    _resolution * _resolution +
                    _resolution * _resolution +
                    (_resolution - 1) * (_resolution) +
                    (_resolution - 1) * (_resolution - 1);

            public static int GetVertexIndex(int _i,int _j,int _resolution,int _sideIndex)
            {
                bool firstColumn = _j == 0;
                bool lastColumn = _j == _resolution;
                bool firstRow = _i == 0;
                bool lastRow = _i == _resolution;
                int index = -1;
                if (_sideIndex == 0)
                {
                    index = new Int2(_i, _j).ToIndex(_resolution + 1);
                }
                else if (_sideIndex == 1)
                {
                    if (firstColumn)
                        index = GetVertexIndex(_j, _i,_resolution, 0);
                    else
                        index = (_resolution + 1) * (_resolution + 1) + new Int2(_i, _j - 1).ToIndex(_resolution + 1);
                }
                else if (_sideIndex == 2)
                {
                    if (firstRow)
                        index = GetVertexIndex(_j, _i,_resolution, 0);
                    else if (firstColumn)
                        index = GetVertexIndex(_j, _i,_resolution, 1);
                    else
                        index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                                new Int2(_i - 1, _j - 1).ToIndex(_resolution);
                }
                else if (_sideIndex == 3)
                {
                    if (firstColumn)
                        index = GetVertexIndex(_i, _resolution,_resolution, 1);
                    else if (firstRow)
                        index = GetVertexIndex(_resolution, _j,_resolution, 2);
                    else
                        index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                                _resolution * _resolution + new Int2(_i - 1, _j - 1).ToIndex(_resolution);
                }
                else if (_sideIndex == 4)
                {
                    if (firstColumn)
                        index = GetVertexIndex(_i, _resolution,_resolution, 2);
                    else if (firstRow)
                        index = GetVertexIndex(_resolution, _j,_resolution, 0);
                    else if (lastRow)
                        index = GetVertexIndex(_j, _resolution,_resolution, 3);
                    else
                        index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                                _resolution * _resolution + (_resolution * _resolution) +
                                new Int2(_i - 1, _j - 1).ToIndex(_resolution - 1);
                }
                else if (_sideIndex == 5)
                {
                    if (firstColumn)
                        index = GetVertexIndex(_i, _resolution,_resolution, 0);
                    else if (lastColumn)
                        index = GetVertexIndex(_resolution, _i,_resolution, 3);
                    else if (firstRow)
                        index = GetVertexIndex(_resolution, _j,_resolution, 1);
                    else if (lastRow)
                        index = GetVertexIndex(_j, _resolution,_resolution, 4);
                    else
                        index = (_resolution + 1) * (_resolution + 1) + (_resolution + 1) * _resolution +
                                _resolution * _resolution + (_resolution * _resolution) + (_resolution - 1) * (_resolution) +
                                new Int2(_i - 1, _j - 1).ToIndex(_resolution - 1);
                }

                return index;
            }
            
        }


        public static class LowDiscrepancySequences
        {
            public static float3 Fibonacci(int _index,int _count) 
            {
                float j = _index + .5f;
                float phi = acos(1f - 2f * j / _count);
                float theta = kPI2 * j / kGoldenRatio;
        
                sincos(theta,out var sinT,out var cosT);
                sincos(phi,out var sinP,out var cosP);
                return new float3(cosT  * sinP, sinT * sinP ,cosP);
            }

            public static float3 Hammersley(uint _index, uint _count)
            {
                var hammersley2D = ULowDiscrepancySequences.Hammersley2D(_index,_count);
        
                float phi = 2f * kPI * hammersley2D.x;
                float cosTheta = 1f - 2f * hammersley2D.y;
                float sinTheta = sqrt(1f - cosTheta * cosTheta);
        
                float x = sinTheta * cos(phi);
                float y = sinTheta * sin(phi);
                float z = cosTheta;
        
                return new float3(x, y, z);
            }
        }
        

    }
}