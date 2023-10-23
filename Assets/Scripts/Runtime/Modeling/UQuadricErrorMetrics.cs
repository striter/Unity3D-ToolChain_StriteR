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
        public static float kSqrEdgeClosure = 0.01f * 0.01f;
        public static float4x4 GetQuadricErrorMatrix(GPlane _plane)
        {
            float a = _plane.normal.x;
            float b = _plane.normal.y;
            float c = _plane.normal.z;
            float d = _plane.distance;
            return new float4x4()
            {
                c0 = new float4(a * a, a * b, a * c, a * d),
                c1 = new float4(b * a, b * b, b * c, b * d),
                c2 = new float4(c * a, c * b, c * c, c * d),
                c3 = new float4(d * a, d * b, d * c, d * d),
            };
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
        public float4x4 errorMatrix = float4x4.zero;
        
        public List<QEMEdge> edges = new List<QEMEdge>();

        public void Update(IEnumerable<int> _newEdges,IList<QEMVertex> _vertices)
        {
            edges.Clear();
            var srcPosition = position.to4(1f);
            foreach (var edge in _newEdges)
            {
                var edgeVertexIndex = edge;
                var edgeVertex = _vertices[edgeVertexIndex];
                var edgePosition = edgeVertex.position.to4(1f);
                var midPosition = (srcPosition + edgePosition) / 2;
                float finalError;
                float4 finalBestVertex;
                if ((srcPosition - edgePosition).sqrmagnitude() < KQEM.kSqrEdgeClosure)
                {
                    finalError = -1f;
                    finalBestVertex = midPosition;
                    goto result;
                }

                var errorCombine = errorMatrix + edgeVertex.errorMatrix;
                Matrix4x4 differentialMatrix = errorCombine;
                differentialMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
                // if (differentialMatrix.determinant != 0)
                // {
                    // finalBestVertex = math.mul( differentialMatrix.inverse , Vector4.zero.SetW(1f));
                    // finalError = Vector4.Dot(finalBestVertex, math.mul(errorCombine , finalBestVertex));
                    // goto result;
                // }
                
                float srcError = math.dot(srcPosition, math.mul(errorCombine , srcPosition));
                float dstError = math.dot(edgePosition, math.mul(errorCombine , edgePosition));
                float midError = math.dot(midPosition, math.mul(errorCombine , midPosition));
                if (srcError <= dstError && srcError <= midError)
                {
                    finalBestVertex = srcPosition;
                    finalError = srcError;
                }
                else if (dstError <= srcError && dstError <= midError)
                {
                    finalBestVertex = edgePosition;
                    finalError = dstError;
                }
                else
                {
                    finalBestVertex = midPosition;
                    finalError = midError;
                }
                
                //     finalError = Mathf.Max(finalError, 0f);

                result:

                edges.Add(new QEMEdge()
                {
                    end = edge,
                    error = finalError,
                    vBest = finalBestVertex.to3xyz(),
                });
            }
        }
    }

    public class QEMConstructor
    {
        public List<QEMVertex> vertices { get; private set; }
        private List<int> indexes = new List<int>();
        public QEMConstructor(Mesh _srcMesh)
        {
            var srcVertices  = new List<Vector3>();
            _srcMesh.GetVertices(srcVertices);
            var polygons = _srcMesh.GetPolygons(out var indexesArray).ToList();
            indexesArray.FillList(indexes);

            var triangles = polygons.Select(p => (GTriangle)p.Convert(srcVertices)).ToArray();

            var vertexLength = srcVertices.Count;
            var triangleLength = polygons.Count;

            vertices = new List<QEMVertex>(vertexLength);
            for (var i = 0; i < vertexLength; i++)
                vertices.Add(new QEMVertex(){position = srcVertices[i]});

            //Initialize
            for (var i = 0; i < triangleLength; i++)
            {
                var polygon = polygons[i];
                var triangle = triangles[i];
                var errorMatrix = KQEM.GetQuadricErrorMatrix(triangle.GetPlane());

                for (var j = 0; j < 3; j++)
                    vertices[polygon[j]].errorMatrix += errorMatrix;
            }

            for (var i = 0; i < vertexLength; i++)
            {
                var qemVertex = vertices[i];
                var contractionIndexes =
                    polygons.Collect(_p => _p.Contains(i)).Select(_p => (IEnumerable<int>)_p).Resolve()
                        .Collect(_p => _p != i).Distinct();

                //Lets care about the non contraction later
                // var vertex = vertices[srcIndex];
                // var nonContractionIndexes = vertices.CollectIndex(_position =>
                    // Vector3.SqrMagnitude(vertex - _position) < KQEM.kSqrEdgeClosure);
                
                qemVertex.Update(contractionIndexes,vertices);
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
                case EContractMode.MinError: desireCount = 10;
                    break;
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
            int qemVertexId = vertices.Count;
            var index0 = _vertexIndex;
            var index1 = _edge.end;
            if (index0 < index1)
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
                .Collect(p => p != index0 && p != index1 && p != qemVertexId)
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
                    indexes[i + matchIndex] = concatVertex;
                    continue;
                }

                indexes.RemoveAt(i);
                indexes.RemoveAt(i);
                indexes.RemoveAt(i); //....
                i -= 3;
            }

            foreach (var qemVertex in vertices)
            {
                for (int i = qemVertex.edges.Count - 1; i >= 0; i--)
                {
                    var edge = qemVertex.edges[i].end;

                    if (edge != index0 && edge != index1)
                        continue;

                    qemVertex.edges.RemoveAt(i);
                }

                for (int i = 0; i < qemVertex.edges.Count; i++)
                {
                    int offset = 0;
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

            vertices.RemoveAt(index0);
            vertices.RemoveAt(index1);
        }
    }
}

//https://www.gamedev.net/forums/topic/656486-high-speed-quadric-mesh-simplification-without-problems-resolved/
namespace QuadricErrorsMetric2
{
    public class QEMConstructor
    {
        private List<FatVertex> vertices = new List<FatVertex>();
        private List<FatTriangle> triangles = new List<FatTriangle>();
        
        public QEMConstructor(Mesh _srcMesh)
        {
            var srcVertices  = new List<Vector3>();
            _srcMesh.GetVertices(srcVertices);
            var polygons = _srcMesh.GetPolygons(out var indexesArray).ToList();

            foreach (var vertex in srcVertices)
            {
                vertices.Add(new FatVertex()
                {
                    m_Checked = false,
                    m_Collapsed = false,
                    m_Position = vertex,
                    vertexNeighbors = new List<int>(),
                    triangleNeighbors = new List<int>(),
                });
            }

            for (int i = 0; i < polygons.Count; i++)
            {
                var polygon = polygons[i];

                for (int j = 0; j < 3; j++)
                {
                    var index = polygon[j];
                    vertices[index].vertexNeighbors.TryAddRange(polygon.Collect(p=>p!=index));
                    vertices[index].triangleNeighbors.Add(i);
                }

                triangles.Add(new FatTriangle(polygon,srcVertices));
            }
        }

        public void Collapse(float _maxCost)
        {
            UQuadricErrorMetrics.CollapseEdges(vertices, triangles, _maxCost);

        }
        
        public void PopulateMesh(Mesh _dstMesh)
        {
            _dstMesh.Clear();
            _dstMesh.SetVertices(vertices.Collect(p=>p.triangleNeighbors.Count>0).Select(p=>(Vector3)p.m_Position).ToArray());
            
        }
    }

    public static class UQuadricErrorMetrics
    {
        internal static bool CollapseEdges(IList<FatVertex> _vertices, IList<FatTriangle> _triangles, float _maxCost)
        {
            int numVertices = _vertices.Count;
            QEMVertex[] qemVertices = new QEMVertex[numVertices];
            List<EdgeCollapse> edgeCollapses = new List<EdgeCollapse>();
            for (int i = 0; i < numVertices; i++)
            {
                var vertex = _vertices[i];
                vertex.m_Checked = false;
                qemVertices[i] = new QEMVertex() { m_Q = ComputeQ(vertex, _triangles) };
            }

            for (int i = 0; i < numVertices; i++)
            {
                FatVertex vertex = _vertices[i];
                var qemVertex = qemVertices[i];
                var numNeighbors = vertex.vertexNeighbors.Count;
                if (vertex.m_Collapsed)
                    continue;

                for (int j = 0; j < numNeighbors; j++)
                {
                    var neighborIndex = vertex.vertexNeighbors[j];
                    var vertex2 = _vertices[neighborIndex];
                    if (vertex2.m_Checked || vertex2.m_Collapsed)
                        continue;
                    var qemVertex2 = qemVertices[neighborIndex];
                    var Q12 = qemVertex.m_Q + qemVertex2.m_Q;
                    var target = ComputeCollapseVertex(Q12, vertex.m_Position, vertex2.m_Position);
                    var cost = ComputeQError(Q12, target);
                    edgeCollapses.Add(new EdgeCollapse()
                    {
                        m_Cost = cost,
                        m_Index1 = i,
                        m_Index2 = neighborIndex,
                        m_Target = target,
                    });
                }

                vertex.m_Checked = true;
            }

            var edgeCount = edgeCollapses.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                var collapse = edgeCollapses[i];
                var qemV1 = qemVertices[collapse.m_Index1];
                var qemV2 = qemVertices[collapse.m_Index2];
                qemV1.m_Collapses.Add(collapse);
                qemV2.m_Collapses.Add(collapse);
                edgeCollapses.Add(collapse);
            }

            while (edgeCollapses.Count > 0)
            {
                edgeCollapses.Sort((a, b) => a.m_Cost > b.m_Cost ? 1 : -1);

                var collapse = edgeCollapses.MinElement(p => p.m_Cost, out var index);
                edgeCollapses.RemoveAt(index);
                if (collapse.m_Cost > _maxCost)
                    break;

                var fromIndex = collapse.m_Index1;
                var toIndex = collapse.m_Index2;
                if (fromIndex == toIndex)
                    continue;

                var fromVertex = _vertices[fromIndex];
                var toVertex = _vertices[toIndex];

                if (fromVertex.m_Collapsed || toVertex.m_Collapsed)
                    continue;

                if (VertexIsBorder(fromVertex, _triangles) || VertexIsBorder(toVertex, _triangles))
                    continue;
                
                if(WillInvertTriangle(fromVertex,fromIndex,toIndex,collapse.m_Target,_vertices,_triangles)
                   || WillInvertTriangle(toVertex,toIndex,fromIndex,collapse.m_Target,_vertices,_triangles))
                    continue;
                
                fromVertex.m_Collapsed = true;
                toVertex.m_Position = collapse.m_Target;

                for (int i = 0; i < fromVertex.triangleNeighbors.Count; i++)
                {
                    var triangleIndex = fromVertex.triangleNeighbors[i];
                    var triangle = _triangles[triangleIndex];

                    if (triangle.m_Indexes.Contains(toIndex))
                    {
                        for (int j = 0; j < 3; j++)
                            if (triangle.m_Indexes[j] != fromIndex)
                                _vertices[triangle.m_Indexes[j]].triangleNeighbors.Remove(triangleIndex);
                    }
                    else
                    {
                        triangle.ReplaceVertex(fromIndex, toIndex, _vertices);
                        toVertex.triangleNeighbors.Add(triangleIndex);
                    }
                }

                fromVertex.triangleNeighbors.Clear();
                toVertex.vertexNeighbors.Remove(fromIndex);

                for (int i = 0; i < fromVertex.vertexNeighbors.Count; i++)
                {
                    var neighborIndex = fromVertex.vertexNeighbors[i];
                    if (neighborIndex == toIndex)
                        continue;

                    if (!toVertex.vertexNeighbors.Contains(neighborIndex))
                    {
                        toVertex.vertexNeighbors.Add(neighborIndex);
                        var neighbor = _vertices[neighborIndex];
                        neighbor.vertexNeighbors.Remove(fromIndex);
                        neighbor.vertexNeighbors.Add(toIndex);
                    }
                }

                fromVertex.vertexNeighbors.Clear();
                var fromQEM = qemVertices[fromIndex];
                var toQEM = qemVertices[toIndex];
                toQEM.m_Q += fromQEM.m_Q;
                toQEM.m_Collapses.Remove(collapse);

                for (int i = 0; i < fromQEM.m_Collapses.Count; i++)
                {
                    var fromCollapse = fromQEM.m_Collapses[i];
                    if (fromCollapse.m_Index1 == fromIndex)
                        fromCollapse.m_Index1 = toIndex;
                    else if (fromCollapse.m_Index2 == fromIndex)
                        fromCollapse.m_Index2 = toIndex;

                    if (fromCollapse.m_Index1 == fromCollapse.m_Index2)
                        continue;
                    
                    if(!toQEM.m_Collapses.Contains(fromCollapse))
                        toQEM.m_Collapses.Add(fromCollapse);
                }
                
                fromQEM.m_Collapses.Clear();
                for (int i = 0; i < toQEM.m_Collapses.Count; i++)
                {
                    var toCollapse = toQEM.m_Collapses[i];
                    var index1 = toCollapse.m_Index1;
                    var index2 = toCollapse.m_Index2;
                    var q12 = qemVertices[index1].m_Q + qemVertices[index2].m_Q;
                    toCollapse.m_Target = ComputeCollapseVertex(q12,_vertices[index1].m_Position,_vertices[index2].m_Position);
                    toCollapse.m_Cost = ComputeQError(q12, toCollapse.m_Target);
                }
            }

            return true;
        }

        internal static float4x4 ComputeQ(FatVertex _vertex, IList<FatTriangle> _triangles)
        {
            var numTriangleNeighbors = _vertex.triangleNeighbors.Count;
            float4x4 Q = float4x4.identity;

            for (int i = 0; i < numTriangleNeighbors; i++)
            {
                var triangleIndex = _vertex.triangleNeighbors[i];
                var triangle = _triangles[triangleIndex];

                var p = (float4)triangle.m_Plane;
                var kp = new float4x4(p * p.x, p * p.y, p * p.z, p * p.w);
                Q += kp;
            }

            return Q;
        }

        internal static float ComputeQError(float4x4 _Q, float3 _v)
        {
            var v4 = _v.to4(1f);
            return math.abs(math.dot(v4, math.mul(_Q, v4)));
        }

        internal static Vector3 ComputeCollapseVertex(float4x4 _Q12, float3 _v1, float3 _v2)
        {
            var midPoint = (_v1 + _v2) / 2;
            var midPointCost = ComputeQError(_Q12, midPoint);
            var v1Cost = ComputeQError(_Q12, _v1);
            var v2Cost = ComputeQError(_Q12, _v2);

            // Return the vertex or midpoint with the least cost.
            if (v1Cost < v2Cost)
            {
                if (v1Cost < midPointCost)
                    return _v1;
                return midPoint;
            }

            if (v2Cost < midPointCost)
                return _v2;
            return midPoint;
        }

        internal static bool VertexIsBorder(FatVertex _vertex, IList<FatTriangle> _triangles)
        {
            var numVertexNeighbors = _vertex.vertexNeighbors.Count;
            var numTriangleNeighbors = _vertex.triangleNeighbors.Count;
            for (int i = 0; i < numVertexNeighbors; i++)
            {
                var neighborIndex = _vertex.vertexNeighbors[i];
                var numNeighborTriangles = 0;
                for (int j = 0; j < numTriangleNeighbors; j++)
                {
                    var triangle = _triangles[_vertex.triangleNeighbors[j]];
                    if (triangle.m_Indexes.Contains(neighborIndex))
                        numNeighborTriangles++;
                }

                if (numNeighborTriangles == 1)
                    return true;
            }

            return false;
        }

        internal static bool WillInvertTriangle(FatVertex _from,int _fromIndex,int _toIndex,float3 _target,IList<FatVertex> _vertices, IList<FatTriangle> _triangles)
        {
            var triangleCount = _from.triangleNeighbors.Count;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangleIndex = _from.triangleNeighbors[i];
                var triangle = _triangles[triangleIndex];
                if (!triangle.m_Indexes.Contains(_toIndex))
                {
                    var plane = GPlane.FromPositions(triangle.m_Indexes[0] == _fromIndex ? _target : _vertices[triangle.m_Indexes[0]].m_Position,
                        triangle.m_Indexes[1] == _fromIndex ? _target : _vertices[triangle.m_Indexes[1]].m_Position,
                        triangle.m_Indexes[2] == _fromIndex ? _target : _vertices[triangle.m_Indexes[2]].m_Position);
                    
                    if (math.dot(triangle.m_Plane.normal, plane.normal) < 0)
                        return true;
                }
                

            }

            return false;
        }
    }
}