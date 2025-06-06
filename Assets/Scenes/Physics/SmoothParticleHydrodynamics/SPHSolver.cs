﻿using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.PhysicsScenes.SmoothParticleHydrodynamics
{
    [Serializable]
    public class SPHSolver
    {
        public SPHData m_Data;
        public List<double> m_Pressures = new();
        [Range(0,1)] public float m_NegativePressureScale = 0f;
        [Min(0)] public float m_EOSExponent = 5f;
        public void Destroy()
        {
            m_Data.Destroy();
        }

        public void AccumulateForces(float _fixedDeltaTime,List<ParticleData> _particles)
        {
            var _kernel = m_Data.GetKernel();
            m_Data.Construct(_kernel,_particles);
            m_Pressures.Resize(_particles.Count);
            
            var eosScale = m_Data.m_TargetDensity * umath.sqr(m_Data.m_SpeedOfSound);
            for (var i = 0; i < _particles.Count; i++)
                m_Pressures[i] = ComputePressureFromEOS(m_Data.Density(i),m_Data.m_TargetDensity,eosScale,m_EOSExponent,m_NegativePressureScale);

            AccumulatePressureForces(_fixedDeltaTime,_kernel , m_Data , _particles);
            AccumulateViscosityForces(_kernel , m_Data, _particles);
        }

        protected virtual void AccumulatePressureForces(float _fixedDeltaTime,ISPH _kernel,SPHData _data,List<ParticleData> _particles)
        {
            for (var i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                var mass2 = (double)umath.sqr(particle.mass);
                var pressure = m_Pressures[i];
                var density = _data.Density(i);
                var density2 = (density * density);
                var neighbors = _data.Query(i);
                var pressureForce = float3.zero;
                for (var j = neighbors.Count - 1; j >= 0; j--)
                {
                    var neighbor = neighbors[j];
                    var delta = neighbor.position - particle.position;
                    var distance = delta.magnitude();
                    if (distance <= 0) 
                        continue;
                    var neighborIndex = neighbor.index;
                    var dir = delta / distance;
                    var gradient = _kernel.Gradient(distance, dir);
                    var nearbyDensity2 = umath.sqr(_data.Density(neighborIndex));
                    var nearbyPressure = m_Pressures[neighborIndex];

                    var pressureMultiplier = mass2 * (pressure / density2 + nearbyPressure / nearbyDensity2);
                    pressureForce -= (float)pressureMultiplier * gradient;
                }

                particle.force += pressureForce;
                _particles[i] = particle;
            }
        }
        
        [Range(0,1)]public float m_ViscosityCoefficient = 0.1f;
        void AccumulateViscosityForces(ISPH _kernel,SPHData _data,List<ParticleData> _particles)
        {
            for (var i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];
                var mass2 = (double)umath.sqr(particle.mass);
                var velocity = particle.velocity;
                var neighbors = _data.Query(i);
                var viscosityForce = float3.zero;
                for (var j = 0;j < neighbors.Count; j++)
                {
                    var neighbor = neighbors[j];
                    var neighborIndex = neighbor.index;
                    var distance = (particle.position - neighbor.position).magnitude();
                    var secondDerivative = _kernel.SecondDerivative(distance);
                    var velocityDelta =  (neighbor.velocity - velocity) / (float)(_data.Density(neighborIndex) * secondDerivative);
                    viscosityForce += (float)mass2  * m_ViscosityCoefficient * velocityDelta;
                }

                particle.force += viscosityForce;
                _particles[i] = particle;
            }
        }
        
        static double ComputePressureFromEOS(double _density, double _targetDensity, double _eosScale, double _eosExponent,double _negativePressureValue)
        {
            var density = _density;
            var targetDensity = _targetDensity;

            var p = _eosScale / _eosExponent;
            var multiplier = math.pow((density / targetDensity), _eosExponent) - 1.0;
            p *= multiplier;
            if (p < 0)
                p *= _negativePressureValue;
            return (float)p;
        }

        
        public float m_ParticleVisualizeRadius = 1f;
        public int m_DebugIndex;
        public void DrawGizmos(List<ParticleData> _particles)
        {
            m_Data.DrawGizmos(_particles);
            Gizmos.color = Color.white;
            foreach (var particle in _particles)
                Gizmos.DrawWireSphere(particle.position,m_ParticleVisualizeRadius);
            
            if (_particles.Count <= 0)
                return;
            
            var position = _particles[m_DebugIndex].position;
            Gizmos.DrawWireSphere(position,m_Data.KernelRadius);
            foreach (var query in m_Data.Query(_particles,position))
                Gizmos.DrawSphere(query.position,m_ParticleVisualizeRadius);
        }

    }
}