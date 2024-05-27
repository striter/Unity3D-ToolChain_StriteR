using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static class SAT //Separating Axis Theorem
    {

        public static bool Intersect(this IConvex2D _convex, IConvex2D _comparer) => _2D.Intersect(_convex,_comparer);
        public static bool Intersect(this IConvex _convex, IConvex _comparer) => _3D.Intersect(_convex,_comparer);
        internal static class _2D
        {
            public static bool Intersect(IConvex2D _convex, IConvex2D _comparer)
            {
                foreach (var axis in GetAxes(_convex).Concat(GetAxes(_comparer)))
                {
                    var projection1 = ProjectOntoAxis(_convex,axis);
                    var projection2 = ProjectOntoAxis(_comparer,axis);

                    if (!intersects(projection1,projection2))
                        return false; // Separating axis found, no collision
                }

                return true;
            }
            
            public static float2 ProjectOntoAxis(IConvex2D _convex, float2 _axis)
            {
                var min = float.MaxValue;
                var max = float.MinValue;

                foreach (var vertex in _convex)
                {
                    var dotProduct = math.dot(vertex, _axis);
                    min = math.min(min, dotProduct);
                    max = math.max(max, dotProduct);
                }

                return new float2(min, max);
            }

            static bool intersects(float2 _projection1, float2 _projection2)
            {
                if (_projection1.y < _projection2.x || _projection2.y < _projection1.x)
                    return false;

                return true;
            }

            static IEnumerable<float2> GetAxes(IConvex2D _convex) => _convex.GetEdges().Select(p => new float2( p.direction.y,-p.direction.x ));
        }

        internal static class _3D
        {
            public static bool Intersect(IConvex _convex, IConvex _comparer)
            {
                foreach (var axis in _convex.GetAxes().Concat(_comparer.GetAxes()))
                {
                    var projection1 = ProjectOntoAxis(_convex,axis);
                    var projection2 = ProjectOntoAxis(_comparer,axis);

                    if (!intersects(projection1,projection2))
                        return false; // Separating axis found, no collision
                }

                return true;
            }
            
            public static float2 ProjectOntoAxis(IConvex _convex, float3 _axis)
            {
                var min = float.MaxValue;
                var max = float.MinValue;

                foreach (var vertex in _convex)
                {
                    var dotProduct = math.dot(vertex, _axis);
                    min = math.min(min, dotProduct);
                    max = math.max(max, dotProduct);
                }

                return new float2(min, max);
            }

            static bool intersects(float2 _projection1, float2 _projection2)
            {
                if (_projection1.y < _projection2.x || _projection2.y < _projection1.x)
                    return false;

                return true;
            }

        }

    }
}