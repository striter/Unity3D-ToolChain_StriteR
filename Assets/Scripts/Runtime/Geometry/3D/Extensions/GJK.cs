using System.Collections.Generic;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    //https://cse442-17f.github.io/Gilbert-Johnson-Keerthi-Distance-Algorithm/
    public static class GJK
    {
        private static readonly List<float3> kSimplex = new List<float3>();
        public static bool Intersect(this IVolume _a,IVolume _b) 
        {
            kSimplex.Clear();
            var d = (_a.Origin - _b.Origin).normalize();
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
        
        static bool LineCase(List<float3> _simplex, ref float3 _direction)
        {
            var B = _simplex[0];
            var A = _simplex[1];
            var AB = B - A;
            var AO = -A;
            _direction = umath.tripleProduct(AB, AO, AB).normalize();
            return false;
        }

        static bool TriangleCase(List<float3> _simplex, ref float3 _direction)
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

        static bool HandleSimplex(List<float3> _simplex,ref float3 _direction)
        {
            if (_simplex.Count == 2)
                return LineCase(_simplex, ref _direction);
            return TriangleCase(_simplex, ref _direction);
        }

        static float3 Support(IVolume _a, IVolume _b, float3 _direction) => _a.GetSupportPoint(_direction) - _b.GetSupportPoint(-_direction);
        public static GPointSets Difference(IVolume _a, IVolume _b, int _sampleCount = 64)
        {
            kSimplex.Clear();
            for(var i=0;i<_sampleCount;i++)
            {
                var direction = USphereMapping.LowDiscrepancySequences.Hammersley((uint)i,(uint)_sampleCount);
                var supportPoint = Support(_a, _b, direction);
                if(kSimplex.Contains(supportPoint))
                    continue;
                kSimplex.Add(supportPoint);
            }
            return new GPointSets(kSimplex);
        }
    }
        
}