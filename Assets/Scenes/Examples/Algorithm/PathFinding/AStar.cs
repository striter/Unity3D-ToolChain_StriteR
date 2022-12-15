using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Geometry;
using Procedural;
using UnityEditor;

namespace ExampleScenes.Algorithm.PathFinding
{
    using static DAStar;
    static class DAStar
    {
        public static int kGridWidth = 16, kGridHeight = 16;
        public static float kCellSize = 2;

        public static Vector3 GetCellPosition(int2 _identity) => new Vector3(kCellSize * _identity.x, 0, kCellSize * _identity.y) - new Vector3(kGridWidth,0,kGridHeight)*kCellSize/2f;
        
        public static readonly Vector3 kFlatCube = Vector3.one.SetY(0f) * kCellSize;
        public static readonly Vector3 kCube = Vector3.one * kCellSize;
        public static readonly Vector3 kCubeOffset = Vector3.up * kCellSize / 2;
    }
    
    [ExecuteInEditMode]
    public class AStar : MonoBehaviour
    {
        private Dictionary<int2, Node> m_Nodes = new Dictionary<int2, Node>();

        private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void OnValidate()
        {
            m_Nodes.Clear();
            for(int i = 0; i < kGridWidth; i++)
            for (int j = 0; j < kGridHeight; j++)
            {
                var id = new int2(i, j);
                m_Nodes.Add(id,new Node(id));
            }

            foreach (var node in m_Nodes.Values)
                node.UpdateAdjacency(m_Nodes);
            RecreateObstacles();
        }

        void RecreateObstacles()
        {
            foreach (var node in m_Nodes.Values)
                node.SetAvailable(true);
            float obstacleRange = .1f;
            for(int i = 0; i < kGridWidth; i++)
            for (int j = 0; j < kGridHeight; j++)
            {
                var id = new int2(i, j);
                if (URandom.Random01()>obstacleRange)
                    continue;
                var node =  m_Nodes[id];
                node.SetAvailable(false);
            }
        }
        
        private void OnSceneGUI(SceneView _sceneView)
        {
            GRay ray = _sceneView.camera.ScreenPointToRay(UnityEditor.Extensions.UECommon.GetScreenPoint(_sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPoint = ray.GetPoint(UGeometryIntersect.RayPlaneDistance(plane, ray));
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                    {
                        var node = Validate(hitPoint);
                        node?.SetAvailable(!node.m_Available);
                    }
                        break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R:RecreateObstacles();  break;
                }
            }
        }

        Node Validate(Vector3 _hitPoint)
        {
            foreach (var node in m_Nodes.Values)
            {
                if(!node.m_Bounds.Contains(_hitPoint))
                    continue;
                return node;
            }
            return null;
        }
        
        private void OnDrawGizmos()
        {
            foreach (var node in m_Nodes.Values)
            {
                if (!node.m_Available)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(node.m_Position + kCubeOffset,kCube);
                }
                else
                {
                    Gizmos.color = Color.white.SetAlpha(.1f);
                    Gizmos.DrawWireCube(node.m_Position,kFlatCube);
                }
            }
        }
    }

    class Node
    {
        public int2 m_Identity { get; private set; }
        public Vector3 m_Position { get; private set; }
        private List<Node> m_AdjacentNodes = new List<Node>();
        public bool m_Available { get; private set; }
        public Bounds m_Bounds { get; private set; }
        public Node(int2 _identity)
        {
            m_Identity = _identity;
            m_Position = GetCellPosition(_identity);
            m_Bounds = new Bounds(m_Position + kCubeOffset, kCube);
        }

        public void UpdateAdjacency(Dictionary<int2, Node> _nodes)
        {
            m_AdjacentNodes.Clear();
            Node adjacency = default;
            if(_nodes.TryGetValue(m_Identity-new int2(-1,0),out adjacency)) m_AdjacentNodes.Add(adjacency);
            if(_nodes.TryGetValue(m_Identity-new int2(1,0),out adjacency)) m_AdjacentNodes.Add(adjacency);
            if(_nodes.TryGetValue(m_Identity-new int2(0,-1),out adjacency)) m_AdjacentNodes.Add(adjacency);
            if(_nodes.TryGetValue(m_Identity-new int2(0,1),out adjacency)) m_AdjacentNodes.Add(adjacency);
        }

        public void SetAvailable(bool _available)
        {
            m_Available = _available;
        }
    }

    
}