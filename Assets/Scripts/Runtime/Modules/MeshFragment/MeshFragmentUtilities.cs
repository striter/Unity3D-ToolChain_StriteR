
using System.Collections.Generic;
using System.Linq;
using TPoolStatic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshFragment
{
    internal class MeshFragmentCollector
    {
        public  int embedMaterial { get; private set; }
        public readonly List<Vector3> vertices=new List<Vector3>();
        public readonly List<Vector3> normals=new List<Vector3>();
        public readonly List<Vector4> tangents=new List<Vector4>();
        public readonly List<Color> colors=new List<Color>();
        public readonly List<Vector2> uvs=new List<Vector2>();
        public readonly List<int> indexes=new List<int>();

        public MeshFragmentCollector Initialize(int _materialIndex)
        {
            embedMaterial = _materialIndex;
            return this;
        }
    
        public void Append(IEnumerable<IMeshFragment> _meshFragments)
        {
            TSPoolList<IMeshFragment>.Spawn(out var tempList);
            _meshFragments.FillList(tempList);

            vertices.Clear();
            normals.Clear();
            tangents.Clear();
            colors.Clear();
            uvs.Clear();
            indexes.Clear();
            
            int totalVertexCount = 0;
            int totalIndexCount = 0;
            foreach (var fragment in tempList)
            {
                totalVertexCount += fragment.vertices.Count;
                totalIndexCount += fragment.indexes.Count;
            }
            if (totalVertexCount > vertices.Count)
            {
                vertices.Capacity = totalVertexCount;
                normals.Capacity = totalVertexCount;
                tangents.Capacity = totalVertexCount;
                colors.Capacity = totalVertexCount;
                uvs.Capacity = totalVertexCount;
                indexes.Capacity = totalIndexCount;
            }
            
            foreach (var fragment in tempList)
            {
                int indexOffset = vertices.Count;
                for (int i = 0; i < fragment.indexes.Count; i++)
                    indexes.Add(fragment.indexes[i]+indexOffset);
                
                var vertexCount = fragment.vertices.Count;

                var colorValid = fragment.colors != null && fragment.colors.Count > 0;
                var tangentValid = fragment.tangents != null && fragment.tangents.Count > 0;
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices.Add(fragment.vertices[i]);
                    normals.Add(fragment.normals[i]);
                    uvs.Add( fragment.uvs[i]);
                
                    if(colorValid)
                        colors.Add(fragment.colors[i]);
                    if(tangentValid)
                        tangents.Add(fragment.tangents[i]);
                }
            }
            TSPoolList<IMeshFragment>.Recycle(tempList);
        }
    }

    public static class UMeshFragment
    {
        private static readonly List<Vector3> kVertices=new List<Vector3>();
        private static readonly List<Vector3> kNormals=new List<Vector3>();
        private static readonly List<Vector4> kTangents=new List<Vector4>();
        private static readonly List<Vector2> kUVs=new List<Vector2>();
        private static readonly List<Color> kColors=new List<Color>();
        private static readonly List<int> kIndexes=new List<int>();

        public static void Combine(IEnumerable<IMeshFragment> _subMeshes,Mesh _mesh,Material[] _materialLibrary,out Material[] _embedMaterials)
        {
            TSPoolList<MeshFragmentCollector>.Spawn(out var subMeshCollectors);
            var vertexCount = 0;
            var indexCount = 0;
            foreach (var meshGroup in _subMeshes.GroupBy(p=>p.embedMaterial))
            {
                TSPool<MeshFragmentCollector>.Spawn(out var subMeshCollector);
                subMeshCollector.Initialize(meshGroup.Key).Append(meshGroup);
                subMeshCollectors.Add(subMeshCollector);
                vertexCount += subMeshCollector.vertices.Count;
                indexCount += subMeshCollector.indexes.Count;
            }
            
            _mesh.Clear();
            
            kVertices.Clear();
            kNormals.Clear();
            kTangents.Clear();
            kUVs.Clear();
            kColors.Clear();
            kIndexes.Clear();
            
            var subMeshCount = subMeshCollectors.Count;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = subMeshCollectors[i]; 
                kVertices.AddRange(subMesh.vertices);
                kNormals.AddRange(subMesh.normals);
                kTangents.AddRange(subMesh.tangents);
                kUVs.AddRange(subMesh.uvs);
                kColors.AddRange(subMesh.colors);
            }
            
            _mesh.SetVertices(kVertices);
            _mesh.SetNormals(kNormals);
            _mesh.SetTangents(kTangents);
            _mesh.SetUVs(0,kUVs);
            _mesh.SetColors(kColors);
            _embedMaterials = new Material[subMeshCount];

            var indexStart = 0;
            var indexOffset = 0;
            _mesh.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = subMeshCollectors[i];
                var indexes = subMesh.indexes;
                kIndexes.Clear();
                for(int j=0;j<subMesh.indexes.Count;j++)
                    kIndexes.Add(indexes[j]+indexOffset);
                _mesh.SetIndices(kIndexes,MeshTopology.Triangles,i,false);
                _mesh.SetSubMesh(i,new SubMeshDescriptor(indexStart,indexes.Count));
                indexStart += indexes.Count;
                indexOffset += subMesh.vertices.Count;
                _embedMaterials[i] = _materialLibrary[subMesh.embedMaterial];
            }
            subMeshCollectors.Traversal(TSPool<MeshFragmentCollector>.Recycle);
            TSPoolList<MeshFragmentCollector>.Recycle(subMeshCollectors);
        }
    }

}