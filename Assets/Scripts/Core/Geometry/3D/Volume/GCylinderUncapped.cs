using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public struct GCylinderUncapped : IRayVolumeIntersection
    {
        public float3 origin;
        public float3 normal;
        public float radius;

        public GCylinderUncapped(float3 _origin, float3 _normal, float _radius)
        {
            origin = _origin;
            normal = _normal;
            radius = _radius;
        }
        
        public static readonly GCylinderUncapped kDefault = new GCylinderUncapped()
            { origin = float3.zero, normal = kfloat3.up, radius = .5f };

        public bool RayIntersection(GRay _ray, out float2 distances)
        {
            distances = -1;
            var rd = _ray.direction;
            var ca = normal;
            var oc = _ray.origin - origin;
            var cr = radius;
            var card = math.dot(ca,rd);
            var caoc = math.dot(ca,oc);
            var a = 1.0f - card*card;
            var b = math.dot( oc, rd) - caoc*card;
            var c = math.dot( oc, oc) - caoc*caoc - cr*cr;
            var h = b*b - a*c;
            if( h<0f) 
                return false; //no intersection
            h = math.sqrt(h);

            if (a == 0)
            {
                if (UGeometry.Distance(_ray, origin) > radius)
                    return false;
                
                distances = new float2(float.MinValue,float.MaxValue - float.MinValue) ;
                return true;
            }
            
            distances = new float2(-b-h,h*2)/a;
            return true;
        }
    }
}