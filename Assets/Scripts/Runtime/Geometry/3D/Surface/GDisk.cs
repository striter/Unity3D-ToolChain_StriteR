using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Runtime.Geometry
{
    public struct GDisk : ISurface , IVolume , IRayIntersection
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        public GDisk(float3 _origin, float3 _normal, float _radius) { origin = _origin;normal = _normal;radius = _radius; }
        public static GDisk kDefault = new GDisk(float3.zero, kfloat3.rightUpForward.normalize(), .5f);
        public float3 Origin => origin;
        public float3 GetSupportPoint(float3 _direction)
        {
            var diskCenterToPoint =  _direction - normal * math.dot(_direction, normal);
            return origin + diskCenterToPoint * radius;
        }
        
        public GPlane GetPlane() => new GPlane(normal,origin);

        public GBox GetBoundingBox()
        {
            var cen = origin;
            var nor = normal;
            var rad = radius;
            var e = rad*math.sqrt( 1.0f - nor*nor );
            return GBox.Minmax( cen-e, cen+e );
        }

        public GSphere GetBoundingSphere() => new GSphere(origin, radius);
        public bool RayIntersection(GRay _ray, out float distance)
        {
            var plane = this.GetPlane();
            if (!plane.RayIntersection(_ray,out distance))
                return false;

            var pointOnPlane = _ray.GetPoint(distance);
            return math.length(origin - pointOnPlane) <= radius;
        }

        public void DrawGizmos() => UGizmos.DrawWireDisk(origin, normal, radius);

        public float3 Normal => normal;
    }
}