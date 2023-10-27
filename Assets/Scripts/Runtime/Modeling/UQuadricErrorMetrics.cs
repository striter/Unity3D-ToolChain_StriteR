using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace QuadricErrorsMetric
{
    public static class KQEM
    {
        public static float kSqrEdgeClosure = float.Epsilon;
        public static float4x4_symmetric GetErrorMatrix(GPlane _plane)
        {
            float a = _plane.normal.x;
            float b = _plane.normal.y;
            float c = _plane.normal.z;
            float d = -_plane.distance;
            return new float4x4_symmetric(a * a, a * b, a * c, a * d,
                                                 b * b, b * c, b * d,
                                                        c * c, c * d,
                                                               d * d);
        }

        public static float CalculateError(float4x4_symmetric _matrix, float3 _position)
        {
            var x = _position.x;
            var y = _position.y;
            var z = _position.z;
            var q = _matrix;
            return   q.Index(0)*x*x + 2*q.Index(1)*x*y + 2*q.Index(2)*x*z + 2*q.Index(3)*x + q.Index(4)*y*y
                     + 2*q.Index(5)*y*z + 2*q.Index(6)*y + q.Index(7)*z*z + 2*q.Index(8)*z + q.Index(9);
        }
    }

    public class QEMEdge
    {
        public int end;
        public float error;
        public float3 vBest;
    }
    
    public class QEMVertex
    {
        public float3 position = 0f;
        public float4x4_symmetric errorMatrix = float4x4_symmetric.zero;
        
        public List<QEMEdge> edges = new List<QEMEdge>();
        
        public void Update(IEnumerable<int> _newEdges,IList<QEMVertex> _vertices)
        {
            edges.Clear();
            var srcPosition = position;
            foreach (var edge in _newEdges)
            {
                var edgeVertexIndex = edge;
                var edgeVertex = _vertices[edgeVertexIndex];
                var edgePosition = edgeVertex.position;
                var midPosition = (srcPosition + edgePosition) / 2;
                float finalError;
                float3 finalBestVertex;
                if ((srcPosition - edgePosition).sqrmagnitude() < KQEM.kSqrEdgeClosure)
                {
                    finalError = float.MinValue;
                    finalBestVertex = midPosition;
                    goto result;
                }
                
                var q = errorMatrix + edgeVertex.errorMatrix;
                var det = q.determinant(0, 1, 2, 1, 4, 5, 2, 5, 7);
                if ( det < float.Epsilon )
                {
                    // q_delta is invertible
                    finalBestVertex = new float3(-1f/det*q.determinant(1, 2, 3, 4, 5, 6, 5, 7 , 8),	// vx = A41/det(q_delta)
                                                     1f/det*q.determinant(0, 2, 3, 1, 5, 6, 2, 7 , 8),	// vy = A42/det(q_delta)
                                                    -1f/det*q.determinant(0, 1, 3, 1, 4, 6, 2, 5,  8));	// vz = A43/det(q_delta)
                    finalError = KQEM.CalculateError(q, finalBestVertex);
                    goto result;
                }

                var errors = new float[] {
                    KQEM.CalculateError(q, srcPosition),
                    KQEM.CalculateError(q, edgePosition),
                    KQEM.CalculateError(q, midPosition),
                };

                var positions = new float3[] {
                    srcPosition,
                    edgePosition,
                    midPosition,
                };
                finalError = errors.MinElement(p => p, out var index);
                finalBestVertex = positions[index];

                result:
                
                edges.Add(new QEMEdge()
                {
                    end = edge,
                    error = finalError,
                    vBest = finalBestVertex,
                });
            }
        }
    }

    public class QEMConstructor
    {
        public List<QEMVertex> vertices { get; private set; } = new List<QEMVertex>();
        public List<int> indexes { get; private set; } = new List<int>();

        public void Init(Mesh _srcMesh)
        {
            vertices.Clear();
            indexes.Clear();
            
            var srcVertices  = new List<Vector3>();
            _srcMesh.GetVertices(srcVertices);
            var polygons = _srcMesh.GetPolygons(out var indexesArray).ToList();
            indexesArray.FillList(indexes);

            var triangles = polygons.Select(p => (GTriangle)p.Convert(srcVertices)).ToArray();

            var vertexLength = srcVertices.Count;
            var triangleLength = polygons.Count;

            for (var i = 0; i < vertexLength; i++)
                vertices.Add(new QEMVertex(){position = srcVertices[i]});

            //Initialize
            for (var i = 0; i < triangleLength; i++)
            {
                var polygon = polygons[i];
                var triangle = triangles[i];
                var errorMatrix = KQEM.GetErrorMatrix(triangle.GetPlane());

                for (var j = 0; j < 3; j++)
                    vertices[polygon[j]].errorMatrix += errorMatrix;
            }

            for (var i = 0; i < vertexLength; i++)
            {
                var vertex = vertices[i];
                var contractionIndexes =
                    polygons.Collect(_p => _p.Contains(i)).Select(_p => (IEnumerable<int>)_p).Resolve();

                //Lets care about the non contraction later
                var nonContractionIndexes = vertices.CollectIndex(_p =>
                    (vertex.position - _p.position).sqrmagnitude() <= KQEM.kSqrEdgeClosure);

                var edges = contractionIndexes
                    .Concat(nonContractionIndexes)
                    .Collect(_p => _p != i).Distinct();
                
                vertex.Update(edges,vertices);
            }
        }

        public void Collapse(ContractConfigure _data)
        {
            int desireCount = 0;
            switch (_data.mode)
            {
                case EContractMode.Percentage: desireCount = (int)(vertices.Count * _data.percent / 100f); break;
                case EContractMode.DecreaseAmount: desireCount = vertices.Count - _data.count; break;
                case EContractMode.VertexCount: desireCount = _data.count; break;
                case EContractMode.MinError: desireCount = 10; break;
            }

            while (vertices.Count > desireCount)
            {
                var edge = GetMinEdge(out var vertexIndex);
                if (edge == null || _data.mode == EContractMode.MinError && edge.error > _data.minError)
                    break;
                Collapse(vertexIndex,edge);
            }
        }

        public void PopulateMesh(Mesh _mesh)
        {
            _mesh.Clear();
            _mesh.SetVertices(vertices.Select(p => (Vector3)p.position).ToList());
            _mesh.SetTriangles(indexes, 0);
        }
        
        QEMEdge GetMinEdge(out int _vertexIndex)
        {
            _vertexIndex = default;
            QEMEdge minEdge = default;
            float minError = float.MaxValue;
            for (int i = 0; i < vertices.Count; i++)
            {
                var qemVertex = vertices[i];
                if (qemVertex.edges.Count <= 0)
                    continue;

                var min = qemVertex.edges.MinElement(p => p.error);
                if (minError > min.error)
                {
                    _vertexIndex = i;
                    minError = min.error;
                    minEdge = min;
                }
            }

            return minEdge;
        }

        void Collapse(int _vertexIndex,QEMEdge _edge)
        {
            var index0 = _vertexIndex;
            var index1 = _edge.end;
            if (index0 > index1)
                (index0, index1) = (index1, index0);
            var finalPos = _edge.vBest;
            var qemVertex0 = vertices[index0];
            var qemVertex1 = vertices[index1];
            QEMVertex contractVertex = new QEMVertex
            {
                position = finalPos,
                errorMatrix = qemVertex0.errorMatrix+qemVertex1.errorMatrix
            };
            contractVertex.Update(qemVertex0.edges.Concat(qemVertex1.edges)
                    .Select(p=>p.end)
                    .Distinct()
                ,vertices);
            vertices.Add(contractVertex);

            
            int concatVertex = vertices.Count - 1;
            for (int i = 0; i < indexes.Count; i += 3)
            {
                var polygon = new PTriangle(indexes[i], indexes[i + 1], indexes[i + 2]);

                int matchCount = 0;
                int matchIndex = -1;
                for (int j = 0; j < polygon.Length; j++)
                    if (polygon[j] == index0 || polygon[j] == index1)
                    {
                        matchCount++;
                        matchIndex = j;
                    }

                if (matchCount == 0)
                    continue;

                if (matchCount == 1)
                {
                    //Change indexes
                    indexes[i + matchIndex] = concatVertex;
                    foreach (var index in polygon)
                        foreach (var edge in vertices[index].edges)
                            if (edge.end == index0 || edge.end == index1)
                                edge.end = concatVertex;
                    continue;
                }

                indexes.RemoveAt(i);
                indexes.RemoveAt(i);
                indexes.RemoveAt(i); //....
                i -= 3;
            }

            foreach (var qemVertex in vertices)
            {
                for (var i = qemVertex.edges.Count - 1; i >= 0; i--)
                {
                    var edge = qemVertex.edges[i].end;
                    if (edge == index0 || edge == index1)
                    {
                        qemVertex.edges.RemoveAt(i);
                        continue;
                    }
                    
                    var offset = 0;
                    var index = qemVertex.edges[i].end;
                    if (index > index0)
                        offset += 1;
                    if (index > index1)
                        offset += 1;

                    qemVertex.edges[i].end -= offset;
                }
            }

            for (int i = 0; i < indexes.Count; i++)
            {
                int offset = 0;
                if (indexes[i] > index0)
                    offset += 1;

                if (indexes[i] > index1)
                    offset += 1;
                indexes[i] -= offset;
            }
            
            vertices.RemoveAt(index1);
            vertices.RemoveAt(index0);
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.white.SetA(.3f);
            foreach (var vertex in vertices)
            {
                Gizmos.DrawWireSphere(vertex.position,.025f);
            }
        }
    }
}