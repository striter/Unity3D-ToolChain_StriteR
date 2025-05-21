using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Scripting;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.PhysicsScenes.SmoothParticleHydrodynamics
{
    [Serializable]
    public class SPHData
    {
        public float m_TargetSpacing = 0.1f;
        public float m_KernelRadiusOverTargetSpacing = 1.8f;
        public float m_TargetDensity = 50f;
        public float m_SpeedOfSound = 1480f;

        public float KernelRadius => m_TargetSpacing * m_KernelRadiusOverTargetSpacing;
        public ESPHKernel m_KernelType = ESPHKernel.Default;
        public ISPH GetKernel() => m_KernelType switch {
            ESPHKernel.Default => new SPHStdKernel3( m_KernelRadiusOverTargetSpacing * m_TargetSpacing ),
            ESPHKernel.Spiky => new SPHSpikyKernel( m_KernelRadiusOverTargetSpacing * m_TargetSpacing),
            _ => throw new ArgumentOutOfRangeException()
        };

        
        public List<double> m_Densities = new();
        private ParticleBVH m_ParticleQueries = new ParticleBVH(4,8);

        private List<List<ParticleData>> m_ParticleQueryCache = new();
        private static ListPool<ParticleData> kIndexPool = new();
        public void Destroy()
        {
            m_ParticleQueryCache.Clear();
        }
        
        public double Density(int _index) => m_Densities[_index];
        public void Construct(ISPH _kernel,List<ParticleData> particles)
        {
            m_ParticleQueries.Construct(particles);
            var count = particles.Count;
            m_ParticleQueryCache.Resize(count,kIndexPool.Spawn,kIndexPool.Despawn);
            for (var i = 0; i < count; i++)
                m_ParticleQueries.Query(particles,new ParticleDensityQuery(particles[i],KernelRadius),m_ParticleQueryCache[i]);
            
            m_Densities.Resize(count);
            for (var i = count - 1; i >= 0 ; i--)
            {
                var particle = particles[i];
                var origin = particle.position;
                var sum = 0f;
                var nearbyParticles = m_ParticleQueries.Query(particles, new ParticleDensityQuery(particles[i].position, KernelRadius));
                for (var j = nearbyParticles.Count - 1; j >= 0; j--)
                {
                    var nearbyParticle = nearbyParticles[j];
                    var distance = (origin - nearbyParticle.position).magnitude();
                    var kernel = _kernel[distance];
                    var nearbyMass = (double)nearbyParticle.mass;
                    sum += (float)(nearbyMass * kernel);
                }

                m_Densities[i] = sum; 
            }
        }
        
        public List<ParticleData> Query(int particleIndex) => m_ParticleQueryCache[particleIndex];
        public List<ParticleData> Query(List<ParticleData> _data,float3 _position) => m_ParticleQueries.Query(_data,new ParticleDensityQuery(_position,KernelRadius));

        public float3 Interpolate(ISPH kernel,List<ParticleData> particles,float3 _origin, float3[] _values)
        {
            var sum = float3.zero;
            var nearbyParticles = m_ParticleQueries.Query(particles,new ParticleDensityQuery() { circle = new GSphere(_origin, kernel.Radius) });
            for (var i = nearbyParticles.Count - 1; i >= 0; i--)
            {
                var particle = nearbyParticles[i];
                var distance = (_origin - particle.position).magnitude();
                var weight = (float)(particle.mass / m_Densities[i] * kernel[distance]);
                sum += weight * _values[i];
            }
            return sum;
        }
        
        // public float3 GradientAt(ISPH kernel,List<ParticleData> particles,int _index, float[] _values)
        // {
        //     var particle = particles[_index];
        //     var sum = kfloat3.zero;
        //     var origin = particle.position;
        //     var mass = particle.mass;;
        //     var value = _values[_index];
        //     var density = m_Densities[_index];
        //     var nearbyParticles = Query(_index);
        //     for(var j = nearbyParticles.Count - 1 ; j>=0;j--)
        //     {
        //         var nearbyParticle = nearbyParticles[j];
        //         var distance = (nearbyParticle.position - origin).magnitude();
        //         if (distance > 0)
        //         {
        //             var dir = (nearbyParticle.position - origin) / distance;
        //             sum += density * mass * 
        //                    (value / umath.sqr(density) + _values[j] / umath.sqr(m_Densities[j]))
        //                     * kernel.Gradient(distance,dir);
        //         }
        //     }
        //     return sum;
        // }

        public float LaplacianAt(ISPH kernel,List<ParticleData> particles,int _index, float[] _values)
        {
            var particle = particles[_index];
            var origin = particle.position;
            var mass = (double)particle.mass;
            var value = _values[_index];
            var nearbyParticles = Query(_index);
            var sum = 0f;
            for(var j = nearbyParticles.Count - 1 ; j>=0;j--)
            {
                var nearbyParticle = nearbyParticles[j];
                var distance = (nearbyParticle.position - origin).magnitude();
                var valueDelta = _values[j] - value;
                if (distance > 0)
                    sum += valueDelta * (float) (mass/ distance * kernel.SecondDerivative(distance));
            }
            return sum;
        }

        public void DrawGizmos(List<ParticleData> _particles)
        {
            m_ParticleQueries.DrawGizmos(_particles);
        }

        public float ComputeMass()
        {
            var kernel = GetKernel();
            var targetDensity = m_TargetDensity;
            var kernelRadius = kernel.Radius;
            var points = ULowDiscrepancySequences.BCCLattice2D((1.5f * kernelRadius)/ m_TargetSpacing,0.01f);
            var maxNumberDensity = 0.0f;

            for (var i = 0; i < points.Length; ++i) {
                var point = points[i];
                var sum = 0.0f;

                for (var j = 0; j < points.Length; ++j) {
                    var neighborPoint = points[j];
                    sum += (float)kernel[(neighborPoint - point).magnitude()];
                }

                maxNumberDensity = math.max(maxNumberDensity, sum);
            }

            Debug.Assert(maxNumberDensity > 0);

            return targetDensity / maxNumberDensity;
        }
    }
}