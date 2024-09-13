using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.PathFinding
{

    
    public class TileGraph : IGraphPathFinding<TileGraph.Node> 
    {
        public static int kGridWidth = 16, kGridHeight = 16;
        public static float kCellSize = 2;
    
        public static readonly float3 kFlatCube = Vector3.one.SetY(0f) * kCellSize;
        public static readonly float3 kCube = Vector3.one * kCellSize;
        public static readonly float3 kCubeOffset = Vector3.up * kCellSize / 2;
        public class Node : INode
        {
            public int2 identity { get; private set; }
            public float3 m_Position =>     new Vector3(kCellSize * identity.x, 0, kCellSize * identity.y) - new Vector3(kGridWidth,0,kGridHeight)*kCellSize/2f;

            public int m_Identity { get; set; }
            public bool m_Available { get; set; }
            public Node(int2 _identity)
            {
                identity = _identity;
            }

            public void SetAvailable(bool _available)
            {
                m_Available = _available;
            }
            public void DrawGizmos()
            {
                Gizmos.DrawWireCube(m_Position + kCubeOffset,kCube);
            }

        }
        private Dictionary<int2, Node> m_Nodes = new Dictionary<int2, Node>();

        public TileGraph()
        {
            m_Nodes.Clear();
            for(int i = 0; i < kGridWidth; i++)
            for (int j = 0; j < kGridHeight; j++)
            {
                var id = new int2(i, j);
                m_Nodes.Add(id,new Node(id));
            }

        }

        public void Randomize()
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
        
        
        public void OnDrawGizmos()
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
                    Gizmos.color = Color.white.SetA(.1f);
                    Gizmos.DrawWireCube(node.m_Position,kFlatCube);
                }
            }
        }

        public IEnumerable<Node> GetAdjacentNodes(Node _node)
        {
            var _src = _node.identity;
            if(m_Nodes.TryGetValue(_src+new int2(-1,0),out var adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
            if(m_Nodes.TryGetValue(_src+new int2(1,0),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
            if(m_Nodes.TryGetValue(_src+new int2(0,-1),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
            if(m_Nodes.TryGetValue(_src+new int2(0,1),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
        }

        public int Count => m_Nodes.Count;
        public float Cost(Node _a, Node _b) => math.length(_a.m_Position - _b.m_Position);
        public int2 PositionToNode(float3 _srcPosition) =>(int2)(_srcPosition.xz / kCellSize);
        public float3 NodeToPosition(Node _node) => _node.m_Position;
        public float Heuristic(Node _a, Node _b) => math.length(_a.m_Position - _b.m_Position);
        public IEnumerator<Node> GetEnumerator() => m_Nodes.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public float3 RandomPosition() => m_Nodes.Values.ToArray().RandomLoop().First(p => p.m_Available).m_Position;

        public bool PositionToNode(float3 _position, out Node _node)
        {
            _node = m_Nodes.Values.MinElement(p => (p.m_Position - _position).sqrmagnitude());
            return true;
        }

        public bool NodeToPosition(Node _node, out float3 _position)
        {
            _position = _node.m_Position;
            return true;
        }
    }
    
}