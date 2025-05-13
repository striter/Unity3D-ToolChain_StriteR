using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace EndlessOcean
{
    [Serializable]
    public class EndlessOceanChunk : ABoundaryTree<G2Box, float2>
    {
        public EndlessOceanChunk(int _maxIteration) : base(0, _maxIteration) { }
        public void Construct(G2Box _boundary)
        {
            var point = UList.Empty<float2>();
            point.Add(float2.zero);
            Construct(_boundary,point);
        }

        protected override void Split(Node _parent, IList<float2> _elements, List<Node> _nodeList)
        {
            foreach (var boundary in _parent.boundary.Divide(3))
            {
                var node = Node.Spawn(_parent.iteration + 1, boundary);
                for (var i = _parent.elementsIndex.Count - 1; i >= 0; i--)
                {
                    var childIndex = _parent.elementsIndex[i];
                    if (!boundary.Contains(_elements[childIndex], 0.1f)) 
                        continue;
                    
                    node.elementsIndex.Add(childIndex);
                    _parent.elementsIndex.RemoveAt(i);   
                }

                _nodeList.Add(node);
            }
        }
    }
    
    public class EndlessOcean_Plane : ARendererBase
    {
        [Range(1,64)]public int m_CellDivision = 3;
        [Range(1,5)]public int m_Division = 3;
        public G2Box m_Boundary;
        public EndlessOceanChunk m_Chunk = new EndlessOceanChunk(3);
        public Camera m_CullingCamera;

        protected override void Validate()
        {
            base.Validate();
            m_Chunk.m_MaxIteration = m_Division;
            m_Chunk.Construct(m_Boundary);
        }

        protected override void PopulateMesh(Mesh _mesh,Transform _viewTransform)
        {
            if (!m_CullingCamera)
                return;
            
            var indexes = UList.Empty<int>();
            var vertices = UList.Empty<float2>();
            var normals = UList.Empty<Vector3>();
            var tangents = UList.Empty<Vector4>();
            var positions = ULowDiscrepancySequences.PoissonDisk2D(m_CellDivision * m_CellDivision);

            var frustumPlanes = new GFrustum(m_CullingCamera).GetFrustumPlanes();
            foreach (var node in m_Chunk.GetLeafs())
            {
                var boundary = node.boundary;
                var boundary3D = node.boundary.To3XZ();
                if(!frustumPlanes.AABBIntersection(boundary3D))
                    continue;
                
                vertices.AddRange(positions.Select(p => boundary.GetPoint(p)));
                normals.AddRange(positions.Select(p => Vector3.up));
                tangents.AddRange(positions.Select(p => Vector3.forward.ToVector4(1f)));
            }
            
            var triangles = UList.Empty<PTriangle>();
            UTriangulation.BowyerWatson(vertices,ref triangles);
            indexes.AddRange(triangles.Resolve<PTriangle,int>());
            
            _mesh.SetVertices(vertices.Select(p=>(Vector3)p.to3xz()).ToList());
            _mesh.SetNormals(normals);
            _mesh.SetTangents(tangents);
            _mesh.SetIndices(indexes,MeshTopology.Triangles,0);
            
            
            // var meshFragments = new List<IMeshFragment>();
            // var vertexLineCount = (m_CellDivision + 1); 
            // foreach (var parent in m_Chunk.GetLeafs())
            // {
            //     var meshFragment = new FMeshFragmentObject(){ m_EmbedMaterial = -1};
            //     var boundary = parent.boundary;
            //     for (var i = 0; i <= m_CellDivision; i++)
            //     for (var j = 0; j <= m_CellDivision; j++)
            //     {
            //         var vertexUV = new Vector2(i / (float)m_CellDivision, j / (float)m_CellDivision);
            //         meshFragment.vertices.Add(boundary.GetPoint(vertexUV).to3xz());
            //     }
            //
            //     for (var i = 0; i < m_CellDivision; i++)
            //     for (var j = 0; j < m_CellDivision; j++)
            //     {
            //         var bottomLeftIndex = i * vertexLineCount + j;
            //         var bottomRightIndex = i * vertexLineCount + j + 1;
            //         var topLeftIndex = (i + 1) * vertexLineCount + j;
            //         var topRightIndex = (i + 1) * vertexLineCount + j + 1;
            //         meshFragment.indexes.Add(topLeftIndex);
            //         meshFragment.indexes.Add(bottomLeftIndex);
            //         meshFragment.indexes.Add(bottomRightIndex);
            //         meshFragment.indexes.Add(topLeftIndex);
            //         meshFragment.indexes.Add(bottomRightIndex);
            //         meshFragment.indexes.Add(topRightIndex);
            //     }
            //     
            //     meshFragments.Add(meshFragment);
            //     
            // }
            // UMeshFragment.Combine(meshFragments,_mesh,null,out var embedMaterials,EVertexAttribute.None);
        }

        public override void DrawGizmos(Transform _viewTransform)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var (index, parent) in m_Chunk.GetLeafs().LoopIndex())
            {
                Gizmos.color = UColor.IndexToColor(index);
                parent.boundary.Resize(.98f).DrawGizmos();
            }
        }

    }

}