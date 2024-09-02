using System;
using Runtime.Geometry.Extension;
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
    
    public static class G2Circle_Extension
    {
        public static G2Triangle GetCircumscribedTriangle(this G2Circle _circle)
        {
            var r = _circle.radius;
            return new G2Triangle(
                _circle.center + new float2(0f,r/kmath.kSin30d) ,
                _circle.center + new float2(r * kmath.kTan60d,-r),
                _circle.center + new float2(-r * kmath.kTan60d,-r));
        }
    }
}