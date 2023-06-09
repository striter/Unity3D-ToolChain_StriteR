using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Geometry
{
    public interface IShape
    {
        float3 GetSupportPoint(float3 _direction);
        float3 Center { get; }
    }

    public interface I2Shape
    {
        float2 GetSupportPoint(float2 _direction);
        float2 Center { get; }
    }

    public static class UShape
    {
        public static void DrawGizmos(this I2Shape _shape)
        { 
            var method = typeof(UGizmos).GetMethod("DrawGizmos", new[] {_shape.GetType()});
            if (method == null)
                throw new NotImplementedException($"Create a DrawGizmos method in {nameof(UGizmos)} for {_shape.GetType()}");
            method.Invoke(null,new object[]{_shape});
        }

        //GJK Algorithm
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
        
        public static bool Intersect(I2Shape _a,I2Shape _b) 
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