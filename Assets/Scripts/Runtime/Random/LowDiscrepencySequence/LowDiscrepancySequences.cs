using Unity.Mathematics;
using static UBit;
using static kmath;
public static partial class ULowDiscrepancySequences // 0 - 1
{ 
    static float RadicalInverseOptimized(uint _n,uint _dimension) =>_dimension == 0 ? RadicalInverse2(_n) : RadicalInverse(_n, kPrimes128[_dimension]);
    public static float Halton(uint _index, uint _dimension) => RadicalInverseOptimized(_index,_dimension);
    public static float Hammersley(uint _index,uint _dimension,uint _numSamples)=>_dimension==0?(_index/(float)_numSamples):RadicalInverseOptimized(_index,_dimension-1);

    private static readonly float kSobolMaxValue = math.pow(2, 32);
    public static float[] Sobel(uint _size)
    {
        var N = _size;
        var points = new float[N];
        var C = new uint[N];
        for (int i = 0; i < N; i++)
        {
            C[i] = 1;
            var value = i;
            while ((value & 1) > 0)
            {
                value >>= 1;
                C[i]++;
            }
        }

        var L = (uint) math.ceil(math.log(N) / math.log(2.0f));
        
        var V = new uint[L + 1];
        for (int i = 1; i <= L; i++) V[i] = 1u << (32 - i);
        
        var X = new uint[N];
        X[0] = 0;
        for (uint i = 1u; i < N; i++)
        {
            X[i] = X[i - 1] ^ V[C[i - 1]];
            points[i] = X[i] / kSobolMaxValue;
        }
        
        return points;
    }
}