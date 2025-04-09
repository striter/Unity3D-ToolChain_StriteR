using System;
using System.Collections.Generic;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.PhysicsScenes.Particle
{
    public class ParticleSimulation : ADrawerSimulation
    {
        public float2 m_Gravity = new float2(0f, -9.8f);
        public float2 m_WindForce = new float2(1f, 0f);
        [Range(0, 1)] public float m_AirDragCoefficient = 0.1f;
        public SmoothedParticleHydrodynamics.SPHStdKernal3 m_SPH = SmoothedParticleHydrodynamics.SPHStdKernal3.kDefault;
        
        [Header("Bounds")]
        public G2Box m_Bounds = new G2Box(new float2(Screen.width *.5f, Screen.height *.5f), new float2( Screen.width *.2f, Screen.height * .2f));
        [Range(0, 1)] public float m_BoundsBounceCoefficient = 0.1f;
        private struct ParticleData
        {
            [NonSerialized] public int index;
            public float2 position;
            public float2 velocity;
            public float2 force;
            public float mass;
            [NonSerialized] public float density;
            public class BVHHelper : IBVHHelper<G2Box,ParticleData>
            {
                private static IList<ParticleData> kElements;
                Comparison<int> CompareX = (_a, _b) => kElements[_a].position.x >= kElements[_b].position.x ? 1 : -1;
                Comparison<int> CompareY = (_a, _b) => kElements[_a].position.y >= kElements[_b].position.y ? 1 : -1;
                public void SortElements(int _median, G2Box _boundary,IList<int> _elementIndexes, IList<ParticleData> _elements)
                {
                    kElements = _elements;
                    switch (_boundary.size.maxAxis())
                    {
                        case EAxis.X: _elementIndexes.Divide(_median,CompareX); break;
                        case EAxis.Y: _elementIndexes.Divide(_median,CompareY); break;
                    }
                }

                public G2Box CalculateBoundary(IList<ParticleData> _elements) => UGeometry.GetBoundingBox(_elements,p=>p.position);
            }
        }
        private class ParticleBVH : BoundingVolumeHierarchy<G2Box, ParticleData,ParticleData.BVHHelper>
        {
            public ParticleBVH(int _nodeCapcity, int _maxIteration) : base(_nodeCapcity, _maxIteration) { }
            public override void DrawGizmos(IList<ParticleData> _elements, bool _parentMode = false)
            {
                // base.DrawGizmos(_elements, _parentMode);
                foreach (var leaf in GetLeafs())
                    leaf.boundary.DrawGizmosXY();
            }
        }

        private List<ParticleData> particles = new List<ParticleData>();
        private ParticleBVH particleQueries = new ParticleBVH(32,4);

        public int Count => particles.Count;

        private void Start()
        {
            if (!Application.isPlaying)
                return;
            for(int i=0;i<2048;i++)
                AddParticle(new ParticleData(){force = URandom.Random2DDirection() * 5000f,mass = math.lerp(1f,5f,URandom.Random01()),position = m_Bounds.center + (float2)URandom.Random2DSphere() * 500f,
                    velocity = URandom.Random2DSphere() * 10f,density = 1f});
        }

        protected override void FixedTick(float _fixedDeltaTime)
        {
            AccumulateForces(_fixedDeltaTime);
            TimeIntegration(_fixedDeltaTime);
            ResolveCollision();
        }

        void AccumulateForces(float _fixedDeltaTime)
        {
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                var force = particle.mass * m_Gravity;
                force += m_WindForce / particle.mass;

                force -= m_AirDragCoefficient * force;

                particle.force = force;
                particles[i] = particle;
            }
        }

        void TimeIntegration(float _fixedDeltaTime)
        {
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];

                particle.velocity += _fixedDeltaTime * particle.force / particle.mass;
                particle.position += particle.velocity * _fixedDeltaTime;
                particles[i] = particle;
            }
        }


        private class ParticleDenstiyQuery : IBoundaryTreeQuery<G2Box,ParticleData>
        {
            public G2Circle circle;
            public bool Query(G2Box _boundary) => _boundary.Intersect(circle);
            public bool Query(ParticleData _element) => circle.Contains(_element.position);
        }

        private ParticleDenstiyQuery kQuery = new ();
        void ResolveCollision()
        {
            particleQueries.Construct( m_Bounds,particles);
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                
                var density = 0f;
                kQuery.circle = new G2Circle(particle.position, 5f);
                var nearbyParticles = kQuery.Query(particleQueries,particles);
                for(var j = nearbyParticles.Count- 1 ; j>=0;j--)
                {
                    var nearbyParticle = nearbyParticles[j];
                    var sqrDistance = (particle.position - nearbyParticle.position).sqrmagnitude();
                    density += m_SPH.Evaluate(sqrDistance);
                }
                particle.density = density;
                particles[i] = particle;
            }
            
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                
                if (m_Bounds.Clamp(particle.position, out var clampedNewPosition))
                {
                    particle.position = clampedNewPosition;
                    particle.velocity = -particle.velocity * m_BoundsBounceCoefficient;
                }
                
                particles[i] = particle;
            }
        }

        
        [InspectorButton]
        void AddParticle(ParticleData _data)
        {
            _data.index = particles.Count;
            particles.Add(_data);
        }

        protected override void Draw(FTextureDrawer _drawer)
        {
            foreach (var particle in particles)
                _drawer.Circle((int2)particle.position,(int)m_SPH.h,Color.white);
            
            _drawer.PixelContinuousStart((int2)m_Bounds.GetPoint(new float2(0,1)));
            foreach (var bound in m_Bounds)
                _drawer.PixelContinuous((int2)bound,Color.white.SetA(.2f));
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(-m_Bounds.center.to3xy());
            particleQueries.DrawGizmos(particles);
            // foreach (var particle in particles)
            // {
                // Gizmos.DrawWireSphere(particle.position.to3xy(),m_SPH.h);
            // }
            
            var position = Input.mousePosition.XY();
            Gizmos.DrawWireSphere(position,5f);
            // foreach (var query in particleQueries.Query(p => p.boundary.Contains(position)))
                // Gizmos.DrawWireSphere(query.position.to3xy(),query.radius);
        }
    }

    public static class SmoothedParticleHydrodynamics
    {
        [Serializable]
        public struct SPHStdKernal3 : ISerializationCallbackReceiver
        {
            public float h;
            [NonSerialized] public float h2;
            [NonSerialized]public float h3;

            SPHStdKernal3 Ctor()
            {
                h2 = h * h;
                h3 = h * h * h;
                return this;
            }
            
            public float Evaluate(float _sqrDistance)
            {
                if (_sqrDistance >= h2)
                    return 0f;
                var x = 1f - _sqrDistance / h2;
                return 135f / (64f * kmath.kPI * h3) * x * x * x;
            }

            public static readonly SPHStdKernal3 kDefault = new SPHStdKernal3() { h = 1 }.Ctor();

            public void OnBeforeSerialize() {}
            public void OnAfterDeserialize() => Ctor();
        }
    }

}