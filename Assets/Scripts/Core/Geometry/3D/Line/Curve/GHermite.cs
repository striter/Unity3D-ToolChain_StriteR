using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using static umath;
namespace Runtime.Geometry.Curves
{
    [Serializable]
    public struct GHermiteCurve : ICurve
    {
        public float3 srcPosition;
        public float3 srcTangent;
        public float3 dstPosition;
        public float3 dstTangent;
        public GHermiteCurve(float3 _srcPosition, float3 _srcTangent, float3 _dstPosition, float3 _dstTangent)
        {
            srcPosition = _srcPosition;
            srcTangent = _srcTangent;
            dstPosition = _dstPosition;
            dstTangent = _dstPosition;
        }
        public float3 Evaluate(float _value) => srcPosition * (1 - 3*pow2(_value) + 2*pow3(_value)) + pow2(_value)*(3-2*_value)*dstPosition + _value*pow2(_value-1)*srcTangent + pow2(_value) * (_value - 1) * dstTangent;

        public static GHermiteCurve kDefault = new GHermiteCurve(kfloat3.left,kfloat3.forward,kfloat3.right,kfloat3.back);

        public float3 Origin => srcPosition;
        public void DrawGizmos() => this.DrawGizmos(64);
    }

}
