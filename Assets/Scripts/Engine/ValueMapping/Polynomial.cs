using System;
using static umath;

public interface IPolynomial : IFunction
{
    public int GetRoots(out float[] _roots);
}

[Serializable]
public struct LinearPolynomial : IPolynomial
{
    public float linear, constant;
    
    public LinearPolynomial(float _linear, float _constant)
    {
        linear = _linear;
        constant = _constant;
    }
    
    public float Evaluate(float _x) => linearPolynomial(_x, linear, constant);
    public int GetRoots(out float[] _roots) => linearPolynomialRoots(linear, constant, out _roots);
    public override string ToString() => $"{linear.ToStringSigned()}x {constant.ToStringSigned()}";
    public static readonly LinearPolynomial kDefault = new LinearPolynomial(2, 1);
}

[Serializable]
public struct QuadraticPolynomial :IPolynomial
{
    public float quadratic, linear, constant;
    public QuadraticPolynomial(float _quadratic, float _linear, float _constant)
    {
        quadratic = _quadratic;
        linear = _linear;
        constant = _constant;
    }
    
    public float Evaluate(float _x) => quadraticPolynomial(_x, quadratic, linear, constant);
    public int GetRoots(out float[] _roots) => quadraticPolynomialRoots(quadratic, linear, constant, out _roots);
    public override string ToString() => $"{quadratic}x² {linear.ToStringSigned()}x {constant.ToStringSigned()}";
    public static readonly QuadraticPolynomial kDefault = new QuadraticPolynomial(3, 2, 1);
}

[Serializable]
public struct CubicPolynomial : IPolynomial
{
    public float cubic, quadratic, linear, constant;
    public CubicPolynomial(float _cubic, float _quadratic, float _linear, float _constant)
    {
        cubic = _cubic;
        quadratic = _quadratic;
        linear = _linear;
        constant = _constant;
    }
    
    public float Evaluate(float _x) => pow3(_x) * cubic + pow2(_x) * quadratic + _x * linear + constant;
    public int GetRoots(out float[] _roots) => cubicPolynomialRoots(cubic, quadratic, linear, constant, out _roots);
    public override string ToString() => $"{cubic}x³ {quadratic.ToStringSigned()}x² {linear.ToStringSigned()}x {constant.ToStringSigned()}";
    public static readonly CubicPolynomial kDefault = new CubicPolynomial(3, 2, 1, 0);
}