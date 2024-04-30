using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.DataStructure
{
    public struct TreeNode_float2 : ITreeNode<G2Box,float2>
    {
        public int iteration { get; set; }
        public G2Box boundary { get; set; }
        public IList<float2> elements { get; set; }
        public G2Box CalculateBounds(IEnumerable<float2> _elements)=> UGeometry.GetBoundingBox(_elements);
        public bool Contains(G2Box _bounds, float2 _element) => _bounds.Contains(_element,0.01f);
    }

    public struct TreeNode_float3 : ITreeNode<GBox, float3>
    {
        public int iteration { get; set; }
        public GBox boundary { get; set; }
        public IList<float3> elements { get; set; }
        public GBox CalculateBounds(IEnumerable<float3> _elements)=> UGeometry.GetBoundingBox(_elements);
        public bool Contains(GBox _bounds, float3 _element)=> _bounds.Contains(_element,0.01f);
    }

    public struct TreeNode_triangle2 : ITreeNode<G2Box, G2Triangle>
    {
        public int iteration { get; set; }
        public G2Box boundary { get; set; }
        public IList<G2Triangle> elements { get; set; }
        public G2Box CalculateBounds(IEnumerable<G2Triangle> _elements)=> UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float2>)p).Resolve());
        public bool Contains(G2Box _bounds, G2Triangle _element) => GJK.Intersect(_bounds,_element);
    }

    public struct TreeNode_triangle3 : ITreeNode<GBox, GTriangle>
    {
        public int iteration { get; set; }
        public GBox boundary { get; set; }
        public IList<GTriangle> elements { get; set; }
        public GBox CalculateBounds(IEnumerable<GTriangle> _elements) => UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float3>)p).Resolve());
        public bool Contains(GBox _bounds, GTriangle _element) => GJK.Intersect(_bounds, _element);
    }
}
