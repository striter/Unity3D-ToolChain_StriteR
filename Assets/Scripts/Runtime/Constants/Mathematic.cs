namespace Unity.Mathematics
{
    public static class kfloat3
    {
        public static readonly float3 zero = 0f;
        public static readonly float3 one = 1f;
        public static readonly float3 up = new float3(0, 1, 0);
        public static readonly float3 down = new float3(0, -1, 0);
        public static readonly float3 left = new float3(-1, 0, 0);
        public static readonly float3 right = new float3(1, 0, 0);
        public static readonly float3 forward = new float3(0, 0, 1);
        public static readonly float3 back = new float3(0, 0, -1);
        
        public static readonly float3 leftDownBack = new float3(-1, -1, -1);
        public static readonly float3 rightUpForward = new float3(1, 1, 1);
        public static readonly float3 min = (float3)float.MinValue ;
        public static readonly float3 max = (float3)float.MaxValue ;
    }

    public static class kfloat2
    {
        public static readonly float2 zero = new float2(0f, 0f);
        public static readonly float2 half = new float2(.5f, .5f);
        public static readonly float2 one = new float2(1, 1);
        public static readonly float2 up = new float2(0, 1);
        public static readonly float2 down = new float2(0, -1);
        public static readonly float2 left = new float2(-1, 0);
        public static readonly float2 right = new float2(1, 0);
        public static readonly float2 max = float.MaxValue;
        public static readonly float2 min = float.MinValue;
    }

    public static class kfloat4
    {
        public static readonly float4 zero = new(0f, 0f, 0f, 0f);
        public static readonly float4 one = new(1f, 1f, 1f, 1f);
    }

    public static class kint2
    {
        public static readonly int2 one = new(1, 1);
        public static readonly int2 k00 = new(0, 0); public static readonly int2 k01 = new(0, 1); public static readonly int2 k02 = new(0, 2); public static readonly int2 k03 = new(0, 3);
        public static readonly int2 k10 = new(1, 0); public static readonly int2 k11 = new(1, 1); public static readonly int2 k12 = new(1, 2); public static readonly int2 k13 = new(1, 3);
        public static readonly int2 k20 = new(2, 0); public static readonly int2 k21 = new(2, 1); public static readonly int2 k22 = new(2, 2); public static readonly int2 k23 = new(2, 3);
        public static readonly int2 k30 = new(3, 0); public static readonly int2 k31 = new(3, 1); public static readonly int2 k32 = new(3, 2); public static readonly int2 k33 = new(3, 3);
        public static readonly int2 kLeft = new(-1, 0);
        public static readonly int2 kRight = new(1, 0);
        public static readonly int2 kUp = new(0, 1);
        public static readonly int2 kDown = new(0, -1);
    }
}