using System.Collections.Generic;
using Unity.Mathematics;

namespace Geometry
{

    public interface I2Shape : IShapeDimension<float2>
    {
        float2 GetSupportPoint(float2 _direction);
    }


    public static class GJKAlgorithm
    {
        static bool LineCase(List<float2> _simplex, ref float2 _direction)
        {
            var B = _simplex[0];
            var A = _simplex[1];
            var AB = B - A;
            var AO = -A;
            _direction = umath.tripleProduct(AB, AO, AB).normalize();
            return false;
        }

        static bool TriangleCase(List<float2> _simplex, ref float2 _direction)
        {
            var C = _simplex[0];
            var B = _simplex[1];
            var A = _simplex[2];
            var AB = B - A;
            var AC = C - A;
            var AO = -A;
            var ABperp = umath.tripleProduct(AC, AB, AB);
            var ACperp = umath.tripleProduct(AB, AC, AC);
            if (math.dot(ABperp, AO) > 0)
            {
                _simplex.Remove(C);
                _direction = ABperp;
                return false;
            }

            if (math.dot(ACperp, AO) > 0)
            {
                _simplex.Remove(B);
                _direction = ACperp;
                return false;
            }
            return true;
        }

        static bool HandleSimplex(List<float2> _simplex,ref float2 _direction)
        {
            if (_simplex.Count == 2)
                return LineCase(_simplex, ref _direction);
            return TriangleCase(_simplex, ref _direction);
        }
        
        static float2 Support(I2Shape _a, I2Shape _b, float2 _direction)=> _a.GetSupportPoint(_direction) - _b.GetSupportPoint(-_direction);
        
        public static bool Intersect(this I2Shape _a,I2Shape _b) 
        {
            var d = (_a.Center - _b.Center).normalize();
            var simplex = new List<float2> {Support(_a,_b,d)};
            d = -simplex[0];
            while (true)
            {
                var A = Support(_a, _b, d);
                if (math.dot(A, d) < 0)
                    return false;
                simplex.Add(A);
                if (HandleSimplex(simplex,ref d))
                    return true;
            }
        }
    }
    
}