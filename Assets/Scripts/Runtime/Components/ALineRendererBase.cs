using System;
using System.Collections.Generic;
using System.Linq;
using TPoolStatic;
using UnityEngine;

namespace Runtime
{
    public abstract class ALineRendererBase : ARuntimeRendererBase
    {
        public float m_Width;
        public bool m_Billboard;
        private void OnValidate() => PopulateMesh();
        private void Update() => PopulateMesh();

        protected abstract void PopulatePositions(List<Vector3> _vertices, List<Vector3> _normals);

        protected sealed override void PopulateMesh(Mesh _mesh)
        {
            Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
            TSPoolList<Vector3>.Spawn(out var positions);
            TSPoolList<Vector3>.Spawn(out var normals);
            PopulatePositions(positions, normals);
            if (positions.Count > 0)
            { 
                TSPoolList<Vector3>.Spawn(out var vertices);
                TSPoolList<Vector4>.Spawn(out var uvs);
                TSPoolList<int>.Spawn(out var indexes);

                var totalLength = 0f;
                var count = positions.Count;
                var curIndex = 0;
                
                for (int i = 0; i < count; i++)
                {
                    var position = positions[i];
                    var normal = normals[i];

                    var vertexNormal = normal;
                    
                    if (m_Billboard)        //Better to keep these things here.
                    {
                        var currentCamera = Camera.current;
                        if (currentCamera)
                        {
                            var C = currentCamera.transform.position;
                            var Z = (C - position).normalized;
                            var T = Vector3.zero;
                            if (i == 0)
                                T = positions[1] - position;
                            else if (i == count - 1)
                                T = position - positions[count - 2];
                            else
                                T = positions[i + 1] - position;
                            T = T.normalized;
                            vertexNormal = Vector3.Cross(T,Z);
                        }
                    }
                    
                    vertices.Add( worldToLocal.MultiplyPoint(position - vertexNormal * m_Width));
                    uvs.Add(new Vector4(totalLength, 0));
                    vertices.Add( worldToLocal.MultiplyPoint(position + vertexNormal * m_Width));
                    uvs.Add(new Vector4(totalLength, 1));

                    if (i != count - 1)
                    {
                        totalLength += (positions[i]-positions[i+1]).magnitude;

                        indexes.Add(curIndex);
                        indexes.Add(curIndex + 1);
                        indexes.Add(curIndex + 2);

                        indexes.Add(curIndex + 2);
                        indexes.Add(curIndex + 1);
                        indexes.Add(curIndex + 3);
                        curIndex += 2;
                    }
                }
                

                var lastPoint = positions.Last();
                var lastUpDelta = normals.Last().normalized;
                totalLength += lastUpDelta.magnitude;
                var lastNormal = lastUpDelta.normalized;
                vertices.Add(worldToLocal.MultiplyPoint(lastPoint - lastNormal * m_Width));
                uvs.Add( new Vector4(totalLength, 0));
                vertices.Add(worldToLocal.MultiplyPoint(lastPoint + lastNormal * m_Width));
                uvs.Add(new Vector4(totalLength, 1));
                
                _mesh.SetVertices(vertices);
                _mesh.SetUVs(0,uvs);
                _mesh.SetTriangles(indexes,0,false);
                _mesh.RecalculateBounds();
                _mesh.RecalculateNormals();
                
                TSPoolList<Vector3>.Recycle(positions);
                TSPoolList<Vector3>.Recycle(vertices);
                TSPoolList<Vector4>.Recycle(uvs);
                TSPoolList<int>.Recycle(indexes);
            }
            TSPoolList<Vector3>.Recycle(positions);
            TSPoolList<Vector3>.Recycle(normals);
        }
    }
}
