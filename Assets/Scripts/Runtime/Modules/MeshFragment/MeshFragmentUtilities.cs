
using System.Collections.Generic;
using System.Linq;
using TPoolStatic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshFragment
{
    internal class MeshFragmentHarvester
    {
        public int m_EmbedMaterial { get; private set; }
        private const int kStartVertexCount = 2048;
        public readonly List<Vector3> vertices=new List<Vector3>(kStartVertexCount);
        public readonly List<Vector3> normals=new List<Vector3>(kStartVertexCount);
        public readonly List<Vector4> tangents=new List<Vector4>(kStartVertexCount);
        public readonly List<Color> colors=new List<Color>(kStartVertexCount);
        public readonly List<Vector2> uvs=new List<Vector2>(kStartVertexCount);
        public readonly List<int> indexes=new List<int>(kStartVertexCount*2);

        public MeshFragmentHarvester Initialize(int _materialIndex)
        {
            m_EmbedMaterial = _materialIndex;
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

    internal class MeshFragmentCombiner
    {
        public int m_EmbedMaterial { get; private set; }
        public IList<IMeshFragment> m_MeshFragments { get; private set; } = new List<IMeshFragment>();
        public int m_TotalIndexCount { get; private set; } = 0;
        public int m_TotalVertexCount { get; private set; } = 0;
        public MeshFragmentCombiner Initialize(int _materialIndex)
        {
            m_EmbedMaterial = _materialIndex;
            m_MeshFragments.Clear();
            m_TotalVertexCount = 0;
            m_TotalIndexCount = 0;
            return this;
        }

        public void Append(IMeshFragment _fragment)
        {
            m_MeshFragments.Add(_fragment);
            m_TotalVertexCount += _fragment.vertices.Count;
            m_TotalIndexCount += _fragment.indexes.Count;
        }
    }

    public static class UMeshFragment
    {
        private static readonly Dictionary<int,MeshFragmentCombiner> kMeshFragmentHelper = new Dictionary<int, MeshFragmentCombiner>();
        public static void Combine(IList<IMeshFragment> _fragments,Mesh _mesh,Material[] _materialLibrary,out Material[] _embedMaterials)
        {
            TSPoolList<MeshFragmentCombiner>.Spawn(out var subMeshCombiners);
            var totalVertexCount = 0;
            kMeshFragmentHelper.Clear();
            for (int i = 0; i < _fragments.Count; i++)
            {
                var fragment = _fragments[i];
                if (!kMeshFragmentHelper.ContainsKey(fragment.embedMaterial))
                {
                    TSPool<MeshFragmentCombiner>.Spawn(out var fragmentCollectorInstance);
                    subMeshCombiners.Add(fragmentCollectorInstance);
                    kMeshFragmentHelper.Add(fragment.embedMaterial, fragmentCollectorInstance.Initialize(fragment.embedMaterial));
                }

                var fragmentCollector = kMeshFragmentHelper[fragment.embedMaterial];
                fragmentCollector.Append(fragment);
                totalVertexCount += fragment.vertices.Count;
            }
            
            NativeArray<Vector3> vertices=new NativeArray<Vector3>(totalVertexCount,Allocator.Temp);
            NativeArray<Vector3> normals=new NativeArray<Vector3>(totalVertexCount,Allocator.Temp);
            NativeArray<Vector4> tangents=new NativeArray<Vector4>(totalVertexCount,Allocator.Temp);
            NativeArray<Vector2> uvs=new NativeArray<Vector2>(totalVertexCount,Allocator.Temp);
            NativeArray<Color> colors=new NativeArray<Color>(totalVertexCount,Allocator.Temp);
            
            _mesh.Clear();
            var subMeshCount = subMeshCombiners.Count;
            int currrentVertexIndex = 0;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = subMeshCombiners[i];
                
                var subMeshFragmentCount = subMesh.m_MeshFragments.Count;
                for (int j = 0; j < subMeshFragmentCount; j++)
                {
                    var fragment = subMesh.m_MeshFragments[j];
                    var fragmentVertexCount = fragment.vertices.Count;
                    for (int k = 0; k < fragmentVertexCount; k++)
                    {
                        vertices[currrentVertexIndex+k]=fragment.vertices[k];
                        normals[currrentVertexIndex+k]=fragment.normals[k];
                        tangents[currrentVertexIndex+k]=fragment.tangents[k];
                        uvs[currrentVertexIndex+k]=fragment.uvs[k];
                        colors[currrentVertexIndex+k]=fragment.colors[k];
                    }

                    currrentVertexIndex += fragmentVertexCount;
                }
            }
            
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetTangents(tangents);
            _mesh.SetUVs(0,uvs);
            _mesh.SetColors(colors);
            
            vertices.Dispose();
            normals.Dispose();
            tangents.Dispose();
            uvs.Dispose();
            colors.Dispose();
            
            _embedMaterials = new Material[subMeshCount];

            var indexStart = 0;
            var indexOffset = 0;
            _mesh.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMeshCombiner = subMeshCombiners[i];

                NativeArray<int> subMeshIndexes=new NativeArray<int>(subMeshCombiner.m_TotalIndexCount, Allocator.Temp);

                var currentTriangleIndex = 0;
                var subMeshFragmentCount = subMeshCombiner.m_MeshFragments.Count;
                for (int j = 0; j < subMeshFragmentCount; j++)
                {
                    var fragment = subMeshCombiner.m_MeshFragments[j];
                    var fragmentIndexCount = fragment.indexes.Count;
                    for (int k = 0; k < fragmentIndexCount; k++)
                        subMeshIndexes[ currentTriangleIndex + k ] = indexOffset+fragment.indexes[k];
                    indexOffset += fragment.vertices.Count;
                    currentTriangleIndex += fragmentIndexCount;
                }
                
                _mesh.SetIndices(subMeshIndexes,MeshTopology.Triangles,i,false);
                _mesh.SetSubMesh(i,new SubMeshDescriptor(indexStart,subMeshCombiner.m_TotalIndexCount));
                indexStart += subMeshCombiner.m_TotalIndexCount;
                subMeshIndexes.Dispose();
                _embedMaterials[i] = _materialLibrary[subMeshCombiner.m_EmbedMaterial];
            }

            for (int i = 0; i < subMeshCombiners.Count; i++)
                TSPool<MeshFragmentCombiner>.Recycle(subMeshCombiners[i]);
            TSPoolList<MeshFragmentCombiner>.Recycle(subMeshCombiners);
        }
    }

}