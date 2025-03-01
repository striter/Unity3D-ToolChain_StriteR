using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Examples.PhysicsScenes.WaveSimulation;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

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
    }
    
    public class SpringSimulation : ADrawerSimulation
    {
        [Readonly] public Ticker m_Ticker = new Ticker(1f/60f);
        public float2 m_Gravity = kfloat2.down * 0.98f;
        public ColorPalette m_ColorPalette = ColorPalette.kDefault;
        public float m_Stiffness = 500;
        public float m_DampingCoefficient = 1f;
        public float m_DragCoefficient = 0.1f;
        public float2 m_WindForce = new float2(0, 0);
        [Range(0f,1f)]public float m_BounceReflective = 0.5f;
        public G2Box m_Bounds = new G2Box(new float2(Screen.width *.5f, Screen.height *.5f), new float2( Screen.width *.2f, Screen.height * .2f));
        
        public List<SpringJoint> config;

        [Readonly] public List<SpringJoint> joints = new List<SpringJoint>();
        [Readonly] public List<SpringEdge> edges = new List<SpringEdge>();

        private void Start()
        {
            if (!UnityEngine.Application.isPlaying)
                return;
            Initialize();
        }

        [InspectorButton(true)]
        public void Bunny(float _scale = 100f,float2 _offset = default,int _mass =1)
        {
            config.Clear();
            config.AddRange( (G2Polygon.kBunny).Select(p=>new SpringJoint(){position = p * _scale + _offset,mass = _mass}));
        }
        
        [InspectorButton]
        public void Initialize()
        {
            m_Ticker.Reset();
            joints.Clear();
            joints.AddRange(config.DeepCopy());
            edges.Clear();
            for (var i = 0; i < joints.Count - 1; i++)
                edges.Add(new SpringEdge(){startIndex = i, endIndex = (i + 1) % joints.Count, distance = math.length(joints[i].position - joints[(i + 1) % joints.Count].position)});
        }

        [InspectorButton]
        public void Clear()
        {
            joints.Clear();
            edges.Clear();
        }

        private static readonly Color kBoundsColor = Color.white.SetA(.2f);
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
            
            _drawer.PixelContinuousStart((int2)m_Bounds.GetPoint(new float2(0,1)));
            foreach (var bound in m_Bounds)
                _drawer.PixelContinuous((int2)bound,kBoundsColor);
        }
        
        protected override void TickDrawer(FTextureDrawer _drawer, float _deltaTime)
        {
            if (joints.Count == 0)
            {
                Draw(_drawer,config);
                return;
            }

            var maxSimulatePerTick= 5;
            while (maxSimulatePerTick-- > 0 && m_Ticker.Tick(_deltaTime))
            {
                _deltaTime = 0f;
                var simulateDeltaTime = m_Ticker.duration;
                foreach (var joint in joints)
                {
                    joint.force = joint.mass * m_Gravity;
                    joint.force += m_WindForce;
                    joint.force -= m_DragCoefficient * joint.velocity;
                }

                foreach (var edge in edges)
                {
                    var lengthOffset = joints[edge.startIndex].position - joints[edge.endIndex].position;
                    var length = math.length(lengthOffset);
                    if (length != 0f)
                    {
                        var force = (length - edge.distance) * lengthOffset.normalize() * -m_Stiffness;
                        joints[edge.startIndex].force += force;
                        joints[edge.endIndex].force -= force;
                    }

                    var velocityOffset = joints[edge.startIndex].velocity - joints[edge.endIndex].velocity;
                    var velocity = math.length(velocityOffset);
                    var dampingForce = velocity * -m_DampingCoefficient;
                    joints[edge.startIndex].force += dampingForce;
                }

                foreach (var joint in joints)
                {
                    if (joint.mass == 0f)
                        continue;
                    
                    var acceleration = joint.force / joint.mass;
                    var newVelocity = joint.velocity + acceleration * simulateDeltaTime;
                    var newPosition = joint.position + newVelocity * simulateDeltaTime;

                    if (m_Bounds.Clamp(newPosition, out var clampedNewPosition))
                    {
                        newPosition = clampedNewPosition;
                        newVelocity = -newVelocity * m_BounceReflective;
                    }
                    
                    joint.velocity = newVelocity;
                    joint.position = newPosition;
                }
            }
            
            Draw(_drawer,joints);
        }
    }

}