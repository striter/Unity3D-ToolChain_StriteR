using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.SignalProcessing
{
    //https://iquilezles.org/articles/fourier/
    public static class UFourier
    {
        static List<cfloat2> kComplexFiller = new();
        public static class DiscreteFourier
        {
            public static IList<cfloat2> Transform(IEnumerable<float> _time,IList<cfloat2> _frequencies) => Transform(_time.Select(p=>new cfloat2(p,0)),_frequencies);
            public static IList<cfloat2> Transform(IEnumerable<cfloat2> _time,IList<cfloat2> _frequencies) => Transform(_time.FillList(kComplexFiller),_frequencies);
            public static IList<cfloat2> Transform(IList<cfloat2> _time,IList<cfloat2> _frequencies)
            {
                var N = _time.Count;
                if (_frequencies is null)
                {
                    Debug.LogError($"[{nameof(DiscreteFourier)}]: null OutputFrequencies");
                    return null;
                }
                
                var coefficients = _frequencies.Count;
                if (coefficients <= 0 || coefficients > N)
                {
                    Debug.LogError($"[{nameof(DiscreteFourier)}]: Invalid OutputFrequencies {coefficients}");
                    return null;
                }
                for (var k = 0; k < coefficients; k++)
                {
                    var fc = cfloat2.zero;
                    for (var n = 0; n < N; n++)
                    {
                        var input = _time[n]; 
                        var an = cfloat2.exp(-kmath.kPI2 * n * k / N * cfloat2.iOne);
                        fc += input * an;
                    }
                    _frequencies[k] = fc * coefficients / N;
                }
                return _frequencies;
            }

            public static IEnumerable<cfloat2> Inverse(IList<cfloat2> _input,int _count = -1)
            {
                var N = _count == -1 ? _input.Count : _count;
                
                for (var i = 0; i < N; i++)
                    yield return Inverse(_input,(float)i / N);
            }
            
            public static cfloat2 Inverse(IEnumerable<cfloat2> _coefficients,float _value) => Inverse(_coefficients.FillList(kComplexFiller),_value);
            public static cfloat2 Inverse(IList<cfloat2> _coefficients, float _value)
            {
                var N = _coefficients.Count;
                var result = cfloat2.zero;
                for (var k = 0; k < N; k++)
                {
                    var angle = kmath.kPI2 * k * _value * cfloat2.iOne;
                    var exp = cfloat2.exp(angle);
                    result += _coefficients[k]*exp;
                }
                return result / N;
            }
        }

        //https://rosettacode.org/wiki/Fast_Fourier_transform
        public static class CooleyTukeyFastFourier
        {
            public static bool Transform(IList<cfloat2> _input)
            {
                if (!math.ispow2(_input.Count))
                {
                    Debug.LogWarning($"Input Count {_input.Count} Not a power of 2");
                    return false;
                }
            
                var bits = (int)math.log2(_input.Count);
                for (var j = 1; j < _input.Count; j++)
                {
                    var swapPos = UBit.Reverse(j, bits);
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

                            var term = -kmath.kPI2 * k / N;
                            umath.sincos_fast(term,out var sin,out var cos);
                            var exp = new cfloat2(cos,sin) * odd;

                            _input[evenIndex] = even + exp;
                            _input[oddIndex] = even - exp;
                        }
                    }
                }
                return true;
            }
        }

        public static class StockhamFastFourier
        {
        }
    }
}