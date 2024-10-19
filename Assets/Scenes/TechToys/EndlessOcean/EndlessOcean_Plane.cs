using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using MeshFragment;
using Runtime;
using Runtime.DataStructure;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace EndlessOcean
{
    [Serializable]
    public class EndlessOceanChunk : ABoundaryTree<G2Box, float2>
    {
        protected override bool Optimize => false;
        protected override IEnumerable<(G2Box, IList<float2>)> Split(int _iteration, G2Box _boundary, IList<float2> _elements)
        {
            foreach (var (index,boundary) in _boundary.Divide(3).LoopIndex())
                yield return (boundary,index == 4 ? _elements: new List<float2>());
        }
    }
    
    [Serializable]
    public class EndlessOceanPlaneConstructor : ARuntimeRendererBase, ISerializationCallbackReceiver
    {
        [Range(1,5)]public int m_BoundaryDivision = 3;
        [Range(1,64)]public int m_CellDivision = 3;
        public G2Box m_Boundary;
        public EndlessOceanChunk m_Chunk = new EndlessOceanChunk();

        void Ctor()
        {
            var elements = UList.Empty<float2>();
            elements.Add(float2.zero);
            m_Chunk.Construct(elements,m_Boundary,m_BoundaryDivision,0);
        }
        
        protected override void PopulateMesh(Mesh _mesh, Transform _transform, Transform _viewTransform)
        {
            var meshFragments = new List<IMeshFragment>();
            var vertexLineCount = (m_CellDivision + 1); 
            foreach (var parent in m_Chunk.GetLeafs())
            {
                var meshFragment = new FMeshFragmentObject(){ m_EmbedMaterial = -1};
                var boundary = parent.boundary;
                for (var i = 0; i <= m_CellDivision; i++)
                for (var j = 0; j <= m_CellDivision; j++)
                {
                    var vertexUV = new Vector2(i / (float)m_CellDivision, j / (float)m_CellDivision);
                    meshFragment.vertices.Add(boundary.GetPoint(vertexUV).to3xz());
                }

                for (var i = 0; i < m_CellDivision; i++)
                for (var j = 0; j < m_CellDivision; j++)
                {
                    var bottomLeftIndex = i * vertexLineCount + j;
                    var bottomRightIndex = i * vertexLineCount + j + 1;
                    var topLeftIndex = (i + 1) * vertexLineCount + j;
                    var topRightIndex = (i + 1) * vertexLineCount + j + 1;
                    meshFragment.indexes.Add(topLeftIndex);
                    meshFragment.indexes.Add(bottomLeftIndex);
                    meshFragment.indexes.Add(bottomRightIndex);
                    meshFragment.indexes.Add(topLeftIndex);
                    meshFragment.indexes.Add(bottomRightIndex);
                    meshFragment.indexes.Add(topRightIndex);
                }
                
                meshFragments.Add(meshFragment);
                
            }
            UMeshFragment.Combine(meshFragments,_mesh,null,out var embedMaterials,EVertexAttribute.None);
        }

        public override void DrawGizmos(Transform _transform, Transform _viewTransform)
        {
            Gizmos.matrix = _transform.localToWorldMatrix;
            foreach (var (index, parent) in m_Chunk.GetLeafs().LoopIndex())
            {
                Gizmos.color = UColor.IndexToColor(index);
                parent.boundary.Resize(.98f).DrawGizmos();
            }
        }

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();
    }
    
    
    public class EndlessOcean_Plane : ARuntimeRendererMonoBehaviour<EndlessOceanPlaneConstructor>
    {
        
    }

}