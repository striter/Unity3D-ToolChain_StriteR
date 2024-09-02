using Unity.Mathematics;

namespace Runtime.Geometry.Extension.Sphere
{
    using static math;
    using static kmath;
    
    public struct Cube : ISphereUVMapping
    {
        public float2 ToUV(float3 _direction)
        {
            var phi = acos(-_direction.y) / kPI;
            var theta = atan2(_direction.z, _direction.x) / kPI2;
            theta += theta < 0 ? 1.0f : 0;
            return new float2(theta , phi );
        }

        public float3 ToPosition(float2 _uv) 
        {
            float3 position = 0;
            float uvRadius = sin(_uv.y * kPI);
            sincos(kPI2 * _uv.x, out position.z, out position.x);
            position.xz *= uvRadius;
            position.y = -cos(kPI * _uv.y);
            return position;
        }

        public static readonly Cube kDefault = default;
        public bool IsHemisphere => false;
    }
}