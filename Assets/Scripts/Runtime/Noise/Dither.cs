using Unity.Mathematics;

namespace Noise
{
    public static class Dither
    {
        public static float4x4 kDither4x4 = new float4x4(
                0,8,2,10,
                12,4,14,6,
                3,11,1,9,
                15,7,13,5
            ) / 16f;

        public static float[] kDither8x8 = new float[]
        {
            1, 49, 13, 61, 4, 52, 16, 64,
            33, 17, 45, 29, 36, 20, 48, 32,
            9, 57, 5, 53, 12, 60, 8, 56,
            41, 25, 37, 21, 44, 28, 40, 24,
            3, 51, 15, 63, 2, 50, 14, 62,
            35, 19, 47, 31, 34, 18, 46, 30,
            11, 59, 7, 55, 10, 58, 6, 53,
            43, 27, 39, 23, 42, 26, 38, 22
        }.Remake((index,p)=>p/16f);
    }
    
}