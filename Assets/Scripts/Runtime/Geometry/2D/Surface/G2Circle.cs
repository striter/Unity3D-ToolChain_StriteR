using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry
{

    [Serializable]
    public partial struct G2Circle
    {
        public float2 center;
        public float radius;

        public G2Circle(float2 _center,float _radius)
        {
            center = _center;
            radius = _radius;
        }
        
        public static readonly G2Circle kDefault = new G2Circle(float2.zero, .5f);
        public static readonly G2Circle kZero = new G2Circle(float2.zero, 0f);
        public static readonly G2Circle kOne = new G2Circle(float2.zero, 1f);
    }
}