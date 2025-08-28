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
            for (var i = 0; i < kPoints.Count; i++)
                yield return new G2Line(kPoints[i], kPoints[(i + 1)%kPoints.Count]);
        }

        public static bool Contains(this IConvex2 _convex, float2 _point)
        {
            var intersection = 0;
            var ray = new G2Ray(_point, kfloat2.up);
            foreach (var edge in _convex.GetEdges())
            {
                if (edge.Intersect(ray, out _))
                    intersection++;
            }
            return intersection % 2 == 1;
        }
    }
}