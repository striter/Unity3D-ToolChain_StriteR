using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AlgorithmExtension;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public struct BVHNode_triangle2 : IBVHNode<G2Box, G2Triangle>
    {
        public int iteration { get; set; }
        public IList<G2Triangle> elements { get; set; }
        public G2Box boundary { get; set; }

        public void SortElements(int _median, IList<G2Triangle> _elements)
        {
            var axis = boundary.size.maxAxis();
            elements.Divide(_median,
                // .Sort(
                // ESortType.Bubble,
                (_a, _b) =>
                {
                    switch (axis)
                    {
                        default: throw new InvalidEnumArgumentException();
                        case EAxis.X: return _a.baryCentre.x >= _b.baryCentre.x ? 1 : -1;
                        case EAxis.Y: return _a.baryCentre.y >= _b.baryCentre.y ? 1 : -1;
                    }
                });
        }
        
        public G2Box CalculateBounds(IEnumerable<G2Triangle> _elements) => UBounds.GetBoundingBox(_elements.Select(p => (IEnumerable<float2>)p).Resolve());
        public bool Contains(G2Box _bounds, G2Triangle _element) => GJK.Intersect(_bounds,_element);
    }
}