using System;
using Unity.Mathematics;

public static class UNumericalAnalysis
{
    public static float NewtonsMethod(Func<float, float> _polynomial, Func<float, float> _derivative, float _startGuess, float _approximation = float.Epsilon,short _maxIteration = 1024)
    {
        float guess = _startGuess;
        float value = _polynomial(_startGuess);
        short iteration = 0;
        while (math.abs(value) > _approximation && iteration++<_maxIteration)
        {
            var derivative = _derivative(guess) ;
            guess -= value / derivative;
            value = _polynomial(guess);
        }
        return guess;
    }
}
