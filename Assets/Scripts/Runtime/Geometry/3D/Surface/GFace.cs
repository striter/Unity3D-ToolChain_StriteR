using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{

    public partial struct GFace
    {
        public float3 origin;
        public float2 size;
        public float3 normal;
        public float3 tangent;
        
        public GFace(float3 _origin, float2 _size, float3 _normal, float3 _tangent) => (origin, size, normal, tangent) = (_origin, _size, _normal, _tangent);

        public quaternion GetRotation() => quaternion.LookRotation(normal, tangent);
    }
}