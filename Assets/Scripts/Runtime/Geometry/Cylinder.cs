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
}