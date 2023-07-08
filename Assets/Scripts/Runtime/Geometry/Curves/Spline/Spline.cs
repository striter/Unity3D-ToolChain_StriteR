using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Curves.Spline
{
    using static KSpline;
    static class KSpline
    {
        public const int kMinDegree = 2;
    }

    public enum EBSplineMode
    {
        OpenUniformClamped,
        OpenUniform,
        Uniform,
        NonUniform,
    }
    
    [Serializable]
    public struct GSpline:ISpline<float3>,ISerializationCallbackReceiver
    {
        public float3[] coordinates;
        [Clamp(kMinDegree)] public int k;

        public EBSplineMode mode;
        public float[] knotVectors;
        
        public static readonly GSpline kDefault = new GSpline() {
            coordinates = new float3[]{new float3(-1,0,-1),new float3(0,0,1),new float3(1,0,-1)},
            k = 3,
            mode = EBSplineMode.OpenUniformClamped,
        }.Ctor();

        public IEnumerable<float3> Coordinates => coordinates;
        public float3 Evaluate(float _value)
        {
            var n = coordinates.Length -1;
            var t = math.lerp(knotVectors[k-1] ,knotVectors[n+1] ,_value);
            float3 result = 0;
            for (int i = 1; i <= n + 1; i++)
            {
                var nik = Basis(i - 1 ,k,t,knotVectors);
                result += nik * coordinates[i - 1];
            }
            return result;
        }

        static float Divide(float _numerator,float _denominator)
        {
            if (math.abs(_denominator) < 0.01f)
                return 0;
            return _numerator / _denominator;
        }
        
        static float Basis(int _i,int _k,float _t,float[] _knots)
        {
            if (_k == 1)
                return (_knots[_i] <= _t && _t < _knots[_i + 1])?1:0;

            float coefficient1 = Divide(
                _t - _knots[_i],
                _knots[_i+_k - 1] - _knots[_i]);
            float coefficient2 = 
                Divide(_knots[_i+_k] - _t,
                _knots[_i+_k] - _knots[_i + 1]);
            
            var nextK = _k - 1;
            return coefficient1 * Basis(_i,nextK, _t,_knots) + coefficient2 * Basis(_i + 1,nextK, _t,_knots) ;
        }

        public GSpline Ctor()
        {
            var n = coordinates.Length - 1;
            k = math.min(n + 1, k);
            int knotVectorLength = coordinates.Length + k + 1;
            
            switch (mode)
            {
                default:
                {
                    if (knotVectors == null || knotVectors.Length != knotVectorLength)
                        knotVectors = new float[knotVectorLength];
                }
                    break;
                case EBSplineMode.Uniform:
                {
                    knotVectors = new float[knotVectorLength];
                    for (int i = 1; i <= knotVectors.Length; i++)
                        knotVectors[i - 1] = i;
                }
                    break;
                case EBSplineMode.OpenUniform:
                {
                    float constant = 0;
                    knotVectors = new float[knotVectorLength];
                    
                    for (int i = 1; i <= knotVectors.Length; i++)
                    {
                        float value = 0;
                        if (i < k)
                            value = constant;
                        else if (i <= n+2 )
                            value = constant++;
                        else
                            value = constant;
                        knotVectors[i - 1] = value;
                    }
                }
                    break;
                case EBSplineMode.OpenUniformClamped:
                {
                    float constant = 0;
                    knotVectors = new float[knotVectorLength];
                    
                    for (int i = 1; i <= knotVectors.Length; i++)
                    {
                        float value = 0;
                        if (i < k)
                            value = constant;
                        else if (i <= n + 1)
                            value = constant++;
                        else if (i <= n + k)
                            value = constant;
                        else
                            value = ++constant;
                        knotVectors[i - 1] = value;
                    }
                }
                    break;
            }
            return this;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() => Ctor();
    }

    [Serializable]
    public struct GBezierSplineUniform:ISpline<float3>
    {
        public float3[] coordinates;
        [Clamp(kMinDegree)]public int k;
        
        public static readonly GBezierSplineUniform kDefault = new GBezierSplineUniform() {
            coordinates = new float3[]{new float3(-1,0,-1),new float3(0,0,1),new float3(1,0,-1)},
            k = 3,
        };

        public IEnumerable<float3> Coordinates => coordinates;
        public float3 Evaluate(float _value)
        {
            var n = coordinates.Length -1;
            var t = math.lerp(k ,n + 2  ,_value);
            float3 result = 0;
            for (int i = 1; i <= n + 1; i++)
            {
                var nik = Basis(i ,k,t);
                result += nik * coordinates[i - 1];
            }
            return result;
        }

        static float Basis(int _i,int _k,float _t)
        {
            if (_k == 1)
                return (_i <= _t && _t < _i + 1) ? 1:0;

            var nextK = _k - 1;
            
            float coefficient1 = (_t - _i) / nextK;
            float coefficient2 = (_i +_k - _t) / nextK;
            
            return coefficient1 * Basis(_i,nextK, _t) + coefficient2 * Basis(_i + 1,nextK, _t) ;
        }
    }


}