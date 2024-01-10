using System;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GCone : IShape3D , IBoundingBox3D
    {
        public float3 origin;
        public float3 normal;
        [Range(0,90f)]public float angle;
        public float height;
        public GCone(float3 _origin, float3 _normal, float _angle, float _height)
        {
            origin = _origin;
            normal =_normal;
            angle = _angle;
            height = _height;
        }
        
        public static implicit operator GConeUnheighted(GCone _cone)=>new GConeUnheighted(_cone.origin,_cone.normal,_cone.angle);
        public float Radius => ((GConeUnheighted)this).GetRadius(height);
        public float3 Bottom => origin + normal * height;
        public float3 Center => origin + normal * height/2;
        public GBox GetBoundingBox()        //https://iquilezles.org/articles/diskbbox/
        {
            var a = normal*height;
            var pa = origin;
            var pb = pa + a;
            var ra = 0; //GetRadius(startHeight)
            var rb = Radius;
            var e = math.sqrt( 1.0f - a*a/math.dot(a,a) );
            return GBox.Minmax( math.min( pa - e*ra, pb - e*rb ),
                                math.max( pa + e*ra, pb + e*rb ) );     
        }
        
        public static readonly GCone kDefault = new GCone(){origin = kfloat3.up*.5f, normal = kfloat3.down, angle = 30f, height = 1f};
    }
}