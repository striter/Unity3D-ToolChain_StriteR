using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.PhysicsScenes.Particle
{
    public enum ESPHKernel
    {
        Default,
        Spiky,
    }
    
    public class ParticleSimulation : ADrawerSimulation
    {
        public bool m_Simulate = false;
        public float m_InitialMass = 0.1f;
        public float3 m_Gravity = new(0f, -9.8f,0);
        public ParticleSystemSolver m_Solver = new();
        [Range(0f, 1f)] public float m_EmitterRange = 0.3f;
        
        [Header("Bounds")]
        public GBox m_Bounds = new GBox(0f, new float3(1f,1.5f,1f));
        [Range(0, 1)] public float m_BoundsBounceCoefficient = 0.1f;
        private List<ParticleData> particles = new List<ParticleData>();
        public int Count => particles.Count;

        private void OnValidate()
        {
            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.mass = m_InitialMass;
                particles[i] = particle;
            }
        }

        [InspectorButtonRuntime]
        private void Start()
        {
            if (!Application.isPlaying)
                return;
            particles.Clear();
            var bounds = GBox.Minmax(m_Bounds.GetPoint(0f),m_Bounds.GetPoint(1f,m_EmitterRange,1f));
            var positions = ULowDiscrepancySequences.BCCLattice3D(m_Solver.m_Data.m_TargetSpacing / bounds.size,0.01f);
            for(int i=0;i<positions.Length;i++)
                AddParticle(new ParticleData(){force = 0f,
                    mass = m_InitialMass,
                    position = bounds.GetPoint(positions[i]),
                    velocity = 0f});
        }

        
        private void OnDestroy()
        {
            m_Solver.Destroy();
        }

        protected override void Update()
        {
            if(Input.GetKeyDown(KeyCode.R))
                Start();
            
            if (Input.GetKey(KeyCode.Space))
                m_Simulate = true;
            base.Update();
        }

        protected override void FixedTick(float _fixedDeltaTime)
        {
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                particle.force = 0f;
                particles[i] = particle;
            }
            m_Solver.AccumulateForces(_fixedDeltaTime,particles);
            if (m_Simulate)
            {
                AccumulateExtraForces(_fixedDeltaTime);
                TimeIntegration(_fixedDeltaTime);
                m_Simulate = false;
            }
        }

        void AccumulateExtraForces(float _fixedDeltaTime) 
        {
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                particle.force += particle.mass * m_Gravity;
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
                particle.force = 0f;
                particles[i] = particle;
            }
            
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                
                if (m_Bounds.Clamp(particle.position, out var clampedNewPosition))
                {
                    particle.position.xy = clampedNewPosition.xy;
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

        [InspectorButton(true)]
        void ComputeMass()
        {
            m_InitialMass = m_Solver.m_Data.ComputeMass();
        }

        protected override void Draw(FTextureDrawer _drawer)
        {
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.Translate(-m_Bounds.center);
            m_Bounds.DrawGizmos();
            m_Solver.DrawGizmos(particles);
        }
    }

}