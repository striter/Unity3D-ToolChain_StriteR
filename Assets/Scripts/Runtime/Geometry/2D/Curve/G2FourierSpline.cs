using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    [Serializable]
    public struct G2FourierSpline : ISerializationCallbackReceiver
    {
        public List<float2> paths;
        [Clamp(2)] public int coefficients;
        [HideInInspector] public float2x2[] fourierCoefficients;

        public G2FourierSpline(List<float2> _paths, int _coefficients = 5)
        {
            paths = _paths;
            coefficients = _coefficients;
            fourierCoefficients = null;
            Ctor();
        }
        public G2FourierSpline(float2[] _paths,int _coefficients = 5) : this(new List<float2>(_paths),_coefficients) { }
        void Ctor()
        {
            int length = paths.Count;
            fourierCoefficients = new float2x2[coefficients];
            for (int c = 0; c < coefficients; c++)
            {
                float2x2 fc = 0;
                for (int i = 0; i < paths.Count; i++)
                {
                    float an = (-kmath.kPI2 * c * i) / length ;
                    math.sincos(an,out var san,out var can);
                    float2 ex = new float2(can, san);
                    fc.c0 += paths[i].x * ex;
                    fc.c1 += paths[i].y * ex;
                }
                fourierCoefficients[c] = fc;
            }
        }

        public float2 Evaluate(float _value)
        {
            float2 result = 0;
            for (int i = 0; i < coefficients; i++)
            {
                float w = (i == 0 || i == coefficients - 1) ? 1.0f : 2;
                float an = -kmath.kPI2 * i * _value;
                math.sincos(an,out var san,out var can);
                float2 ex = new float2(can, san);
                ref float2x2 fc = ref fourierCoefficients[i];
                
                result.x += w * math.dot(fc.c0,ex);
                result.y += w * math.dot(fc.c1,ex);
            }
            return result / paths.Count ;
        }

        public static G2FourierSpline kDefault = new G2FourierSpline(new float2[]{kfloat2.left,kfloat2.up,kfloat2.right,kfloat2.down});
        public static G2FourierSpline kBunny = new G2FourierSpline(G2Polygon.kBunny.positions, 20);

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize() =>  Ctor();
        public IEnumerable<float2> Coordinates => paths;
    }
    
}