using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Polygon;
using Geometry.Voxel;
using UnityEngine;

namespace QuadricErrorsMetric
{
    public static class KQEM
    {
         public static Bounds kBounds = new(Vector3.zero, Vector3.one * .2f);
         public static float kSqrEdgeClosure = 0.01f*0.01f;
         
         public static Matrix4x4 GetQuadricErrorMatrix(GPlane _plane)
         {
             float a = _plane.normal.x;
             float b = _plane.normal.y;
             float c = _plane.normal.z;
             float d = _plane.distance;
             return new Matrix4x4()
             {
                 m00 = a*a,m10 = a*b,m20 = a*c,m30 = a*d,
                 m01 = b*a,m11 = b*b,m21 = b*c,m31 = b*d,
                 m02 = c*a,m12 = c*b,m22 = c*c,m32 = c*d,
                 m03 = d*a,m13 = d*b,m23 = d*c,m33 = d*d
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

        private List<int> indexes=new List<int>();
        private List<Vector3> normals=new List<Vector3>();
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
            _srcMesh.SetTriangles(indexes,0);
            
            var triangles = polygons.Select(p => new GTriangle(p.GetVertices(vertices))).ToArray();

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
                    polygons.Collect(_p => _p.Contains(srcIndex)).Select(_p => (IEnumerable<int>) _p).Resolve().
                        Concat(vertices.CollectIndex(_position=>Vector3.SqrMagnitude(vertex-_position)<KQEM.kSqrEdgeClosure)).      
                        Collect(_p => _p != srcIndex).
                        Distinct();
                
                qemVertex.adjacentVertexIndexes.AddRange(edgeIndexes);
                CalcError(i);
            }
        }

        public void DoContract(Mesh _mesh,ContractConfigure _data)
        {
            int desireCount = 0;
            switch (_data.mode)
            {
                case EContractMode.Percentage:
                    desireCount = (int) (vertices.Count * _data.percent / 100f);
                    break;
                case EContractMode.DecreaseAmount:
                    desireCount = vertices.Count - _data.count;
                    break;
                case EContractMode.VertexCount:
                    desireCount = _data.count;
                    break;
            }
            
            while( vertices.Count > desireCount)
            {
                FindContractVertex(out var index0,out var index1,out var finalPos);
                ContractVertex(index0,index1,finalPos);
            }
            
            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            // _mesh.SetTangents(tangents);
            // _mesh.SetUVs(0,uvs);
            _mesh.SetTriangles(indexes,0);
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
                if (Vector3.SqrMagnitude(srcVertex-dstVertex) < KQEM.kSqrEdgeClosure)
                {
                    finalError = -1f;
                    finalBestVertex = srcVertex;
                }
                else
                {
                    Matrix4x4 errorCombine = srcQemVertex.errorMatrix.add(dstQemVertex.errorMatrix);
                    Matrix4x4 differentialMatrix = errorCombine;
                    differentialMatrix.SetRow(0, new Vector4(errorCombine.m00, errorCombine.m01, errorCombine.m02, errorCombine.m03));
                    differentialMatrix.SetRow(1, new Vector4(errorCombine.m01, errorCombine.m11, errorCombine.m12, errorCombine.m13));
                    differentialMatrix.SetRow(2, new Vector4(errorCombine.m02, errorCombine.m12, errorCombine.m22, errorCombine.m23));
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
                        finalError = Vector4.Dot(finalBestVertex,errorCombine * finalBestVertex);
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
        
        void FindContractVertex(out int _index0,out int _index1,out Vector3 _finalPos)
        {
            _index0 = -1;
            _index1 = -1;
            _finalPos = Vector3.zero;
            float minError = float.MaxValue;
            for (int index = 0; index < qemVertices.Count; index++)
            {
                var qemVertex = qemVertices[index];
                if(qemVertex.errors.Count<=0)
                    continue;
                
                float min = qemVertex.errors.Min(p=>p,out var minIndex);
                if (minError > min)
                {
                    minError = min;
                    _index0 = index;
                    _index1 = qemVertex.adjacentVertexIndexes[minIndex];
                    _finalPos = qemVertex.vBest[minIndex];
                }
            }
        }

        void ContractVertex(int _index0, int _index1,Vector3 _finalPos)
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
            qemVertex0.adjacentVertexIndexes.Concat(qemVertex1.adjacentVertexIndexes).Distinct().Collect(p=>p!=_index0&&p!=_index1&&p!=qemVertexId).FillList(contractVertex.adjacentVertexIndexes);
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
            for (int i = 0; i<indexes.Count;i+=3)
            {
                var polygon = new GTrianglePolygon(indexes[i], indexes[i + 1], indexes[i + 2]);
            
                int matchCount = 0;
                int matchIndex=-1;
                for(int j=0;j<polygon.Length;j++)
                    if (polygon[j] == _index0 || polygon[j] == _index1)
                    {
                        matchCount++;
                        matchIndex = j;
                    }
                
                if (matchCount == 0)
                    continue;
                
                if (matchCount == 1)
                {
                    indexes[i+matchIndex] = concatVertex;
                    continue;
                }
                
                indexes.RemoveAt(i); indexes.RemoveAt(i); indexes.RemoveAt(i);        //....
                i -= 3;
            }
            
            foreach (var qemVertex in qemVertices)
            {
                for (int i = qemVertex.adjacentVertexIndexes.Count - 1; i >=0; i--)
                {
                    var index = qemVertex.adjacentVertexIndexes[i];
                    
                    if( index!=_index0 && index!=_index1 )
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