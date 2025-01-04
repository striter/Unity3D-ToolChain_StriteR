using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.SignalProcessing
{
    //https://iquilezles.org/articles/fourier/
    public static class Fourier
    {
        static List<cfloat2> kComplexFiller = new List<cfloat2>();
        public static IEnumerable<cfloat2> DFT(IEnumerable<float> _input,int _coefficients) => DFT(_input.Select(p=>new cfloat2(p,0)),_coefficients);
        public static IEnumerable<cfloat2> DFT(IEnumerable<cfloat2> _input,int _coefficients) => DFT(_input.FillList(kComplexFiller),_coefficients);
        public static IEnumerable<cfloat2> DFT(IList<cfloat2> _input,int _coefficients)
        {
            var N = _input.Count;
            _coefficients = _coefficients == -1 ? N : _coefficients;
            for (var k = 0; k < _coefficients; k++)
            {
                var fc = cfloat2.zero;
                for (var n = 0; n < N; n++)
                {
                    var input = _input[n]; 
                    var an = cfloat2.exp(-kmath.kPI2 / N * k * n* cfloat2.iOne);
                    fc += input * an;
                }

                yield return fc;
            }
        }

        //https://rosettacode.org/wiki/Fast_Fourier_transform
        public static bool FFT(IEnumerable<float> _input,cfloat2[] _output) => FFT(_input.Select(p=>new cfloat2(p,0)).FillList(kComplexFiller),_output);
        public static bool FFT(IList<cfloat2> _input,cfloat2[] _output)
        {
            if (!math.ispow2(_input.Count))
            {
                Debug.LogWarning($"Input Count {_input.Count} Not a power of 2");
                return false;
            }
            
            var bits = (int)Math.Log(_input.Count,2);
            for (var j = 1; j < _input.Count; j++)
            {
                var swapPos = UBitwise.Reverse(j, bits);
                if (swapPos <= j)
                    continue;
                (_input[j], _input[swapPos]) = (_input[swapPos], _input[j]);
            }

            for (var N = 2; N <= _input.Count; N <<= 1)
            {
                for (var i = 0; i < _input.Count; i += N)
                {
                    for (var k = 0; k < N / 2; k++)
                    {
                        var evenIndex = i + k;
                        var oddIndex = i + k + (N / 2);
                        var even = _input[evenIndex];
                        var odd = _input[oddIndex];

                        var term = -2 * kmath.kPI * k / N;
                        umath.sincos_fast(term,out var sin,out var cos);
                        var exp = new cfloat2(cos,sin) * odd;

                        _output[evenIndex] = even + exp;
                        _output[oddIndex] = even - exp;
                    }
                }
            }
            return true;
        }
        
        public static float IFT(IEnumerable<cfloat2> _coefficients,int _N,float _value) => IFT(_coefficients.FillList(kComplexFiller),_N,_value);
        public static float IFT(IList<cfloat2> _coefficients, int _N, float _value)
        {
            var n = _value * _N;
            var k = _coefficients.Count;
            var result = 0f;
            for (var i = 0; i < k; i++)
            {
                var w = ( i == 0 || i == k - 1 )?1.0f:2.0f;
                var an = cfloat2.exp(kmath.kPI2 / _N * i * n * cfloat2.iOne);
                result += w * math.dot(_coefficients[i], an);
            }
            return result / _N;
        }

    }
}