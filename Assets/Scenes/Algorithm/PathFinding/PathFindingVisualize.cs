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
        public int m_Identity { get; }
        public bool m_Available { get; set; }
        public float3 m_Position { get; }
        public void DrawGizmos();
    }
    
    [ExecuteInEditMode]
    public class PathFindingVisualize : MonoBehaviour
    {
        public TileGraph m_TileGraph = new TileGraph();
        
        private Vector3 m_Agent,m_Destination;
        private Queue<float3> m_Paths = new Queue<float3>();

        private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void OnValidate()
        {
            Randomize();
        }

        void Randomize()
        {
            foreach (var node in m_TileGraph)
                node.SetAvailable(URandom.Random01()>0.1f);
            
            m_Agent = m_TileGraph.RandomPosition();
            m_Destination = m_TileGraph.RandomPosition();
            PathFind();
        }
        
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
                        if(m_TileGraph.PositionToNode(hitPoint,out var node))
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

        private void OnDrawGizmos()
        {
            
            m_TileGraph.OnDrawGizmos();
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_Agent,.2f);
            UGizmos.DrawLines(m_Paths);
            foreach (var path in m_Paths)
                Gizmos.DrawWireSphere(path,.1f);
            
            if(m_TileGraph.PositionToNode(m_Agent,out var startNode))
                startNode.DrawGizmos();
            if(m_TileGraph.PositionToNode(m_Destination,out var endNode))
                endNode.DrawGizmos();
        }

        void PathFind()
        {
            if (m_TileGraph.PositionToNode(m_Agent,out var startNode) && m_TileGraph.PositionToNode(m_Destination,out var endNode))
            {
                Stack<TileGraph.Node> outputs = new Stack<TileGraph.Node>();
                m_TileGraph.AStar(startNode,endNode, outputs);
                m_Paths.Clear();
                m_Paths.EnqueueRange(outputs.Select(p=> p.m_Position));
            }
        }
        
    }

    
}