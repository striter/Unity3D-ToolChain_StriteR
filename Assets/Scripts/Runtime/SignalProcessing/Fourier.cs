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
                if (coefficients < 2 || coefficients > N)
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

        static bool FastFourierParametersCheck(IList<cfloat2> _input, IList<cfloat2> _output)
        {
            if (_output is null)
            {
                Debug.LogError($"[{nameof(CooleyTukeyFastFourier)}]: null OutputFrequencies");
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
        
        //https://rosettacode.org/wiki/Fast_Fourier_transform
        public static class CooleyTukeyFastFourier
        {
            static void FFT0(IList<cfloat2> _output)
            {
                var bits = (int)math.log2(_output.Count);
                var count = _output.Count;
                for (var j = 1; j < count; j++)
                {
                    var swapPos = UBit.Reverse(j, bits);
                    if (swapPos <= j)
                        continue;
                    (_output[j], _output[swapPos]) = (_output[swapPos], _output[j]);
                }
                
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
            }
            
            public static IList<cfloat2> Transform(IList<cfloat2> _input,IList<cfloat2> _output =null)
            {
                _output ??= _input;
                if (!FastFourierParametersCheck(_input,_output))
                    return null;
                
                if(!Equals(_input, _output))
                    _input.Fill(_output);
                FFT0(_output);
                return _output;
            }

            public static IList<cfloat2> Inverse(IList<cfloat2> _input, IList<cfloat2> _output = null)
            {
                if (!FastFourierParametersCheck(_input, _output))
                    return null;
                
                _input.Select(cfloat2.conjugate).Fill(_output);
                FFT0(_output);
                var size = _output.Count;
                _output.Remake(p=> p/size);
                return _output;
            }
        }

        //http://wwwa.pikara.ne.jp/okojisan/otfft-en/stockham2.html
        public static class StockhamFastFourier
        {
            private static List<cfloat2> kTempBuffer0 = new();
            static void FFT0(IList<cfloat2> _output)
            {
                var yIsOutput = false;
                var N = _output.Count;
                var s = 1;
                var x = _output;
                kTempBuffer0.Resize(N);
                var y = (IList<cfloat2>)kTempBuffer0;

                var stages = (int)math.log2(N);
                for(var stage = 0; stage <= stages; stage++)
                {
                    if (N == 1)
                    {
                        if (yIsOutput)
                        {
                            for (var q = 0; q < s; ++q)
                                y[q] = x[q];
                        }

                        break;
                    }
                    var m = N/2;
                    var theta0 = -kmath.kPI2 / N;

                    for (var p = 0; p < m; ++p)
                    {
                        var phase = p * theta0;
                        var wp = new cfloat2(math.cos(phase),math.sin(phase));

                        for (var q = 0; q < s; ++q)
                        {
                            var a = x[q + s*(p)];
                            var b = x[q + s*(p + m)];

                            y[q + s*(2*p + 0)] =  a + b;
                            y[q + s*(2*p + 1)] = (a - b) * wp;
                        }
                    }

                    (x, y) = (y, x);
                    N  = m;
                    s *= 2;
                    yIsOutput = !yIsOutput;
                }
            }
            public static IList<cfloat2> Transform(IList<cfloat2> _input, IList<cfloat2> _output)
            {
                _output ??= _input;
                if (!FastFourierParametersCheck(_input, _output))
                    return null;

                if(!Equals(_input, _output))
                    _input.Fill(_output);
                
                FFT0(_output);
                return _output;
            }
            
            public static IList<cfloat2> Inverse(IList<cfloat2> _input, IList<cfloat2> _output)
            {
                if (!FastFourierParametersCheck(_input, _output))
                    return null;
                
                _input.Select(cfloat2.conjugate).Fill(_output);
                FFT0(_output);
                var size = _output.Count;
                _output.Remake(p=> p/size);
                return _output;
            }
        }
    }
}