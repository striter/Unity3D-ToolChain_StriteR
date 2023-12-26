using Unity.Mathematics;

namespace Geometry
{
    public struct GCylinderCapped
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        public float height;

        public static readonly GCylinderCapped kDefault = new GCylinderCapped()
            { origin = float3.zero, normal = kfloat3.up, radius = .5f, height = 2f };
    }
}