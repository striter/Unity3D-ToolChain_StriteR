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
            shData = SphericalHarmonicsExport.ExportL2Gradient(gradientSky, gradientEquator,gradientGround);
            return this;
        }

        public static readonly SHGradient kDefault = new SHGradient()
        {
            gradientSky = Color.cyan,
            gradientGround = Color.black,
            gradientEquator = Color.white.SetA(.5f),
        }.Ctor();
        
        public static SHGradient Interpolate(SHGradient _a, SHGradient _b,
            float _interpolate)
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
        static SHL2Data CalculateSHL2Contribution(float3 direction)
        {
            float x = direction.x;
            float y = direction.y;
            float z = direction.z;
            SHL2Data data = new SHL2Data()
            {
                l00 = SHBasis.kL00,
                
                l10 = SHBasis.kL1N1 * z,
                l11 = SHBasis.kL10 * y,
                l12 = SHBasis.kL1P1 * x,

                l20 = SHBasis.kL2N2 * x * y,
                l21 = SHBasis.kL2N1 * y * z,
                l22 = SHBasis.kL20 * (-x * x - y * y + 2 * z * z),
                l23 = SHBasis.kL2P1 * z * x,
                l24 = SHBasis.kL2P2 * (x * x - y * y),
            };
            return data;
        }

        
        static SHL2Data ExportSample(ESHSampleMode _mode,int _sampleCount,Func<float3, float3> _sampleColor,string _randomSeed = null)
        {
            SHL2Data data = default;
            var random = _randomSeed == null ? null : new System.Random(_randomSeed.GetHashCode());
            for (int i = 0; i < _sampleCount; i++)
            {
                switch (_mode)
                {
                    default: throw new InvalidEnumArgumentException();
                    case ESHSampleMode.Random:
                    {
                        var randomPos = URandom.RandomDirection(random);
                        var color = _sampleColor(randomPos);
                        data += CalculateSHL2Contribution(randomPos) *  color;
                    }
                        break;
                    case ESHSampleMode.Fibonacci:
                    {
                        var randomPos = ULowDiscrepancySequences.FibonacciSphere(i, _sampleCount);
                        data += CalculateSHL2Contribution(randomPos) * _sampleColor(randomPos);
                    }
                        break;
                }
            }

            data /= _sampleCount / kmath.kPIMul4;
            return data;
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
                float xAbs = Mathf.Abs(_p.x);
                float yAbs = Mathf.Abs(_p.y);
                float zAbs = Mathf.Abs(_p.z);
                int index;
                Vector2 uv = new Vector2();
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

                uv = (uv + Vector2.one) / 2f;
                int width = _cubemap.width - 1;
                int x = (int) (width * uv.x);
                int y = (int) (width * uv.y);
                return _cubemap.GetPixel((CubemapFace) index, x, y).to3()*_intensity;
            },_randomSeed);
        }
        
        public static SHL2Data ExportL2Gradient(Color _top,Color _equator,Color _bottom)
        {
            var topColor = _top.to3();
            var equatorColor = _equator.to3();
            var bottomColor = _bottom.to3();
            //Closest take cause i can't get the source code
            var top = math.lerp(topColor, equatorColor, .5f);
            var bottom = math.lerp( bottomColor,equatorColor, .5f);
            var center = equatorColor * .9f + (bottomColor + equatorColor) * .1f;
            return ExportSample(ESHSampleMode.Fibonacci,16, p =>
            {
                float value = p.y;
                var tb = math.lerp(center, top,  math.smoothstep(0, 1, value));
                var color =  math.lerp(tb, bottom,  math.smoothstep(0, 1, -value));
                return color;
            });
        }

        public static SHL2Data ExportDirectionalLight(float3 _direction, float3 _color,bool _clamp) => ExportSample(ESHSampleMode.Fibonacci, 32, p =>
        {
            var color = math.dot(p, -_direction);
            if (_clamp)
                color = math.max(0, color);
            return color;
        })*_color;

    }
}