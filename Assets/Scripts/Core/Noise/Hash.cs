using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Noise
{
    public readonly struct SmallXXHash
    {
        private const uint primeA = 0b10011110001101110111100110110001;
        private const uint primeB = 0b10000101111010111100101001110111;
        private const uint primeC = 0b11000010101100101010111000111101;
        private const uint primeD = 0b00100111110101001110101100101111;
        private const uint primeE = 0b00010110010101100110011110110001;
        public readonly uint accumulator;

        private SmallXXHash(uint _accumulator)=>accumulator = _accumulator;
        public SmallXXHash Eat(int _data) =>RotateLeft(accumulator + (uint) _data * primeC,17) * primeD;
        public SmallXXHash Eat(byte _data) => RotateLeft(accumulator + _data * primeE, 11) * primeA;

        static uint RotateLeft(uint _data, int _steps) => (_data << _steps) | (_data >> 32 - _steps);
        
        public static SmallXXHash Seed(int _seed) => new SmallXXHash((uint) _seed + primeE);
        public static implicit operator SmallXXHash(uint _accumulator)=>new SmallXXHash(_accumulator);
        public static implicit operator uint(SmallXXHash _hash)
        {
            uint avalanche = _hash.accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= primeB;
            avalanche ^= avalanche >> 13;
            avalanche *= primeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }
    }
    
    public readonly struct SmallXXHash4
    {
        private const uint primeB = 0b10000101111010111100101001110111;
        private const uint primeC = 0b11000010101100101010111000111101;
        private const uint primeD = 0b00100111110101001110101100101111;
        private const uint primeE = 0b00010110010101100110011110110001;
        private readonly uint4 accumulator;

        public static SmallXXHash4 Seed(uint4 _seed) => new SmallXXHash4((uint4) _seed + primeE);
        private SmallXXHash4(uint4 _accumulator)=>accumulator = _accumulator;
        public SmallXXHash4 Eat(int4 _data) =>RotateLeft(accumulator + (uint4) _data * primeC,17) * primeD;

        static uint4 RotateLeft(uint4 _data, int _steps) => (_data << _steps) | (_data >> 32 - _steps);
        
        public static implicit operator SmallXXHash4(SmallXXHash hash)=>new SmallXXHash4(hash.accumulator);
        public static implicit operator SmallXXHash4(uint4 _accumulator)=>new SmallXXHash4(_accumulator);
        public static implicit operator uint4(SmallXXHash4 _hash)
        {
            uint4 avalanche = _hash.accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= primeB;
            avalanche ^= avalanche >> 13;
            avalanche *= primeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }
    }
}