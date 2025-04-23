using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public interface IConvex2 : IGeometry2 , IEnumerable<float2> { }
    public static class IConvex2D_Extension
    {
        private static readonly List<float2> kPoints = new List<float2>();

        public static IEnumerable<G2Line> GetEdges(this IConvex2 _convex)
        {
            kPoints.Clear();
            kPoints.AddRange(_convex);
            for (var i = 0; i < kPoints.Count - 1; i++)
                yield return new G2Line(kPoints[i], kPoints[i + 1]);
        }
    }
}