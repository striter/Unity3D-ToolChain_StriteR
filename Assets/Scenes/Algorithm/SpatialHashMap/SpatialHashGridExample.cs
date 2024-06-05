using System;
using System.Collections.Generic;
using TPool;
using System.Linq.Extensions;
using UnityEngine;

namespace Examples.Algorithm.SpatialHashGrid
{
    using static SpatialHashGridExample;
    public class SpatialHashGridExample : MonoBehaviour
    {
        public static SpatialHashGridExample Instance;
        public float m_Velocity;
        public int m_Amount;

        [Header("Boids")]
        public RangeFloat m_SenseRadius;
        public float m_Cohesion;
        public float m_Alignment;
        public float m_Separation;
        
        private ObjectPoolClass<int, Actor> m_Actors;
        private TileGraph m_Graph;
        private SpatialHashMap<Int2, TileGraph, Actor> m_SpatialHashMap;
        
        private void Awake()
        {
            Instance = this;
            m_Actors = new ObjectPoolClass<int, Actor>(transform.Find("Actor"));
            m_Graph = new TileGraph(m_SenseRadius.end);
            m_SpatialHashMap = new SpatialHashMap<Int2, TileGraph, Actor>(m_Graph);
            Spawn();
        }

        private void OnValidate()
        {
            if (m_SpatialHashMap==null)
                return;
            
            Spawn();
        }

        private void OnDestroy()
        {
            Instance = null;
            m_SpatialHashMap.Dispose();
        }

        void Spawn()
        {
            m_Actors.Clear();
            m_SpatialHashMap.Reset();
            for (int i = 0; i < m_Amount; i++)
            {
                var actor = m_Actors.Spawn().Init(URandom.Random2DSphere().ToVector3_XZ()*m_SenseRadius.end*10,URandom.Random2DSphere().ToVector3_XZ());
                m_SpatialHashMap.Register(actor);
            }
        }

        // Update is called once per frame
        void Update()
        {
            float deltaTime = Time.deltaTime;
            m_SpatialHashMap.Tick(deltaTime);
            m_Actors.Traversal(p=>p.Tick(deltaTime,m_SpatialHashMap.QueryRange(m_Actors.transform.position,m_SenseRadius.end)));


            foreach (var actor in m_Actors)
                actor.ApplyColor(Color.white);
            
            if (m_Actors.Count > 0)
            {
                foreach (var actor in m_SpatialHashMap.QueryRange(m_Actors[0].position,m_SenseRadius.end))
                    actor.ApplyColor(Color.green);
                m_Actors[0].ApplyColor(Color.red);
            }
        }

        private void OnDrawGizmos()
        {
            if (m_Actors == null || m_Actors.Count == 0)
                return;
            
            Gizmos.DrawWireSphere(m_Actors[0].position,m_SenseRadius.end);

            var srcNode = m_Graph.GetNode(m_Actors[0].position);
            foreach (var node in m_Graph.GetAdjacentNodes(srcNode).Extend(srcNode))
            {
                Gizmos.color = node == srcNode ? Color.red : Color.green.SetA(.3f);
                m_Graph.DrawGizmos(node);
            }
            
        }
    }

    public class Actor:ITransformHandle,ITransform
    {
        public Transform transform { get; }
        public Vector3 position { get; private set; }
        public Vector3 direciton { get; private set; }
        public Actor(Transform _transform){transform = _transform;}
        private MeshRenderer m_Renderer;
        private MaterialPropertyBlock m_Block;
        public Actor Init(Vector3 _position,Vector3 _direction)
        {
            m_Block = new MaterialPropertyBlock();
            position = _position;
            direciton = _direction;
            m_Renderer = transform.GetComponent<MeshRenderer>();
            return this;
        }
        
        public void Tick(float _deltaTime,IEnumerable<Actor> _actors)
        {
            Quaternion rotation = Quaternion.LookRotation(direciton,Vector3.up);
            position += direciton * Instance.m_Velocity * _deltaTime;
            transform.SetPositionAndRotation(position,rotation);

            Vector3 separation = Vector3.zero;
            Vector3 com = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            int localBehaviourCount = 0;
            foreach (var actor in _actors)
            {
                localBehaviourCount++;
                alignment += actor.direciton;

                Vector3 offset = position - actor.position;
                float distance = offset.magnitude;
                if (distance > Instance.m_SenseRadius.start)
                {
                    com += actor.position;
                    continue;
                }
                separation -= offset;
            }
            Vector3 final = Vector3.zero;
            if (localBehaviourCount > 0)
            {
                com /= localBehaviourCount;
                final += (com - position) * (_deltaTime * Instance.m_Cohesion);
            }
            final += separation * (_deltaTime * Instance.m_Separation);
            final += alignment * (_deltaTime * Instance.m_Alignment);

            direciton += final;
            direciton = direciton.normalized;
        }

        public void ApplyColor(Color _color)
        {
            m_Block.SetColor(KShaderProperties.kColor,_color);
            m_Renderer.SetPropertyBlock(m_Block);
        }

    }
}
