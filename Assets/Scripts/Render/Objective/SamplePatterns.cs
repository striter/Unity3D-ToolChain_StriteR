using Unity.Mathematics;

namespace Rendering.Pipeline
{
    using static ULowDiscrepancySequences;
    public static class SamplePattern2D
    {
        public static float2[] Grid(int _width,int _height)
        {
            float2[] grid = new float2[_width * _height];
            float2 uvOffset = new float2(1f/(_width ),1f / (_height));

            float2 start = -.5f + uvOffset * .5f;
            for(int y = 0; y < _height; y++)
            for(int x = 0; x < _width; x++)
                grid[y * _width + x] = start +  new float2(x, y) * uvOffset;
            return grid;
        }

        public static float2[] Stratified(int _width, int _height, bool _jitter = false, System.Random _random = null)
        {
            float2 uvOffset = 1f / new float2(_width,_height);
            float2[] grid = new float2[_width*_height];
            float2 start = -.5f;
            for(int x = 0; x < _width; x++)
            for(int y = 0; y < _height; y++)
            {
                var jx = _jitter ? URandom.Random01(_random) : .5f;
                var jy = _jitter ? URandom.Random01(_random) : .5f;
                grid[y * _width + x] = start + new float2(x+jx,y+jy)*uvOffset;
            }
            URandom.LatinHypercube(grid,(uint)grid.Length,(uint)_width,_random);
            return grid;
        }
        
        public static float2[] Halton(uint _size)
        {
            float2[] sequence = new float2[_size];
            for (uint i = 0; i < _size; i++)
                sequence[i] = new float2( VanDerCorputElement2Base(i),VanDerCorput(i,KMath.kPrimes128[1])) - .5f;
            return sequence;
        }

        public static float2[] Hammersley(uint _size)
        {
            float n = _size;
            float2[] sequence = new float2[_size];
            for (uint i = 0; i < _size; i++)
                sequence[i] = new float2(i/n,VanDerCorputElement2Base(i)) - .5f;
            return sequence;
        }
    }
    
}