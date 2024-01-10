using System;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GEllipsoid:IShape3D, IBoundingBox3D
    {
        public float3 center;
        public float3 radius;
        public GEllipsoid(float3 _center,float3 _radius) {  center = _center; radius = _radius;}
        public static readonly GEllipsoid kDefault = new GEllipsoid(float3.zero, new float3(.5f,1f,0.5f));
        public float3 Center => center;
        public GBox GetBoundingBox() => GBox.Minmax( center-radius, center+radius );
    }
}