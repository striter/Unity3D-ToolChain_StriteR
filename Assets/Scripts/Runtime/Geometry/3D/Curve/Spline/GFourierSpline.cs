using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.SignalProcessing;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    //https://iquilezles.org/articles/fourier
    [Serializable]
    public struct GFourierSpline : ISpline ,ISerializationCallbackReceiver
    {
        public float3[] paths;
        [Clamp(2,nameof(paths))] public int coefficient;
        [NonSerialized] private cfloat2[] frequencyX;
        [NonSerialized] private cfloat2[] frequencyY;
        [NonSerialized] private cfloat2[] frequencyZ;
        public GFourierSpline(float3[] _paths,int _coefficients = 2)
        {
            this = default;
            paths = _paths;
            coefficient = math.clamp(paths.Length * coefficient,2,paths.Length);
            Ctor();
        }
        
        void Ctor()
        {
            frequencyX = new cfloat2[coefficient];
            frequencyY = new cfloat2[coefficient];
            frequencyZ = new cfloat2[coefficient];
            UFourier.DiscreteFourier.Transform(paths.Select(p=>p.x),frequencyX);
            UFourier.DiscreteFourier.Transform(paths.Select(p=>p.y),frequencyY);
            UFourier.DiscreteFourier.Transform(paths.Select(p=>p.z),frequencyZ);
            for (var i = 0; i < coefficient; i++)
            {
                if (i == 0 || i == coefficient - 1)
                    continue;
                frequencyX[i] *= 2f;
                frequencyY[i] *= 2f;
                frequencyZ[i] *= 2f;
            }
        }

        public float3 Evaluate(float _value)
        {
            float3 result = 0;
            result.x = UFourier.DiscreteFourier.Inverse(frequencyX,_value).x;
            result.y = UFourier.DiscreteFourier.Inverse(frequencyY,_value).x;
            result.z = UFourier.DiscreteFourier.Inverse(frequencyZ,_value).x;
            return result;
        }

        public IEnumerable<float3> Coordinates => paths;
        public static GFourierSpline kDefault = new(G2Fourier.kDefault.paths.Select(p=>p.to3xz()).ToArray());
        public static GFourierSpline kBunny = new(G2Fourier.kBunny.paths.Select(p=>p.to3xz()).ToArray(), G2Fourier.kBunny.coefficient);

        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() =>  Ctor();
        public float3 Origin => paths[0];

        public void DrawGizmos()
        {
            Gizmos.color = Color.white.SetA(.1f);
            UGizmos.DrawLinesConcat(paths);
            this.DrawGizmos(paths.Length * 8);
        }
    }
}