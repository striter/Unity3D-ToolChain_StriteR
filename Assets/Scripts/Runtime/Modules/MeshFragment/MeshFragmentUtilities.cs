
using System.Collections.Generic;
using System.Linq;
using TPoolStatic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshFragment
{
    internal class MeshFragmentCollector
    {
        public int embedMaterial;
        public List<Vector3> vertices;
        public List<int> indexes;
        public List<Vector3> normals;
        public List<Vector4> tangents;
        public List<Color> colors;
        public List<Vector2> uvs;
        private MeshFragmentCollector() {  }
        public static MeshFragmentCollector Spawn(int _materialIndex)
        {
            return new MeshFragmentCollector()
            {
                embedMaterial = _materialIndex,
                vertices = TSPoolList<Vector3>.Spawn(),
                indexes = TSPoolList<int>.Spawn(),
                normals = TSPoolList<Vector3>.Spawn(),
                tangents=TSPoolList<Vector4>.Spawn(),
                colors = TSPoolList<Color>.Spawn(),
                uvs = TSPoolList<Vector2>.Spawn(),
            };
        }
        public static void Recycle(MeshFragmentCollector _collector)
        {
            TSPoolList<Vector3>.Recycle(_collector.vertices);
            TSPoolList<int>.Recycle(_collector.indexes);
            TSPoolList<Vector3>.Recycle(_collector.normals);
            TSPoolList<Vector4>.Recycle(_collector.tangents);
            TSPoolList<Color>.Recycle(_collector.colors);
            TSPoolList<Vector2>.Recycle(_collector.uvs);
        }
    }

    public static class UMeshFragment
    {
        static void AppendSubMesh(this MeshFragmentCollector _collector,MeshFragmentData _meshFragment)
        {
            int indexOffset = _collector.vertices.Count;
            _collector.indexes.AddRange(_meshFragment.indexes.Select(p=> p + indexOffset));
            var vertexCount = _meshFragment.vertices.Length;

            var colorValid = _meshFragment.colors != null && _meshFragment.colors.Length > 0;
            var tangentValid = _meshFragment.tangents != null && _meshFragment.tangents.Length > 0;
            for (int i = 0; i < vertexCount; i++)
            {
                _collector.vertices.Add(_meshFragment.vertices[i]);
                _collector.normals.Add(_meshFragment.normals[i]);
                _collector.uvs.Add( _meshFragment.uvs[i]);
                
                if(colorValid)
                    _collector.colors.Add(_meshFragment.colors[i]);
                if(tangentValid)
                    _collector.tangents.Add(_meshFragment.tangents[i]);
            }
        }
        
        public static void Combine(IList<MeshFragmentData> subMeshes,Mesh _mesh,Material[] _materialLibrary,out Material[] _embedMaterials)
        {
            TSPoolList<MeshFragmentCollector>.Spawn(out var subMeshCollectors);
            foreach (var meshData in subMeshes)
            {
                var embedMaterial = meshData.embedMaterial;
                var subMeshCollector = subMeshCollectors.Find(p => p.embedMaterial == embedMaterial);
                if (subMeshCollector == null)
                {
                    subMeshCollector=MeshFragmentCollector.Spawn(embedMaterial);
                    subMeshCollectors.Add(subMeshCollector);
                }

                subMeshCollector.AppendSubMesh(meshData);
            }
            
            _mesh.Clear();
            
            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<int>.Spawn(out var indexes); 
            TSPoolList<Vector3>.Spawn(out var normals);
            TSPoolList<Vector4>.Spawn(out var tangents);
            TSPoolList<Color>.Spawn(out var colors);
            TSPoolList<Vector2>.Spawn(out var uvs);
            var subMeshCount = subMeshCollectors.Count;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = subMeshCollectors[i]; 
                vertices.AddRange(subMesh.vertices);
                normals.AddRange(subMesh.normals);
                tangents.AddRange(subMesh.tangents);
                uvs.AddRange(subMesh.uvs);
                colors.AddRange(subMesh.colors);
            }
            
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetTangents(tangents);
            _mesh.SetUVs(0,uvs);
            _mesh.SetColors(colors);
            _embedMaterials = new Material[subMeshCount];

            var indexStart = 0;
            var indexOffset = 0;
            _mesh.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = subMeshCollectors[i];
                indexes.Clear();
                indexes.AddRange(subMesh.indexes.Select(p=>p+indexOffset));
                _mesh.SetIndices(indexes,MeshTopology.Triangles,i,false);
                _mesh.SetSubMesh(i,new SubMeshDescriptor(indexStart,subMesh.indexes.Count));
                indexStart += subMesh.indexes.Count;
                indexOffset += subMesh.vertices.Count;
                _embedMaterials[i] = _materialLibrary[subMesh.embedMaterial];
            }
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector3>.Recycle(normals);
            TSPoolList<Vector4>.Recycle(tangents);
            TSPoolList<Color>.Recycle(colors);
            TSPoolList<Vector2>.Recycle(uvs);

            subMeshCollectors.Traversal(MeshFragmentCollector.Recycle);
            TSPoolList<MeshFragmentCollector>.Recycle(subMeshCollectors);
        }
    }

}