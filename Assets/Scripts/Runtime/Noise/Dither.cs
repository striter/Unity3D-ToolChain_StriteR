using System.Linq.Extensions;
using Unity.Mathematics;

namespace Noise
{
    public static class Dither
    {
        public static float2x2 kDither2x2 = new float2x2(
            1,3,
            4,2
        ) / 5f;

        public static float3x3 kDither3x3 = new float3x3(
            1,8,4,
            7,6,4, 
            5,2,9
        )/10f;
        
        public static float4x4 kDither4x4 = new float4x4(
            1,9,3,11,
            13,5,15,7,
            4,12,2,10,
            16,8,14,6
        ) / 17f;

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
        }.Remake((_,p)=>p/65f);
    }
    
}