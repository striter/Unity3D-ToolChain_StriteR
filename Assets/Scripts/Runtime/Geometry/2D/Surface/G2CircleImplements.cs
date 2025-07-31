using Runtime.Geometry.Extension;
using Unity.Mathematics;
namespace Runtime.Geometry
{
    public partial struct G2Circle : IGeometry2, IArea2, IRayArea2Intersection, ISDF<float2>
    {
        public static G2Circle operator +(G2Circle _src, float2 _dst) => new G2Circle(_src.center+_dst,_src.radius);
        public static G2Circle operator -(G2Circle _src, float2 _dst) => new G2Circle(_src.center - _dst, _src.radius);
        public float2 GetSupportPoint(float2 _direction) => center + _direction * radius;
        public float GetArea() => kmath.kPI * radius * radius;
        public bool RayIntersection(G2Ray _ray, out float2 distances)
        {
            distances = -1;
            var shift = _ray.origin - center;
            var dotOffsetDirection = math.dot(_ray.direction, shift);
            var sqrRadius = radius * radius;
            var radiusDelta = math.dot(shift, shift) - sqrRadius;
            if (dotOffsetDirection > 0 && radiusDelta > 0)
                return false;
            var dotOffset = math.dot(shift, shift);
            var discriminant = dotOffsetDirection * dotOffsetDirection - dotOffset + sqrRadius;
            if (discriminant < 0)
                return false;

            discriminant = math.sqrt(discriminant);
            var t0 = -dotOffsetDirection - discriminant;
            var t1 = discriminant * 2;
            distances =  new float2(t0, t1);
            return !(t0 < 0);
        }

        public float SDF(float2 _position) => math.length(center-_position) - radius;
        public float2 Origin => center;
        public override string ToString() => $"G2Circle({center},{radius})";
        public void DrawGizmos() => UGizmos.DrawWireDisk(center.to3xz(), kfloat3.up, radius);
    }
}