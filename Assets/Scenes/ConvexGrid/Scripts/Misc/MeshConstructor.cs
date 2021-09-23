using System;
using System.Collections.Generic;
using LinqExtentions;
using Procedural;
using Procedural.Hexagon;
using TPool;
using TPoolStatic;
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

            public ConvexGridRenderer(Transform _transform)
            {
                iTransform = _transform;
                m_MeshFilter = _transform.GetComponent<MeshFilter>();
            }
            public void OnPoolInit(Action<int> _DoRecycle) { }

            public void GenerateMesh(ConvexArea _area)
            {
                int quadCount = _area.m_Quads.Count;
                int vertexCount = quadCount * 4;
                int indexCount = quadCount * 6;
                List<Vector3> vertices = TSPoolList<Vector3>.Spawn(vertexCount);
                List<Vector3> normals = TSPoolList<Vector3>.Spawn(vertexCount);
                List<Vector2> uvs = TSPoolList<Vector2>.Spawn(vertexCount); 
                List<int> indices = TSPoolList<int>.Spawn(indexCount);
                var center = _area.m_Identity.centerCS.ToCoord();
                foreach (var quad in _area.m_Quads)
                {
                    int startIndex = vertices.Count;
                    foreach (var tuple in quad.m_CoordQuad.LoopIndex())
                    {
                        var coord = tuple.value;
                        var index = tuple.index;
                        var positionOS = (coord-center).ToPosition();
                        vertices.Add(positionOS);
                        normals.Add(Vector3.up);
                        uvs.Add(URender.IndexToQuadUV(index));
                    }

                    indices.Add(startIndex);
                    indices.Add(startIndex+1);
                    indices.Add(startIndex+2);
                    indices.Add(startIndex+3);
                }
            
                var areaMesh = new Mesh {hideFlags = HideFlags.HideAndDontSave, name = _area.m_Identity.coord.ToString()};
                areaMesh.SetVertices(vertices);
                areaMesh.SetNormals(normals);
                areaMesh.SetIndices(indices,MeshTopology.Quads,0);
                areaMesh.SetUVs(0,uvs);
                areaMesh.Optimize();
                areaMesh.UploadMeshData(true);
                m_MeshFilter.sharedMesh = areaMesh;
                iTransform.SetPositionAndRotation(_area.m_Identity.centerCS.ToPosition(),Quaternion.identity);
                iTransform.localScale=Vector3.one;
                
                TSPoolList<Vector3>.Recycle(vertices);
                TSPoolList<Vector3>.Recycle(normals);
                TSPoolList<Vector2>.Recycle(uvs);
                TSPoolList<int>.Recycle(indices);
            }
            public void OnPoolSpawn(int identity)
            {
                
            }

            public void OnPoolRecycle()
            {
                if (m_MeshFilter.sharedMesh == null)
                    return;
                GameObject.DestroyImmediate(m_MeshFilter.mesh);
                m_MeshFilter.sharedMesh = null;
            }
        }
        
        public void Tick(float _deltaTime)
        {
        }

        public void Init(Transform _transform)
        {
            m_Selection = _transform.Find("Selection");
            m_SelectionMesh = new Mesh {name="Selection",hideFlags = HideFlags.HideAndDontSave};
            m_SelectionMesh.MarkDynamic();
            m_Selection.GetComponent<MeshFilter>().sharedMesh = m_SelectionMesh;
            m_AreaMeshes = new TObjectPoolClass<ConvexGridRenderer>(_transform.Find("AreaContainer/AreaMesh"));
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
            Debug.LogWarning($"Area Mesh Populate {_area.m_Identity.coord}");
            m_AreaMeshes.Spawn().GenerateMesh (_area);
        }

        public void Clear()
        {
            m_Selection.SetActive(false);
            m_SelectionMesh.Clear();
            m_AreaMeshes.Clear();
        }

        public void OnSelectVertex(ConvexVertex _vertex,byte _height)
        {
            Vector3 centerWS;
            if (_height == 0)
            {
                _vertex.ConstructLocalMesh(m_SelectionMesh,EGridQuadGeometry.Full,EGridVoxelGeometry.Plane,out centerWS,true,false);
                m_Selection.position = centerWS;
            }
            else
            {
                _vertex.ConstructLocalMesh(m_SelectionMesh,EGridQuadGeometry.Half,EGridVoxelGeometry.VoxelTopBottom,out centerWS,true,false);
                m_Selection.position = UTile.GetCornerHeight(_height)+ centerWS;
            }
        }
    }
}
