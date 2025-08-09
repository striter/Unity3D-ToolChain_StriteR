using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        public static void LlyodRelaxation(IList<float2> _vertices,G2Box _bounds)
        {
            var diagram = G2VoronoiDiagram.FromPositions(_vertices);
            foreach (var (index,cell) in diagram.ToCells(_bounds).WithIndex())
                _vertices[index] = cell.simplex.center;
        }
    }
}