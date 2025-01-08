using Unity.Mathematics;

namespace Runtime.SignalProcessing
{
    public static class UAudio
    {
        public static float Hanning(int i,int _N) => 0.5f + 0.5f * math.cos(2 * kmath.kPI * (i + _N / 2) / (_N - 1));
    }
}