using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Validation
{
    public static class GJKAlgorithm
    {
        //2D
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
            var ABperp = umath.tripleProduct(AC, AB, AB).normalize();
            var ACperp = umath.tripleProduct(AB, AC, AC).normalize();
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

        static float2 Support(IShape2D _a, IShape2D _b, float2 _direction) => _a.GetSupportPoint(_direction) - _b.GetSupportPoint(-_direction);

        private static readonly List<float2> kSimplex = new List<float2>();
        public static bool Intersect(this IShape2D _a,IShape2D _b) 
        {
            kSimplex.Clear();
            var d = (_a.Center - _b.Center).normalize();
            kSimplex.Add(Support(_a, _b, d));
            d = -kSimplex[^1].normalize();
            while (true)
            {
                kSimplex.Add(Support(_a, _b, d));
                if (math.dot(kSimplex[^1], d) < 0)
                    return false;
                
                if (HandleSimplex(kSimplex,ref d))
                    return true;
            }
        }

        public static G2Polygon Sum(this IShape2D _a, IShape2D _b,int _sampleCount=64)  //Minkowski sum
        {
            kSimplex.Clear();
            for(int i=0;i<_sampleCount;i++)
            {
                var rad = i * kmath.kPI2 / _sampleCount;
                var direction = umath.Rotate2D(rad).mul(kfloat2.up);
                var supportPoint = _a.GetSupportPoint(direction) + _b.GetSupportPoint(direction);
                if(kSimplex.Contains(supportPoint))
                    continue;
                kSimplex.Add(supportPoint);
            }
            return new G2Polygon(kSimplex);
        }


        public static G2Polygon Difference(this IShape2D _a, IShape2D _b, int _sampleCount = 64)
        {
            kSimplex.Clear();
            for(int i=0;i<_sampleCount;i++)
            {
                var rad = i * kmath.kPI2 / _sampleCount;
                var direction = umath.Rotate2D(rad).mul(kfloat2.up);
                var supportPoint = Support(_a, _b, direction);
                if(kSimplex.Contains(supportPoint))
                    continue;
                kSimplex.Add(supportPoint);
            }
            return new G2Polygon(kSimplex);
        }
    }
}