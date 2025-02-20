using System;
using Runtime.Swizzlling;
using Unity.Mathematics;

namespace Runtime.Algorithm
{
    public class NumericalIntegration
    {
        public static float TrapezoidRule<T>(Func<float,T> _function,float _a = 0f,float _b = 1f, int _stepCount = 64,Func<T,T,float> _evaluate = null,IDecimal<T> _decimal = default) where T : struct
        {
            _decimal ??= FDecimal.Helper<T>();
            _evaluate ??= _decimal.distance;
            var length = 0f;
            var sample = _function(_a);
            for (var i = 1; i <= _stepCount; i++)
            {
                var value = math.lerp(_a,_b, i / (float) _stepCount);
                var next = _function(value);
                length += _evaluate(next,sample);
                sample = next;
            }
            return length;
        }
        
        public static float RombergIntegration<T>(Func<float, T> _function,int _n = 10,float _a = 0, float _b = 1,Func<T,T,float> _evaluate = null, IDecimal<T> _decimal = default) where T : struct
        {
            _decimal ??= FDecimal.Helper<T>();
            _evaluate ??= _decimal.distance;
        
            var R = new float[_n + 1, _n + 1];
            for (var i = 1; i <= _n; i++)
                R[i,0] = TrapezoidRule(_function,_a,_b,umath.pow(2, i),_evaluate,_decimal);

            for (var j = 1; j <= _n; j++)
            {
                for(var i = j;i<=_n;i++)
                   R[i, j] = R[i, j - 1] + (R[i, j - 1] - R[i - 1, j - 1])/ umath.pow(4, j - 1);
            }
            return R[_n, _n];
        }
    }
}