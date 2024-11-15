using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.SignalProcessing;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    [Serializable]
    public struct G2Fourier : ISerializationCallbackReceiver
    {
        public List<float2> paths;
        [Clamp(2)] public int coefficients;
        [HideInInspector] public cfloat2x2[] fourierCoefficients;

        public G2Fourier(List<float2> _paths, int _coefficients = 5)
        {
            paths = _paths;
            coefficients = _coefficients;
            fourierCoefficients = null;
            Ctor();
        }
        public G2Fourier(float2[] _paths,int _coefficients = 5) : this(new List<float2>(_paths),_coefficients) { }
        void Ctor()
        {
            var fourierCoefficients = new cfloat2x2[coefficients];
            Fourier.DFT(paths.Select(p=>p.x),coefficients).Traversal((index, value) => fourierCoefficients[index].c0 = value);
            Fourier.DFT(paths.Select(p=>p.y),coefficients).Traversal((index, value) => fourierCoefficients[index].c1 = value);
            this.fourierCoefficients = fourierCoefficients;
        }


        public float3 Evaluate(float _value)
        {
            float3 result = 0;
            var N = paths.Count;
            result.x = Fourier.IDFT(fourierCoefficients.Select(p=>p.c0),N,_value);
            result.y = Fourier.IDFT(fourierCoefficients.Select(p=>p.c1),N,_value);
            return result;
        }

        public static G2Fourier kDefault = new G2Fourier(new float2[]{kfloat2.left,kfloat2.up,kfloat2.right,kfloat2.down});
        public static G2Fourier kBunny = new G2Fourier(G2Polygon.kBunny.positions, 20);

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize() =>  Ctor();
        public IEnumerable<float2> Coordinates => paths;
    }
    
}