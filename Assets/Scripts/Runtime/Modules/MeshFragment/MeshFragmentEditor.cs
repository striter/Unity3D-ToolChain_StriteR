using System;
using System.Collections.Generic;
using System.Linq;
using TPoolStatic;
using UnityEngine;

#if UNITY_EDITOR
namespace MeshFragment
{
    using UnityEditor;
    public static class UMeshFragmentEditor
    {
        static void AppendFragment(MeshFragmentCollector _collector,Matrix4x4 localToWorldMatrix,Matrix4x4 worldToLocalMatrix,Mesh mesh,int _subMeshIndex,Func<Vector3,Vector3> _objectToOrientedVertex)
        {
            var subMesh = mesh.GetSubMesh(_subMeshIndex);
            var vertexBegin = _collector.vertices.Count;
            
            var vertexOffset = subMesh.firstVertex;
            var vertexCount = subMesh.vertexCount;
            var indexOffset = subMesh.firstVertex;

            TSPoolList<Vector3>.Spawn(out var curVertices);
            TSPoolList<int>.Spawn(out var curIndexes);
            TSPoolList<Vector3>.Spawn(out var curNormals);
            TSPoolList<Vector4>.Spawn(out  var curTangents);
            TSPoolList<Color>.Spawn(out var curColors);
            TSPoolList<Vector2>.Spawn(out var curUVs);

            mesh.GetVertices(curVertices);
            mesh.GetNormals(curNormals);
            mesh.GetTangents(curTangents);
            mesh.GetUVs(0,curUVs);
            mesh.GetColors(curColors);
                
            for (int i = 0; i < vertexCount; i++)
            {
                var vertexIndex = vertexOffset + i;
                var positionWS = localToWorldMatrix.MultiplyPoint(curVertices[vertexIndex]);
                var positionOS = worldToLocalMatrix.MultiplyPoint(positionWS);
                var normalWS = localToWorldMatrix.rotation*curNormals[vertexIndex];
                var normalOS = worldToLocalMatrix.rotation*normalWS;

                var tangentDirection = curTangents[vertexIndex].w;
                var tangentWS = localToWorldMatrix.rotation*curTangents[vertexIndex].ToVector3();
                var tangentOS = worldToLocalMatrix.rotation*tangentWS;

                _collector.vertices.Add(_objectToOrientedVertex(positionOS));
                _collector.normals.Add(normalOS);
                _collector.tangents.Add(tangentOS.ToVector4(tangentDirection));
                var color = vertexIndex >= curColors.Count ? Color.white : curColors[vertexIndex];
                _collector.colors.Add(color);
                _collector.uvs.Add(curUVs[vertexIndex]);
            }

            mesh.GetIndices(curIndexes,_subMeshIndex);
            var indexDelta = subMesh.baseVertex-vertexBegin;
            var indexCount = subMesh.indexCount;
            for (int i = 0; i < indexCount; i++)
                _collector.indexes.Add(curIndexes[i]-indexDelta-indexOffset);
            
            TSPoolList<Vector3>.Recycle(curVertices);
            TSPoolList<int>.Recycle(curIndexes);
            TSPoolList<Vector3>.Recycle(curNormals);
            TSPoolList<Vector4>.Recycle(curTangents);
            TSPoolList<Color>.Recycle(curColors);
            TSPoolList<Vector2>.Recycle(curUVs);
        }
        
        public static MeshFragmentData[] BakeMeshFragment(Transform _transform, ref List<Material> _materialLibrary,Func<Vector3,Vector3> _objectToOrientedVertex)
        {
           TSPoolList<MeshFragmentCollector>.Spawn(out var collectList); 
            
            foreach (var meshFilter in _transform.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = meshFilter.sharedMesh;
                var materials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;
                if (mesh.subMeshCount != materials.Length)
                {
                    EditorGUIUtility.PingObject(_transform.gameObject);
                    throw new Exception("Sub Mesh Count Not Match Material Count!");
                }
            
                foreach (var (subMeshIndex,material) in  materials.LoopIndex())
                {
                    var materialIndex = _materialLibrary.FindIndex(p => p == material);
                    if (materialIndex == -1)
                    {
                        _materialLibrary.Add(material);
                        materialIndex = _materialLibrary.Count - 1;
                    }

                    var collector = collectList.Find(p => p.embedMaterial == materialIndex);
                    if (collector == null)
                    {
                        collector = MeshFragmentCollector.Spawn(materialIndex);
                        collectList.Add(collector);
                    }

                    AppendFragment(collector, meshFilter.transform.localToWorldMatrix,_transform.worldToLocalMatrix,mesh,subMeshIndex,_objectToOrientedVertex);
                }
            }

            var data = collectList.Select(p => new MeshFragmentData
            {
                embedMaterial = p.embedMaterial,
                vertices = p.vertices.ToArray(),
                indexes = p.indexes.ToArray(),
                uvs = p.uvs.ToArray(),
                normals = p.normals.ToArray(),
                tangents = p.tangents.ToArray(),
                colors = p.colors.ToArray()
            }).ToArray();
            
            collectList.Traversal(MeshFragmentCollector.Recycle);
            TSPoolList<MeshFragmentCollector>.Recycle(collectList);
            return data;
        }
    }
}
#endif