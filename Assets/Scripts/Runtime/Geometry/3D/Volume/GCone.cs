using System;
using Runtime.Geometry.Extension;
using UnityEngine;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GCone  : IRayVolumeIntersection
    {
        public float3 origin;
        public float3 normal;
        [Range(0, 90f)] public float angle;

        public GCone(float3 _origin, float3 _normal, float _angle)
        {
            origin = _origin;
            normal = _normal;
            angle = _angle; 
        }
        public float GetRadius(float _height) => _height * math.tan(angle*kmath.kDeg2Rad);
        
        public static float2 ConeCalculate(GRay _ray,GCone _cone)
        {
            float2 distances = -1f;
            float3 offset = _ray.origin - _cone.origin;

            float RDV = math.dot(_ray.direction, _cone.normal);
            float ODN = math.dot(offset, _cone.normal);
            float cosA = math.cos(kmath.kDeg2Rad * _cone.angle);
            float sqrCosA = cosA * cosA;

            float a = RDV * RDV - sqrCosA;
            float b = 2f * (RDV * ODN - math.dot(_ray.direction, offset) * sqrCosA);
            float c = ODN * ODN - math.dot(offset, offset) * sqrCosA;
            float determination = b * b - 4f * a * c;
            if (determination < 0)
                return distances;
            determination = math.sqrt(determination);
            distances.x = (-b + determination) / (2f * a);
            distances.y = (-b - determination) / (2f * a);
            return distances;
        }
        
        public bool RayIntersection(GRay _ray, out float2 distances)
        {
            distances = -1;
            var offset = _ray.origin - origin;

            var RDV = math.dot(_ray.direction, normal);
            var ODN = math.dot(offset, normal);
            var cosA = math.cos(kmath.kDeg2Rad * angle);
            var sqrCosA = cosA * cosA;

            var a = RDV * RDV - sqrCosA;
            var b = 2f * (RDV * ODN - math.dot(_ray.direction, offset) * sqrCosA);
            var c = ODN * ODN - math.dot(offset, offset) * sqrCosA;
            var determination = b * b - 4f * a * c;
            if (determination < 0)
                return false;
            
            determination = math.sqrt(determination);
            distances = new float2( (-b + determination) / (2f * a),(-b - determination) / (2f * a));
            if (math.dot(normal, _ray.GetPoint(distances.x) - origin) < 0)
                distances.x = -1;
            if (math.dot(normal, _ray.GetPoint(distances.y) - origin) < 0)
                distances.y = -1;
            return true;
        }
    }
}
