using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static kmath;

namespace Rendering.GI.SphericalHarmonics
{
    //http://www.ppsloan.org/publications/StupidSH36.pdf
    
    public struct SHL2Contribution
    {
        public float l00, l10, l11, l12, l20, l21, l22, l23, l24;
        public SHL2Contribution(float3 _direction) {
            l00 = SHBasis.GetBasis(0, 0,_direction);
            l10 = SHBasis.GetBasis(1, -1,_direction); l11 = SHBasis.GetBasis(1, 0,_direction); l12 = SHBasis.GetBasis(1, 1,_direction);
            l20 = SHBasis.GetBasis(2, -2,_direction); l21 = SHBasis.GetBasis(2, -1,_direction); l22 = SHBasis.GetBasis(2, 0,_direction); l23 = SHBasis.GetBasis(2, 1,_direction); l24 = SHBasis.GetBasis(2, 2,_direction);
        }
        
        public static SHL2Data operator *(SHL2Contribution _contribution, float3 _value) => new() {
            l00 = _contribution.l00 * _value,
            l10 = _contribution.l10 * _value, l11 = _contribution.l11 * _value, l12 = _contribution.l12 * _value,
            l20 = _contribution.l20 * _value, l21 = _contribution.l21 * _value, l22 = _contribution.l22 * _value, l23 = _contribution.l23 * _value, l24 = _contribution.l24 * _value
        };
        
        public static SHL2Data operator *(float3 _value, SHL2Contribution _contribution) => _contribution * _value;
        public static SHL2Contribution operator *(SHL2Contribution _contribution, float _value) => new() {
            l00 = _contribution.l00 * _value,
            l10 = _contribution.l10 * _value, l11 = _contribution.l11 * _value, l12 = _contribution.l12 * _value,
            l20 = _contribution.l20 * _value, l21 = _contribution.l21 * _value, l22 = _contribution.l22 * _value, l23 = _contribution.l23 * _value, l24 = _contribution.l24 * _value
        };
        public static SHL2Contribution operator *(SHL2Contribution _c1, SHL2Contribution _c2) => new() {
            l00 = _c1.l00 * _c2.l00,
            l10 = _c1.l10 * _c2.l10, l11 = _c1.l11 * _c2.l11, l12 = _c1.l12 * _c2.l12,
            l20 = _c1.l20 * _c2.l20, l21 = _c1.l21 * _c2.l21, l22 = _c1.l22 * _c2.l22, l23 = _c1.l23 * _c2.l23, l24 = _c1.l24 * _c2.l24
        };
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
        
        public static SHL2Data Ambient(float3 ambient) => new() {
                l00 = ambient, //Constant.kAmbientNormalizataionFactor * Basis.kB00,    //Equals 1
            };

        public static SHL2Data Direction(float3 _direction, float3 _color) =>  (new SHL2Contribution(_direction) * Constant.kNormalizationConstants) * (_color * Constant.kDirectionNormalizationFactor);

        public static SHL2Data operator *(SHL2Data _data, SHL2Contribution _contribution) => new() {
            l00 = _data.l00 * _contribution.l00,
            l10 = _data.l10 * _contribution.l10, l11 = _data.l11 * _contribution.l11, l12 = _data.l12 * _contribution.l12,
            l20 = _data.l20 * _contribution.l20, l21 = _data.l21 * _contribution.l21, l22 = _data.l22 * _contribution.l22, l23 = _data.l23 * _contribution.l23, l24 = _data.l24 * _contribution.l24,
        };
        public static SHL2Data operator *(SHL2Data _data1, SHL2Data _data2) => new () {
            l00 = _data1.l00 * _data2.l00,
            l10 = _data1.l10 * _data2.l10, l11 = _data1.l11 * _data2.l11, l12 = _data1.l12 * _data2.l12,
            l20 = _data1.l20 * _data2.l20, l21 = _data1.l21 * _data2.l21, l22 = _data1.l22 * _data2.l22, l23 = _data1.l23 * _data2.l23, l24 = _data1.l24 * _data2.l24,
        };
        public static SHL2Data operator *(SHL2Data _data, float3 _color) => new () {
            l00 = _data.l00 * _color,
            l10 = _data.l10 * _color, l11 = _data.l11 * _color, l12 = _data.l12 * _color,
            l20 = _data.l20 * _color, l21 = _data.l21 * _color, l22 = _data.l22 * _color, l23 = _data.l23 * _color, l24 = _data.l24 * _color,
        };
        public static SHL2Data operator *(SHL2Data _data, float _value) => new () {
            l00 = _data.l00 * _value,
            l10 = _data.l10 * _value, l11 = _data.l11 * _value, l12 = _data.l12 * _value,
            l20 = _data.l20 * _value, l21 = _data.l21 * _value, l22 = _data.l22 * _value, l23 = _data.l23 * _value, l24 = _data.l24 * _value,
        };
        
        public static SHL2Data operator /(SHL2Data _data, float _value) => new () {
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
            SphericalHarmonicsL2Utils.GetL2(_data,out var l20, out var l21, out var l22, out var l23, out var l24);
            return new SHL2Data() {
                l00 = SphericalHarmonicsL2Utils.GetCoefficient(_data,0),
                l10 = SphericalHarmonicsL2Utils.GetCoefficient(_data,1), l11 = SphericalHarmonicsL2Utils.GetCoefficient(_data,2), l12 = SphericalHarmonicsL2Utils.GetCoefficient(_data,3),
                l20 = l20, l21 = l21, l22 = l22, l23 = l23, l24 = l24,
            };
        }

        public static readonly SHL2Data kZero = new SHL2Data();
        public static readonly SHL2Data kDefault = new SHL2Data();
    }
}