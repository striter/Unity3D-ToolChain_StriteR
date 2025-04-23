using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Examples.PhysicsScenes.Particle
{
    [Serializable]
    public class ParticleSystemData
    {
        private List<float> m_Densities = new();
        private ParticleBVH kParticleQueries = new ParticleBVH(32,4);
        public List<ParticleData> Query(float2 _origin, float _radius, List<ParticleData> _particles)
        {
            var query = new ParticleDensityQuery() { circle = new G2Circle(_origin, _radius) };
            return kParticleQueries.Query(_particles,query);
        }
        public float Density(int _index) => m_Densities[_index];
        public void Construct(ISPH _kernel,List<ParticleData> particles)
        {
            kParticleQueries.Construct(particles);
            var count = particles.Count;
            m_Densities.Resize(particles.Count);
            for (var i = 0; i < count; i++)
                m_Densities[i] = SumOfKernelsNearby(_kernel,particles,particles[i].position) * particles[i].mass;
        }

        public float2 Interpolate(ISPH kernel,List<ParticleData> particles,float2 _origin, float2[] _values)
        {
            var sum = float2.zero;
            var nearbyParticles = Query(_origin,kernel.Radius, particles);
            for (var i = nearbyParticles.Count - 1; i >= 0; i--)
            {
                var particle = nearbyParticles[i];
                var distance = (_origin - particle.position).magnitude();
                var weight = particle.mass / m_Densities[i] * kernel[distance];
                sum += weight * _values[i];
            }
            return sum;
        }

        public float SumOfKernelsNearby(ISPH kernel,List<ParticleData> particles,float2 _origin)
        {
            var sum = 0f;
            var nearbyParticles  = Query(_origin,kernel.Radius, particles); 
            for (var i = nearbyParticles.Count - 1; i >= 0; i--)
            {
                var particle = nearbyParticles[i];
                var distance = (_origin - particle.position).magnitude();
                sum += kernel[distance];
            }
            return sum;
        }

        public float2 GradientAt(ISPH kernel,List<ParticleData> particles,int _index, float[] _values)
        {
            var particle = particles[_index];
            var sum = kfloat2.zero;
            var origin = particle.position;
            var mass = particle.mass;;
            var value = _values[_index];
            var density = m_Densities[_index];
            var nearbyParticles = Query(particle.position,kernel.Radius, particles);
            for(var j = nearbyParticles.Count - 1 ; j>=0;j--)
            {
                var nearbyParticle = nearbyParticles[j];
                var distance = (nearbyParticle.position - origin).magnitude();
                if (distance > 0)
                {
                    var dir = (nearbyParticle.position - origin) / distance;
                    sum += density * mass * 
                           (value / umath.sqr(density) + _values[j] / umath.sqr(m_Densities[j]))
                            * kernel.Gradient(distance,dir);
                }
            }
            return sum;
        }

        public float LaplacianAt(ISPH kernel,List<ParticleData> particles,int _index, float[] _values)
        {
            var particle = particles[_index];
            var origin = particle.position;
            var mass = particle.mass;;
            var value = _values[_index];
            var nearbyParticles = Query(particle.position,kernel.Radius, particles);
            var sum = 0f;
            for(var j = nearbyParticles.Count - 1 ; j>=0;j--)
            {
                var nearbyParticle = nearbyParticles[j];
                var distance = (nearbyParticle.position - origin).magnitude();
                if (distance > 0)
                    sum += mass * (_values[j] - value) / distance * kernel.SecondDerivative(distance);
            }
            return sum;
        }

        public void DrawGizmos(List<ParticleData> _particles)
        {
            kParticleQueries.DrawGizmos(_particles);
        }
        
    }
}