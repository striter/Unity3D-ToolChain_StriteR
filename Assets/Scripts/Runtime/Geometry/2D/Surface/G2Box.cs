using System;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public partial struct G2Box
    {
        public float2 center;
        public float2 extent;
        [NonSerialized] public float2 size;
        [NonSerialized] public float2 min;
        [NonSerialized] public float2 max;
        public G2Box(float2 _center, float2 _extent)
        {
            this = default;
            center = _center;
            extent = _extent;
            Ctor();
        }
        
        void Ctor()
        {
            size = extent * 2f;
            min = center - extent;
            max = center + extent;
        }
        public static readonly G2Box kDefault = new G2Box(0f,.5f);
        public static readonly G2Box kOne = new G2Box(.5f,.5f);
        public static readonly G2Box kZero = new G2Box(0f,0f);
    }
}