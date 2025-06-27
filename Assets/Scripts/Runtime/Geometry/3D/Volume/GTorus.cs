using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public class GTorus : IVolume , ISDF
    {
        public float3 center;
        public float majorRadius;
        public float minorRadius;
        public static readonly GTorus kDefault = new() { center = 0, majorRadius = .5f, minorRadius = .05f };
        
        public float SDF(float3 _position)
        {    
            _position -= center;
            var q = new float2(math.length(_position.xz)-majorRadius,_position.y);
            return math.length(q)-minorRadius;
        }

        public void DrawGizmos()
        {
            UGizmos.DrawWireDisk(center, Vector3.up, majorRadius + minorRadius);
            UGizmos.DrawWireDisk(center, Vector3.up, majorRadius - minorRadius);
            UGizmos.DrawWireDisk(center + new float3(0, minorRadius, 0), Vector3.up, majorRadius);
            UGizmos.DrawWireDisk(center - new float3(0, minorRadius, 0), Vector3.up, majorRadius);
            UGizmos.DrawWireDisk(center + new float3(majorRadius,0, 0), Vector3.forward, minorRadius);
            UGizmos.DrawWireDisk(center - new float3(majorRadius,0, 0), Vector3.forward, minorRadius);
            UGizmos.DrawWireDisk(center + new float3(0, 0, majorRadius), Vector3.right, minorRadius);
            UGizmos.DrawWireDisk(center - new float3(0, 0, majorRadius), Vector3.right, minorRadius);
        }

        public float3 Origin => center;
        public GBox GetBoundingBox() => GBox.Minmax(center - new float3(majorRadius, minorRadius, majorRadius), center + new float3(majorRadius, minorRadius, majorRadius));
        public GSphere GetBoundingSphere() => new GSphere(center, majorRadius + minorRadius);
        
        public float3 GetSupportPoint(float3 _direction)
        {
            var R = majorRadius;
            var r = minorRadius;
            var dx = _direction.x;
            var dy = _direction.y;
            var dz = _direction.z;
            var N = math.sqrt(dx * dx + dy * dy);
            var M = math.sqrt(dx * dx + dy * dy + dz * dz);
            if (M == 0)
                return new float3 (R + r, 0, 0);
            float ux, uz;
            if (N > 0)
            {
                ux = dx / N;
                uz = dy / N;                
            }
            else
            {
                ux = 1;
                uz = 0;
            }

            var px = R * ux + (r * dx) / M;
            var py = (r * dy) / M;
            var pz = R * uz + (r * dz) / M;
            return new float3 (px, py, pz);
        }

    }
}