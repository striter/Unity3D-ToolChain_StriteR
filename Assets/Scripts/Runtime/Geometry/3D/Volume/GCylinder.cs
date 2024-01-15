using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Geometry
{
    public struct GCylinder : IShape3D , IBoundingBox3D
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
        public float3 Center => origin;
        public GBox GetBoundingBox()        //https://iquilezles.org/articles/diskbbox/
        {
            var pa = origin;
            var pb = origin + normal * height;
            var ra = radius;
            var a = normal * height;
            var e = ra* math.sqrt( 1.0f - a*a/math.dot(a,a) );
            return GBox.Minmax( math.min( pa - e, pb - e ), math.max( pa + e, pb + e ) );
        }
    }
}