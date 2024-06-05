using Procedural.Tile;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Explicit
{
    using static math;
    using static kmath;
    public static class USphereExplicit
    {
        public static float3 CubeToSpherePosition(float3 _point)
        {
            float3 sqrP = _point * _point;
            return _point * sqrt(1f - (sqrP.yxx + sqrP.zzy) / 2f + sqrP.yxx * sqrP.zzy / 3f);
        }

        public static float2 SpherePositionToUV(float3 _point,bool _poleValidation=false)
        {
            float2 texCoord=new float2(atan2(_point.x, -_point.z) / -kPI2, math.asin(_point.y) / kPI)+.5f;
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
        
        
        public static class UV
        {
            public static float3 GetPoint(float2 _uv)        //uv [0,1)
            {
                float3 position = 0;
                float uvRadius = sin(_uv.y * kPI);
                sincos(kPI2 * _uv.x, out position.z, out position.x);
                position.xz *= uvRadius;
                position.y = -cos(kPI * _uv.y);
                return position;
            }

            public static float3 Octahedral(float2 _uv)
            {
                var sample = 2 * _uv - kfloat2.one;
                var p = new float3(sample.x, sample.y, 1.0f - abs(sample.x) - abs(sample.y));
                if (p.z < 0)
                {
                    var x = p.x;
                    var y = p.y;
                    p.x = (1 - abs(y)) * sign(p.x);
                    p.y = (1 - abs(x)) * sign(p.y);
                }

                return p.normalize();
            }

            public static float3 ConcentricOctahedral(float2 _uv)
            {
                var offset = 2 * _uv - kfloat2.one;
                var u = offset.x;
                var v = offset.y;
                var d = 1 - (abs(u) + abs(v));
                var r = 1 - abs(d);

                var z = sign(d) * (1 - r * r);
                var theta = kPI / 4 * ((abs(v) - abs(u)) / r + 1);
                var sinTheta = sign(v) * sin(theta);
                var cosTheta = sign(u) * cos(theta);
                var radius = sqrt(2 - r * r);
                return new float3(cosTheta * r * radius, sinTheta * r * radius, z);
            }
        }

        public static class Polygon
        {
            public static float3 GetPoint(float2 _uv,Axis _axis,bool _geodesic)        //uv [0,1)
            {
                float3 position = 0;
                if (_geodesic)
                {
                    sincos(kPI*_uv.y,out var sine,out position.y);
                    position += _axis.origin + math.lerp(_axis.uDir * sine, _axis.vDir * sine,_uv.x);
                }
                else
                {
                    float2 posUV = (_uv.x+_uv.y)<=1?_uv:(1f-_uv.yx);
                    position = new float3(0,_uv.x+_uv.y-1, 0) + _axis.origin + posUV.x * _axis.uDir + posUV.y * _axis.vDir;
                }
                return normalize(position);
            }
        }

    }
}