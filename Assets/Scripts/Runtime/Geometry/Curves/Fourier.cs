using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Geometry.Curves
{
    //& https://iquilezles.org/articles/fourier
    [Serializable]
    public struct G2FourierCurve : ICurve<float2> ,ISerializationCallbackReceiver
    {
        public float2[] paths;
        [Clamp(1)] public int coefficients;
        [HideInInspector] public float2[] fourierCoefficientsX,fourierCoefficientsY;

        public G2FourierCurve(float2[] _paths,int _coefficients = 5)
        {
            paths = _paths;
            coefficients = _coefficients;
            fourierCoefficientsX = null;
            fourierCoefficientsY = null;

            Ctor();
        }
        
        void Ctor()
        {
            int length = paths.Length;
            fourierCoefficientsX = new float2[coefficients];
            fourierCoefficientsY = new float2[coefficients];
            for (int c = 0; c < coefficients; c++)
            {
                float2 fcX = 0;
                float2 fcY = 0;
                for (int i = 0; i < paths.Length; i++)
                {
                    float an = (-kmath.kPI2 * c * i) / length ;
                    math.sincos(an,out var san,out var can);
                    float2 ex = new float2(can, san);
                    fcX += paths[i].x * ex;
                    fcY += paths[i].y * ex;
                }
                fourierCoefficientsX[c] = fcX;
                fourierCoefficientsY[c] = fcY;
            }
        }

        public float2[] Coordinates => paths;

        public float2 Evaluate(float _value)
        {
            float2 result = 0;
            for (int i = 0; i < coefficients; i++)
            {
                float w = (i == 0 || i == coefficients - 1) ? 1.0f : 2;
                float an = -kmath.kPI2 * i * _value;
                math.sincos(an,out var san,out var can);
                float2 ex = new float2(can, san);
                result.x += w * math.dot(fourierCoefficientsX[i],ex);
                result.y += w * math.dot(fourierCoefficientsY[i],ex);
            }
            return result / paths.Length ;
        }

        public static G2FourierCurve kDefault = new G2FourierCurve(new float2[]{kfloat2.left,kfloat2.up,kfloat2.right,kfloat2.down});
        public static G2FourierCurve kBunny = new G2FourierCurve(new float2[]{
                new float2( 0.098f, 0.062f ), new float2( 0.352f, 0.073f ), new float2( 0.422f, 0.136f ), new float2( 0.371f, 0.085f ), new float2( 0.449f, 0.140f ),
                new float2( 0.352f, 0.187f ), new float2( 0.379f, 0.202f ), new float2( 0.398f, 0.202f ), new float2( 0.266f, 0.198f ), new float2( 0.318f, 0.345f ),
                new float2( 0.402f, 0.359f ), new float2( 0.361f, 0.425f ), new float2( 0.371f, 0.521f ), new float2( 0.410f, 0.491f ), new float2( 0.410f, 0.357f ),
                new float2( 0.502f, 0.482f ), new float2( 0.529f, 0.435f ), new float2( 0.426f, 0.343f ), new float2( 0.449f, 0.343f ), new float2( 0.504f, 0.335f ),
                new float2( 0.664f, 0.355f ), new float2( 0.748f, 0.208f ), new float2( 0.738f, 0.277f ), new float2( 0.787f, 0.308f ), new float2( 0.748f, 0.183f ),
                new float2( 0.623f, 0.081f ), new float2( 0.557f, 0.099f ), new float2( 0.648f, 0.116f ), new float2( 0.598f, 0.116f ), new float2( 0.566f, 0.195f ),
                new float2( 0.584f, 0.228f ), new float2( 0.508f, 0.083f ), new float2( 0.457f, 0.140f ), new float2( 0.508f, 0.130f ), new float2( 0.625f, 0.071f ),
                new float2( 0.818f, 0.093f ), new float2( 0.951f, 0.066f ), new float2( 0.547f, 0.081f )}, 20);

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize() =>  Ctor();
    }
    
    [Serializable]
    public struct GFourierCurve : ICurve<float3> ,ISerializationCallbackReceiver
    {
        public float3[] paths;
        [Clamp(1)] public int coefficients;
        [HideInInspector] public float2[] fourierCoefficientsX,fourierCoefficientsY,fourierCoefficientsZ;

        public GFourierCurve(float3[] _paths,int _coefficients = 5)
        {
            paths = _paths;
            coefficients = _coefficients;
            fourierCoefficientsX = null;
            fourierCoefficientsY = null;
            fourierCoefficientsZ = null;
            Ctor();
        }
        
        void Ctor()
        {
            int length = paths.Length;
            fourierCoefficientsX = new float2[coefficients];
            fourierCoefficientsY = new float2[coefficients];
            fourierCoefficientsZ = new float2[coefficients];
            for (int c = 0; c < coefficients; c++)
            {
                float2 fcX = 0;
                float2 fcY = 0;
                float2 fcZ = 0;
                for (int i = 0; i < paths.Length; i++)
                {
                    float an = (-kmath.kPI2 * c * i) / length ;
                    math.sincos(an,out var san,out var can);
                    float2 ex = new float2(can, san);
                    fcX += paths[i].x * ex;
                    fcY += paths[i].y * ex;
                    fcZ += paths[i].z * ex;
                }
                fourierCoefficientsX[c] = fcX;
                fourierCoefficientsY[c] = fcY;
                fourierCoefficientsZ[c] = fcZ;
            }
        }

        public float3[] Coordinates => paths;

        public float3 Evaluate(float _value)
        {
            float3 result = 0;
            for (int i = 0; i < coefficients; i++)
            {
                float w = (i == 0 || i == coefficients - 1) ? 1.0f : 2;
                float an = -kmath.kPI2 * i * _value;
                math.sincos(an,out var san,out var can);
                float2 ex = new float2(can, san);
                result.x += w * math.dot(fourierCoefficientsX[i],ex);
                result.y += w * math.dot(fourierCoefficientsY[i],ex);
                result.z += w * math.dot(fourierCoefficientsZ[i],ex);
            }
            return result / paths.Length ;
        }

        public static GFourierCurve kDefault = new GFourierCurve(G2FourierCurve.kDefault.Coordinates.Select(p=>p.to3xz()).ToArray() );
        public static GFourierCurve kBunny = new GFourierCurve(G2FourierCurve.kBunny.Coordinates.Select(p=>p.to3xz()).ToArray(), G2FourierCurve.kBunny.coefficients);

        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() =>  Ctor();
    }

}