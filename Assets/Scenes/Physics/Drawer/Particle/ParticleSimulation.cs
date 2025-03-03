using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.PhysicsScenes.Particle
{
    public class ParticleSimulation : ADrawerSimulation
    {
        public G2Box m_Bounds = new G2Box(new float2(Screen.width *.5f, Screen.height *.5f), new float2( Screen.width *.2f, Screen.height * .2f));

        private List<float2> positions;
        private List<float2> velocities;
        private List<float2> forces;
        
        protected override void FixedTick(float _fixedDeltaTime)
        {
            
        }

        void AccumulateForces(float _fixedDeltaTime)
        {
            
        }

        void TimeIntegration(float _fixedDeltaTime)
        {
            
        }

        void ResolveCollision()
        {
            
        }

        protected override void Draw(FTextureDrawer _drawer)
        {
            _drawer.PixelContinuousStart((int2)m_Bounds.GetPoint(new float2(0,1)));
            foreach (var bound in m_Bounds)
                _drawer.PixelContinuous((int2)bound,Color.white.SetA(.2f));
        }
    }

}