using Unity.Mathematics;

namespace Geometry
{
    public struct GCylinderUncapped
    {
        public float3 origin;
        public float3 normal;
        public float radius;

        public static readonly GCylinderUncapped kDefault = new GCylinderUncapped()
            { origin = float3.zero, normal = kfloat3.up, radius = .5f };
    }
}