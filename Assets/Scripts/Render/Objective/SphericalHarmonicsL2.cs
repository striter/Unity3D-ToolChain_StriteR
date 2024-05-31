using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static kmath;

namespace Rendering.GI.SphericalHarmonics
{
    using static SphericalHarmonics;
    //http://www.ppsloan.org/publications/StupidSH36.pdf
    public class SphericalHarmonics
    {
        public static class Constant
        {
            public const float kDirectionNormalizationFactor = 16f * kPI / 17f;
            public const float kAmbientNormalizataionFactor = 2 * kSQRTPi;
        }
        
        public static class Basis
        {
            public const int kAvailableBands = 3;
            private static Func<float3, float>[][] kPolynomialBasis = new Func<float3, float>[kAvailableBands+ 1][]{
                new Func<float3,float>[]
                {
                    _ => 1f/(2*kSQRTPi)
                },
                new Func<float3,float>[]
                {
                    p => -kSQRT3*p.y/(2*kSQRTPi),
                    p => kSQRT3*p.z/(2*kSQRTPi),
                    p => -kSQRT3*p.x/(2*kSQRTPi),
                },
                new Func<float3,float>[]{
                    p => kSQRT15 * p.y * p.x/(2*kSQRTPi),
                    p => -kSQRT15 * p.y * p.z /(2*kSQRTPi),
                    p => kSQRT5*(3 * p.z * p.z - 1) / (4*kSQRTPi),
                    p => -kSQRT15 * p.x * p.z /(2*kSQRTPi),
                    p => kSQRT15 * (p.x * p.x - p.y*p.y) / (4*kSQRTPi),
                },
                new Func<float3, float>[] {
                    p => -(kSQRT2 * kSQRT35*p.y*(3*p.x*p.x -p.y*p.y))/(8*kSQRTPi),
                    p => kSQRT105 * p.y*p.x*p.z / (2*kSQRTPi),
                    p => -(kSQRT2 * kSQRT21 * p.y *(-1 + 5*p.z*p.z)) / (8*kSQRTPi),
                    p => kSQRT7 * p.z *(5*p.z*p.z - 3) / ( 4 * kSQRTPi),
                    p => -(kSQRT2 * kSQRT21 * p.x * (-1 + 5*p.z*p.z)) / (8*kSQRTPi),
                    p => kSQRT105 * (p.x*p.x - p.y*p.y)*p.z / (4*kSQRTPi),
                    p=> -(kSQRT2 * kSQRT35 * p.x * (p.x*p.x - 3*p.y*p.y)) / (8*kSQRTPi),
                },
            };

            public static Func<float3, float> GetBasisFunction(int _band, int _basis)
            {
                if(_band >= kPolynomialBasis[_band].Length)
                    throw new Exception($"Invalid band {_band}");
                
                var functions = kPolynomialBasis[_band];
                var index = (_basis + functions.Length / 2);
                if(index >= functions.Length)
                    throw new Exception($"Invalid basis {_basis}");

                return functions[index];
            }

            public static float GetBasis(int _band, int _basis,float3 _position) => GetBasisFunction(_band,_basis)(_position);
            
            private static readonly float kC0 = 1f / (2f *kSQRTPi);
            private static readonly float kC1 = kSQRT3 / (3f * kSQRTPi);
            private static readonly float kC2 = kSQRT15 / (8f * kSQRTPi);
            private static readonly float kC3 = kSQRT5 / (16f * kSQRTPi);
            private static readonly float kC4 = 0.5f * kC2;
            
            public static readonly float kB00 = kC0;
            public static readonly float kB1N1 = -kC1,kB10 = kC1,kB1P1 = -kC1;
            public static readonly float kB2N2 = kC2,kB2N1 = -kC2,kB20 = kC3,kB2P1 = -kC2,kB2P2 = kC4;
        }


    }
    

    [Serializable]
    public struct SHL2Output
    {
        public float4 shAr;
        public float4 shAg;
        public float4 shAb;
        public float4 shBr;
        public float4 shBg;
        public float4 shBb;
        public float3 shC;
    }

    [Serializable]
    public struct SHL2Data
    {
        [Header("Band 0")]
        public float3 l00;

        [Header("Band 1")] 
        public float3 l10;
        public float3 l11;
        public float3 l12;

        [Header("Band 2")] 
        public float3 l20;
        public float3 l21;
        public float3 l22;
        public float3 l23;
        public float3 l24;

        public float3 this[int _index] => _index switch {
            0 => l00,
            1 => l10, 2 => l11, 3 => l12,
            4 => l20, 5 => l21, 6 => l22, 7 => l23, 8 => l24,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        public SHL2Output Output()
        {
            var diffuseConstant = l00 - l22;
            var quadraticDiffuseConstant = l22 * 3;
            return new SHL2Output() {
                shAr = new float4(l12.x,l10.x,l11.x,diffuseConstant.x),
                shAg = new float4(l12.y,l10.y,l11.y,diffuseConstant.y),
                shAb = new float4(l12.z,l10.z,l11.z,diffuseConstant.z),
                shBr = new float4(l20.x,l21.x,quadraticDiffuseConstant.x,l23.x),
                shBg = new float4(l20.y,l21.y,quadraticDiffuseConstant.y,l23.y),
                shBb = new float4(l20.z,l21.z,quadraticDiffuseConstant.z,l23.z),
                shC = l24,
            };
        }
        // public static SHL2Coefficiences PackUp(SHL2Output shOutput)
        // {
        //     //Revert output SHL2Data from above to L2data
        //     var shAr = shOutput.shAr;
        //     var shAg = shOutput.shAg;
        //     var shAb = shOutput.shAb;
        //     var shBr = shOutput.shBr;
        //     var shBg = shOutput.shBg;
        //     var shBb = shOutput.shBb;
        //     var shC = shOutput.shC;
        //
        //     float3 c1n1 = new(shAr.x,shAg.y,shAb.z),c10 = new(shAr.y,shAg.z,shAb.x),c1p1 = new(shAr.z,shAg.x,shAb.y);
        //     float3 c2n2 = new(shBr.x,shBg.y,shBb.z),c2n1 = new(shBr.y,shBg.z,shBb.x),c20 = new float3( shBr.z,shBg.x,shBb.y) / 3,c2p1 = new(shBr.w,shBg.w,shBb.w), c2p2= shC;
        //     var c00 = new float3(shAr.w, shAg.w, shAb.z) + c20;
        //     
        //     return new SHL2Coefficiences() {
        //         c00 = c00,
        //         c1n1 = c1n1, c10 = c10, c1p1 = c1p1,
        //         c2n2 = c2n2, c2n1 = c2n1, c20 = c20, c2p1 = c2p1, c2p2 = c2p2
        //     };
        // }
        public static SHL2Data Ambient(float3 ambient) => new() {
                l00 = ambient, //Constant.kAmbientNormalizataionFactor * Basis.kB00,    //Equals 1
            };

        public static SHL2Data Direction(float3 _direction,float3  _color) => Contribution(_direction)* (_color * Constant.kDirectionNormalizationFactor);
        public static SHL2Data Contribution(float3 _direction) 
        {
            Func<int,int, Func<float3,float>> basis = Basis.GetBasisFunction;
            return new SHL2Data()
            {
                l00 = basis(0, 0)(_direction) * Basis.kB00,
                l10 = basis(1, -1)(_direction) * Basis.kB1N1, l11 = basis(1, 0)(_direction) * Basis.kB10,
                l12 = basis(1, 1)(_direction) * Basis.kB1P1,
                l20 = basis(2, -2)(_direction) * Basis.kB2N2, l21 = basis(2, -1)(_direction) * Basis.kB2N1,
                l22 = basis(2, 0)(_direction) * Basis.kB20, l23 = basis(2, 1)(_direction) * Basis.kB2P1,
                l24 = basis(2, 2)(_direction) * Basis.kB2P2,
            };
        }
        
        public static SHL2Data operator *(SHL2Data _data, float3 _color) => new SHL2Data()
        {
            l00 = _data.l00 * _color,
            l10 = _data.l10 * _color, l11 = _data.l11 * _color, l12 = _data.l12 * _color,
            l20 = _data.l20 * _color, l21 = _data.l21 * _color, l22 = _data.l22 * _color, l23 = _data.l23 * _color, l24 = _data.l24 * _color,
        };
        public static SHL2Data operator /(SHL2Data _data, float _value) => new SHL2Data() {
            l00 = _data.l00 * _value,
            l10 = _data.l10 * _value, l11 = _data.l11 * _value, l12 = _data.l12 * _value,
            l20 = _data.l20 * _value, l21 = _data.l21 * _value, l22 = _data.l22 * _value, l23 = _data.l23 * _value, l24 = _data.l24 * _value,
        };
        
        public static SHL2Data operator +(SHL2Data _data1, SHL2Data _data2) => new SHL2Data()
        {
            l00 = _data1.l00 + _data2.l00,
            l10 = _data1.l10 + _data2.l10, l11 = _data1.l11 + _data2.l11, l12 = _data1.l12 + _data2.l12,
            l20 = _data1.l20 + _data2.l20, l21 = _data1.l21 + _data2.l21, l22 = _data1.l22 + _data2.l22, l23 = _data1.l23 + _data2.l23, l24 = _data1.l24 + _data2.l24,
        };

        public static SHL2Data operator -(SHL2Data _data1, SHL2Data _data2) => new SHL2Data()
        {
            l00 = _data1.l00 - _data2.l00,
            l10 = _data1.l10 - _data2.l10, l11 = _data1.l11 - _data2.l11, l12 = _data1.l12 - _data2.l12,
            l20 = _data1.l20 - _data2.l20, l21 = _data1.l21 - _data2.l21, l22 = _data1.l22 - _data2.l22, l23 = _data1.l23 - _data2.l23, l24 = _data1.l24 - _data2.l24,
        };

        public static SHL2Data Interpolate(SHL2Data _a, SHL2Data _b, float _interpolate) => _a * (1 - _interpolate) + _b * _interpolate;

        public static implicit operator SHL2Data(SphericalHarmonicsL2 _data)
        {
            SphericalHarmonicsL2Utils.GetL1(_data,out var l10, out var l11, out var l12);
            SphericalHarmonicsL2Utils.GetL2(_data,out var l20, out var l21, out var l22, out var l23, out var l24);
            return new SHL2Data()
            {
                l00 = SphericalHarmonicsL2Utils.GetCoefficient(_data,0),
                l10 = l10,
                l11 = l11,
                l12 = l12,
                l20 = l20,
                l21 = l21,
                l22 = l22,
                l23 = l23,
                l24 = l24,
            };
        }

        public static readonly SHL2Data kZero = new SHL2Data();
        public static readonly SHL2Data kDefault = new SHL2Data();
    }
}