using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.DataStructure
{
    public struct TreeNode_float2 :ITreeIncrementHelper<G2Box, float2>
    {
        public G2Box CalculateBoundary(IList<float2> _elements)=> UGeometry.GetBoundingBox(_elements);
        public bool Contains(G2Box _bounds, float2 _element) => _bounds.Contains(_element,0.01f);
    }

    public struct TreeNode_float3 :ITreeIncrementHelper<GBox, float3>
    {
        public GBox CalculateBoundary(IList<float3> _elements)=> UGeometry.GetBoundingBox(_elements);
        public bool Contains(GBox _bounds, float3 _element)=> _bounds.Contains(_element,0.01f);
    }

    public struct TreeNode_triangle2 : ITreeIncrementHelper<G2Box, G2Triangle>
    {
        public G2Box CalculateBoundary(IList<G2Triangle> _elements)=> UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float2>)p).Resolve());
        public bool Contains(G2Box _bounds, G2Triangle _element) => GJK.Intersect(_bounds,_element);
    }

    public struct TreeNode_triangle3 : ITreeIncrementHelper<GBox, GTriangle>
    {
        public GBox CalculateBoundary(IList<GTriangle> _elements) => UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float3>)p).Resolve());
        public bool Contains(GBox _bounds, GTriangle _element) => GJK.Intersect(_bounds, _element);
    }
}
