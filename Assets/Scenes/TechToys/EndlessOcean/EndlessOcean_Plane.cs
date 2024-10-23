using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using MeshFragment;
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
        [Range(2,3)]public int m_Division;
        protected override bool Optimize => false;
        protected override IEnumerable<(G2Box, IList<float2>)> Split(int _iteration, G2Box _boundary, IList<float2> _elements)
        {
            foreach (var boundary in _boundary.Divide(m_Division))
            {
                var list = new List<float2>();
                for (var i = _elements.Count - 1; i >= 0; i--)
                {
                    if (!boundary.Contains(_elements[i], 0.1f)) 
                        continue;
                    
                    list.Add(_elements[i]);
                    _elements.RemoveAt(i);
                }
                yield return (boundary,list);
            }
        }
    }
    
    
    
    public class EndlessOcean_Plane : ARendererBase
    {
        
        [Range(1,5)]public int m_BoundaryDivision = 3;
        [Range(1,64)]public int m_CellDivision = 3;
        public G2Box m_Boundary;
        public EndlessOceanChunk m_Chunk = new EndlessOceanChunk();
        public Camera m_CullingCamera;

        protected override void Validate()
        {
            base.Validate();
            var elements = UList.Empty<float2>();
            elements.Add(float2.zero);
            m_Chunk.Construct(elements,m_Boundary,m_BoundaryDivision,0);
        }

        protected override void PopulateMesh(Mesh _mesh,Transform _viewTransform)
        {
            if (!m_CullingCamera)
                return;
            
            var indexes = UList.Empty<int>();
            var vertices = UList.Empty<float2>();
            var normals = UList.Empty<Vector3>();
            var tangents = UList.Empty<Vector4>();
            var positions = ULowDiscrepancySequences.PoissonDisk2D(m_CellDivision * m_CellDivision,30).Remake(p=>p+.5f);

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