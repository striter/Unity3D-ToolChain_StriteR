using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Examples.PhysicsScenes.WaveSimulation;
using Runtime.Geometry;
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

    [Serializable]
    public struct SpringEdge
    {
        public int startIndex;
        public int endIndex;
        public float distance;
        public Damper damper;
    }
    
    public class SpringSimulation : ADrawerSimulation
    {
        [Readonly] public Ticker m_Ticker = new Ticker(1f/60f);
        public float2 m_Gravity = kfloat2.down * 0.98f;
        public ColorPalette m_ColorPalette = ColorPalette.kDefault;
        public float m_Stiffness = 500;
        public Damper m_EdgeDamper = new Damper(){mode = EDamperMode.SpringCritical,halfLife = 0.1f};
        public List<SpringJoint> config;

        [Readonly] public List<SpringJoint> joints = new List<SpringJoint>();
        [Readonly] public List<SpringEdge> edges = new List<SpringEdge>();

        [InspectorButton]
        public void Initialize()
        {
            m_Ticker.Reset();
            joints.Clear();
            joints.AddRange(config.DeepCopy());
            edges.Clear();
            for (var i = 0; i < joints.Count - 1; i++)
                edges.Add(new SpringEdge(){startIndex = i, endIndex = (i + 1) % joints.Count, distance = math.length(joints[i].position - joints[(i + 1) % joints.Count].position),damper = m_EdgeDamper});
        }

        [InspectorButton]
        public void Clear()
        {
            joints.Clear();
            edges.Clear();
        }

        [InspectorButton(true)]
        public void BunnyConfig(float scale = 100f,float2 offset = default)
        {
            config.Clear();
            config.AddRange( (G2Polygon.kBunny).Select(p=>new SpringJoint(){position = p * scale + offset,mass = 10}));
        }
        
        void Draw(FTextureDrawer _drawer, IList<SpringJoint> _joints)
        {
            if (_joints.Count == 0)
                return;
            
            _drawer.PixelContinuousStart((int2)_joints[0].position);
            foreach (var joint in _joints)
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

            if (m_Ticker.Tick(_deltaTime))
            {
                foreach (var joint in joints)
                {
                    joint.force = joint.mass * m_Gravity;
                }

                foreach (var edge in edges)
                {
                    var lengthOffset = joints[edge.startIndex].position - joints[edge.endIndex].position;
                    var length = math.length(lengthOffset);
                    var force = (length - edge.distance) * lengthOffset.normalize() * -m_Stiffness;
                    joints[edge.startIndex].force += force;
                    joints[edge.endIndex].force -= force;
                }

                foreach (var joint in joints)
                {
                    if (joint.mass == 0f)
                        continue;
                    
                    var acceleration = joint.force / joint.mass;
                    joint.velocity += acceleration * _deltaTime;
                    joint.position += joint.velocity * _deltaTime;
                }
            }
            
            Draw(_drawer,joints);
        }
    }

}