using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.PathFinding;
using UnityEditor;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.PathFinding
{
    public interface INode
    {
        public bool m_Available { get; set; }
        public float3 m_Position { get; }
        public void DrawGizmos();
    }

    public interface IGraph :IGraphPathFinding<INode>
    {
        public float3 RandomPosition() => this.ToArray().RandomLoop().First(p => p.m_Available).m_Position;
    }

    public enum EGraph
    {
        Tile,
        Hexagon,
        Poisson,
        Sphere,
    }

    public enum EPathFind
    {
        AStar,
        Dijkstra,
    }
    
    [ExecuteInEditMode]
    public class PathFindingVisualize : MonoBehaviour
    {
        public EPathFind m_PathFind = EPathFind.AStar;
        public EGraph m_Graph;
        [Foldout(nameof(m_Graph),EGraph.Tile)] public TileGraph m_TileGraph = new TileGraph();
        [Foldout(nameof(m_Graph),EGraph.Hexagon)] public HexagonGraph m_HexagonGraph = new HexagonGraph();
        [Foldout(nameof(m_Graph),EGraph.Poisson)] public PoissonGraph m_PoissonGraph = new PoissonGraph();
        [Foldout(nameof(m_Graph),EGraph.Sphere)] public SphereGraph m_SphereGraph = new SphereGraph();
        public IGraph Graph => m_Graph switch
        {
            EGraph.Tile => m_TileGraph,
            EGraph.Hexagon => m_HexagonGraph,
            EGraph.Poisson => m_PoissonGraph,
            EGraph.Sphere => m_SphereGraph,
            _ => throw new System.NotImplementedException()
        };
        
        private Vector3 m_Agent,m_Destination;
        private Queue<float3> m_Paths = new Queue<float3>();

        private void OnValidate()
        {
            Randomize();
        }

        [InspectorButton]
        void Randomize()
        {
            foreach (var node in Graph)
                node.m_Available = URandom.Random01()>0.1f;
            
            m_Agent = Graph.RandomPosition();
            m_Destination = Graph.RandomPosition();
            PathFind();
        }

        
    #if UNITY_EDITOR
        private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void OnSceneGUI(SceneView _sceneView)
        {
            GRay ray = _sceneView.camera.ScreenPointToRay(UnityEditor.Extensions.UECommon.GetScreenPoint(_sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            ray.IntersectPoint(plane,out var hitPoint);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                    {
                        if(Graph.PositionToNode(hitPoint,out var node))
                            node.m_Available = !node.m_Available;
                        PathFind();
                    } break;
                    case 1:
                    {
                        m_Destination = hitPoint;
                        PathFind();
                    } break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R:Randomize();  break;
                }
            }
        }
    #endif

        private void OnDrawGizmos()
        {
            
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var node in Graph)
            {
                Gizmos.color = Color.white.SetA(.3f);
                var adjacent = Graph.GetAdjacentNodes(node);
                if (node.m_Available)
                {
                    foreach (var adjacentNode in adjacent)
                    {
                        Gizmos.color = Color.white.SetA(.3f);
                        Gizmos.DrawLine(node.m_Position,adjacentNode.m_Position);
                    }
                    
                }
                
                if (!node.m_Available)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.color = Color.red;
                    node.DrawGizmos();
                }
            }
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_Agent,.2f);
            UGizmos.DrawLines(m_Paths);
            foreach (var path in m_Paths)
                Gizmos.DrawWireSphere(path,.1f);
            
            if(Graph.PositionToNode(m_Agent,out var startNode))
                startNode.DrawGizmos();
            if(Graph.PositionToNode(m_Destination,out var endNode))
                endNode.DrawGizmos();
        }

        void PathFind()
        {
            if (Graph.PositionToNode(m_Agent,out var startNode) && Graph.PositionToNode(m_Destination,out var endNode))
            {
                Stack<INode> outputs = new Stack<INode>();
                switch (m_PathFind)
                {
                    case EPathFind.AStar:
                            Graph.AStar(startNode, endNode, outputs);
                        break;
                    case EPathFind.Dijkstra:
                            Graph.Dijkstra(startNode, endNode, outputs);
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
                m_Paths.Clear();
                m_Paths.EnqueueRange(outputs.Select(p=> p.m_Position));
            }
        }
    }
}