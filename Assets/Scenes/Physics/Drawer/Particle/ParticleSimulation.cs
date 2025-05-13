using System;
using System.Collections.Generic;
using Rendering.PostProcess;
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
        public G2Box m_Bounds = new G2Box(new float2(Screen.width *.5f, Screen.height *.5f), new float2( Screen.width *.2f, Screen.height * .2f));
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
            var bounds = m_Bounds.Collapse(new float2(1f,.5f),kfloat2.down * m_EmitterRange);
            var positions = ULowDiscrepancySequences.BCCLattice2D(m_Solver.m_Data.m_TargetSpacing / bounds.size);
            for(int i=0;i<positions.Length;i++)
                AddParticle(new ParticleData(){force = 0f,
                    mass = m_InitialMass,
                    position = bounds.GetPoint(positions[i]).to3xy(),
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
                
                if (m_Bounds.Clamp(particle.position.xy, out var clampedNewPosition))
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
            _drawer.PixelContinuousStart((int2)m_Bounds.GetPoint(new float2(0,1)));
            foreach (var bound in m_Bounds)
                _drawer.PixelContinuous((int2)bound.xy,Color.white.SetA(.2f));
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix =  Matrix4x4.Translate(-m_Bounds.center.to3xy());
            m_Bounds.DrawGizmosXY();
            m_Solver.DrawGizmos(particles);
            // foreach (var particle in particles)
            // {
                // Gizmos.DrawWireSphere(particle.position.to3xy(),m_SPH.h);
            // }
        }
    }

}