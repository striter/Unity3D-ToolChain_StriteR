using System;
using Unity.Mathematics;
using UnityEngine;

public interface IFunction
{
    public float Evaluate(float _x);
}

//https://iquilezles.org/articles/functions/
public static class Function
{
    public enum EIdentity
    {
        AlmostIdentity,
        SmoothStepIntegral,
    }
    
    [Serializable]
    public struct Identity : IFunction
    {
        public EIdentity type;
        [Foldout(nameof(type),EIdentity.AlmostIdentity),Range(0,1)] public float n;
        [Foldout(nameof(type),EIdentity.SmoothStepIntegral),Min(0.01f)] public float T;
        public static readonly Identity kDefault = new() { n = 0f,T = 1f};
        
        public float Evaluate(float _x)
        {
            switch (type)
            {
                default: {
                    Debug.LogError("Invalid FunctionAlmostIdentity Type" + type);
                    return 0;
                }
                case EIdentity.AlmostIdentity: return umath.almostIdentity(_x,n);
                case EIdentity.SmoothStepIntegral: return umath.smoothStepIntegral(_x,T);
            }
        }
    }

    public enum EImpulse
    {
        ExpImpulse,
        QuadraticImpulse,
        PolynomialImpulse,
        ExpSustainedImpulse,
        SincImpulse,
    }

    [Serializable]
    public struct Impulse : IFunction
    {
        public EImpulse type;
        [Foldout(nameof(type), EImpulse.ExpImpulse,EImpulse.QuadraticImpulse,EImpulse.PolynomialImpulse),Range(0.01f,1f)] public float impulsePeak;
        [Foldout(nameof(type),EImpulse.PolynomialImpulse),Range(0,360f)] public float degree;
        [Foldout(nameof(type), EImpulse.ExpSustainedImpulse), Min(0.01f)] public float attack;
        [Foldout(nameof(type), EImpulse.ExpSustainedImpulse), Min(0.01f)] public float release;
        [Foldout(nameof(type), EImpulse.SincImpulse), Min(0.01f)] public float frequency;
        public float Evaluate(float _x)
        {
            return type switch
            {
                EImpulse.ExpImpulse => umath.expImpulse(_x, 1f / impulsePeak),
                EImpulse.QuadraticImpulse => umath.quadraticImpulse(_x, 1f / (impulsePeak * impulsePeak)),
                EImpulse.PolynomialImpulse => umath.polynomialImpulse(_x, 1f/(math.pow(impulsePeak , degree * kmath.kDeg2Rad) *(degree * kmath.kDeg2Rad - 1)), degree * kmath.kDeg2Rad),
                EImpulse.ExpSustainedImpulse => umath.expSustainedImpulse(_x,release,attack),
                EImpulse.SincImpulse => umath.sincImpulse(_x,frequency),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public static readonly Impulse kDefault = new() { impulsePeak = .3f, degree = 180f,frequency = 5,attack = 5f, release = 2f,};
    }

    public enum EUnitaryRemapping
    {
        AlmostUnitIdentity,
        Gain,
        Parabola,
        PowerCurve,
        Tonemap,
    }
    
    [Serializable]
    public struct UnitaryRemapping : IFunction
    {
        public EUnitaryRemapping type;
        [Fold(nameof(type),EUnitaryRemapping.PowerCurve), Range(-1f,2f)]public float k;
        [Foldout(nameof(type),EUnitaryRemapping.PowerCurve),Range(0f, 1f)] public float powercurveA;
        [Foldout(nameof(type),EUnitaryRemapping.PowerCurve),Range(0f, 1f)] public float powercurveB;
        public float Evaluate(float _x)
        {
            switch (type)
            {
                default: {
                    Debug.LogError("Invalid FunctionUnitaryRemapping Type" + type);
                    return 0;
                }
                case EUnitaryRemapping.AlmostUnitIdentity: return umath.almostUnitIdentity(_x);
                case EUnitaryRemapping.Gain: return umath.gain(_x,k);
                case EUnitaryRemapping.Parabola: return umath.parabola(_x,k);
                case EUnitaryRemapping.PowerCurve: return umath.powerCurve(_x,powercurveA,powercurveB);
                case EUnitaryRemapping.Tonemap: return umath.tonemap(_x,k);
            }
        }
        public static readonly UnitaryRemapping kDefault = new() {type = EUnitaryRemapping.AlmostUnitIdentity,k = 1f,powercurveA = .5f,powercurveB = .5f};
    }
}