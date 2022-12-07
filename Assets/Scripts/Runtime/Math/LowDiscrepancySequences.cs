using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class ULowDiscrepancySequences
{
    static uint ReverseBits32(uint _n)
    {
        _n = (_n << 16) | (_n >> 16);
        _n = ((_n & 0x00ff00ff) << 8) | ((_n & 0xff00ff00) >> 8);
        _n = ((_n & 0x0f0f0f0f) << 4) | ((_n & 0xf0f0f0f0) >> 4);
        _n = ((_n & 0x33333333) << 2) | ((_n & 0xcccccccc) >> 2);
        _n = ((_n & 0x55555555) << 1) | ((_n & 0xaaaaaaaa) >> 1);
        return _n;
    }

    static ulong ReverseBits64(ulong _n)
    {
        ulong n0 = ReverseBits32((uint)_n);
        ulong n1 = ReverseBits32((uint) (_n >> 32));
        return (n0 << 32) | n1;
    }
    
    public static float VanDerCorputElement2Base(uint _i) => ReverseBits32(_i) * 2.3283064365386963e-10f;
    public static float VanDerCorputElement(uint _i, uint _base)
    {
        var invB = 1f / _base;
        var element = 0f;
        var powB = invB;
        var numDigits = (uint) (Mathf.Log(_i + 1) / Mathf.Log(_base)) + 1;
        for (uint j = 0; j < numDigits; j++)
        {
            var digit = (_i / UMath.Pow(_base, j)) % _base;
            element += digit * powB;
            powB *= invB;
        }

        return element;
    }

    
    public static float[] HaltonSequence(uint _size, uint _primeIndex=0)
    {
        float[] sequence = new float[_size];
        for (uint i = 0; i < _size; i++)
            sequence[i] = VanDerCorputElement(i,KMath.kPrimes[_primeIndex]);
        return sequence;
    }

}