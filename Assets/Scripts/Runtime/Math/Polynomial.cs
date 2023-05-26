using System;
using Unity.Mathematics;

public interface IPolynomial
{
    public float Evaluate(float _x);
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
    public float Evaluate(float _x)=>  _x*linear + constant;

    public int GetRoots(out float[] _roots)
    {
        _roots = null;
        if (linear == 0)
            return 0;
        
        _roots = new float[] {-constant/linear};
        return 1;
    }

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
    
    public float Evaluate(float _x)=> umath.pow2(_x)*quadratic + _x*linear + constant;
    public float Determinant => linear * linear - 4 * quadratic * constant;
    public int GetRoots(out float[] _roots)
    {
        _roots = null;
        var d = Determinant;
        if (d < 0)
            return 0;

        var sqrtD = math.sqrt(d);
        var value = (-linear + sqrtD) / (2 * quadratic);
        
        if (d == 0)
        {
            _roots = new float[] {value};
            return 1;
        }
        
        var value2 = (-linear - sqrtD) / (2 * quadratic);
        _roots = new float[] {value,value2};
        return 2;
    }

    public override string ToString() => $"{quadratic}x² {linear.ToStringSigned()}x {constant.ToStringSigned()}";

    
}

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
    
    public float Evaluate(float _x) => umath.pow3(_x) * cubic + umath.pow2(_x) * quadratic + _x * linear + constant;
    public int GetRoots(out float[] _roots)
    {
        _roots = null;
        return 0;
    }

    public override string ToString() => $"{cubic}x³ {quadratic.ToStringSigned()}x² {linear.ToStringSigned()}x {constant.ToStringSigned()}";
}
