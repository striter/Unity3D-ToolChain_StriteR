﻿using System.Collections.Generic;
using System.Linq;
using Runtime.Pool;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class GJK
    {
        public static bool Intersect(this IGeometry2 _a,IGeometry2 _b)
        {
            var simplexHelper = PoolList<float2>.Empty(nameof(GJK));
            var d = (_a.Origin - _b.Origin).normalize();
            simplexHelper.Add(Support(_a, _b, d));
            d = -simplexHelper[^1].normalize();
            while (true)
            {
                simplexHelper.Add(Support(_a, _b, d));
                if (math.dot(simplexHelper[^1], d) < 0)
                    return false;
                
                if (HandleSimplex(simplexHelper,ref d))
                    return true;
            }
        }
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

        static float2 Support(IGeometry2 _a, IGeometry2 _b, float2 _direction) => _a.GetSupportPoint(_direction) - _b.GetSupportPoint(-_direction);

        public static G2Polygon Sum(IGeometry2 _a, IGeometry2 _b,int _sampleCount=64)  //Minkowski sum
        {
            var simplexHelper = PoolList<float2>.Empty(nameof(GJK));
            for(var i=0;i<_sampleCount;i++)
            {
                var rad = i * kmath.kPI2 / _sampleCount;
                var direction = umath.Rotate2D(rad).mul(kfloat2.up);
                var supportPoint = _a.GetSupportPoint(direction) + _b.GetSupportPoint(direction);
                if(simplexHelper.Contains(supportPoint))
                    continue;
                simplexHelper.Add(supportPoint);
            }
            return new G2Polygon(simplexHelper);
        }

        public static G2Polygon Difference(IGeometry2 _a, IGeometry2 _b, int _sampleCount = 64)
        {
            var simplexHelper = PoolList<float2>.Empty(nameof(GJK));
            for(var i=0;i<_sampleCount;i++)
            {
                var rad = i * kmath.kPI2 / _sampleCount;
                var direction = umath.Rotate2D(rad).mul(kfloat2.up);
                var supportPoint = Support(_a, _b, direction);
                if(simplexHelper.Contains(supportPoint))
                    continue;
                simplexHelper.Add(supportPoint);
            }
            return new G2Polygon(simplexHelper);
        }
    }
}