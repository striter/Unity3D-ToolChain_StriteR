using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Extensions;
using AlgorithmExtension;
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

    public struct TreeNode_triangle2 : ITreeIncrementHelper<G2Box, G2Triangle> , IBVHHelper<G2Box, G2Triangle> 
    {
        public G2Box CalculateBoundary(IList<G2Triangle> _elements)=> UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float2>)p).Resolve());
        public bool Contains(G2Box _bounds, G2Triangle _element) => GJK.Intersect(_bounds,_element);
        
        public void SortElements(int _median, G2Box _boundary, IList<G2Triangle> _elements)
        {
            var axis = _boundary.size.maxAxis();
            _elements.Divide(_median,
                // .Sort(
                // ESortType.Bubble,
                (_a, _b) => axis switch
                {
                    EAxis.X => _a.baryCentre.x >= _b.baryCentre.x ? 1 : -1,
                    EAxis.Y => _a.baryCentre.y >= _b.baryCentre.y ? 1 : -1,
                    _ => throw new InvalidEnumArgumentException()
                });
        }
    }

    public struct TreeHelper_Box_Triangle : ITreeIncrementHelper<GBox, GTriangle> , IBVHHelper<GBox, GTriangle>
    {
        public GBox CalculateBoundary(IList<GTriangle> _elements) => UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float3>)p).Resolve());
        public bool Contains(GBox _bounds, GTriangle _element) => GJK.Intersect(_bounds, _element);
        
        public void SortElements(int _median, GBox _boundary, IList<GTriangle> _elements)
        {
            var axis = _boundary.size.maxAxis();
            _elements.Divide(_median,
                // .Sort(
                // ESortType.Bubble,
                (_a, _b) => axis switch
                {
                    EAxis.X => _a.baryCentre.x >= _b.baryCentre.x ? 1 : -1,
                    EAxis.Y => _a.baryCentre.y >= _b.baryCentre.y ? 1 : -1,
                    EAxis.Z => _a.baryCentre.z >= _b.baryCentre.z ? 1 : -1,
                    _ => throw new InvalidEnumArgumentException()
                });
        }
    }
}
