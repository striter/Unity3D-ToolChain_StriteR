using System;
using UnityEngine;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GConeUnheighted
    {
        public float3 origin;
        public float3 normal;
        [Range(0, 90f)] public float angle;
        public GConeUnheighted(float3 _origin, float3 _normal, float _angle) { origin = _origin;normal = _normal;angle = _angle; }
        public float GetRadius(float _height) => _height * math.tan(angle*kmath.kDeg2Rad);
    }
}
