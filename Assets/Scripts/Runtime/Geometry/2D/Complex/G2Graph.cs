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
    public struct G2Graph : IGeometry2 , IGraphFinite<float2>, ISerializationCallbackReceiver
    {
        [NonSerialized] public float2 center;
        private List<float2> positions;
        private List<List<int>> connections;
        G2Graph Ctor()
        {
            center = positions.Average();
            return this;
        }
        public int Count => positions.Count;
        public float2 this[int _index] => positions[_index];

        IEnumerable<float2> IGraphFinite<float2>.Nodes => positions;
        public IEnumerable<float2> GetAdjacentNodes(float2 _src)
        {
            var index = positions.MinIndex(p => math.distancesq(p, _src));
            foreach (var connection in connections[index])
                yield return positions[connection];
        }
        
        public IEnumerator<float2> GetEnumerator() => positions.GetEnumerator(); 

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void DrawGizmos()
        {
            foreach (var node in positions)
                Gizmos.DrawSphere(node.to3xz(), 0.025f);

            foreach (var (index, nodeConnections) in connections.WithIndex())
            {
                var node = positions[index];           
                foreach (var connection in nodeConnections)
                    Gizmos.DrawLine(node.to3xz(),math.lerp(node,positions[connection],0.48f).to3xz());         
            }
        }

        public float2 Origin => center;
        public float2 GetSupportPoint(float2 _direction)
        {
            var center = this.center;
            return positions.MaxElement(_p => math.dot(_direction, _p - center));
        }

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();

        public static G2Graph FromTriangles(IList<float2> _vertices, IList<PTriangle> _triangles)
        {
            var graph = new G2Graph {
                positions = _vertices.ToList(),
                connections = new List<List<int>>(_vertices.Count).Resize(_vertices.Count,()=>new List<int>()),
            };

            foreach (var edge in _triangles.GetDistinctEdges())
            {
                var nodeStart = graph.connections[edge.start];
                var nodeEnd = graph.connections[edge.end];
                nodeStart.Add(edge.end);
                nodeEnd.Add(edge.start);
            }
            return graph.Ctor();
        }
    }
}