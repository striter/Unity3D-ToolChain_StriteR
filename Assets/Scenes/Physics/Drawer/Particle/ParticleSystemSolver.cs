using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Examples.PhysicsScenes.Particle
{
    [Serializable]
    public class ParticleSystemSolver
    {
        public float m_TargetDensity = 50f;
        public float m_NegativePressureScale = -.5f;
        public float m_SpeedOfSound = 1480f;
        public float m_EOSExponent = 5f;

        public float m_ViscosityCoefficient = 0.1f;
        
        public List<double> m_Pressures = new();
        public List<float3> m_PressureForces = new();
        public List<float3> m_ViscosityForces = new();
        public double Pressure(int _index) => m_Pressures[_index];
        public void AccumulateForces(float _fixedDeltaTime,ISPH _kernel,ParticleSystemData _data,List<ParticleData> _particles)
        {
            var eosScale = m_TargetDensity * umath.sqr(m_SpeedOfSound);
            
            m_Pressures.Resize(_particles.Count);
            for (var i = 0; i < _particles.Count; i++)
                m_Pressures[i] = ComputePressureFromEOS(_data.Density(i),m_TargetDensity,eosScale,m_EOSExponent,m_NegativePressureScale);

            AccumulatePressureForces(_kernel , _data , _particles);
            // AccumulateViscosityForces(_kernel , _data, _particles);
        }

        void AccumulatePressureForces(ISPH _kernel,ParticleSystemData _data,List<ParticleData> _particles)
        {
            m_PressureForces.Resize(_particles.Count);
            for (var i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                var mass2 = (double)umath.sqr(particle.mass);
                var pressure = Pressure(i);
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
                    var nearbyPressure = Pressure(neighborIndex);

                    var pressureMultiplier = mass2 * (pressure / density2 + nearbyPressure / nearbyDensity2);
                    pressureForce -= (float)pressureMultiplier * gradient;
                }

                m_PressureForces[i] = pressureForce;
                particle.force += pressureForce;
                _particles[i] = particle;
            }
        }
        
        void AccumulateViscosityForces(ISPH _kernel,ParticleSystemData _data,List<ParticleData> _particles)
        {
            m_ViscosityForces.Resize(_particles.Count);
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
                    viscosityForce += (float)mass2 * velocityDelta * m_ViscosityCoefficient;
                }

                m_ViscosityForces[i] = viscosityForce;
                particle.force += viscosityForce;
                _particles[i] = particle;
            }
        }
        
        static double ComputePressureFromEOS(double _density, double _targetDensity, double _eosScale, double _eosExponent,double _negativePressureValue)
        {
            var density = _density;
            var targetDensity = _targetDensity;

            var p = _eosScale / _eosExponent;
            var multiplier = (math.pow((density / targetDensity), _eosExponent) - 1.0);
            p *= multiplier;
            if (p < 0)
                p *= _negativePressureValue;
            return (float)p;
        }

    }
}