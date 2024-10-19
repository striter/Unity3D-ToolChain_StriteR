using System;
using Unity.Mathematics;
using UnityEngine;
using static kmath;
using static umath;
using static Unity.Mathematics.math;

namespace Runtime.Physics
{
    [Serializable]
    public struct SecondOrderDynamics : ISerializationCallbackReceiver
    {
        [Range(0.01f, 20)] public float f;
        [Range(0, 1.5f)] public float z;
        [Range(-5, 5)] public float r;
        public bool poleMatching;
        private float4 _xp;
        private float _w,_d,_k1, _k2, _k3;
        public float duration => 4 / (z * f);
        SecondOrderDynamics Ctor()
        {
            _w = 2 * kPI * f;
            _d = _w * sqrt(abs(z*z-1));
            _k1 = z / (kPI * f);
            _k2 = 1 / sqr(_w);
            _k3 = r * z / (_w);
            return this;
        }

        public SecondOrderDynamics Initialize(float4 _position)
        {
            _xp = _position;
            return this;
        }

        public void Evaluate(ref float4 value,ref float4 v, float _deltaTime,float4 _desire,float4 _desireVelocity = default)
        {
            var xd = _desire;
            var vd = _desireVelocity;
            var dt = _deltaTime;
            
            if (vd.sqrmagnitude() < float.Epsilon)
            {
                vd = (xd - _xp) / dt;
                _xp = xd;
            }

            float k1Stable, k2Stable;
            if (!poleMatching || _w * dt < z)
            {
                k1Stable = _k1;
                k2Stable = max(_k2,max(dt*dt/2 + dt*_k1/2,dt*_k1));
            }
            else
            {
                float t1 = exp(-z * _w * dt);
                float alpha = 2 * t1 * (z <= 1 ? cos(dt * _d) : cosH(dt * _d));
                float beta = sqr(t1);
                float t2 = dt / (1 + beta - alpha);
                k1Stable = (1 - beta) * t2;
                k2Stable = dt * t2;
            }
                
            value += v * dt;
            v += dt * (xd + _k3 * vd - value - k1Stable * v) / k2Stable;
        }
        
        public static SecondOrderDynamics kDefault = new SecondOrderDynamics()
        {
            f = 5,
            z = 0.35f,
            r = -.5f,
            poleMatching = true
        }.Ctor();

        public void OnBeforeSerialize(){}

        public void OnAfterDeserialize() => Ctor();
    }
}