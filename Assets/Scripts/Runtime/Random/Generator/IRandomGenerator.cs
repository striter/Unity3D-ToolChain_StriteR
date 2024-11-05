using Unity.Mathematics;

namespace Runtime.Random
{
    public interface IRandomGenerator
    {
        /// <summary>
        ///   <para>Returns a random float within [0.0..1.0] (range is inclusive)\.</para>
        /// </summary>
        public float NextFloat();
    }

    public static class IRandomGenerator_Extension
    {
        public static int NextInt(this IRandomGenerator _random, int _max) => (int)math.round(_random.NextFloat() * _max);
        public static int NextInt(this IRandomGenerator _random, int _min, int _max) => _min + _random.NextInt( _max - _min);
        public static float NextFloat(this IRandomGenerator _random, float _max) => _random.NextFloat() * _max;
        public static float NextFloat(this IRandomGenerator _random, float _min, float _max) => _min + _random.NextFloat(_max - _min);
    }
    
    public class LCGRandom : IRandomGenerator
    {
        private const uint kModulus = int.MaxValue , kModulusM1 = kModulus - 1, kA = 48271, kC = 1;
        private uint seed;

        public LCGRandom(int _hashCode)
        {
            seed = (uint)_hashCode;
        }

        public float NextFloat()
        {
            seed = (seed * kA + kC) % kModulus;
            return (float)seed / kModulusM1;
        }
    }

    public class UnityRandom : IRandomGenerator
    {
        public float NextFloat() => UnityEngine.Random.value;
        public static UnityRandom kDefault = new UnityRandom();
    }
}