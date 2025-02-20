using System;
using System.Collections.Generic;
using Examples.PhysicsScenes.WaveSimulation;
using Unity.Mathematics;
using UnityEditor.Extensions;

namespace Examples.PhysicsScenes.SpringSimulation 
{
    [Serializable]
    public class SpringJoint
    {
        public float2 position;
        public float2 velocity = 0;
        public float2 force = 0;
        public float mass = 10;
    }
    public class SpringSimulation : ADrawerSimulation
    {
        public List<SpringJoint> config;

        private float timeElapsed;
        private List<SpringJoint> joints = new List<SpringJoint>();

        public float2 m_Gravity = kfloat2.down * 0.98f;
        public ColorPalette m_ColorPalette = ColorPalette.kDefault;
        
        [InspectorButton]
        public void Initialize()
        {
            timeElapsed = 0f;
            joints.Clear();
            joints.AddRange(config);
        }

        [InspectorButton]
        public void Clear()
        {
            joints.Clear();
        }

        void Draw(FTextureDrawer _drawer, IList<SpringJoint> _joints)
        {
            if (_joints.Count == 0)
                return;
                
            _drawer.PixelContinuousStart((int2)config[0].position);
            foreach (var joint in config)
            {
                var color = m_ColorPalette.Evaluate(joint.position.x / _drawer.SizeX);
                _drawer.Circle((int2)joint.position, 5, color);
                _drawer.PixelContinuous((int2)joint.position, color);
            }
        }
        
        protected override void TickDrawer(FTextureDrawer _drawer, float _deltaTime)
        {
            if (joints.Count == 0)
            {
                Draw(_drawer,config);
                return;
            }

            timeElapsed += _deltaTime;
            
            foreach (var joint in joints)
                joint.force = joint.mass * m_Gravity;
            
            Draw(_drawer,joints);
        }
    }

}