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
        [Clamp(0,nameof(paths))] public int coefficient;
        [HideInInspector] public cfloat2[] frequencyX;
        [HideInInspector] public cfloat2[] frequencyY;
        public G2Fourier(List<float2> _paths, int _coefficients = 2)
        {
            this = default;
            paths = _paths;
            coefficient = math.clamp(2,paths.Count - 1, (int) (paths.Count * _coefficients));
            Ctor();
        }
        public G2Fourier(float2[] _paths,int _coefficients = 2) : this(new List<float2>(_paths),_coefficients) { }
        void Ctor()
        {
            frequencyX = new cfloat2[coefficient];
            frequencyY = new cfloat2[coefficient];
            UFourier.DiscreteFourier.Transform(paths.Select(p=>p.x),frequencyX);
            UFourier.DiscreteFourier.Transform(paths.Select(p=>p.y),frequencyY);
            for(var i =0 ; i < coefficient;i++)
            {
                if (i == 0 || i == coefficient - 1)
                    continue;
                frequencyX[i] *= 2f;
                frequencyY[i] *= 2f;
            }
        }

        public float3 Evaluate(float _value)
        {
            float3 result = 0;
            result.x = UFourier.DiscreteFourier.Inverse(frequencyX,_value).x;
            result.y = UFourier.DiscreteFourier.Inverse(frequencyY,_value).x;
            return result;
        }

        public static G2Fourier kDefault = new (new[]{kfloat2.left,kfloat2.up,kfloat2.right,kfloat2.down});
        public static G2Fourier kBunny = new (G2Polygon.kBunny.positions, G2Polygon.kBunny.positions.Count / 2);

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize() =>  Ctor();
    }
    
}