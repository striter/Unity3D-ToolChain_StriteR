using System.Collections.Generic;

namespace Runtime.Random
{
    public class UShuffle
    {
        public static void Shuffle<T>(IList<T> _array,int _count,int _dimension)
        {
            var shuffleTimes = _count / _dimension;
        
            for (var i = 0; i < shuffleTimes; i++)
            {
                var other = i + (int)(Noise.Value.Unit1f1((float)shuffleTimes / _count) * (shuffleTimes - i));
                other *= _dimension;
                var src = i * _dimension;
                for (var j = 0; j < _dimension; j++)
                {
                    var srcDimension = src + j;
                    var otherDimension = other + j;
                    (_array[srcDimension], _array[otherDimension]) = (_array[otherDimension], _array[srcDimension]);
                }
            }
        }

        public static void LatinHypercube<T>(IList<T> _array,int _count, int _dimension, IRandomGenerator _random = null)
        {
            var shuffleTimes = _count / _dimension;
            for (var i = 0; i < shuffleTimes; i++)
            {
                var other = i + (int)(URandom.Random01(_random) * (shuffleTimes - i));
                other *= _dimension;
                var src = i * _dimension;
                var replace = (int) (URandom.Random01(_random) * _dimension);

                other += replace;
                src += replace;
                (_array[src], _array[other]) = (_array[other], _array[src]);
            }
        }
    }
}