using Unity.Mathematics;

namespace Runtime.Geometry.Explicit.Sphere
{
    using static math;
    using static kmath;
    
    public struct Cube : IUVMapping
    {
        public float3 ToPosition(float2 _uv) 
        {
            float3 position = 0;
            float uvRadius = sin(_uv.y * kPI);
            sincos(kPI2 * _uv.x, out position.z, out position.x);
            position.xz *= uvRadius;
            position.y = -cos(kPI * _uv.y);
            return position;
        }

        public float2 ToUV(float3 _direction)
        {
            var phi = acos(-_direction.y);
            var theta = atan2(_direction.z, _direction.x);
            return new float2(theta / kPI2, phi / kPI);
        }

        public static readonly Cube kDefault = default;
    }
}