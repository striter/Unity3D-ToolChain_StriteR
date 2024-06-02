using System;
using Unity.Mathematics;

namespace Rendering.GI.SphericalHarmonics
{
    using static kmath;
    public static class Constant
    {
        public const float kDirectionNormalizationFactor = 16f * kPI / 17f;
        public const float kAmbientNormalizataionFactor = 2 * kSQRTPi;
        private static readonly float kC0 = 1f / (2f *kSQRTPi);
        private static readonly float kC1 = kSQRT3 / (3f * kSQRTPi);
        private static readonly float kC2 = kSQRT15 / (8f * kSQRTPi);
        private static readonly float kC3 = kSQRT5 / (16f * kSQRTPi);
        private static readonly float kC4 = 0.5f * kC2;
        public static readonly SHL2Contribution kNormalizationConstants = new SHL2Contribution(){
            l00 = kC0,
            l10 = -kC1, l11 = kC1, l12 = -kC1,
            l20 = kC2, l21 = -kC2, l22 = kC3, l23 = -kC2, l24 = kC4
        };
    }
    
    public static class SHBasis
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
    }
}