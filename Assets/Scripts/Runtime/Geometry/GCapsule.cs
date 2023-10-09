using Unity.Mathematics;

namespace Geometry
{
    public struct GCapsule : IShape
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        public float height;

        public float3 Center => origin;
        public float3 GetSupportPoint(float3 _direction)
        {
            throw new System.NotImplementedException();
        }
        public GCapsule(float3 _center,float _radius,float3 _normal,float _height) { 
            origin = _center;
            radius = _radius;
            normal = _normal;
            height = _height;
        }
        public static readonly GCapsule kDefault = new GCapsule() {origin = float3.zero,radius = .5f, normal = kfloat3.up, height = 1f};
    }
}