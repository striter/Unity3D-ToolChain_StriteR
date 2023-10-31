using System;
using Geometry.Explicit;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering.GI.SphericalHarmonics
{
    public class SHShaderProperties
    {
        public readonly int kSHAr;
        public readonly int kSHAg;
        public readonly int kSHAb;
        public readonly int kSHBr;
        public readonly int kSHBg;
        public readonly int kSHBb;
        public readonly int kSHC;
        public SHShaderProperties(string _prefix = "")
        {
            kSHAr = Shader.PropertyToID(_prefix+"_SHAr");
            kSHAg = Shader.PropertyToID(_prefix+"_SHAg");
            kSHAb = Shader.PropertyToID(_prefix+"_SHAb");
            kSHBr = Shader.PropertyToID(_prefix+"_SHBr");
            kSHBg = Shader.PropertyToID(_prefix+"_SHBg");
            kSHBb = Shader.PropertyToID(_prefix+"_SHBb");
            kSHC = Shader.PropertyToID(_prefix+"_SHC");
        }
        
        public static SHShaderProperties kDefault = new SHShaderProperties();
        public static SHShaderProperties kUnity = new SHShaderProperties("unity");
    }
    
    public struct SHL2Output
    {
        public float4 shAr;
        public float4 shAg;
        public float4 shAb;
        public float4 shBr;
        public float4 shBg;
        public float4 shBb;
        public float3 shC;
        
        public SHL2Data PackUp()
        {
            return new SHL2Data()
            {
                l00 = new float3(shAr.w / SHBasis.kL00, shAg.w / SHBasis.kL00, shAb.w / SHBasis.kL00),
                l10 = new float3(shAr.x / SHBasis.kL10, shAg.x / SHBasis.kL10, shAb.x / SHBasis.kL10),
                l11 = new float3(shAr.y / SHBasis.kL11, shAg.y / SHBasis.kL11, shAb.y / SHBasis.kL11),
                l12 = new float3(shAr.z / SHBasis.kL12, shAg.z / SHBasis.kL12, shAb.z / SHBasis.kL12),
                l20 = new float3(shBr.x / SHBasis.kL20, shBg.x / SHBasis.kL20, shBb.x / SHBasis.kL20),
                l21 = new float3(shBr.y / SHBasis.kL21, shBg.y / SHBasis.kL21, shBb.y / SHBasis.kL21),
                l22 = new float3(shBr.z / SHBasis.kL22, shBg.z / SHBasis.kL22, shBb.z / SHBasis.kL22),
                l23 = new float3(shBr.w / SHBasis.kL23, shBg.w / SHBasis.kL23, shBb.w / SHBasis.kL23),
                l24 = new float3(shC.x, shC.y, shC.z) / SHBasis.kL24,
            };
        }

        public void Apply(MaterialPropertyBlock _block,SHShaderProperties _properties)
        {
            _block.SetVector(_properties.kSHAr, shAr);
            _block.SetVector(_properties.kSHAg, shAg);
            _block.SetVector(_properties.kSHAb, shAb);
            _block.SetVector(_properties.kSHBr, shBr);
            _block.SetVector(_properties.kSHBg, shBg);
            _block.SetVector(_properties.kSHBb, shBb);
            _block.SetVector(_properties.kSHC, shC.to4());
        }

        public void ApplyGlobal(SHShaderProperties _properties)
        {
            Shader.SetGlobalVector(_properties.kSHAr, shAr);
            Shader.SetGlobalVector(_properties.kSHAg, shAg);
            Shader.SetGlobalVector(_properties.kSHAb, shAb);
            Shader.SetGlobalVector(_properties.kSHBr, shBr);
            Shader.SetGlobalVector(_properties.kSHBg, shBg);
            Shader.SetGlobalVector(_properties.kSHBb, shBb);
            Shader.SetGlobalVector(_properties.kSHC, shC.to4());
        }
    }
    
    [Serializable]
    public struct SHL2Data
    {
        public float3 l00;
        public float3 l10;
        public float3 l11;
        public float3 l12;
        public float3 l20;
        public float3 l21;
        public float3 l22;
        public float3 l23;
        public float3 l24;

        public SHL2Output Output()
        {
            return new SHL2Output()
            {
                shAr = new float4(SHBasis.kL10 * l10.x, SHBasis.kL11 * l11.x, SHBasis.kL12 * l12.x, SHBasis.kL00 * l00.x),
                shAg = new float4(SHBasis.kL10 * l10.y, SHBasis.kL11 * l11.y, SHBasis.kL12 * l12.y, SHBasis.kL00 * l00.y),
                shAb = new float4(SHBasis.kL10 * l10.z, SHBasis.kL11 * l11.z, SHBasis.kL12 * l12.z, SHBasis.kL00 * l00.z),
                shBr = new float4(SHBasis.kL20 * l20.x, SHBasis.kL21 * l21.x, SHBasis.kL22 * l22.x, SHBasis.kL23 * l23.x),
                shBg = new float4(SHBasis.kL20 * l20.y, SHBasis.kL21 * l21.y, SHBasis.kL22 * l22.y, SHBasis.kL23 * l23.y),
                shBb = new float4(SHBasis.kL20 * l20.z, SHBasis.kL21 * l21.z, SHBasis.kL22 * l22.z, SHBasis.kL23 * l23.z),
                shC = l24 * SHBasis.kL24,
            };
        }

        public static SHL2Data Interpolate(SHL2Data _a, SHL2Data _b, float _interpolate)
        {
            return new SHL2Data()
            {
                l00 = Vector3.Lerp(_a.l00, _b.l00, _interpolate),
                l10 = Vector3.Lerp(_a.l10, _b.l10, _interpolate),
                l11 = Vector3.Lerp(_a.l11, _b.l11, _interpolate),
                l12 = Vector3.Lerp(_a.l12, _b.l12, _interpolate),
                l20 = Vector3.Lerp(_a.l20, _b.l20, _interpolate),
                l21 = Vector3.Lerp(_a.l21, _b.l21, _interpolate),
                l22 = Vector3.Lerp(_a.l22, _b.l22, _interpolate),
                l23 = Vector3.Lerp(_a.l23, _b.l23, _interpolate),
                l24 = Vector3.Lerp(_a.l24, _b.l24, _interpolate),
            };
        }

        public static SHL2Data operator +(SHL2Data _a, SHL2Data _b) => new SHL2Data()
        {
            l00 = _a.l00 + _b.l00,
            l10 = _a.l10 + _b.l10,
            l11 = _a.l11 + _b.l11,
            l12 = _a.l12 + _b.l12,
            l20 = _a.l20 + _b.l20,
            l21 = _a.l21 + _b.l21,
            l22 = _a.l22 + _b.l22,
            l23 = _a.l23 + _b.l23,
            l24 = _a.l24 + _b.l24,
        };

        public static SHL2Data operator *(SHL2Data _a, float _b) => new SHL2Data()
        {
            l00 = _a.l00 * _b,
            l10 = _a.l10 * _b,
            l11 = _a.l11 * _b,
            l12 = _a.l12 * _b,
            l20 = _a.l20 * _b,
            l21 = _a.l21 * _b,
            l22 = _a.l22 * _b,
            l23 = _a.l23 * _b,
            l24 = _a.l24 * _b,
        };
        public static SHL2Data operator /(SHL2Data _a, float _b) => new SHL2Data()
        {
            l00 = _a.l00 / _b,
            l10 = _a.l10 / _b,
            l11 = _a.l11 / _b,
            l12 = _a.l12 / _b,
            l20 = _a.l20 / _b,
            l21 = _a.l21 / _b,
            l22 = _a.l22 / _b,
            l23 = _a.l23 / _b,
            l24 = _a.l24 / _b,
        };

        public static readonly SHL2Data kZero = new SHL2Data();
    }

    public static class SHBasis
    {
        public static readonly float kL00 = 0.5f * Mathf.Sqrt(1.0f / kmath.kPI); //Constant

        //L1
        static readonly float kL1P = Mathf.Sqrt(3f / (4f * Mathf.PI));
        public static readonly float kL10 = kL1P; //*y
        public static readonly float kL11 = kL1P; //*z
        public static readonly float kL12 = kL1P; //*x

        //L2
        static readonly float kL2P = Mathf.Sqrt(15f / Mathf.PI);
        public static readonly float kL20 = 0.5f * kL2P; //*x*y
        public static readonly float kL21 = 0.5f * kL2P; //*y*z
        public static readonly float kL22 = 0.25f * Mathf.Sqrt(5f / Mathf.PI); //*z * z
        public static readonly float kL23 = 0.5f * kL2P; //* z * x
        public static readonly float kL24 = 0.25f * kL2P; //*(x*x - z*z)
        
        public static float SHNormalizationConstant(int band,int order)=>Mathf.Sqrt((2.0f * band + 1.0f) * umath.Factorial(band - order) / (4.0f * Mathf.PI * umath.Factorial(band + order)));
    }

    public static class SphericalHarmonicsExport
    {
        public static SHL2Data CalculateSHL2Contribution(float3 color, float3 direction)
        {
            float x = direction.x;
            float y = direction.y;
            float z = direction.z;
            SHL2Data data = new SHL2Data()
            {
                l00 = color * SHBasis.kL00,
                l10 = color * SHBasis.kL10 * z,
                l11 = color * SHBasis.kL11 * y,
                l12 = color * SHBasis.kL12 * x,

                l20 = color * SHBasis.kL20 * x * y,
                l21 = color * SHBasis.kL21 * y * z,
                l22 = color * SHBasis.kL22 * (-x * x - y * y + 2 * z * z),
                l23 = color * SHBasis.kL23 * z * x,
                l24 = color * SHBasis.kL24 * (x * x - y * y),
            };
            return data;
        }
        
        static SHL2Data SampleData(int _sampleCount,Func<float3, float3> _sampleColor,string _randomSeed)
        {
            SHL2Data data = default;
            var random = _randomSeed == null ? null : new System.Random(_randomSeed.GetHashCode());
            for (int i = 0; i < _sampleCount; i++)
            {
                var randomPos = URandom.RandomDirection(random);
                var color = _sampleColor(randomPos);
                data += CalculateSHL2Contribution(color, randomPos);
            }

            data /= _sampleCount / kmath.kPI4;
            return data;
        }
        
        public static SHL2Data ExportL2Cubemap(int _sampleCount, Cubemap _cubemap,float _intensity, string _randomSeed)
        {
            
            if (!_cubemap.isReadable)
            {
                Debug.LogError($"{_cubemap.name} is Not Readable",_cubemap);
                return default;
            }
            return SampleData(_sampleCount, _p =>
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
                return _cubemap.GetPixel((CubemapFace) index, x, y).ToFloat3()*_intensity;
            },_randomSeed);
        }
        
        public static SHL2Data ExportL2Gradient(Color _top,Color _equator,Color _bottom)
        {
            var topColor = _top.ToFloat3();
            var equatorColor = _equator.ToFloat3();
            var bottomColor = _bottom.ToFloat3();
            //Closest take cause i can't get the source code
            var top = math.lerp(topColor, equatorColor, .5f);
            var bottom = math.lerp( bottomColor,equatorColor, .5f);
            var center = equatorColor * .9f + (bottomColor + equatorColor) * .1f;

            const int sampleCount = 32;
            SHL2Data data = default;
            for (int i = 0; i < sampleCount; i++)
            {
                var randomPos = USphereExplicit.Fibonacci.GetPoint(i, sampleCount);
                float value = randomPos.y;
                var tb = math.lerp(center, top,  math.smoothstep(0, 1, value));
                var color =  math.lerp(tb, bottom,  math.smoothstep(0, 1, -value));
                data += CalculateSHL2Contribution(color, randomPos);
            }

            data /= sampleCount / kmath.kPI4;
            return data;
        }
    }
    
}