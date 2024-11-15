using System.Collections.Generic;
using System.Linq.Extensions;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.SignalProcessing
{
    public static class Fourier
    {
        static List<float> kTransformHelper = new List<float>();
        public static IEnumerable<cfloat2> DFT(IEnumerable<float> _input,int _coefficients) => DFT(_input.FillList(kTransformHelper),_coefficients);
        
        public static IEnumerable<cfloat2> DFT(IList<float> _input,int _coefficients)
        {
            _coefficients = _coefficients == -1 ? _input.Count : _coefficients;
            var N = _input.Count;
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

        static List<cfloat2> kInvTransformHelper = new List<cfloat2>();
        public static float IDFT(IEnumerable<cfloat2> _coefficients,int _N,float _value) => IDFT(_coefficients.FillList(kInvTransformHelper),_N,_value);
        public static float IDFT(IList<cfloat2> _coefficients, int _N, float _value)
        {
            var n = _value * _N;
            var k = _coefficients.Count;
            var result = 0f;
            for (var i = 0; i < k; i++)
            {
                var w = (i==0||i==k - 1)?1.0f:2.0f;
                var an = cfloat2.exp(kmath.kPI2 / _N * i * n * cfloat2.iOne);
                result += w * math.dot(_coefficients[i], an);
            }
            return result / _N;
        }
    }
}