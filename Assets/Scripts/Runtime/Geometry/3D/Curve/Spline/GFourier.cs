using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.SignalProcessing;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    //& https://iquilezles.org/articles/fourier
    [Serializable]
    public struct GFourierSpline : ISpline ,ISerializationCallbackReceiver
    {
        public float3[] paths;
        [Clamp(2)] public int coefficients;
        [NonSerialized] private cfloat2x3[] fourierCoefficients;

        public GFourierSpline(float3[] _paths,int _coefficients = 5)
        {
            paths = _paths;
            coefficients = _coefficients;
            fourierCoefficients = null;
            Ctor();
        }
        
        void Ctor()
        {
            var fourierCoefficients = new cfloat2x3[coefficients];
            Fourier.DFT(paths.Select(p=>p.x),coefficients).Traversal((index, value) => fourierCoefficients[index].c0 = value);
            Fourier.DFT(paths.Select(p=>p.y),coefficients).Traversal((index, value) => fourierCoefficients[index].c1 = value);
            Fourier.DFT(paths.Select(p=>p.z),coefficients).Traversal((index, value) => fourierCoefficients[index].c2 = value);
            this.fourierCoefficients = fourierCoefficients;
        }


        public float3 Evaluate(float _value)
        {
            float3 result = 0;
            var N = paths.Length;
            result.x = Fourier.IDFT(fourierCoefficients.Select(p=>p.c0),N,_value);
            result.y = Fourier.IDFT(fourierCoefficients.Select(p=>p.c1),N,_value);
            result.z = Fourier.IDFT(fourierCoefficients.Select(p=>p.c2),N,_value);
            return result;
        }

        public IEnumerable<float3> Coordinates => paths;
        public static GFourierSpline kDefault = new(G2Fourier.kDefault.paths.Select(p=>p.to3xz()).ToArray());
        public static GFourierSpline kBunny = new(G2Fourier.kBunny.paths.Select(p=>p.to3xz()).ToArray(), G2Fourier.kBunny.coefficients);

        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() =>  Ctor();
        public float3 Origin => paths[0];

        public void DrawGizmos()
        {
            Gizmos.color = Color.white.SetA(.1f);
            UGizmos.DrawLinesConcat(paths);
            this.DrawGizmos(paths.Length * 4);
        }
    }
}