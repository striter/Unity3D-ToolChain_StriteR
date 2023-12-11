using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public struct GCylinder
    {
        public float3 origin;
        public float3 normal;
        public float height;

        public static readonly GCylinder kDefault = new GCylinder() {origin = float3.zero, normal = kfloat3.up, height = 1f};
    }

    public struct GCylinderCapped
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        public float height;

        public static readonly GCylinderCapped kDefault = new GCylinderCapped()
            { origin = float3.zero, normal = kfloat3.up, radius = .5f, height = 2f };
    }
}