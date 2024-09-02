using System.Linq;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public static class SAT     //Separating axis testing
    {   
        public static bool Intersect(this IConvex _convex, IConvex _comparer)
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
            
        public static float2 ProjectOntoAxis(this IConvex _convex, float3 _axis)
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