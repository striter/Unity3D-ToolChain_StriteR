using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static umath;
using static kmath;

public static class UNoise
{
    public static class Value
    {
        static readonly float kRandomValue = 143758.5453f;
        public static float Unit1f1(float _unitValue) => frac(sin(_unitValue) * kRandomValue);
    
        static readonly float3 kRandomVec = new float3(12.0909f,89.233f,37.719f);
        public static float Unit1f2(float2 _randomUnit) => Unit1f1(dot(_randomUnit, kRandomVec.to2xy()));
        public static float Unit1f2(float _x, float _y) => Unit1f2(new float2(_x, _y));
        
        public static float Unit1f3(float3 _random) => Unit1f1(dot(_random, kRandomVec));
        public static float Unit1f3(float _x, float _y,float _z) => Unit1f3(new float3(_x, _y,_z));
        
        public static float2 Unit2f2(float2 _random) => new float2(Unit1f2(_random),Unit1f2(new float2(_random.y, _random.x)) );
    }
    
    
    public static class Perlin
    {
        //Simply extend the size to avoid out of range
        static readonly int[] kPerlinPermutation = { 151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180 ,
                                                     151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180 };
            
        static int Inc(int src) => (src + 1) % 255;
        static float Lerp(float a, float b, float x) => a + x * (b - a);
        static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
        static float Gradient(int hash, float x, float y, float z)
        {
            switch (hash & 0xF)
            {
                case 0x0: return x + y;
                case 0x1: return -x + y;
                case 0x2: return x - y;
                case 0x3: return -x - y;
                case 0x4: return x + z;
                case 0x5: return -x + z;
                case 0x6: return x - z;
                case 0x7: return -x - z;
                case 0x8: return y + z;
                case 0x9: return -y + z;
                case 0xA: return y - z;
                case 0xB: return -y - z;
                case 0xC: return y + x;
                case 0xD: return -y + z;
                case 0xE: return y - x;
                case 0xF: return -y - z;
                default: throw new Exception("Invalid Gradient Result Here!");
            }
        }

        public static float Unit1f2(float2 _sample) => Unit1f3(_sample.x,0f,_sample.y);
        public static float Unit1f2(float _x, float _y) => Unit1f3(_x,0f,_y);
        public static float Unit1f3(float3 _sample) => Unit1f3(_sample.x, _sample.y, _sample.z);
        public static float Unit1f3(float _x, float _y, float _z)
        {
            int xi = (int)_x & 255;
            int yi = (int)_y & 255;
            int zi = (int)_z & 255;
            float xf = _x - (int)_x;
            float yf = _y - (int)_y;
            float zf = _z - (int)_z;
            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);
            int[] p = kPerlinPermutation;
            int aaa = p[p[p[xi] + yi] + zi];
            int aba = p[p[p[xi] + Inc(yi)] + zi];
            int aab = p[p[p[xi] + yi] + Inc(zi)];
            int abb = p[p[p[xi] + Inc(yi)] + Inc(zi)];
            int baa = p[p[p[Inc(xi)] + yi] + zi];
            int bba = p[p[p[Inc(xi)] + Inc(yi)] + zi];
            int bab = p[p[p[Inc(xi)] + yi] + Inc(zi)];
            int bbb = p[p[p[Inc(xi)] + Inc(yi)] + Inc(zi)];
            float x1 = Lerp(Gradient(aaa, xf, yf, zf), Gradient(baa, xf - 1, yf, zf), u);
            float x2 = Lerp(Gradient(aba, xf, yf - 1, zf), Gradient(bba, xf - 1, yf - 1, zf), u);
            float y1 = Lerp(x1, x2, v);
            x1 = Lerp(Gradient(aab, xf, yf, zf - 1), Gradient(bab, xf - 1, yf, zf - 1), u);
            x2 = Lerp(Gradient(abb, xf, yf - 1, zf - 1), Gradient(bbb, xf - 1, yf - 1, zf - 1), u);
            float y2 = Lerp(x1, x2, v);
            return Lerp(y1, y2, w);
        }
    }
    
    public static class Simplex
    {
        const float c_Mod289 = 1.0f / 290f;
        static readonly float4 s_Simplex_C = new float4((3f - kSQRT3) / 6f, (kSQRT3 - 1f) * .5f, -1f + 2f * ((3f - kSQRT3) / 6f), 1f / 41f);
        static readonly float3 s_Simplex_M1 = 1.79284291400159f;
        static readonly float s_Simplex_M2 = 0.85373472095314f;
        static float Mod289(float _x) { return _x - floor(_x * c_Mod289) * 289f; }
        static float2 Mod289(float2 _vec) => new float2(Mod289(_vec.x),Mod289(_vec.y));
        static float3 Mod289(float3 _vec) => new float3(Mod289(_vec.x), Mod289(_vec.y), Mod289(_vec.z));
        static float3 Permute(float3 vec) { return Mod289((vec * 34f + 1f)*vec); }
        public static float Unit1f2(float _x, float _y) => Unit1f2(new float2(_x,_y));
        public static float Unit1f2(float2 _v)
        {
            float2 i = floor(_v + dot(_v, s_Simplex_C.y));
            float2 x0 = _v - i + dot(i, s_Simplex_C.x);
            float2 i1 = x0.x > x0.y ? new float2(1, 0) : new float2(0, 1);
            float4 x12 = new float4(x0.x, x0.y, x0.x, x0.y) + new float4(s_Simplex_C.x, s_Simplex_C.x, s_Simplex_C.z, s_Simplex_C.z);
            x12-=i1.to4();
            i = Mod289(i);
            float3 p = Permute(Permute(new float3(i.y,i.y+i1.y,i.y+1))+new float3(i.x,i.x+i1.x,i.x+1));
            float2 x12xy = new float2(x12.x, x12.y);
            float2 x12zw = new float2(x12.z, x12.w);
            float3 m = max(0.5f-new float3(dot(x0,x0),dot(x12xy,x12xy),dot(x12zw,x12zw)) ,0);
            m *= m;
            m *= m;
            float3 x = 2.0f * frac(p*s_Simplex_C.w)-1.0f;
            float3 h = abs(x)-0.5f;
            float3 ox = floor(x + 0.5f);
            float3 a0 = x - ox;
            m = (s_Simplex_M1 - s_Simplex_M2 * (a0*a0 + h*h))*m;
            float gx = a0.x*x0.x+h.x*x0.y;
            float2 gyz = new float2(a0.y,a0.z)*new float2(x12.x,x12.z)+new float2(h.y,h.z)*new float2(x12.y,x12.w);
            return 130f * dot(m,new float3(gx,gyz.x,gyz.y));
        }
    }
    
    public static class Voronoi
    {  
        public static float2 Unit2f2(float _x, float _y) => Unit2f2(new float2( _x, _y));
        public  static float2 Unit2f2(float2 _v)
        {
            float sqrDstToCell = float.MaxValue;
            float2 baseCell = floor(_v);
            float2 closetCell = baseCell;
            for (int i=-1;i<=1;i++)
                for(int j=-1;j<=1;j++)
                {
                    float2 cell = baseCell+new float2(i,j);
                    float2 cellPos = cell + Value.Unit2f2(cell);
                    float2 toCell = cellPos - _v;
                    float sqrDistance = toCell.sqrmagnitude();
                    if (sqrDstToCell < sqrDistance)
                        continue;

                    sqrDstToCell = sqrDistance;
                    closetCell = cell;
                }
            return new float2(sqrDstToCell,  Value.Unit1f2(closetCell) );
        }
    }
}
