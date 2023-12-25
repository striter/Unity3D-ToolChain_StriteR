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

        public float3 Evaluate(float _normalizedTime) => EvaluateTime(duration*_normalizedTime);
        public float3 EvaluateTime(float _time) => origin + initialSpeed * _time + 0.5f * friction * umath.pow2(_time);
        
        public static GProjectileCurve StartEndGravity(float3 _start,float3 _end,float _duration,float _gravity = 0.98f)
        {
            var friction = kfloat3.down * _gravity;
            var origin = _start;
            var duration = _duration;

            var travelTime = _duration;
        
            // Calculate initial velocities
            var displacement = _end - _start;
            var horizontalDisplacement = new float3(displacement.x, 0, displacement.z);
            var horizontalDistance = horizontalDisplacement.magnitude();
            var verticalDistance = displacement.y;

            // Calculate initial velocities
            float initialHorizontalVelocity = horizontalDistance / travelTime;
            float initialVerticalVelocity = (verticalDistance + (0.5f * _gravity * travelTime * travelTime)) / travelTime;

            // Calculate direction vector
            var initialSpeed = horizontalDisplacement.normalize() * initialHorizontalVelocity +
                                        kfloat3.up * initialVerticalVelocity;
            
            return new GProjectileCurve
            {
                origin = origin,
                initialSpeed = initialSpeed,
                duration = duration,
                friction = friction
            };; //To be continued
        }

        public static GProjectileCurve kDefault =
            GProjectileCurve.StartEndGravity(float3.zero, kfloat3.forward * 10, 2f);


    }
}