using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Runtime.SignalProcessing
{
    public enum EWindow
    {
        Rectangular,
        Hamming,
        Hanning,
        Blackman,
        BlackmanHarris,
        Hann,
        Barlett,
        Tukey,
    }

    public static class UWindow
    {
        static float Tukey(int i, int N,float alpha = 0.5f)
        {
            var key = alpha * N;
            var frame1 = key / 2f;
            var frame2 = N - frame1;
            if(i < frame1)
                return .5f + 0.5f * math.cos(kmath.kPI2 * i / key);
            if (i < frame2)
                return 1f;
            return .5f - .5f * math.cos(kmath.kPI2 * (N - i) / key);
        }
        
        public static void Window(this IList<cfloat2> _signals, EWindow _window)
        {
            var N = _signals.Count;
            var halfN = N / 2;

            switch (_window)
            {
                case EWindow.Rectangular: break;
                case EWindow.Hann: _signals.Remake((i, p) => p * (0.5f + 0.5f * math.cos(kmath.kPI2 * i / N))); break;
                case EWindow.Hamming: _signals.Remake((i, p) => p * (0.54f - 0.46f * math.cos(kmath.kPI2 * i / N))); break;
                case EWindow.Hanning: _signals.Remake((i, p) => p * (0.5f * (1f - math.cos(kmath.kPI2 * i / N)))); break;
                case EWindow.Blackman: _signals.Remake((i, p) => p * (0.42f - 0.5f * math.cos(kmath.kPI2 * i / N) + 0.08f * math.cos(2f * kmath.kPI2 * i / N))); break;
                case EWindow.BlackmanHarris: _signals.Remake((i, p) => p * (0.35875f - 0.48829f * math.cos(1f * kmath.kPI2 * i / N) + 0.14128f * math.cos(2f * kmath.kPI2 * i / N) - 0.01168f * math.cos(3f * kmath.kPI2 * i / N))); break;
                case EWindow.Barlett: _signals.Remake((i, p) => p* (i <= halfN ? (2f * i / N) : 2 - 2f*i/N)) ; break;
                case EWindow.Tukey: _signals.Remake((i,p)=>p * Tukey(i,N,.5f)); break;
                default: throw new ArgumentOutOfRangeException(nameof(_window), _window, null);
            }
        }
        
    }
    public static class UAudio
    {
        public static float Hanning(int i,int _N) => 0.5f + 0.5f * math.cos(2 * kmath.kPI * (i + _N / 2) / (_N - 1));
    }
}