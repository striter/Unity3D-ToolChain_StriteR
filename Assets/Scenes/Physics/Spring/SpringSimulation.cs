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
        public RectTransform m_Extruder;
        public int m_ExtruderSize = 20;
        public float m_ExtruderForce = 20;
        public float2 m_Gravity = kfloat2.down * 0.98f;
        public ColorPalette m_ColorPalette = ColorPalette.kDefault;
        public float m_Stiffness = 500;
        public float m_DampingCoefficient = 1f;
        public float m_DragCoefficient = 0.1f;
        public float2 m_WindForce = new float2(0, 0);
        [Range(0f,1f)]public float m_BounceReflective = 0.5f;
        public G2Box m_Bounds = new G2Box(new float2(Screen.width *.5f, Screen.height *.5f), new float2( Screen.width *.2f, Screen.height * .2f));
        
        public List<SpringJoint> config;

        public List<SpringJoint> joints = new List<SpringJoint>();
        [Readonly] public List<SpringEdge> edges = new List<SpringEdge>();

        private void Start()
        {
            if (!Application.isPlaying)
                return;
            Initialize();
        }

        private void OnDestroy()
        {
            Clear();
        }

        [InspectorButton(true)]
        public void Bunny(float _scale = 100f,float2 _offset = default,int _mass =1)
        {
            config.Clear();
            config.AddRange( (G2Polygon.kBunny).Select(p=>new SpringJoint(){position = p * _scale + _offset,mass = _mass}));
        }

        [InspectorButton(true)]
        public void Line(float2 _start = default, float2 _end = default,int _count = 10, int _mass = 1)
        {
            config.Clear();
            for (var i = 0; i < _count; i++)
                config.Add(new SpringJoint(){position = math.lerp(_start,_end,(float)i / (_count - 1)),mass = _mass});
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

        void Draw(FTextureDrawer _drawer, IList<SpringJoint> _joints)
        {
            if (_joints.Count == 0)
                return;
            
            _drawer.PixelContinuousStart((int2)_joints[0].position);
            foreach (var (index,joint) in _joints.LoopIndex())
            {
                var color = m_ColorPalette.Evaluate((float)index / _joints.Count);
                _drawer.Circle((int2)joint.position, 5, color);
                _drawer.PixelContinuous((int2)joint.position, color);
            }
            
            _drawer.PixelContinuousStart((int2)m_Bounds.GetPoint(new float2(-.5f,.5f)));
            foreach (var bound in m_Bounds)
                _drawer.PixelContinuous((int2)bound,Color.white.SetA(.2f));

            if (m_Extruder != null)
                _drawer.Circle((int2)(float2)m_Extruder.anchoredPosition, m_ExtruderSize, Color.white);
        }


        protected override void Draw(FTextureDrawer _drawer)
        {
            if (joints.Count == 0)
            {
                Draw(_drawer,config);
                return;
            }
            Draw(_drawer,joints);
        }

        protected override void FixedTick(float _fixedDeltaTime)
        {
            foreach (var joint in joints)
            {
                joint.force = joint.mass * m_Gravity;
                joint.force += m_WindForce / joint.mass;
                joint.force -= m_DragCoefficient * joint.velocity;
                if (m_Extruder != null)
                {
                    var extruderPosition = (float2)m_Extruder.anchoredPosition;
                    var distance = math.distance(extruderPosition, joint.position);
                    if (distance < m_ExtruderSize)
                    {
                        var force = m_ExtruderForce * (joint.position - extruderPosition).normalize();
                        joint.force += force;
                    }
                }
            }

            foreach (var edge in edges)
            {
                var lengthOffset = joints[edge.startIndex].position - joints[edge.endIndex].position;
                var length = math.length(lengthOffset);
                if ((lengthOffset != float2.zero).any())
                {
                    var force = (length - edge.distance) * lengthOffset.normalize() * -m_Stiffness;
                    joints[edge.startIndex].force += force;
                    joints[edge.endIndex].force -= force;
                }

                var velocityOffset = joints[edge.startIndex].velocity - joints[edge.endIndex].velocity;
                var dampingForce = velocityOffset * -m_DampingCoefficient;
                joints[edge.startIndex].force += dampingForce;
            }

            foreach (var joint in joints)
            {
                if (joint.mass == 0f)
                    continue;
                
                var acceleration = joint.force / joint.mass;
                var newVelocity = joint.velocity + acceleration * _fixedDeltaTime;
                var newPosition = joint.position + newVelocity * _fixedDeltaTime;
                
                if (m_Bounds.Clamp(newPosition, out var clampedNewPosition))
                {
                    newPosition = clampedNewPosition;
                    newVelocity = -newVelocity * m_BounceReflective;
                }
                
                joint.velocity = newVelocity;
                joint.position = newPosition;
            }
        }
    }

}