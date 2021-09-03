using System;
using System.Collections;
using System.Collections.Generic;
using GridTest;
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

        private TObjectPoolClass<ConvexGridRenderer> m_Meshes;
        private class ConvexGridRenderer : ITransform,IPoolCallback
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

            public void ApplyMesh(ConvexArea _area,Dictionary<HexCoord,ConvexVertex> _vertices)
            {
                m_Mesh.name = _area.m_Area.m_Coord.ToString();
                List<Vector3> vertices = TSPoolList<Vector3>.Spawn(); 
                List<Vector2> uvs = TSPoolList<Vector2>.Spawn(); 
                List<int> indices = TSPoolList<int>.Spawn();

                foreach (var quad in _area.m_Quads)
                {
                    int index0 = vertices.Count;
                    int index1 = index0 + 1;
                    int index2 = index0 + 2;
                    int index3 = index0 + 3;
                    quad.Traversal((index, coord) =>
                    {
                        var positionOS = _vertices[coord].m_Coord.ToWorld();
                        vertices.Add(positionOS);
                        uvs.Add(URender.IndexToQuadUV(index));
                    });
                    indices.Add(index0);
                    indices.Add(index1);
                    indices.Add(index3);
                    indices.Add(index1);
                    indices.Add(index2);
                    indices.Add(index3);
                }
            
                m_Mesh.Clear();
                m_Mesh.SetVertices(vertices);
                m_Mesh.SetUVs(0,uvs);
                m_Mesh.SetIndices(indices,MeshTopology.Triangles,0,true);
            }
        }
        

        public void Init(Transform _transform)
        {
            m_Selection = _transform.Find("Selection");
            m_SelectionMesh = new Mesh {name="Selection",hideFlags = HideFlags.HideAndDontSave};
            m_Selection.GetComponent<MeshFilter>().sharedMesh = m_SelectionMesh;
            m_Meshes = new TObjectPoolClass<ConvexGridRenderer>(_transform.Find("AreaContainer/AreaMesh"));
        }

        public void Tick(float _deltaTime)
        {
        }

        public void Select(bool _valid,HexCoord _coord, ConvexVertex _vertex)
        {
            m_Selection.SetActive(_valid);
            if (!_valid)
                return;

            
            List<Vector3> vertices = TSPoolList<Vector3>.Spawn();
            List<int> indices = TSPoolList<int>.Spawn();
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn();
            
            foreach (ConvexQuad quad in _vertex.m_RelativeQuads)
            {
                int startIndex = vertices.Count;
                int[] indexes = {startIndex, startIndex + 1, startIndex + 2, startIndex + 3};

                var hexQuad = quad.m_HexQuad;
                var geometryQuad = quad.m_CoordQuad;
                var hexVertices = hexQuad;
                var offset = hexVertices.FindIndex(p=>p==_coord);
                for (int i = 0; i < 4; i++)
                {
                    int index=(i+offset)%4;
                    vertices.Add(  geometryQuad[index].ToWorld());
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
            m_SelectionMesh.SetVertices(vertices);
            m_SelectionMesh.SetUVs(0,uvs);
            m_SelectionMesh.SetIndices(indices,MeshTopology.Triangles,0,false);
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<int>.Recycle(indices);
        }


        public void ConstructArea(ConvexArea _area,Dictionary<HexCoord,ConvexVertex> _vertices)
        {
            Debug.LogWarning($"Area Construct{_area}");
            m_Meshes.AddItem().ApplyMesh (_area,_vertices);
        }
        
        public void Clear()
        {
            m_Selection.SetActive(false);
            m_SelectionMesh.Clear();
            m_Meshes.Clear();
        }
    }
}
