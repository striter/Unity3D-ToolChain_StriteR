using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Validation;
using UnityEditor;

namespace Examples.Algorithm.PathFinding
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
    public class AStar : MonoBehaviour,IGraph<int2>,IGraphPathFinding<int2>
    {
        private Dictionary<int2, Node> m_Nodes = new Dictionary<int2, Node>();

        private Vector3 m_Agent,m_Destination;
        private Queue<Vector3> m_Paths = new Queue<Vector3>();

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

            Randomize();
        }

        void Randomize()
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
            
            m_Agent = m_Nodes.Values.ToArray().RandomLoop().First(p => p.m_Available).m_Position;
            m_Destination = m_Nodes.Values.ToArray().RandomLoop().First(p => p.m_Available).m_Position;
            PathFind();
        }
        
        private void OnSceneGUI(SceneView _sceneView)
        {
            GRay ray = _sceneView.camera.ScreenPointToRay(UnityEditor.Extensions.UECommon.GetScreenPoint(_sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            UGeometry.Intersect.Eval(ray,plane,out var hitPoint);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                    {
                        var node = Validate(hitPoint);
                        node?.SetAvailable(!node.m_Available);
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

        void PathFind()
        {
            var src = Validate(m_Agent);
            var node = Validate(m_Destination);
            if (node!=null)
            {
                Stack<int2> outputs = new Stack<int2>();
                UAStar<int2>.PathFind(this,src,node,ref outputs);
                m_Paths.Clear();
                m_Paths.EnqueueRange(outputs.Select(p=>m_Nodes[p].m_Position));
                            
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_Agent,.2f);
            UGizmos.DrawLines(m_Paths);
            foreach (var path in m_Paths)
            {
                Gizmos.DrawWireSphere(path,.1f);
            }
            
            foreach (var node in m_Nodes.Values)
            {
                if (!node.m_Available)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(node.m_Position + kCubeOffset,kCube);
                }
                else
                {
                    Gizmos.color = Color.white.SetA(.1f);
                    Gizmos.DrawWireCube(node.m_Position,kFlatCube);
                }
            }
        }

        public IEnumerable<int2> GetAdjacentNodes(int2 _src)
        {
            if(m_Nodes.TryGetValue(_src+new int2(-1,0),out var adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode.identity;
            if(m_Nodes.TryGetValue(_src+new int2(1,0),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode.identity;
            if(m_Nodes.TryGetValue(_src+new int2(0,-1),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode.identity;
            if(m_Nodes.TryGetValue(_src+new int2(0,1),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode.identity;
        }

        public float Cost(int2 _a, int2 _b) => math.length(_a - _b);
        public float Heuristic(int2 _a, int2 _b) => math.length(_a - _b);

    }

    public class Node:INode<int2>
    {
        public int2 identity { get; private set; }
        public Vector3 m_Position { get; private set; }
        public bool m_Available { get; private set; }
        public Bounds m_Bounds { get; private set; }
        public Node(int2 _identity)
        {
            identity = _identity;
            
            m_Position = GetCellPosition(_identity);
            m_Bounds = new Bounds(m_Position + kCubeOffset, kCube);
        }

        public void SetAvailable(bool _available)
        {
            m_Available = _available;
        }
    }
    
}