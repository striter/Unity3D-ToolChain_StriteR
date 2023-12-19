using System;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Geometry.Curves
{
    [Serializable]
    public struct GProjectileCurve : ICurve<float3>
    {
        public float3 origin;
        public float3 initialSpeed;
        public float3 friction;
        public float duration;
        
        public float3 Evaluate(float _normalizedTime)
        {
            var time = _normalizedTime * duration;
            return origin + initialSpeed * time - 0.5f * friction * umath.pow2(time);
        }


        public static GProjectileCurve StartEndGravity(float3 _start,float3 _end,float _speed)
        {
            return default; //To be continued
        }

        public static GProjectileCurve kDefault =
            GProjectileCurve.StartEndGravity(float3.zero, kfloat3.forward * 10, 2f);
    }
}