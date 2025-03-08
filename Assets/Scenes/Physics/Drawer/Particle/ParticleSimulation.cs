using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Extensions;
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
            public int index;
            public float2 position;
            public float2 velocity;
            public float2 force;
            public float mass;
            public float density;
            public struct BVHHelper : IBVHHelper<G2Box,ParticleData>
            {
                public void SortElements(int _median, G2Box _boundary, IList<ParticleData> _elements)
                {
                    var axis = _boundary.size.maxAxis();
                    _elements.Divide(_median,
                        // .Sort(
                        // ESortType.Bubble,
                        (_a, _b) => axis switch
                        {
                            EAxis.X => _a.position.x >= _b.position.x ? 1 : -1,
                            EAxis.Y => _a.position.y >= _b.position.y ? 1 : -1,
                            _ => throw new InvalidEnumArgumentException()
                        });
                }

                public G2Box CalculateBoundary(IList<ParticleData> _elements) =>
                    UGeometry.GetBoundingBox(_elements.Select(p => p.position));
            }
        }

        private class ParticleBVH : BoundingVolumeHierarchy<ParticleData.BVHHelper,G2Box, ParticleData>
        {
            public override void DrawGizmos(bool _parentMode = false)
            {
                foreach (var node in GetLeafs())
                    node.boundary.DrawGizmosXY();
            }

            protected override bool Optimize { get; }
        }

        private List<ParticleData> particles = new List<ParticleData>();
        private ParticleBVH particleQueries = new ParticleBVH();

        public int Count => particles.Count;

        private void Start()
        {
            if (!Application.isPlaying)
                return;
            for(int i=0;i<2048;i++)
                AddParticle(new ParticleData(){force = URandom.Random2DDirection() * 5000f,mass = math.lerp(1f,5f,URandom.Random01()),position = m_Bounds.center,
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

        void ResolveCollision()
        {
            particleQueries.Construct( particles.FillList(UList.Empty<ParticleData>()),m_Bounds,4,16);
            UpdateDensity((_position,_radius) => particleQueries.Query(p=>p.boundary.Intersect(new G2Circle(_position,_radius))));
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

        void UpdateDensity(Func<float2, float, IEnumerable<ParticleData>> _query)
        {
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                
                var density = 0f;
                foreach (var nearbyParticles in _query(particle.position,50f))
                {
                    var sqrDistance = (particle.position - nearbyParticles.position).sqrmagnitude();
                    density += m_SPH.Evaluate(sqrDistance);
                }
                particle.density = density;
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
            particleQueries.DrawGizmos();

            foreach (var particle in particles)
            {
                Gizmos.DrawWireSphere(particle.position.to3xy(),m_SPH.h);
            }
            
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