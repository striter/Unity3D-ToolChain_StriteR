using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Geometry
{
    [Serializable]
    public struct GPolygon:IShape3D
    {
        public float3[] positions;
        [NonSerialized] public float3 center;
        public GPolygon(params float3[] _positions)
        {
            positions = _positions;
            center = _positions.Average();
        }

        public float3 GetSupportPoint(float3 _direction)=>positions.MaxElement(_p => math.dot(_direction, _p));
        public float3 Center => center;

        public static readonly GPolygon kDefault = new GPolygon(kfloat3.forward,kfloat3.right,kfloat3.back,kfloat3.left);
    }

}