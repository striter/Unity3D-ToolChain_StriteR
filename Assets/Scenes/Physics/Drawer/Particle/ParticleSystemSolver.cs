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
        
        private List<float> m_Pressures = new();
        public float Pressure(int _index) => m_Pressures[_index];
        public void AccumulateForces(float _fixedDeltaTime,ISPH _kernel,ParticleSystemData _data,List<ParticleData> _particles)
        {
            var eosScale = m_TargetDensity * umath.sqr(m_SpeedOfSound);
            
            m_Pressures.Resize(_particles.Count);
            for (var i = 0; i < _particles.Count; i++)
                m_Pressures[i] = ComputePressureFromEOS(_data.Density(i),m_TargetDensity,eosScale,m_EOSExponent,m_NegativePressureScale);

            AccumulatePressureForces(_kernel , _data , _particles);
            AccumulateViscosityForces(_kernel , _data, _particles);
        }

        void AccumulatePressureForces(ISPH _kernel,ParticleSystemData _data,List<ParticleData> _particles)
        {
            for (var i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                var mass2 = umath.sqr(particle.mass);
                var pressure = Pressure(i);
                var density = _data.Density(i);
                var density2 = density * density;
                var neighbors = _data.Query(particle.position,_kernel.Radius,_particles);
                var pressureForce = float2.zero;
                for (var j = neighbors.Count - 1; j >= 0; j--)
                {
                    var neighbor = neighbors[j];
                    var distance = (particle.position - neighbor.position).magnitude();
                    if (distance > 0)
                    {
                        var dir = (particle.position - neighbor.position) / distance;
                        pressureForce -= mass2 * (pressure / density2) + Pressure(j) / umath.sqr(_data.Density(j)) * _kernel.Gradient(distance,dir);
                    }
                }

                particle.force += pressureForce;
                _particles[i] = particle;
            }
        }
        
        void AccumulateViscosityForces(ISPH _kernel,ParticleSystemData _data,List<ParticleData> _particles)
        {
            for (var i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                var mass2 = umath.sqr(particle.mass);
                var velocity = particle.velocity;
                var neighbors = _data.Query(particle.position,_kernel.Radius,_particles);
                var viscosityForce = float2.zero;
                for (var j = neighbors.Count - 1; j >= 0; j--)
                {
                    var neighbor = neighbors[j];
                    var distance = (particle.position - neighbor.position).magnitude();
                    viscosityForce += m_ViscosityCoefficient * mass2
                                                             * (_particles[j].velocity - velocity) / _data.Density(j)
                                                             * _kernel.SecondDerivative(distance);
                }

                particle.force += viscosityForce;
                _particles[i] = particle;
            }
        }
        
        static float ComputePressureFromEOS(float _density, float _targetDensity, float _eosScale, float _eosExponent,float _negativePressureValue)
        {
            var p = _eosScale / _eosExponent 
                    * (math.pow((_density / _targetDensity), _eosExponent) - 1f);
            if (p < 0)
                p *= _negativePressureValue;
            return p;
        }

    }
}