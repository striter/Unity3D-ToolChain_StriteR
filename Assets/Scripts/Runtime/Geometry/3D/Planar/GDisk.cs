using Unity.Mathematics;

namespace Geometry
{
    public struct GDisk : IShape3D, IBoundingBox3D
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        
        public GDisk(float3 _origin, float3 _normal, float _radius) { origin = _origin;normal = _normal;radius = _radius; }

        public static GDisk kDefault = new GDisk(float3.zero, kfloat3.rightUpForward.normalize(), .5f);
        
        public float3 Center => origin;
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