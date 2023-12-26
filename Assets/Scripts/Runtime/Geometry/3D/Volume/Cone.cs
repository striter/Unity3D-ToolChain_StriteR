using System;
using UnityEngine;
using Unity.Mathematics;

namespace Geometry
{
    [Serializable]
    public struct GCone
    {
        public float3 origin;
        public float3 normal;
        [Range(0, 90f)] public float angle;
        public GCone(float3 _origin, float3 _normal, float _angle) { origin = _origin;normal = _normal;angle = _angle; }
        public float GetRadius(float _height) => _height * math.tan(angle*kmath.kDeg2Rad);
    }

    [Serializable]
    public struct GHeightCone
    {
        public float3 origin;
        public float3 normal;
        [Range(0,90f)]public float angle;
        public float height;
        public float Radius => ((GCone)this).GetRadius(height);
        public float3 Bottom => origin + normal * height;
        public GHeightCone(float3 _origin, float3 _normal, float _angle, float _height)
        {
            origin = _origin;
            normal =_normal;
            angle = _angle;
            height = _height;
        }
        public static implicit operator GCone(GHeightCone _cone)=>new GCone(_cone.origin,_cone.normal,_cone.angle);
    }
    
}
