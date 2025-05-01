using System;
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
        public int m_ParticleCount = 1024;
        public float m_InitialMass = 0.1f;
        public float m_KernelRadius;
        public float3 m_Gravity = new(0f, -9.8f,0);

        public ESPHKernel m_KernelType = ESPHKernel.Default;
        private ISPH GetKernel(float _radius) => m_KernelType switch {
            ESPHKernel.Default => new SPHStdKernel3(_radius),
            ESPHKernel.Spiky => new SPHSpikyKernel(_radius),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        public ParticleSystemData m_Data = new ();
        public ParticleSystemSolver m_Solver = new();
        
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

        private void Start()
        {
            if (!Application.isPlaying)
                return;
            for(int i=0;i<m_ParticleCount;i++)
                AddParticle(new ParticleData(){force = 0f,
                    mass = m_InitialMass,
                    position = m_Bounds.Resize(.5f).GetPoint(ULowDiscrepancySequences.Hammersley2D((uint)i,(uint)m_ParticleCount)).to3xy(),
                    velocity = 0f});
        }

        private void OnDestroy()
        {
            m_Data.Destroy();
        }

        protected override void FixedTick(float _fixedDeltaTime)
        {
            var kernel = GetKernel(m_KernelRadius);
            m_Data.Construct(kernel,particles);
            m_Solver.AccumulateForces(_fixedDeltaTime,kernel,m_Data,particles);
            AccumulateExtraForces(_fixedDeltaTime);
            TimeIntegration(_fixedDeltaTime);
        }

        void AccumulateExtraForces(float _fixedDeltaTime) 
        {
            for (var i = 0; i < Count; i++)
            {
                var particle = particles[i];
                particle.force += particle.mass * m_Gravity;
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
                _drawer.Circle((int2)particle.position.xy,(int)m_KernelRadius,Color.white);
            
            _drawer.PixelContinuousStart((int2)m_Bounds.GetPoint(new float2(0,1)));
            foreach (var bound in m_Bounds)
                _drawer.PixelContinuous((int2)bound.xy,Color.white.SetA(.2f));
        }

        public int m_DebugIndex;
        private void OnDrawGizmos()
        {
            Gizmos.matrix =  Matrix4x4.Translate(-m_Bounds.center.to3xy());
            m_Bounds.DrawGizmosXY();
            m_Data.DrawGizmos(particles);
            // foreach (var particle in particles)
            // {
                // Gizmos.DrawWireSphere(particle.position.to3xy(),m_SPH.h);
            // }

            Gizmos.color = Color.white;
            if (m_DebugIndex == -1)
            {
                foreach (var particle in particles)
                    Gizmos.DrawWireSphere(particle.position,m_KernelRadius);
            }
            
            var position = particles[m_DebugIndex].position;
            var kernel = GetKernel(m_KernelRadius);
            Gizmos.DrawSphere(position,kernel.Radius);
            Gizmos.DrawWireSphere(position,5f);
            foreach (var query in m_Data.Query(particles,kernel,position))
                Gizmos.DrawWireSphere(query.position,m_KernelRadius);
        }
    }

}