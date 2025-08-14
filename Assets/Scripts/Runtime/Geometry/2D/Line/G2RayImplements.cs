using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Ray  : ISDF<float2> , IRayIntersection2
    {
        public float2 GetPoint(float _distance) => origin + direction * _distance;
        
        public static G2Ray StartEnd(float2 _start, float2 _end) => new G2Ray(_start, math.normalize(_end - _start));
        
        public G2Ray Forward(float _distance) => new G2Ray(origin + direction * _distance, direction);
        public G2Ray SetOrigin(float2 _origin) => new G2Ray(_origin, direction);

        public float2 Origin => origin;
        public float SDF(float2 _position)
        {
            var lineDirection = direction;
            var pointToStart = _position - origin;
            return math.length(umath.cross(lineDirection, pointToStart));
        }

        public static G2Ray FromEquation(float _m, float _b) => new G2Ray(new(0, _b), math.normalize(new float2(1,_m)));

        public G2Plane ToPlane() => new G2Plane(direction, origin);
        public G2Ray RotateCW90(int _times = 1) => new G2Ray(origin, umath.Rotate2DCW90(direction,_times));
        public bool SideSign(float2 _point) => math.dot( umath.Rotate2DCW90(direction),(_point - origin)) > 0;
        public void DrawGizmos() => Gizmos.DrawRay(origin.to3xz(),direction.to3xz());
        public bool Intersect(G2Ray _ray, out float distance)
        {
            var projection = _ray.Projection(this);
            distance = projection.x;
            return projection is { x: > 0, y: >= 0 };
        }
        
        public float Projection(float2 _point) => math.dot(_point - origin, direction);
        
        public float2 Projection(G2Ray _dstRay)
        {
            var diff = origin - _dstRay.origin;
            var a01 = -math.dot(direction, _dstRay.direction);
            var b0 = math.dot(diff, direction);
            var b1 = -math.dot(diff, _dstRay.direction);
            var det = 1f - a01 * a01;
            return new float2((a01 * b1 - b0) / det, (a01 * b0 - b1) / det);
        }
    }
}