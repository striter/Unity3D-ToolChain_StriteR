using System;
using System.Collections;
using System.Collections.Generic;
using GridTest;
using LinqExtentions;
using ObjectPool;
using ObjectPoolStatic;
using Procedural.Hexagon;
using TTouchTracker;
using UnityEngine;

namespace ConvexGrid
{
    public class MeshConstructor : IConvexGridControl
    {
        private Transform m_Selection;
        private Mesh m_SelectionMesh;
        private TObjectPoolClass<ConvexGridRenderer> m_AreaMeshes;
        private class ConvexGridRenderer : ITransform,IPoolCallback<int>
        {
            public Transform iTransform { get; }
            public MeshFilter m_MeshFilter { get; }
            private Mesh m_Mesh;

            public ConvexGridRenderer(Transform _transform)
            {
                iTransform = _transform;
                m_MeshFilter = _transform.GetComponent<MeshFilter>();
                m_Mesh = new Mesh(){hideFlags = HideFlags.HideAndDontSave};
                m_MeshFilter.sharedMesh = m_Mesh;
            }
            public void OnPoolInit(Action<int> _DoRecycle) { }

            public void OnPoolSpawn(int identity)
            {
                
            }

            public void OnPoolRecycle()
            {
            }

            public void ApplyMesh(ConvexArea _area)
            {
                m_Mesh.name = _area.m_Coord.ToString();
                int quadCount = _area.m_Quads.Count;
                int vertexCount = quadCount * 4;
                int indexCount = quadCount * 6;
                List<Vector3> vertices = TSPoolList<Vector3>.Spawn(vertexCount);
                List<Vector3> normals = TSPoolList<Vector3>.Spawn(vertexCount);
                List<Vector2> uvs = TSPoolList<Vector2>.Spawn(vertexCount); 
                List<int> indices = TSPoolList<int>.Spawn(indexCount);

                foreach (var quad in _area.m_Quads)
                {
                    int index0 = vertices.Count;
                    int index1 = index0 + 1;
                    int index2 = index0 + 2;
                    int index3 = index0 + 3;
                    foreach (var tuple in quad.m_CoordQuad.LoopIndex())
                    {
                        var coord = tuple.value;
                        var index = tuple.index;
                        var positionOS = coord.ToPosition();
                        vertices.Add(positionOS);
                        normals.Add(Vector3.up);
                        uvs.Add(URender.IndexToQuadUV(index));
                    }
                    
                    indices.Add(index0);
                    indices.Add(index1);
                    indices.Add(index3);
                    indices.Add(index1);
                    indices.Add(index2);
                    indices.Add(index3);
                }
            
                m_Mesh.Clear();
                m_Mesh.SetVertices(vertices);
                m_Mesh.SetNormals(normals);
                m_Mesh.SetUVs(0,uvs);
                m_Mesh.SetIndices(indices,MeshTopology.Triangles,0,false);
                
                TSPoolList<Vector3>.Recycle(vertices);
                TSPoolList<Vector3>.Recycle(normals);
                TSPoolList<Vector2>.Recycle(uvs);
                TSPoolList<int>.Recycle(indices);
            }
        }
        
        public void Tick(float _deltaTime)
        {
        }

        public void Init(Transform _transform)
        {
            m_Selection = _transform.Find("Selection");
            m_SelectionMesh = new Mesh {name="Selection",hideFlags = HideFlags.HideAndDontSave};
            m_Selection.GetComponent<MeshFilter>().sharedMesh = m_SelectionMesh;
            m_AreaMeshes = new TObjectPoolClass<ConvexGridRenderer>(_transform.Find("AreaContainer/AreaMesh"));
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
            Debug.LogWarning($"Area Mesh Populate {_area.m_Coord}");
            m_AreaMeshes.Spawn().ApplyMesh (_area);
        }

        public void Clear()
        {
            m_Selection.SetActive(false);
            m_SelectionMesh.Clear();
            m_AreaMeshes.Clear();
        }

        public void OnSelectVertex(ConvexVertex _vertex,byte _height,bool _construct)
        {
            m_Selection.SetActive(_construct);
            if(_construct)            
                PopulateSelectionMesh(_vertex,_height);
        }

        void PopulateSelectionMesh(ConvexVertex _vertex,int _height)
        {
            int quadCount = _vertex.m_NearbyQuads.Count;
            int vertexCount = quadCount * 4;
            int indexCount = quadCount * 6;
            List<Vector3> vertices = TSPoolList<Vector3>.Spawn(vertexCount);
            // List<Vector3> normals=TSPoolList<Vector3>.Spawn(vertexCount);
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn(vertexCount);
            List<int> indices = TSPoolList<int>.Spawn(indexCount);
            
            foreach (var tuple in _vertex.m_NearbyQuads.LoopIndex())
            {
                int startIndex = vertices.Count;
                int[] indexes = {startIndex, startIndex + 1, startIndex + 2, startIndex + 3};
                var quad = tuple.value;
                var offset = _vertex.m_NearbyQuadsStartIndex[tuple.index];
                var geometryQuad = quad.m_CoordQuad;
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(  geometryQuad[(i+offset)%4].ToPosition());
                    // normals.Add(Vector3.up);
                    uvs.Add(URender.IndexToQuadUV(i));
                }

                indices.Add(indexes[0]);
                indices.Add(indexes[1]);
                indices.Add(indexes[3]);
                indices.Add(indexes[1]);
                indices.Add(indexes[2]);
                indices.Add(indexes[3]);
            }
            
            m_SelectionMesh.Clear();
            // m_SelectionMesh.SetNormals(normals);
            m_SelectionMesh.SetVertices(vertices);
            m_SelectionMesh.SetUVs(0,uvs);
            m_SelectionMesh.SetIndices(indices,MeshTopology.Triangles,0,false);
            
            TSPoolList<Vector3>.Recycle(vertices);
            // TSPoolList<Vector3>.Recycle(normals);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<int>.Recycle(indices);
            
            // Debug.LogWarning("Selection Mesh Populate");
        }
    }
}
