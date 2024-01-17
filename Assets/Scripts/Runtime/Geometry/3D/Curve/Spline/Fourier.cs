using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    //& https://iquilezles.org/articles/fourier
    [Serializable]
    public struct GFourierSpline : ISplineDimensions<float3> ,ISerializationCallbackReceiver
    {
        public float3[] paths;
        [Clamp(2)] public int coefficients;
        [HideInInspector] public float2x3[] fourierCoefficients;

        public GFourierSpline(float3[] _paths,int _coefficients = 5)
        {
            paths = _paths;
            coefficients = _coefficients;
            fourierCoefficients = null;
            Ctor();
        }
        
        void Ctor()
        {
            int length = paths.Length;
            fourierCoefficients = new float2x3[coefficients];
            for (int c = 0; c < coefficients; c++)
            {
                float2x3 fc = 0;
                for (int i = 0; i < paths.Length; i++)
                {
                    float an = (-kmath.kPI2 * c * i) / length ;
                    math.sincos(an,out var san,out var can);
                    float2 ex = new float2(can, san);
                    fc.c0 += paths[i].x * ex;
                    fc.c1 += paths[i].y * ex;
                    fc.c2 += paths[i].z * ex;
                }
                fourierCoefficients[c] = fc;
            }
        }

        public IEnumerable<float3> Coordinates => paths;

        public float3 Evaluate(float _value)
        {
            float3 result = 0;
            for (int i = 0; i < coefficients; i++)
            {
                float w = (i == 0 || i == coefficients - 1) ? 1.0f : 2;
                float an = -kmath.kPI2 * i * _value;
                math.sincos(an,out var san,out var can);
                float2 ex = new float2(can, san);
                result.x += w * math.dot(fourierCoefficients[i].c0,ex);
                result.y += w * math.dot(fourierCoefficients[i].c1,ex);
                result.z += w * math.dot(fourierCoefficients[i].c2,ex);
            }
            return result / paths.Length ;
        }

        public static GFourierSpline kDefault = new GFourierSpline(G2FourierSpline.kDefault.paths.Select(p=>p.to3xz()).ToArray() );
        public static GFourierSpline kBunny = new GFourierSpline(G2FourierSpline.kBunny.paths.Select(p=>p.to3xz()).ToArray(), G2FourierSpline.kBunny.coefficients);

        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() =>  Ctor();
    }
}