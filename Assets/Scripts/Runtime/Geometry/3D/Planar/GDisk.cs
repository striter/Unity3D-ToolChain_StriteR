using Unity.Mathematics;

namespace Runtime.Geometry
{
    public struct GDisk : IShape3D, IBoundingBox3D
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        
        public GDisk(float3 _origin, float3 _normal, float _radius) { origin = _origin;normal = _normal;radius = _radius; }

        public static GDisk kDefault = new GDisk(float3.zero, kfloat3.rightUpForward.normalize(), .5f);
        
        public float3 Center => origin;

        public float3 GetSupportPoint(float3 _direction)
        {
            _direction = math.normalize(_direction);
            var diskCenterToPoint =  _direction - normal * math.dot(_direction, normal);
            return origin + diskCenterToPoint * radius;
        }
        
        public GPlane GetPlane() => new GPlane(origin, normal);

        public GBox GetBoundingBox()
        {
            var cen = origin;
            var nor = normal;
            var rad = radius;
            var e = rad*math.sqrt( 1.0f - nor*nor );
            return GBox.Minmax( cen-e, cen+e );
        }
    }
}