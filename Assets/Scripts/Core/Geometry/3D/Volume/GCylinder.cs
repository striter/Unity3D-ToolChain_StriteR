using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Runtime.Geometry
{
    public struct GCylinder : IVolume , IRayIntersection , ISDF //, IRayVolumeIntersection 
    {
        public float3 origin;
        public float3 normal;
        public float height;
        public float radius;
        
        public GCylinder(float3 _origin, float3 _normal, float _height, float _radius)
        {
            origin = _origin;
            normal = _normal;
            height = _height;
            radius = _radius;
        }

        public static readonly GCylinder kDefault = new GCylinder() {origin = kfloat3.down*.5f, normal = kfloat3.up, radius = .5f, height = 1f};
        public float3 Bottom => origin;
        public float3 Top => origin + normal * height;
        public float3 Origin => origin + normal * (height / 2);
        public float3 GetSupportPoint(float3 _direction)
        {
            var projectDisk = new GDisk(origin + normal * height * (math.dot(_direction, normal)/2 + .5f),normal,radius);
            return projectDisk.GetSupportPoint(_direction.setY(0f).normalize());
        }

        public GBox GetBoundingBox()        //https://iquilezles.org/articles/diskbbox/
        {
            var pa = origin;
            var pb = origin + normal * height;
            var ra = radius;
            var a = normal * height;
            var e = ra* math.sqrt( 1.0f - a*a/math.dot(a,a) );
            return GBox.Minmax( math.min( pa - e, pb - e ), math.max( pa + e, pb + e ) );
        }
        public GSphere GetBoundingSphere() => new GSphere(Origin, math.sqrt(umath.sqr(height/2)+ umath.sqr(radius)));
        public float SDF(float3 _position)
        {
            var p = _position;
            var a = Top;
            var b = Bottom;
            var r = radius;
            var  ba = b - a;
            var  pa = p - a;
            var baba = math.dot(ba,ba);
            var paba = math.dot(pa,ba);
            var x = math.length(pa*baba-ba*paba) - r*baba;
            var y = math.abs(paba-baba*0.5f)-baba*0.5f;
            var x2 = x*x;
            var y2 = y*y*baba;
            var d = (math.max(x,y)<0.0f)?-math.min(x2,y2):(((x>0.0f)?x2:0.0f)+((y>0.0f)?y2:0.0f));
            return math.sign(d)*math.sqrt(math.abs(d))/baba;
        }

        public static implicit operator GCylinderUncapped(GCylinder _cylinder)=>new GCylinderUncapped(_cylinder.origin,_cylinder.normal,_cylinder.radius);
        // public bool RayIntersection(GRay _ray, out float2 distances)
        // {
        //     distances = new float2(float.MinValue,float.MaxValue);
        //     if (new GPlane(normal, origin).RayIntersection(_ray, out var plane0Distance) && new GPlane(normal,origin + normal*height).RayIntersection(_ray,out var plane1Distance))
        //         distances = plane0Distance < plane1Distance ? new float2(plane0Distance,plane1Distance - plane0Distance) : new float2(plane1Distance,plane0Distance - plane1Distance);
        //
        //     if (!((GCylinderUncapped)this).RayIntersection(_ray, out var cylinderDistances)) 
        //         return false;
        //     
        //     if(cylinderDistances.x > distances.x)
        //         distances.x = cylinderDistances.x;
        //         
        //     if(cylinderDistances.sum() < distances.sum())
        //         distances.y = cylinderDistances.sum() - distances.x;
        //
        //     return true;
        // }

        public bool RayIntersection(GRay _ray, out float distance)
        {
            distance = -1;
            var b = origin;
            var a = b + normal*height;
            var ro = _ray.origin;
            var rd = _ray.direction;
            var ra = radius;
            var  ba = b  - a;
            var  oc = ro - a;
            var baba = math.dot(ba,ba);
            var bard = math.dot(ba,rd);
            var baoc = math.dot(ba,oc);
            var k2 = baba            - bard*bard;
            var k1 = baba*math.dot(oc,rd) - baoc*bard;
            var k0 = baba*math.dot(oc,oc) - baoc*baoc - ra*ra*baba;
            var h = k1*k1 - k2*k0;
            if (h < 0.0) 
                return false;
            h = math.sqrt(h);
            var t = (-k1-h)/k2;
            // body
            var y = baoc + t*bard;
            if (y > 0.0 && y < baba)
            {
                distance = t;
                return true;
            }
            // caps
            t = ((y<0.0f ? 0.0f : baba) - baoc)/bard;
            if (math.abs(k1 + k2 * t) < h)
            {
                distance = t;
                return true;
            }

            return false;
        }
        
        public void DrawGizmos() => UGizmos.DrawCylinder(origin,normal,radius,height);
    }
}