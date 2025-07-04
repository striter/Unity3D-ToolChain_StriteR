using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.SignalProcessing
{
    public static class UFourier
    {
        static List<cfloat2> kComplexFiller = new();
        
        //https://iquilezles.org/articles/fourier/
        public static class DiscreteFourier
        {
            public static IList<cfloat2> Transform(IEnumerable<float> _time,IList<cfloat2> _output) => Transform(_time.Select(p=>new cfloat2(p,0)),_output);
            public static IList<cfloat2> Transform(IEnumerable<cfloat2> _time,IList<cfloat2> _output) => Transform(_time.FillList(kComplexFiller),_output);
            public static IList<cfloat2> Transform(IList<cfloat2> _input,IList<cfloat2> _output)
            {
                var N = _input.Count;
                if (_output is null)
                {
                    Debug.LogError($"[{nameof(DiscreteFourier)}]: null OutputFrequencies");
                    return null;
                }
                
                var coefficients = _output.Count;
                if (coefficients <= 2 || coefficients > N)
                {
                    Debug.LogError($"[{nameof(DiscreteFourier)}]: OutputFrequencies Length Invalid {coefficients} Range[{2},{N}]");
                    return null;
                }
                for (var k = 0; k < coefficients; k++)
                {
                    var fc = cfloat2.zero;
                    for (var n = 0; n < N; n++)
                    {
                        var input = _input[n]; 
                        var angle = -kmath.kPI2 * n * k / N * cfloat2.iOne;
                        var an = cfloat2.exp(angle);
                        fc += input * an;
                    }
                    _output[k] = fc * coefficients / N;
                }
                return _output;
            }

            public static IList<cfloat2> Inverse(IList<cfloat2> _frequencies,IList<cfloat2> _output)
            {
                var N = _output.Count;
                for (var i = 0; i < N; i++)
                    _output[i] = Inverse(_frequencies,(float)i / N);
                return _output;
            }
            
            public static cfloat2 Inverse(IEnumerable<cfloat2> _frequencies,float _value) => Inverse(_frequencies.FillList(kComplexFiller),_value);
            public static cfloat2 Inverse(IList<cfloat2> _frequencies, float _value)
            {
                var N = _frequencies.Count;
                var result = cfloat2.zero;
                for (var k = 0; k < N; k++)
                {
                    var angle = kmath.kPI2 * k * _value * cfloat2.iOne;
                    var exp = cfloat2.exp(angle);
                    result += _frequencies[k]*exp;
                }
                return result / N;
            }
        }

        //https://rosettacode.org/wiki/Fast_Fourier_transform
        public static class CooleyTukeyFastFourier
        {
            static bool SafeCheck(IList<cfloat2> _input,IList<cfloat2> _output)
            {
                if (_output is null)
                {
                    Debug.LogError($"[{nameof(CooleyTukeyFastFourier)}]: null OutputFrequencies");
                    return false;
                }

                if (Equals(_input, _output))
                {
                    Debug.LogError($"[{nameof(CooleyTukeyFastFourier)}]: Input == Output");
                    return false;
                }
                
                if (!math.ispow2(_input.Count))
                {
                    Debug.LogError($"[{nameof(CooleyTukeyFastFourier)}]: Input Count {_input.Count} Not a power of 2");
                    return false;
                }

                if (_input.Count != _output.Count)
                {
                    Debug.LogError($"[{nameof(CooleyTukeyFastFourier)}]: Input Count {_input.Count} != Output Count {_output.Count}");
                    return false;
                }

                return true;
            }

            static void BitReverse(IList<cfloat2> _array)
            {
                var bits = (int)math.log2(_array.Count);
                var count = _array.Count;
                for (var j = 1; j < count; j++)
                {
                    var swapPos = UBit.Reverse(j, bits);
                    if (swapPos <= j)
                        continue;
                    (_array[j], _array[swapPos]) = (_array[swapPos], _array[j]);
                }
            }
            public static IList<cfloat2> Transform(IList<cfloat2> _input,IList<cfloat2> _output =null)
            {
                _output ??= new List<cfloat2>(_input.Count);
                if (!SafeCheck(_input,_output))
                    return null;
                
                _input.Fill(_output);
                BitReverse(_output);

                var count = _output.Count;
                for (var N = 2; N <= count; N <<= 1)
                {
                    for (var i = 0; i < count; i += N)
                    {
                        for (var k = 0; k < N / 2; k++)
                        {
                            var evenIndex = i + k;
                            var oddIndex = i + k + (N / 2);
                            var even = _output[evenIndex];
                            var odd = _output[oddIndex];

                            var term = -kmath.kPI2 * k / N;
                            umath.sincos_fast(term,out var sin,out var cos);
                            var exp = new cfloat2(cos,sin) * odd;

                            _output[evenIndex] = even + exp;
                            _output[oddIndex] = even - exp;
                        }
                    }
                }
                return _output;
            }

            public static IList<cfloat2> Inverse(IList<cfloat2> _input, IList<cfloat2> _output = null)
            {
                _input.Remake(cfloat2.conjugate);
                _output = Transform(_input,_output);
                _input.Remake(cfloat2.conjugate);
                var size = _input.Count;
                _output.Remake(p=> p/size);
                return _output;
            }
        }

        public static class StockhamFastFourier
        {
        }
    }
}