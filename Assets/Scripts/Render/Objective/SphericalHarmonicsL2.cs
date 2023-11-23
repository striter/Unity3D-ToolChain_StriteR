using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.GI.SphericalHarmonics
{
    public static class SHBasis
    {
        public static readonly float kL00 = 0.5f * Mathf.Sqrt(1.0f / kmath.kPI); //Constant

        //L1
        static readonly float kL1P = Mathf.Sqrt(3f / (4f * Mathf.PI));
        public static readonly float kL1N1 = kL1P; //*y
        public static readonly float kL10 = kL1P; //*z
        public static readonly float kL1P1 = kL1P; //*x

        //L2
        static readonly float kL2P = Mathf.Sqrt(15f / Mathf.PI);
        public static readonly float kL2N2 = 0.5f * kL2P;
        public static readonly float kL2N1 = 0.5f * kL2P;
        public static readonly float kL20 = 0.25f * Mathf.Sqrt(5f / Mathf.PI);
        public static readonly float kL2P1 = 0.5f * kL2P;
        public static readonly float kL2P2 = 0.25f * kL2P;
    }

    public partial struct SHL2Output
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
                l10 = new float3(shAr.x / SHBasis.kL1N1, shAg.x / SHBasis.kL1N1, shAb.x / SHBasis.kL1N1),
                l11 = new float3(shAr.y / SHBasis.kL10, shAg.y / SHBasis.kL10, shAb.y / SHBasis.kL10),
                l12 = new float3(shAr.z / SHBasis.kL1P1, shAg.z / SHBasis.kL1P1, shAb.z / SHBasis.kL1P1),
                l20 = new float3(shBr.x / SHBasis.kL2N2, shBg.x / SHBasis.kL2N2, shBb.x / SHBasis.kL2N2),
                l21 = new float3(shBr.y / SHBasis.kL2N1, shBg.y / SHBasis.kL2N1, shBb.y / SHBasis.kL2N1),
                l22 = new float3(shBr.z / SHBasis.kL20, shBg.z / SHBasis.kL20, shBb.z / SHBasis.kL20),
                l23 = new float3(shBr.w / SHBasis.kL2P1, shBg.w / SHBasis.kL2P1, shBb.w / SHBasis.kL2P1),
                l24 = new float3(shC.x, shC.y, shC.z) / SHBasis.kL2P2,
            };
        }
    }
    
    [Serializable]
    public partial struct SHL2Data
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

        public float3 Evaluate(float3 _normalizedPosition)
        {
            return l00 * SHBasis.kL00 +
                   l10 * SHBasis.kL1N1 * _normalizedPosition.z +
                   l11 * SHBasis.kL10 * _normalizedPosition.y +
                   l12 * SHBasis.kL1P1 * _normalizedPosition.x +
                   l20 * SHBasis.kL2N2 * _normalizedPosition.x * _normalizedPosition.y +
                   l21 * SHBasis.kL2N1 * _normalizedPosition.y * _normalizedPosition.z +
                   l22 * SHBasis.kL20 * (-_normalizedPosition.x * _normalizedPosition.x - _normalizedPosition.y * _normalizedPosition.y + 2 * _normalizedPosition.z * _normalizedPosition.z) +
                   l23 * SHBasis.kL2P1 * _normalizedPosition.z * _normalizedPosition.x +
                   l24 * SHBasis.kL2P2 * (_normalizedPosition.x * _normalizedPosition.x - _normalizedPosition.y * _normalizedPosition.y);
        }
        
        public SHL2Output Output()
        {
            return new SHL2Output()
            {
                shAr = new float4(SHBasis.kL1N1 * l10.x, SHBasis.kL10 * l11.x, SHBasis.kL1P1 * l12.x, SHBasis.kL00 * l00.x),
                shAg = new float4(SHBasis.kL1N1 * l10.y, SHBasis.kL10 * l11.y, SHBasis.kL1P1 * l12.y, SHBasis.kL00 * l00.y),
                shAb = new float4(SHBasis.kL1N1 * l10.z, SHBasis.kL10 * l11.z, SHBasis.kL1P1 * l12.z, SHBasis.kL00 * l00.z),
                shBr = new float4(SHBasis.kL2N2 * l20.x, SHBasis.kL2N1 * l21.x, SHBasis.kL20 * l22.x, SHBasis.kL2P1 * l23.x),
                shBg = new float4(SHBasis.kL2N2 * l20.y, SHBasis.kL2N1 * l21.y, SHBasis.kL20 * l22.y, SHBasis.kL2P1 * l23.y),
                shBb = new float4(SHBasis.kL2N2 * l20.z, SHBasis.kL2N1 * l21.z, SHBasis.kL20 * l22.z, SHBasis.kL2P1 * l23.z),
                shC = l24 * SHBasis.kL2P2,
            };
        }

        public static SHL2Data Interpolate(SHL2Data _a, SHL2Data _b, float _interpolate)
        {
            return new SHL2Data()
            {
                l00 = math.lerp(_a.l00, _b.l00, _interpolate),
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
        public static SHL2Data operator *(SHL2Data _a, float3 _b) => new SHL2Data()
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

        public static implicit operator SHL2Data(SphericalHarmonicsL2 _l2)
        {
            SphericalHarmonicsL2Utils.GetL1(_l2,out var l10, out var l11, out var l12);
            SphericalHarmonicsL2Utils.GetL2(_l2,out var l20, out var l21, out var l22, out var l23, out var l24);
            return new SHL2Data()
            {
                l00 = SphericalHarmonicsL2Utils.GetCoefficient(_l2,0),
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
    }
}