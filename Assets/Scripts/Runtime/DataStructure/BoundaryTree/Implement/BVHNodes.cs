using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Extensions;
using AlgorithmExtension;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public struct IbvhHelperTriangle2 : IBVHHelper<G2Box, G2Triangle>
    {
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

        public G2Box CalculateBoundary(IList<G2Triangle> _elements) =>
            UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float2>)p).Resolve());
    }

    public struct IbvhHelperTriangle3 : IBVHHelper<GBox, GTriangle>
    {
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

        public GBox CalculateBoundary(IList<GTriangle> _elements) =>
            UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float3>)p).Resolve());
    }
}