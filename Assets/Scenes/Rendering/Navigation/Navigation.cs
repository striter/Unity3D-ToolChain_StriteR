using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.Mathematics;
using UnityEngine;
using Runtime.TouchTracker;
using UnityEngine.AI;
using UnityEngine.VFX;

namespace Examples.Rendering.Navigation
{
    [Serializable]
    public class Constants : ISerializationCallbackReceiver
    {
        public float kForcePerSecond = 5f;
        
        public static Constants Instance;
        
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            Instance = this;
        }
    }
    
    public class Navigation : MonoBehaviour
    {
        public Constants m_Constants;
        private Navigation m_Navigator;
        private Agent m_Character;
        private Visualizer m_Visualizer;
        private Camera m_Camera;

        private void Awake()
        {
            m_Character = new Agent(transform.Find("Character"), new Navigator(transform.Find("Navigator")));
            m_Visualizer = new Visualizer(transform.Find("Visualizer"));
            m_Camera = transform.GetComponentInChildren<Camera>();
        }

        private void OnDestroy()
        {
            m_Visualizer.Destroy();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            m_Character.Tick(deltaTime);
            m_Visualizer.Update(m_Character.m_WayPoints);
            
            var touches =  UTouchTracker.Execute(Time.deltaTime);
            var clicks = touches.ResolveClicks();
            if (clicks.Any() && Physics.Raycast(m_Camera.ScreenPointToRay(clicks.First()),out var hitInfo))
                m_Character.Navigate(hitInfo.point);
        }

        private void FixedUpdate()
        {
            m_Character.FixedTick(Time.fixedDeltaTime);
        }

        private void OnDrawGizmos()
        {
            m_Character?.DrawGizmos();
        }
    }

    public class Visualizer
    {
        private Runtime.LineSegmentRenderer m_LineRenderer;
        private VisualEffect m_Effect;
        private GraphicsBuffer m_Buffer;
        private const int kMaxVertices = 512;
        private float3[] kVerticeList = new float3[kMaxVertices];
        public Visualizer(Transform _root)
        {
            m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, kMaxVertices, sizeof(float) * 3);
            m_LineRenderer = _root.GetComponentInChildren<Runtime.LineSegmentRenderer>();
            m_Effect = _root.GetComponentInChildren<VisualEffect>();
        }

        public void Destroy()
        {
            m_Buffer.Dispose();
        }
        
        public void Update(IEnumerable<WayPoint> _wayPoints)
        {
            var positions = _wayPoints.Select(p => p.position).ToArray();
            m_LineRenderer.SetPositions(positions, _wayPoints.Select(p=>p.normal).ToArray());

            var bufferVertexCount = math.min(positions.Length, kMaxVertices);
            for (int i = 0; i < bufferVertexCount; i++)
                kVerticeList[i] = positions[i];
            
            m_Buffer.SetData(kVerticeList);
            m_Effect.SetInt("VertexCount", bufferVertexCount);
            m_Effect.SetGraphicsBuffer("Buffer",m_Buffer);
        }
    }

    public struct WayPoint
    {
        public float3 position;
        public float3 normal;
    }
    
    public class Navigator
    {
        private Transform transform;
        private NavMeshAgent m_Agent;
        public Navigator(Transform _root)
        {
            transform = _root;
            m_Agent = _root.GetComponentInChildren<NavMeshAgent>();
            m_Agent.SetDestination(_root.transform.position);
        }

        public void Navigate(float3 _position) => m_Agent.SetDestination(_position);
        public float3 GetDestination() => transform.position;

        public void DrawGizmos()
        {
            Gizmos.DrawSphere(m_Agent.destination,.5f);
        }
    }
    public class Agent
    {
        public Navigator m_Navigator;
        public List<WayPoint> m_WayPoints { get; private set; } = new List<WayPoint>();
        private Rigidbody m_Character;
        
        public Agent(Transform _root,Navigator _navigator)
        {
            m_Character = _root.GetComponent<Rigidbody>();
            m_Navigator = _navigator;
        }
        
        public void Navigate(float3 _position)
        {
            m_Navigator.Navigate(_position);
        }

        public void Tick(float _deltaTime)
        {
            var path = new NavMeshPath();
            if( NavMesh.CalculatePath(m_Character.position,m_Navigator.GetDestination(),int.MaxValue,path))
            {
                m_WayPoints.Clear();
                foreach (var corner in path.corners)
                {
                    if (NavMesh.SamplePosition(corner, out var wayPointEdge, float.MaxValue, int.MaxValue)) 
                    {
                         var normal = Vector3.up;
                         if (Physics.Raycast(new Ray(wayPointEdge.position, Vector3.down), out var hitInfo,
                                 float.MaxValue, int.MaxValue))
                             normal = hitInfo.normal;
                         m_WayPoints.Add(new WayPoint(){position = wayPointEdge.position,normal = normal});
                    }
                }
            }
        }

        public void FixedTick(float _deltaTime)
        {
            if (m_WayPoints.Count < 2) 
                return;

            var nextWayPoint = m_WayPoints[1];
            var curPosition = (float3)m_Character.transform.position;
            var direction = (nextWayPoint.position - curPosition).normalize();

            var originalVelocity = m_Character.velocity;

            var reflectVector = math.reflect(-originalVelocity, direction);

            direction = (reflectVector + direction).normalize();
            m_Character.AddForce(direction*Constants.Instance.kForcePerSecond*_deltaTime);
        }

        public void DrawGizmos()
        {
            m_Navigator.DrawGizmos();
            Gizmos.matrix = Matrix4x4.Translate(Vector3.up);
            Gizmos.color = Color.green;
            var index = 0;
            foreach (var waypoint in m_WayPoints)
            {
                Gizmos.color = UColor.IndexToColor(index++);
                Gizmos.DrawWireSphere(waypoint.position,.2f);
            }   
        }
    }

}