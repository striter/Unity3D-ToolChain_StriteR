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

        private List<GTrianglePolygon> polygons;
        private List<Vector3> vertices=new List<Vector3>();
        private List<Vector3> normals=new List<Vector3>();
        private List<Vector4> tangents=new List<Vector4>();
        private List<Vector2> uvs=new List<Vector2>();
        private List<int> indexes=new List<int>();
        private List<QEMVertex> qemVertices;

        public QEMConstructor(Mesh _srcMesh)
        {
            _srcMesh.GetVertices(vertices);
            _srcMesh.GetNormals(normals);
            _srcMesh.GetTangents(tangents);
            _srcMesh.GetUVs(0,uvs);
            polygons = _srcMesh.GetPolygons(out var indexesArray).ToList();
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
            for (int iterate = 0; iterate < _count; iterate++)
            {
                if (vertices.Count < 50)
                    break;
            
                Contract();
            }
            
            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetTangents(tangents);
            _mesh.SetUVs(0,uvs);
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
                    differentialMatrix.SetRow(3,Vector4.zero.SetW(1f));
                
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

                    if (!KQEM.kBounds.Contains(finalBestVertex))
                    {
                        finalError = float.MaxValue;
                        finalBestVertex = Vector3.zero;
                    }

                    finalError = Mathf.Max(finalError, 0f);
                }
                
                srcQemVertex.errors.Add(finalError);
                srcQemVertex.vBest.Add(finalBestVertex);
            }
        }
        
        void RemoveVertex(int _index)
        {
            vertices.RemoveAt(_index);
            tangents.RemoveAt(_index);
            normals.RemoveAt(_index);
            uvs.RemoveAt(_index);

            qemVertices.RemoveAt(_index);
            foreach (var qemVertex in qemVertices)
            {
                if (qemVertex.adjacentVertexIndexes.TryFindIndex(_p => _p == _index, out var dataIndex))
                {
                    qemVertex.adjacentVertexIndexes.RemoveAt(dataIndex);
                    qemVertex.errors.RemoveAt(dataIndex);
                    qemVertex.vBest.RemoveAt(dataIndex);
                }

                for (int i = 0; i < qemVertex.adjacentVertexIndexes.Count; i++)
                {
                    if (qemVertex.adjacentVertexIndexes[i] > _index)
                        qemVertex.adjacentVertexIndexes[i] -= 1;
                }
            }
            
            for (int i = 0; i < indexes.Count; i++)
            {
                if(indexes[i] > _index)
                    indexes[i] -= 1;
            }
        }
        
        void Contract()
        {
            int index0=default, index1=default;
            Vector3 finalPos=default;
            
            float minError = float.MaxValue;
            foreach ((var index,var qemVertex) in qemVertices.LoopIndex())
            {
                float min = qemVertex.errors.Min();
                if (min < minError)
                {
                    minError = min;

                    var errorIndex = qemVertex.errors.IndexOf(min);
                    index0 = index;
                    index1 = qemVertex.adjacentVertexIndexes[errorIndex];
                    finalPos = qemVertex.vBest[errorIndex];
                }
            }

            
            int newVertexId = vertices.Count;
            vertices.Add(finalPos);
            normals.Add(normals[index0]);
            uvs.Add(uvs[index0]);
            tangents.Add(tangents[index0]);
            
            QEMVertex newQemVertex = new QEMVertex();
            newQemVertex.errorMatrix = qemVertices[index0].errorMatrix.add(qemVertices[index1].errorMatrix);
            qemVertices[index0].adjacentVertexIndexes.Concat(qemVertices[index1].adjacentVertexIndexes).Distinct().FillList(newQemVertex.adjacentVertexIndexes);
            qemVertices.Add(newQemVertex);
            
            CalcError(newVertexId);
            if (index0 < index1)
                (index0, index1) = (index1, index0);
            
            RemoveVertex(index0);
            RemoveVertex(index1);
            
            newVertexId = vertices.Count - 1;
            for (int i = 0; i < newQemVertex.adjacentVertexIndexes.Count; i++)
            {
                var edgeVertices = qemVertices[newQemVertex.adjacentVertexIndexes[i]];
                edgeVertices.adjacentVertexIndexes.Add(newVertexId);
                edgeVertices.errors.Add(newQemVertex.errors[i]);
                edgeVertices.vBest.Add(newQemVertex.vBest[i]);
            }
            
            for (int i = polygons.Count - 1; i >= 2 ;i-=3)
            {
                var polygon = polygons[i];
                var matchCount = polygon.Count(p => p==index1 || p==index0);
                if(matchCount==0)
                    continue;
                
                if (matchCount == 1)
                {
                    indexes[i+polygon.FindIndex(p=>p==index1||p==index0)] = newVertexId;
                    return;
                }
                
                indexes.RemoveAt(i);
                indexes.RemoveAt(i+1);
                indexes.RemoveAt(i+2);
            }
        }
    }
}