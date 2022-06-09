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
         public static float kEdgeClosure = 0.05f;
         
         public static Matrix4x4 GetQuadricErrorMatrix(GPlane _plane)
         {
             float a = _plane.normal.x;
             float b = _plane.normal.y;
             float c = _plane.normal.z;
             float d = _plane.distance;
             Matrix4x4 q = new Matrix4x4();
             
             return new Matrix4x4()
             {
                 m00 = a*a,m01 = a*b,m02 = a*c,m03 = a*d,
                 m10 = b*a,m11 = b*b,m12 = b*c,m13 = b*d,
                 m20 = c*a,m21 = c*b,m22 = c*c,m23 = c*d,
                 m30 = d*a,m31 = d*b,m32 = d*c,m33 = d*d
             };
         }

         public static Matrix4x4 add(this Matrix4x4 _src, Matrix4x4 _dst)
         {
             Matrix4x4 dst = Matrix4x4.identity;
             for(int i=0;i<4;i++)
                 dst.SetRow(i,_src.GetRow(i)+_dst.GetRow(i));
             return dst;
         }
    }
    
    public class QEMConstructor
    {
        private class QEMVertex
        {
            public Matrix4x4 errorMatrix = Matrix4x4.zero;
            public List<int> adjacentVertexIndexes = new List<int>();
            public List<float> errors = new List<float>();
            public List<Vector3> vBest = new List<Vector3>();
        }

        private List<Vector3> vertices=new List<Vector3>();
        private List<int> indexes=new List<int>();
        private List<Vector3> normals=new List<Vector3>();
        // private List<Vector4> tangents=new List<Vector4>();
        // private List<Vector2> uvs=new List<Vector2>();
        private List<QEMVertex> qemVertices;

        public QEMConstructor(Mesh _srcMesh)
        {
            _srcMesh.GetVertices(vertices);
            _srcMesh.GetNormals(normals);
            // _srcMesh.GetTangents(tangents);
            // _srcMesh.GetUVs(0,uvs);
            var polygons = _srcMesh.GetPolygons(out var indexesArray).ToList();
            indexesArray.FillList(indexes);
            
            var triangles = polygons.Select(p=>new GTriangle(p.GetVertices(vertices))).ToArray();
            
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
                var errorMatrix = KQEM.GetQuadricErrorMatrix( triangle.GetPlane());
                
                for (int j = 0; j < 3; j++)
                {
                    var vertexIndex = indexes[polygon[j]];
                    var qemVertex = qemVertices[vertexIndex];
                    qemVertex.errorMatrix =qemVertex.errorMatrix.add(errorMatrix);
                }
            }

            for (int i = 0; i < vertexLength; i++)
            {
                int srcIndex = i;
                var qemVertex = qemVertices[srcIndex];
                var vertex = vertices[srcIndex];
                var edgeIndexes = 
                    polygons.Collect(_p => _p.Contains(srcIndex)).Select(_p => (IEnumerable<int>) _p).Resolve().
                        Concat(vertices.CollectIndex(_position=>Vector3.Distance(vertex,_position)<KQEM.kEdgeClosure)).      
                        Collect(_p => _p != srcIndex).
                        Distinct();
                
                qemVertex.adjacentVertexIndexes.AddRange(edgeIndexes);
                CalcError(i);
            }
        }

        public void DoContract(Mesh _mesh,int _count)
        {
            for (int iterate = 0; iterate < _count && vertices.Count > 5; iterate++)
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
        
        void CalcError(int _index)
        {
            var srcQemVertex = qemVertices[_index];
            srcQemVertex.errors.Clear();
            srcQemVertex.vBest.Clear();

            var srcEdge = _index;
            var srcVertex = vertices[srcEdge];
            
            int edgeCount = srcQemVertex.adjacentVertexIndexes.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                var dstEdge = srcQemVertex.adjacentVertexIndexes[i];
                var dstQemVertex = qemVertices[dstEdge];
                var dstVertex = vertices[dstEdge];
                float finalError;
                Vector3 finalBestVertex;
                if (Vector3.Distance(srcVertex,dstVertex) < 0.001f)
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
                
                    finalBestVertex = differentialMatrix.inverse * Vector4.zero.SetW(1f);
                    finalError = Vector4.Dot(finalBestVertex,errorCombine * finalBestVertex);
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
                
                float min = qemVertex.errors.Min();
                if (minError > min)
                {
                    minError = min;

                    var errorIndex = qemVertex.errors.IndexOf(min);
                    _index0 = index;
                    _index1 = qemVertex.adjacentVertexIndexes[errorIndex];
                    _finalPos = qemVertex.vBest[errorIndex];
                }
            }
        }

        void ContractVertex(int _index0, int _index1,Vector3 _finalPos)
        {
            if (_index0 < _index1)
                (_index0, _index1) = (_index1, _index0);
            
            // Debug.Log(_index0+" "+_index1);
            
            var finalNormal = normals[_index0];
            // var finalUV = uvs[_index0];
            // var finalTangent = tangents[_index0];
            var qemVertex0 = qemVertices[_index0];
            var qemVertex1 = qemVertices[_index1];

            vertices.Add(_finalPos);
            normals.Add(finalNormal);
            // uvs.Add(finalUV);
            // tangents.Add(finalTangent);
            
            int qemVertexId = vertices.Count - 1;
            QEMVertex contractVertex = new QEMVertex();
            contractVertex.errorMatrix = qemVertex0.errorMatrix.add(qemVertex1.errorMatrix);
            qemVertex0.adjacentVertexIndexes.Concat(qemVertex1.adjacentVertexIndexes).Distinct().Collect(p=>p!=_index0&&p!=_index1&&p!=qemVertexId).FillList(contractVertex.adjacentVertexIndexes);
            qemVertices.Add(contractVertex);
            CalcError(qemVertexId);
            for (int i = 0; i < contractVertex.adjacentVertexIndexes.Count; i++)
            {
                if (contractVertex.adjacentVertexIndexes.Count > 200)
                    break;
                
                var edgeVertices = qemVertices[contractVertex.adjacentVertexIndexes[i]];
                edgeVertices.adjacentVertexIndexes.Add(qemVertexId);
                edgeVertices.errors.Add(contractVertex.errors[i]);
                edgeVertices.vBest.Add(contractVertex.vBest[i]);
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
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < qemVertex.adjacentVertexIndexes.Count; j++)
                    {
                        if(j==_index0||j==_index1)
                            continue;
                        
                        
                        qemVertex.adjacentVertexIndexes.RemoveAt(j);
                        qemVertex.errors.RemoveAt(j);
                        qemVertex.vBest.RemoveAt(j);
                        break;
                    }
                }

                for (int i = 0; i < qemVertex.adjacentVertexIndexes.Count; i++)
                {
                    int offset = 0;
                    if (qemVertex.adjacentVertexIndexes[i] > _index0)
                        offset += 1;
                    if (qemVertex.adjacentVertexIndexes[i] > _index1)
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