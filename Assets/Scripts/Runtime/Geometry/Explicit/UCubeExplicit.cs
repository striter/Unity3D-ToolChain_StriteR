using System;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Explicit
{
    public static class UCubeExplicit
    {
        public const int kCubeFacingAxisCount = 6;
        public static Axis GetFacingAxis(int _index) => _index switch
        {
            0 => new Axis()
            {
                index = _index,
                origin = -Vector3.one,
                uDir = new Vector3(2f, 0, 0),
                vDir = new Vector3(0, 2f, 0)
            },
            1 => new Axis()
            {
                index = _index,
                origin = -Vector3.one,
                uDir = new Vector3(0, 2f, 0),
                vDir = new Vector3(0f, 0, 2f)
            },
            2 => new Axis()
            {
                index = _index,
                origin = -Vector3.one, 
                uDir = new Vector3(0, 0, 2f), 
                vDir = new Vector3(2f, 0, 0)
            },
            3 => new Axis()
            {
                index = _index, 
                origin = new Vector3(-1f, -1f, 1f), 
                uDir = new Vector3(0, 2f, 0),
                vDir = new Vector3(2f, 0, 0)
            },
            4 => new Axis()
            {
                index = _index,
                origin = new Vector3(1f, -1f, -1f),
                uDir = new Vector3(0, 0, 2f),
                vDir = new Vector3(0, 2f, 0f)
            },
            5 => new Axis()
            {
                index = _index,
                origin = new Vector3(-1f, 1f, -1f),
                uDir = new Vector3(2f, 0f, 0),
                vDir = new Vector3(0f, 0, 2f)
            },
            _ => throw new IndexOutOfRangeException()
        };

        static float3 GetRhombusCorner(int _index, int _rhombusCount)
        {
            math.sincos(kmath.kPIMul2 / _rhombusCount * _index,out var sine,out var cosine);
            return new float3(sine, 0f, cosine);
        }
        
        public static Axis GetOctahedronRhombusAxis(int _index, int _rhombusCount)
        {
            return new Axis()
            {
                index = _index,
                origin = 0f,
                uDir = GetRhombusCorner(_index, _rhombusCount),
                vDir = GetRhombusCorner((_index + 1) % _rhombusCount, _rhombusCount),
            };
        }
    }
}