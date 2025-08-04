using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public struct G2Graph : IGeometry2 , IGraphFinite<G2Graph.Node> , ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Node
        {
            public float2 position;
            public List<int> connections;
        }

        [NonSerialized] public float2 origin;
        public List<Node> nodes;
        G2Graph Ctor()
        {
            origin = nodes.Average(p=>p.position);
            return this;
        }
        public int Count => nodes.Count;

        IEnumerable<Node> IGraphFinite<Node>.Nodes => nodes;
        public IEnumerable<Node> GetAdjacentNodes(Node _src)
        {
            foreach (var connection in _src.connections)
                yield return nodes[connection];
        }
        
        public IEnumerable<float2> GetAdjacentPoints(int _index) => GetAdjacentNodes(nodes[_index]).Select(p=>p.position);
        public IEnumerator<Node> GetEnumerator() => nodes.GetEnumerator(); 

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void DrawGizmos()
        {
            foreach (var node in nodes)
                Gizmos.DrawSphere(node.position.to3xz(), 0.025f);

            foreach (var node in nodes)
                foreach (var connection in node.connections)
                    Gizmos.DrawLine(node.position.to3xz(),math.lerp(node.position,nodes[connection].position,0.48f).to3xz());
        }

        public float2 Origin => Count > 0 ? nodes[0].position : float2.zero;
        public float2 GetSupportPoint(float2 _direction)
        {
            var center = origin;
            return nodes.MaxElement(_p => math.dot(_direction, _p.position - center)).position;
        }

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();

        public static G2Graph FromTriangles(IList<float2> _vertices, IList<PTriangle> _triangles)
        {
            var graph = new G2Graph {
                nodes = new List<Node>(_triangles.Count)
            };
            
            foreach (var vertex in _vertices)
                graph.nodes.Add(new () { position = vertex ,connections = new () });
            foreach (var edge in _triangles.Select(p=>p.Distinct()).SelectMany(p=>p.GetEdges()))
            {
                var nodeStart = graph.nodes[edge.start];
                var nodeEnd = graph.nodes[edge.end];
                nodeStart.connections.Add(edge.end);
                nodeEnd.connections.Add(edge.start);
            }
            return graph.Ctor();
        }
    }
}