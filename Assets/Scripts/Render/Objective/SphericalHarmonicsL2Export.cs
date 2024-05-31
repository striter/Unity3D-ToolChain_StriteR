using System;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

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
            shData = SphericalHarmonicsExport.ExportL2Gradient(gradientSky.to3(), gradientEquator.to3(),gradientGround.to3());
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
    public class SHL2ShaderProperties
    {
        public readonly int kSHAr;
        public readonly int kSHAg;
        public readonly int kSHAb;
        public readonly int kSHBr;
        public readonly int kSHBg;
        public readonly int kSHBb;
        public readonly int kSHC;
        public SHL2ShaderProperties(string _prefix = "")
        {
            kSHAr = Shader.PropertyToID(_prefix+"_SHAr");
            kSHAg = Shader.PropertyToID(_prefix+"_SHAg");
            kSHAb = Shader.PropertyToID(_prefix+"_SHAb");
            kSHBr = Shader.PropertyToID(_prefix+"_SHBr");
            kSHBg = Shader.PropertyToID(_prefix+"_SHBg");
            kSHBb = Shader.PropertyToID(_prefix+"_SHBb");
            kSHC = Shader.PropertyToID(_prefix+"_SHC");
        }
        
        public static SHL2ShaderProperties kDefault = new SHL2ShaderProperties();
        public static SHL2ShaderProperties kUnity = new SHL2ShaderProperties("unity");
        
        public void Apply(MaterialPropertyBlock _block,SHL2Output _output)
        {
            _block.SetVector(kSHAr, _output.shAr);
            _block.SetVector(kSHAg, _output.shAg);
            _block.SetVector(kSHAb, _output.shAb);
            _block.SetVector(kSHBr, _output.shBr);
            _block.SetVector(kSHBg, _output.shBg);
            _block.SetVector(kSHBb, _output.shBb);
            _block.SetVector(kSHC, _output.shC.to4());
        }

        public void ApplyGlobal(SHL2Output _output)
        {
            Shader.SetGlobalVector(kSHAr, _output.shAr);
            Shader.SetGlobalVector(kSHAg, _output.shAg);
            Shader.SetGlobalVector(kSHAb, _output.shAb);
            Shader.SetGlobalVector(kSHBr, _output.shBr);
            Shader.SetGlobalVector(kSHBg, _output.shBg);
            Shader.SetGlobalVector(kSHBb, _output.shBb);
            Shader.SetGlobalVector(kSHC, _output.shC.to4());
        }

        public SHL2Output FetchGlobal()
        {
            return new SHL2Output()
            {
                shAr = Shader.GetGlobalVector(kSHAr),
                shAg = Shader.GetGlobalVector(kSHAg),
                shAb = Shader.GetGlobalVector(kSHAb),
                shBr = Shader.GetGlobalVector(kSHBr),
                shBg = Shader.GetGlobalVector(kSHBg),
                shBb = Shader.GetGlobalVector(kSHBb),
                shC = ((float4)Shader.GetGlobalVector(kSHC)).to3xyz(),
            };
        }
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
                        data += SHL2Data.Contribution(randomPos) *  color;
                    }
                        break;
                    case ESHSampleMode.Fibonacci:
                    {
                        var randomPos = ULowDiscrepancySequences.FibonacciSphere(i, _sampleCount);
                        data += SHL2Data.Contribution(randomPos) * _sampleColor(randomPos);
                    }
                        break;
                }
            }

            return data * (kmath.kPI4 / _sampleCount);
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
        
        public static SHL2Data ExportL2Gradient(float3 _top,float3 _equator,float3 _bottom)
        {
            var eq = _equator;
            var sky = _top - eq;
            var ground = _bottom - eq;
            
            return SHL2Data.Ambient(eq)
                + SHL2Data.Direction(kfloat3.up,sky)
                + SHL2Data.Direction(kfloat3.down,ground);
        }

    }
}