using System;
using System.Numerics;
using Unity.Mathematics;

public static class UNumericalAnalysis
{
    public static float NewtonsMethod(Func<float, float> _polynomial, Func<float, float> _derivative, float _startGuess, float _approximation = float.Epsilon,short _maxIteration = 1024)
    {
        float guess = _startGuess;
        float value = _polynomial(guess);
        short iteration = 0;
        while (math.abs(value) > _approximation && iteration++<_maxIteration)
        {
            guess -= value / _derivative(guess);
            value = _polynomial(guess);
        }
        return guess;
    }

    public static float2 NewtonsFractal(Func<float2,float2> _polynomial,Func<float2,float2> _derivative,float2 _startGuess,float _sqrApproximation = float.Epsilon,int _maxIteration = 1024 )
    {
        var guess = _startGuess;
        var value = _polynomial(guess);
        short iteration = 0;
        while (value.sqrmagnitude() > _sqrApproximation && iteration++<_maxIteration)
        {
            guess -= umath.complexDivide(value ,_derivative(guess));
            value = _polynomial(guess);
        }
        return guess;
    }
}
