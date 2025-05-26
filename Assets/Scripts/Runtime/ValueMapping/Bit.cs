using UnityEngine;

public static class UBit
{
    public static int Reverse(int n, int bits) {
        var reversedN = n;
        var count = bits - 1;

        n >>= 1;
        while (n > 0) {
            reversedN = (reversedN << 1) | (n & 1);
            count--;
            n >>= 1;
        }

        return ((reversedN << count) & ((1 << bits) - 1));
    }
    
    public static uint ReverseBits32(uint _n)
    {
        _n = (_n << 16) | (_n >> 16);
        _n = ((_n & 0x00ff00ff) << 8) | ((_n & 0xff00ff00) >> 8);
        _n = ((_n & 0x0f0f0f0f) << 4) | ((_n & 0xf0f0f0f0) >> 4);
        _n = ((_n & 0x33333333) << 2) | ((_n & 0xcccccccc) >> 2);
        _n = ((_n & 0x55555555) << 1) | ((_n & 0xaaaaaaaa) >> 1);
        return _n;
    }

    public static ulong ReverseBits64(ulong _n)
    {
        ulong n0 = ReverseBits32((uint)_n);
        ulong n1 = ReverseBits32((uint) (_n >> 32));
        return (n0 << 32) | n1;
    }

    public static float RadicalInverse2(uint _i) => ReverseBits32(_i) * 2.3283064365386963e-10f;
    public static float RadicalInverse(uint _i, uint _base)   //VanDerCorput
    {
        var invB = 1f / _base;
        var element = 0f;
        var powB = invB;    
        var numDigits = (uint) (Mathf.Log(_i + 1) / Mathf.Log(_base)) + 1;
        for (uint j = 0; j < numDigits; j++)
        {
            var digit = (_i / umath.pow(_base, j)) % _base;
            element += digit * powB;
            powB *= invB;
        }

        return element;
    }
    
    //returns the highest available index (starts from 0)
    //1010->3
    //10000000001->10
    public static uint HighestBitIndex(uint _n)
    {
        uint bit = 0;
        while (true)
        {
            var sum = _n / 2;
            if ( _n == 2 * sum )
                break;
            _n = sum;
            bit++;
        }
        return bit;
    }
}