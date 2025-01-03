using Unity.Mathematics;

namespace Runtime.SignalProcessing
{
    public static class Audio
    {
        public static float Hanning(int i,int _N) =>  0.5f * (1 - math.cos(2 * kmath.kPI * i / (_N - 1)));
    }
}