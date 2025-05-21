using System;
using System.Collections.Generic;
using TPool;
using System.Linq.Extensions;
using Procedural.Tile;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
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
        
        private TileGraph m_Graph;
        private ObjectPoolClass<int, Actor> m_Actors;
        private SpatialHashMap<Node, Actor> m_SpatialHashMap = new ();
        
        private void Awake()
        {
            Instance = this;
            m_Actors = new ObjectPoolClass<int, Actor>(transform.Find("Actor"));
            m_Graph = new TileGraph(m_SenseRadius.end);
            Spawn();
        }

        Node PositionToNode(Actor _actor)
        {
            m_Graph.PositionToNode(_actor.position, out var _node);
            return new Node(){index = _node,bounds = m_Graph.NodeToBoundingBox(_node)};
        }
        
        private void OnValidate()
        {
            if (m_Actors==null)
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
            m_SpatialHashMap.Clear();
            for (int i = 0; i < m_Amount; i++)
                m_Actors.Spawn().Init(URandom.Random2DSphere().ToVector3_XZ()*m_SenseRadius.end*10,URandom.Random2DSphere().ToVector3_XZ());
        }

        void Update()
        {
            var deltaTime = Time.deltaTime;
            var actors = m_Actors.FillList(PoolList<Actor>.Empty(nameof(SpatialHashGridExample)));
            m_SpatialHashMap.Construct(actors,PositionToNode);
            var querySphere = new GSphere(m_Actors.transform.position, m_SenseRadius.end);
            m_Actors.Traversal(p=>p.Tick(deltaTime,m_SpatialHashMap.Query(p=> p.bounds.Intersect(querySphere),actors)));
            
            foreach (var actor in m_Actors)
                actor.ApplyColor(Color.white);
            
            if (m_Actors.Count > 0)
            {
                var firstActorQuerySphere = new GSphere(m_Actors[0].position,m_SenseRadius.end);
                foreach (var actor in m_SpatialHashMap.Query(p=>p.bounds.Intersect(firstActorQuerySphere),actors))
                    actor.ApplyColor(Color.green);
                foreach (var actor in m_SpatialHashMap.Query(p=>firstActorQuerySphere.Intersect(p.bounds),p=>firstActorQuerySphere.Contains(p.position),actors))
                    actor.ApplyColor(Color.yellow);
                m_Actors[0].ApplyColor(Color.red);
            }
        }

        private void OnDrawGizmos()
        {
            if (m_Actors == null || m_Actors.Count == 0)
                return;
            
            Gizmos.DrawWireSphere(m_Actors[0].position,m_SenseRadius.end);

            m_Graph.PositionToNode(m_Actors[0].position,out var srcNode);
            Gizmos.color = Color.white.SetA(.2f);
            foreach (var node in m_SpatialHashMap)
                node.bounds.DrawGizmos();
        }
    }

    public struct Node : IEqualityComparer<Node> , IEquatable<Node>
    {
        public int2 index;
        public GBox bounds;
        public override int GetHashCode() => index.GetHashCode();

        public bool Equals(Node x, Node y) => x.index.Equals(y.index);

        public int GetHashCode(Node obj) => obj.GetHashCode();

        public bool Equals(Node other) => index.Equals(other.index) ;

        public override bool Equals(object obj) =>  obj is Node other && Equals(other);
    }
    
    public class Actor:ITransform
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
        
        public void Tick(float _deltaTime,IList<Actor> _actors)
        {
            var rotation = Quaternion.LookRotation(direciton,Vector3.up);
            position += direciton * Instance.m_Velocity * _deltaTime;
            transform.SetPositionAndRotation(position,rotation);

            var separation = Vector3.zero;
            var com = Vector3.zero;
            var alignment = Vector3.zero;
            var localBehaviourCount = 0;
            for(var i = _actors.Count - 1; i >=0 ;i--)
            {
                var actor = _actors[i];
                localBehaviourCount++;
                alignment += actor.direciton;

                var offset = position - actor.position;
                var distance = offset.magnitude;
                if (distance > Instance.m_SenseRadius.start)
                {
                    com += actor.position;
                    continue;
                }
                separation -= offset;
            }
            var final = Vector3.zero;
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
