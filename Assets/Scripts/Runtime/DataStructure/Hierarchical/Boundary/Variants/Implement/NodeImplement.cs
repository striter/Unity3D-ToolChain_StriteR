using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.DataStructure
{
    public static class BoundaryTreeHelper
    {
        public class G2Box_float2 : IQuadTreeHelper<G2Box, float2> ,IKDTreeHelper<G2Box,float2>
        {
            public bool QuadTreeValidate(G2Box _boundary, float2 p) => (p <= _boundary.max).all();
            public G2Box CalculateBoundary(IList<float2> _elements)=> UGeometry.GetBoundingBox(_elements);
            public bool KDTreeValidate(int iteration, G2Box _boundary, float2 _element)
            {
                var axis = iteration % 2;
                return _element[axis] < _boundary.center[axis];
            }
        }

        public class GBox_float3 : IQuadTreeHelper<GBox, float3> ,IKDTreeHelper<GBox,float3>
        {
            public bool QuadTreeValidate(GBox _boundary, float3 p) => (p <= _boundary.max).all();
            public GBox CalculateBoundary(IList<float3> _elements)=> UGeometry.GetBoundingBox(_elements);
            public bool KDTreeValidate(int iteration, GBox _boundary, float3 _element)
            {
                var axis = iteration % 3;
                return _element[axis] < _boundary.center[axis];
            }
        }
        
        public class G2Box_G2Triangle :  IBVHHelper<G2Box, G2Triangle> 
        {
            public G2Box CalculateBoundary(IList<G2Triangle> _elements)=> UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float2>)p).Resolve());
            public void SortElements(int _median, G2Box _boundary,IList<int> _elementIndexes, IList<G2Triangle> _elements)
            {
                var axis = _boundary.size.maxAxis();
                _elementIndexes.Divide(_median,
                    // .Sort(
                    // ESortType.Bubble,
                    (_a, _b) =>
                    {
                        var a = _elements[_a];
                        var b = _elements[_b];
                        return axis switch
                        {
                            EAxis.X => a.baryCentre.x >= b.baryCentre.x ? 1 : -1,
                            EAxis.Y => a.baryCentre.y >= b.baryCentre.y ? 1 : -1,
                            _ => throw new InvalidEnumArgumentException()
                        };
                    });
            }
        }
        
        public class GBox_GTriangle : IBVHHelper<GBox, GTriangle> ,IQuadTreeHelper<GBox, GTriangle>
        {
            public GBox CalculateBoundary(IList<GTriangle> _elements) => UGeometry.GetBoundingBox(_elements.Select(p => (IEnumerable<float3>)p).Resolve());
            public void SortElements(int _median, GBox _boundary,IList<int> _elementIndexes, IList<GTriangle> _elements)
            {
                var axis = _boundary.size.maxAxis();
                _elementIndexes.Divide(_median,
                    // .Sort(
                    // ESortType.Bubble,
                    (_a, _b) =>
                    {
                        var a = _elements[_a];
                        var b = _elements[_b];
                        return axis switch
                        {
                            EAxis.X => a.baryCentre.x >= b.baryCentre.x ? 1 : -1,
                            EAxis.Y => a.baryCentre.y >= b.baryCentre.y ? 1 : -1,
                            EAxis.Z => a.baryCentre.z >= b.baryCentre.z ? 1 : -1,
                            _ => throw new InvalidEnumArgumentException()
                        };
                    });
            }

            public bool QuadTreeValidate(GBox _boundary, GTriangle triangle) => triangle.All(p => (p <= _boundary.max).all());
        }
    }

}
