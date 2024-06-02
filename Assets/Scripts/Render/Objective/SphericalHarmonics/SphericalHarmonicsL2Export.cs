using System;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rendering.GI.SphericalHarmonics
{
    [Serializable]
    public struct SHGradient
    {
        [ColorUsage(false,true)]public Color gradientSky;
        [ColorUsage(false,true)]public Color gradientEquator;
        [ColorUsage(false,true)]public Color gradientGround;
        [HideInInspector] public SHL2Data shData;

        public SHGradient Ctor()
        {
            shData = SphericalHarmonicsExport.ExportGradient(gradientSky.to3(), gradientEquator.to3(),gradientGround.to3());
            return this;
        }

        public static readonly SHGradient kDefault = new SHGradient()
        {
            gradientSky = Color.cyan,
            gradientGround = Color.black,
            gradientEquator = Color.white.SetA(.5f),
        }.Ctor();
        
        public static SHGradient Interpolate(SHGradient _a, SHGradient _b, float _interpolate)
        {
            return new SHGradient()
            {
                gradientSky = Color.Lerp(_a.gradientSky,_b.gradientSky,_interpolate),
                gradientEquator = Color.Lerp(_a.gradientEquator,_b.gradientEquator,_interpolate),
                gradientGround = Color.Lerp(_a.gradientGround,_b.gradientGround,_interpolate),
                shData = SHL2Data.Interpolate(_a.shData,_b.shData,_interpolate),
            };
        }
    }
    
    public enum ESHSampleMode
    {
        Random,
        Fibonacci,
    }

    public static class SphericalHarmonicsExport
    {
        static SHL2Data ExportSample(ESHSampleMode _mode,int _sampleCount,Func<float3, float3> _sampleColor,string _randomSeed = null)
        {
            SHL2Data data = default;
            var random = _randomSeed == null ? null : new System.Random(_randomSeed.GetHashCode());
            for (var i = 0; i < _sampleCount; i++)
            {
                switch (_mode)
                {
                    default: throw new InvalidEnumArgumentException();
                    case ESHSampleMode.Random:
                    {
                        var randomPos = URandom.RandomDirection(random);
                        var color = _sampleColor(randomPos);
                        data += new SHL2Contribution(randomPos) * Constant.kNormalizationConstants *  color;
                    }
                        break;
                    case ESHSampleMode.Fibonacci:
                    {
                        var randomPos = ULowDiscrepancySequences.FibonacciSphere(i, _sampleCount);
                        data += new SHL2Contribution(randomPos) * Constant.kNormalizationConstants * _sampleColor(randomPos);
                    }
                        break;
                }
            }

            return data * (kmath.kPI4 / _sampleCount) ;
        }
        
        public static SHL2Data ExportL2Cubemap(int _sampleCount, Cubemap _cubemap,float _intensity,ESHSampleMode _mode = ESHSampleMode.Random, string _randomSeed = null)
        {
            if (!_cubemap.isReadable)
            {
                Debug.LogError($"{_cubemap.name} is Not Readable",_cubemap);
                return default;
            }
            return ExportSample(_mode,_sampleCount, _p =>
            {
                var xAbs = Mathf.Abs(_p.x);
                var yAbs = Mathf.Abs(_p.y);
                var zAbs = Mathf.Abs(_p.z);
                var index = 0;
                var uv = float2.zero;
                if (xAbs >= yAbs && xAbs >= zAbs)
                {
                    index = _p.x > 0 ? 0 : 1;
                    uv.x = _p.y / _p.x;
                    uv.y = _p.z / _p.x;
                }
                else if (yAbs >= xAbs && yAbs >= zAbs)
                {
                    index = _p.y > 0 ? 2 : 3;
                    uv.x = _p.x / _p.y;
                    uv.y = _p.z / _p.y;
                }
                else
                {
                    index = _p.z > 0 ? 4 : 5;
                    uv.x = _p.x / _p.z;
                    uv.y = _p.y / _p.z;
                }

                uv = (uv + kfloat2.one) / 2f;
                var width = _cubemap.width - 1;
                var x = (int) (width * uv.x);
                var y = (int) (width * uv.y);
                var color = _cubemap.GetPixel((CubemapFace) index, x, y);
                if (_cubemap.isDataSRGB)
                    color = color.linear;
                return color.to3();
            },_randomSeed) * _intensity;
        }
        
        private static readonly float3[] kCubemapOrthoBases = new float3[6 * 3] {
            new float3(0, 0, -1), new float3(0, -1, 0), new float3(-1, 0, 0),
            new float3(0, 0, 1), new float3(0, -1, 0), new float3(1, 0, 0),
            new float3(1, 0, 0), new float3(0, 0, 1), new float3(0, -1, 0),
            new float3(1, 0, 0), new float3(0, 0, -1), new float3(0, 1, 0),
            new float3(1, 0, 0), new float3(0, -1, 0), new float3(0, 0, -1),
            new float3(-1, 0, 0), new float3(0, -1, 0), new float3(0, 0, 1),
        };
        
        public static SHL2Data ExportCubemap(Cubemap _cubemap,float intensity = 1f)
        {
            if (!_cubemap.isReadable)
            {
                Debug.LogError($"{_cubemap.name} is Not Readable",_cubemap);
                return SHL2Data.kZero;
            }

            var data = SHL2Data.kZero;
            var size = _cubemap.width;
            var coordBias = -1f + 1f / size;
            var coordScale = 2f / size;
            var floatParam = GraphicsFormatUtility.IsHDRFormat(_cubemap.format) ? 1f / 255f : 1f;
            for (var face = 0; face < 6; face++)
            {
                var pixels = _cubemap.GetPixels((CubemapFace)face);
                var faceData = SHL2Data.kZero;
                var weightSum = 0f;
                var basisX = kCubemapOrthoBases[face * 3 + 0];
                var basisY = kCubemapOrthoBases[face * 3 + 1];
                var basisZ = -kCubemapOrthoBases[face * 3 + 2];
                for (var y = 0; y < size; y++)
                {
                    var fy = y * coordScale + coordBias;
                    for (var x = 0; x < size; x++)
                    {
                        var fx = x * coordScale + coordBias;

                        // fx, fy are pixel coordinates in -1..+1 range
                        var ftmp = 1.0f + fx * fx + fy * fy;
                        var linearWeight = 4.0f / (math.sqrt(ftmp) * ftmp);
                        var dir = (basisZ + basisX * fx + basisY * fy).normalize();
                        
                        var color = pixels[y * size + x];
                        if (_cubemap.isDataSRGB)
                            color = color.linear;

                        faceData += new SHL2Contribution(dir) * Constant.kNormalizationConstants * (color.to3() * linearWeight * floatParam);
                        weightSum += linearWeight;
                    }
                }

                faceData /= (kmath.kPI4 / weightSum / 6f);
                data += faceData;
            }
            return data * intensity;
        }
        
        public static SHL2Data ExportGradient(float3 _top,float3 _equator,float3 _bottom)
        {
            var eq = _equator;
            var sky = _top - _equator;
            var ground = _bottom - _equator;
            return SHL2Data.Ambient(eq * .88f) + SHL2Data.Direction(kfloat3.up,sky * .5f) + SHL2Data.Direction(kfloat3.down,ground * .5f);
        }

    }
}