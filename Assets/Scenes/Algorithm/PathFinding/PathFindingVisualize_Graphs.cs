using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Procedural.Hexagon;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Geometry.Extension.Sphere;
using Runtime.Random;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.PathFinding
{
    [Serializable]
    public class TileGraph : IGraph , ISerializationCallbackReceiver
    {
        [Range(8,32)]public int kGridWidth = 16, kGridHeight = 16;
        [Range(0,3f)] public float kCellSize = 2;
        public float3 kCube => Vector3.one * kCellSize;
        public float3 kCubeOffset => Vector3.up * kCellSize / 2;
        
        class Node : INode
        {
            private TileGraph m_Graph;
            public int2 identity { get; private set; }
            public float3 m_Position => new Vector3(m_Graph.kCellSize * identity.x, 0, m_Graph.kCellSize * identity.y) - new Vector3(m_Graph.kGridWidth,0,m_Graph.kGridHeight)*m_Graph.kCellSize/2f;

            public bool m_Available { get; set; }
            public Node(int2 _identity,TileGraph _graph)
            {
                identity = _identity;
                m_Graph = _graph;
            }
            public void DrawGizmos()
            {
                Gizmos.DrawWireCube(m_Position + m_Graph.kCubeOffset,m_Graph.kCube);
            }

        }
        private Dictionary<int2, Node> m_Nodes = new Dictionary<int2, Node>();

        void Ctor()
        {
            m_Nodes.Clear();
            for(int i = 0; i < kGridWidth; i++)
            for (int j = 0; j < kGridHeight; j++)
            {
                var id = new int2(i, j);
                m_Nodes.Add(id,new Node(id,this));
            }
        }

        

        public IEnumerable<INode> GetAdjacentNodes(INode _node)
        {
            var _src = (_node as Node).identity;
            if(m_Nodes.TryGetValue(_src+new int2(-1,0),out var adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
            if(m_Nodes.TryGetValue(_src+new int2(1,0),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
            if(m_Nodes.TryGetValue(_src+new int2(0,-1),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
            if(m_Nodes.TryGetValue(_src+new int2(0,1),out adjacentNode)&&adjacentNode.m_Available) yield return adjacentNode;
        }

        public int Count => m_Nodes.Count;
        public float Cost(INode _a, INode _b) => math.length(_a.m_Position - _b.m_Position);
        public int2 PositionToNode(float3 _srcPosition) =>(int2)(_srcPosition.xz / kCellSize);
        public float3 NodeToPosition(INode _node) => _node.m_Position;
        public float Heuristic(INode _a, INode _b) => 1f;
        public IEnumerator<INode> GetEnumerator() => m_Nodes.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool PositionToNode(float3 _position, out INode _node)
        {
            _node = m_Nodes.Values.MinElement(p => (p.m_Position - _position).sqrmagnitude());
            return true;
        }

        public bool NodeToPosition(INode _node, out float3 _position)
        {
            _position = _node.m_Position;
            return true;
        }

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();
    }


    [Serializable]
    public class HexagonGraph : IGraph , ISerializationCallbackReceiver
    {
        [Range(8,32)]public int radius = 8;
        public bool rounded;
        public bool flat;
        [Range(0.5f,3f)]public float kCellSize = 1f;
        
        private Dictionary<HexCoord, Node> m_Nodes = new Dictionary<HexCoord, Node>();
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => Ctor();
        void Ctor()
        {
            m_Nodes.Clear();
            var sqrRadius = umath.sqr(radius+1);
    
            for (var i = -radius; i <= radius; i++)
            for (var j = -radius; j <= radius; j++)
            {
                var index = new HexCoord(i, j);
                if (!index.InRange(radius))
                    continue;
                if (rounded && index.x * index.x + index.y * index.y + index.z * index.z >= sqrRadius)
                    continue;
    
                
                m_Nodes.Add(index, new Node(index, this));
            }
        }
        
        class Node : INode
        {
            private HexagonGraph m_Graph;
            public HexCoord m_Identity { get; set; }
            public bool m_Available { get; set; }

            public float3 m_Position => UHexagon.TransformHexToPosition(m_Identity,m_Graph.flat).to3xz() * m_Graph.kCellSize;
    
            public Node(HexCoord _identity,HexagonGraph _graph)
            {
                m_Identity = _identity;
                m_Graph = _graph;
            }

            public void DrawGizmos() => UHexagon.DrawHexagonGizmos(m_Position,m_Graph.kCellSize,m_Graph.kCellSize,m_Graph.flat);
        }
        
        public IEnumerable<INode> GetAdjacentNodes(INode _src)
        {
            foreach (var coord in (_src as Node).m_Identity.GetCoordsNearby())
            {
                if (m_Nodes.TryGetValue(coord, out var adjacentNode) && adjacentNode.m_Available)
                    yield return adjacentNode;
            }
        }

        public IEnumerator<INode> GetEnumerator() => m_Nodes.Values.GetEnumerator();
    
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
        public int Count { get; set; }
        public bool PositionToNode(float3 _position, out INode _node)
        {
            var hex = UHexagon.TransformPositionToHex(_position.xz / kCellSize,flat);
            var successful = m_Nodes.TryGetValue(hex,out var node);
            _node = node;
            return successful;
        }
    
        public bool NodeToPosition(INode _node, out float3 _position)
        {
            _position =  _node.m_Position;
            return true;
        }

        public float Heuristic(INode _src, INode _dst) => 1;
    
        public float Cost(INode _src, INode _dst)
        {
            return math.lengthsq(_src.m_Position - _dst.m_Position);
        }
    }

    [Serializable]
    public class PoissonGraph : IGraph , ISerializationCallbackReceiver
    {
        [Range(8,32)] public int resolution = 16;
        [Range(1,20f)] public float kRadius = 20;
        private readonly int seedHash = "PoissonDisk".GetHashCode();
        private Dictionary<int, Node> m_Nodes = new();
        public int Count => m_Nodes.Count;
        void Ctor()
        {
            var positions = PoolList<float2>.Empty(seedHash);
            var triangles = PoolList<PTriangle>.Empty(seedHash);
            var random = new LCGRandom(seedHash);
            ULowDiscrepancySequences.PoissonDisk2D(resolution,30,random).Select(p=>(p-.5f)*kRadius).FillList(positions);
            
            m_Nodes.Clear();
            foreach (var (index,position) in positions.LoopIndex())
                m_Nodes.Add(index,new Node(index,position.to3xz(),this));
            UTriangulation.BowyerWatson(positions,ref triangles);
            foreach (var triangle in triangles)
            {
                triangle.Traversal(p =>
                {
                    m_Nodes[p].m_Triangles.TryAddRange(triangle);
                });
            }
        }
        
        public void OnBeforeSerialize(){}

        public void OnAfterDeserialize() => Ctor();
        class Node : INode
        {
            private PoissonGraph m_Graph;
            public int m_Identity { get; set; }
            public bool m_Available { get; set; }
            public List<int> m_Triangles = new List<int>();
            public float3 m_Position { get; set; }
            public Node(int _identity,float3 _position,PoissonGraph _graph)
            {
                m_Identity = _identity;
                m_Position = _position;
                m_Graph = _graph;
            }
            public void DrawGizmos()
            {
                Gizmos.DrawSphere(m_Position,(m_Graph.kRadius * .1f) / m_Graph.resolution );
            }
        }
        public IEnumerable<INode> GetAdjacentNodes(INode _src)
        {
            var node = _src as Node;
            foreach (var triangle in node.m_Triangles)
            {
                var triangleNode = m_Nodes[triangle];
                if (triangleNode.m_Available)
                    yield return triangleNode;
            }
        }

        public IEnumerator<INode> GetEnumerator() => m_Nodes.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool PositionToNode(float3 _position, out INode _node)
        {
            _node = m_Nodes.Values.MinElement(p => (p.m_Position - _position).sqrmagnitude());
            return true;
        }

        public bool NodeToPosition(INode _node, out float3 _position)
        {
            _position = _node.m_Position;
            return true;
        }

        public float Heuristic(INode _src, INode _dst) => 1f;

        public float Cost(INode _src, INode _dst) => math.lengthsq(_src.m_Position - _dst.m_Position);

    }
    

    [Serializable]
    public class SphereGraph : IGraph, ISerializationCallbackReceiver
    {
        public ESphereMapping kMapping = ESphereMapping.ConcentricOctahedral;
        [Range(8,32)] public int kGridWidth = 16, kGridHeight = 16;
        [Range(1,20f)] public float kRadius = 20;
        private Dictionary<int2, Node> m_Nodes = new Dictionary<int2, Node>();
        protected float2 kResolutionF => new float2(kGridWidth, kGridHeight);
        protected int2 kResolutionI => new int2(kGridWidth, kGridHeight);
        void Ctor()
        {
            m_Nodes.Clear();
            for (var i = 0; i < kGridWidth; i++)
            for (var j = 0; j < kGridHeight; j++)
            {
                var identity = new int2(i, j);
                m_Nodes.Add(identity, new Node(identity, this));
            }
        }
        
        public void OnBeforeSerialize(){}

        public void OnAfterDeserialize() => Ctor();
        class Node : INode
        {
            private SphereGraph m_Graph;

            public int2 m_Identity { get; set; }
            public bool m_Available { get; set; }
            public float3 m_Position => m_Graph.kMapping.UVToSphere(m_Identity / m_Graph.kResolutionF) * m_Graph.kRadius;
            public Node(int2 _identity, SphereGraph _graph)
            {
                m_Identity = _identity;
                m_Graph = _graph;
            }
            public void DrawGizmos()
            {
                Gizmos.DrawSphere(m_Position,m_Graph.kRadius / m_Graph.kResolutionF.sum());
            }
        }

        Node GetNode(int2 _src, int2 _offset)
        {
            var identity = kMapping.Tilling(_src + _offset, kResolutionI);
            return m_Nodes.TryGetValue(identity, out var node) && node.m_Available ? node : null;
        }


        public IEnumerable<INode> GetAdjacentNodesWithNull(INode _src)
        {
            var node = _src as Node;
            var src = node.m_Identity;
            yield return GetNode(src,kint2.kDown);
            yield return GetNode(src,kint2.kUp);
            yield return GetNode(src,kint2.kLeft);
            yield return GetNode(src,kint2.kRight);
        }

        public IEnumerable<INode> GetAdjacentNodes(INode _src) => GetAdjacentNodesWithNull(_src).Where(n => n != null);
        public IEnumerator<INode> GetEnumerator() => m_Nodes.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => m_Nodes.Count;
        public bool PositionToNode(float3 _position, out INode _node)
        {
            _position = _position.normalize();
            var uv = kMapping.SphereToUV(_position);
            _node = m_Nodes.TryGetValue((int2)(uv * kResolutionF),out var node) ? node : null;
            return _node != null;
        }

        public bool NodeToPosition(INode _node, out float3 _position)
        {
            _position = _node.m_Position;
            return true;
        }

        public float Heuristic(INode _src, INode _dst) => 1f;

        public float Cost(INode _src, INode _dst) => umath.radBetween(_src.m_Position, _dst.m_Position);

    }
    
}