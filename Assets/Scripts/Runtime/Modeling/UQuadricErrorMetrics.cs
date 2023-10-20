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
        public static Bounds kBounds = new Bounds(Vector3.zero, Vector3.one * .2f);
        public static float kSqrEdgeClosure = 0.01f * 0.01f;

        public static Matrix4x4 GetQuadricErrorMatrix(GPlane _plane)
        {
            float a = _plane.normal.x;
            float b = _plane.normal.y;
            float c = _plane.normal.z;
            float d = _plane.distance;
            return new Matrix4x4()
            {
                m00 = a * a, m10 = a * b, m20 = a * c, m30 = a * d,
                m01 = b * a, m11 = b * b, m21 = b * c, m31 = b * d,
                m02 = c * a, m12 = c * b, m22 = c * c, m32 = c * d,
                m03 = d * a, m13 = d * b, m23 = d * c, m33 = d * d
            };
        }
    }

    public class QEMVertex
    {
        public Matrix4x4 errorMatrix = Matrix4x4.zero;
        public List<int> adjacentVertexIndexes = new List<int>();
        public List<float> errors = new List<float>();
        public List<Vector3> vBest = new List<Vector3>();
    }

    public class QEMConstructor
    {

        private List<int> indexes = new List<int>();

        private List<Vector3> normals = new List<Vector3>();

        // private List<Vector4> tangents=new List<Vector4>();
        // private List<Vector2> uvs=new List<Vector2>();
        public List<QEMVertex> qemVertices { get; private set; }
        public List<Vector3> vertices { get; private set; } = new List<Vector3>();

        public QEMConstructor(Mesh _srcMesh)
        {
            _srcMesh.GetVertices(vertices);
            _srcMesh.GetNormals(normals);
            // _srcMesh.GetTangents(tangents);
            // _srcMesh.GetUVs(0,uvs);
            var polygons = _srcMesh.GetPolygons(out var indexesArray).ToList();
            indexesArray.FillList(indexes);


            _srcMesh.Clear();
            _srcMesh.SetVertices(vertices);
            _srcMesh.SetNormals(normals);
            // _mesh.SetTangents(tangents);
            // _mesh.SetUVs(0,uvs);
            _srcMesh.SetTriangles(indexes, 0);

            var triangles = polygons.Select(p => (GTriangle)p.Convert(vertices)).ToArray();

            var vertexLength = vertices.Count;
            var triangleLength = polygons.Count;

            qemVertices = new List<QEMVertex>(vertexLength);
            for (int i = 0; i < vertexLength; i++)
                qemVertices.Add(new QEMVertex());

            //Initialize
            for (int i = 0; i < triangleLength; i++)
            {
                var polygon = polygons[i];
                var triangle = triangles[i];
                var errorMatrix = KQEM.GetQuadricErrorMatrix(triangle.GetPlane());

                for (int j = 0; j < 3; j++)
                {
                    var vertexIndex = polygon[j];
                    var qemVertex = qemVertices[vertexIndex];
                    qemVertex.errorMatrix = qemVertex.errorMatrix.add(errorMatrix);
                }
            }

            for (int i = 0; i < vertexLength; i++)
            {
                int srcIndex = i;
                var qemVertex = qemVertices[srcIndex];
                var vertex = vertices[srcIndex];
                var edgeIndexes =
                    polygons.Collect(_p => _p.Contains(srcIndex)).Select(_p => (IEnumerable<int>)_p).Resolve()
                        .Concat(vertices.CollectIndex(_position =>
                            Vector3.SqrMagnitude(vertex - _position) < KQEM.kSqrEdgeClosure))
                        .Collect(_p => _p != srcIndex).Distinct();

                qemVertex.adjacentVertexIndexes.AddRange(edgeIndexes);
                CalcError(i);
            }
        }

        public void DoContract(Mesh _mesh, ContractConfigure _data)
        {
            int desireCount = 0;
            switch (_data.mode)
            {
                case EContractMode.Percentage:
                    desireCount = (int)(vertices.Count * _data.percent / 100f);
                    break;
                case EContractMode.DecreaseAmount:
                    desireCount = vertices.Count - _data.count;
                    break;
                case EContractMode.VertexCount:
                    desireCount = _data.count;
                    break;
            }

            while (vertices.Count > desireCount)
            {
                FindContractVertex(out var index0, out var index1, out var finalPos);
                ContractVertex(index0, index1, finalPos);
            }

            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            // _mesh.SetTangents(tangents);
            // _mesh.SetUVs(0,uvs);
            _mesh.SetTriangles(indexes, 0);
        }

        void CalcError(int _srcEdge)
        {
            var srcQemVertex = qemVertices[_srcEdge];
            srcQemVertex.errors.Clear();
            srcQemVertex.vBest.Clear();
            var srcVertex = vertices[_srcEdge];

            int edgeCount = srcQemVertex.adjacentVertexIndexes.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                var dstEdge = srcQemVertex.adjacentVertexIndexes[i];
                var dstQemVertex = qemVertices[dstEdge];
                var dstVertex = vertices[dstEdge];
                float finalError;
                Vector3 finalBestVertex;
                if (Vector3.SqrMagnitude(srcVertex - dstVertex) < KQEM.kSqrEdgeClosure)
                {
                    finalError = -1f;
                    finalBestVertex = srcVertex;
                }
                else
                {
                    Matrix4x4 errorCombine = srcQemVertex.errorMatrix.add(dstQemVertex.errorMatrix);
                    Matrix4x4 differentialMatrix = errorCombine;
                    differentialMatrix.SetRow(0,
                        new Vector4(errorCombine.m00, errorCombine.m01, errorCombine.m02, errorCombine.m03));
                    differentialMatrix.SetRow(1,
                        new Vector4(errorCombine.m01, errorCombine.m11, errorCombine.m12, errorCombine.m13));
                    differentialMatrix.SetRow(2,
                        new Vector4(errorCombine.m02, errorCombine.m12, errorCombine.m22, errorCombine.m23));
                    differentialMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
                    if (differentialMatrix.determinant == 0)
                    {
                        Vector3 midVertex = (srcVertex + dstVertex) / 2;
                        float srcError = Vector4.Dot(srcVertex, errorCombine * srcVertex);
                        float dstError = Vector4.Dot(dstVertex, errorCombine * dstVertex);
                        float midError = Vector4.Dot(midVertex, errorCombine * midVertex);
                        if (srcError <= dstError && srcError <= midError)
                        {
                            finalBestVertex = srcVertex;
                            finalError = srcError;
                        }
                        else if (dstError <= srcError && dstError <= midError)
                        {
                            finalBestVertex = dstVertex;
                            finalError = dstError;
                        }
                        else
                        {
                            finalBestVertex = midVertex;
                            finalError = midError;
                        }
                    }
                    else
                    {
                        finalBestVertex = differentialMatrix.inverse * Vector4.zero.SetW(1f);
                        finalError = Vector4.Dot(finalBestVertex, errorCombine * finalBestVertex);
                    }

                    // if (!KQEM.kBounds.Contains(finalBestVertex-srcVertex))
                    // {
                    //     finalError = float.MaxValue;
                    //     finalBestVertex = Vector3.zero;
                    // }
                    finalError = Mathf.Max(finalError, 0f);
                }

                srcQemVertex.errors.Add(finalError);
                srcQemVertex.vBest.Add(finalBestVertex);
            }
        }

        void FindContractVertex(out int _index0, out int _index1, out Vector3 _finalPos)
        {
            _index0 = -1;
            _index1 = -1;
            _finalPos = Vector3.zero;
            float minError = float.MaxValue;
            for (int index = 0; index < qemVertices.Count; index++)
            {
                var qemVertex = qemVertices[index];
                if (qemVertex.errors.Count <= 0)
                    continue;

                float min = qemVertex.errors.Min(p => p, out var minIndex);
                if (minError > min)
                {
                    minError = min;
                    _index0 = index;
                    _index1 = qemVertex.adjacentVertexIndexes[minIndex];
                    _finalPos = qemVertex.vBest[minIndex];
                }
            }
        }

        void ContractVertex(int _index0, int _index1, Vector3 _finalPos)
        {
            if (_index0 < _index1)
                (_index0, _index1) = (_index1, _index0);

            var finalNormal = normals[_index0];
            // var finalUV = uvs[_index0];
            // var finalTangent = tangents[_index0];
            // Debug.DrawRay(_finalPos,finalNormal,Color.red,1f);
            // Debug.DrawRay(vertices[_index0],finalNormal*.5f,Color.green,1f);
            // Debug.DrawRay(vertices[_index1],finalNormal*.5f,Color.blue,1f);
            vertices.Add(_finalPos);
            normals.Add(finalNormal);
            // uvs.Add(finalUV);
            // tangents.Add(finalTangent);

            int qemVertexId = qemVertices.Count;
            var qemVertex0 = qemVertices[_index0];
            var qemVertex1 = qemVertices[_index1];
            QEMVertex contractVertex = new QEMVertex();
            contractVertex.errorMatrix = qemVertex0.errorMatrix.add(qemVertex1.errorMatrix);
            qemVertex0.adjacentVertexIndexes.Concat(qemVertex1.adjacentVertexIndexes).Distinct()
                .Collect(p => p != _index0 && p != _index1 && p != qemVertexId)
                .FillList(contractVertex.adjacentVertexIndexes);
            qemVertices.Add(contractVertex);
            CalcError(qemVertexId);
            for (int i = 0; i < contractVertex.adjacentVertexIndexes.Count; i++)
            {
                var edgeVertexIndex = contractVertex.adjacentVertexIndexes[i];
                if (edgeVertexIndex == qemVertexId)
                    throw new Exception("??????");

                var edgeVertex = qemVertices[edgeVertexIndex];
                edgeVertex.adjacentVertexIndexes.Add(qemVertexId);
                edgeVertex.errors.Add(contractVertex.errors[i]);
                edgeVertex.vBest.Add(contractVertex.vBest[i]);
            }

            int concatVertex = vertices.Count - 1;
            for (int i = 0; i < indexes.Count; i += 3)
            {
                var polygon = new PTriangle(indexes[i], indexes[i + 1], indexes[i + 2]);

                int matchCount = 0;
                int matchIndex = -1;
                for (int j = 0; j < polygon.Length; j++)
                    if (polygon[j] == _index0 || polygon[j] == _index1)
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

            foreach (var qemVertex in qemVertices)
            {
                for (int i = qemVertex.adjacentVertexIndexes.Count - 1; i >= 0; i--)
                {
                    var index = qemVertex.adjacentVertexIndexes[i];

                    if (index != _index0 && index != _index1)
                        continue;

                    qemVertex.adjacentVertexIndexes.RemoveAt(i);
                    qemVertex.errors.RemoveAt(i);
                    qemVertex.vBest.RemoveAt(i);
                }

                for (int i = 0; i < qemVertex.adjacentVertexIndexes.Count; i++)
                {
                    int offset = 0;
                    var index = qemVertex.adjacentVertexIndexes[i];
                    if (index > _index0)
                        offset += 1;
                    if (index > _index1)
                        offset += 1;

                    qemVertex.adjacentVertexIndexes[i] -= offset;
                }
            }

            for (int i = 0; i < indexes.Count; i++)
            {
                int offset = 0;
                if (indexes[i] > _index0)
                    offset += 1;

                if (indexes[i] > _index1)
                    offset += 1;
                indexes[i] -= offset;
            }

            vertices.RemoveAt(_index0);
            normals.RemoveAt(_index0);
            qemVertices.RemoveAt(_index0);
            // tangents.RemoveAt(_index0);
            // uvs.RemoveAt(_index0);

            vertices.RemoveAt(_index1);
            qemVertices.RemoveAt(_index1);
            normals.RemoveAt(_index1);
            // uvs.RemoveAt(_index1);
            // tangents.RemoveAt(_index1);

        }
    }

    

}

//https://www.gamedev.net/forums/topic/656486-high-speed-quadric-mesh-simplification-without-problems-resolved/
namespace QuadricErrorsMetric2
{
    public class QEMMEshConstructor
    {

        public void Initialize(float3[] _vertices, int[] _triangles)
        {



        }


    }

    public static class UQuadricErrorMetrics
    {
        internal static bool CollapseEdges(FatVertex[] _vertices, FatTriangle[] _triangles, float _maxCost)
        {
            int numVertices = _vertices.Length;
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
            List<EdgeCollapse> edgeCollapseQueue = new List<EdgeCollapse>(edgeCount);
            for (int i = 0; i < edgeCount; i++)
            {
                var collapse = edgeCollapses[i];
                var qemV1 = qemVertices[collapse.m_Index1];
                var qemV2 = qemVertices[collapse.m_Index2];
                qemV1.m_Collapses.Add(collapse);
                qemV2.m_Collapses.Add(collapse);
                edgeCollapseQueue.Add(collapse);
            }

            edgeCollapseQueue.Sort((a, b) => a.m_Cost > b.m_Cost ? 1 : -1);

            while (edgeCollapseQueue.Count > 0)
            {
                var collapse = edgeCollapseQueue[0];
                edgeCollapseQueue.RemoveAt(0);
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
                //
                // if(WillInvertTriangle(fromVertex,fromIndex,toIndex,collapse.m_Target,_vertices,_triangles)
                //    || WillInvertTriangle(toVertex,toIndex,fromIndex,collapse.m_Target,_vertices,_triangles))
                //     continue;
                //
                fromVertex.m_Collapsed = true;
                toVertex.m_Position = collapse.m_Target;

                for (int i = 0; i < fromVertex.triangleNeighbors.Count; i++)
                {
                    var triangleIndex = fromVertex.triangleNeighbors[i];
                    var triangle = _triangles[triangleIndex];

                    if (triangle.m_Indexes.Contains(toIndex))
                    {
                        triangle.m_Collapsed = true;
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
                    if (collapse.m_Index1 == fromIndex)
                        collapse.m_Index1 = toIndex;
                    else if (collapse.m_Index2 == fromIndex)
                        collapse.m_Index2 = toIndex;

                    if (collapse.m_Index1 == collapse.m_Index2)
                        continue;



                }
            }

            return true;
        }

        internal static float4x4 ComputeQ(FatVertex _vertex, FatTriangle[] _triangles)
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

        internal static bool VertexIsBorder(FatVertex _vertex, FatTriangle[] _triangles)
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

        // internal static bool WillInvertTriangle(FatVertex _vertex,int)
        // {
        //     
        // }
    }
}