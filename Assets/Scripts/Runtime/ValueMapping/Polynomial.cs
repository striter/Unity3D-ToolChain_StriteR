
using Unity.Mathematics;
using static kmath;
using static Unity.Mathematics.math;
public static partial class umath
{
    public static float linearPolynomial(float _x, float _linear, float _constant) => _x*_linear + _constant;
    public static float quadraticPolynomial(float _x, float _quadratic, float _linear, float _constant) => pow2(_x)*_quadratic + _x*_linear + _constant;
    public static float cubicPolynomial(float _x, float _cubic, float _quadratic, float _linear, float _constant) => pow3(_x)*_cubic + pow2(_x)*_quadratic + _x*_linear + _constant;

    public static int linearPolynomialRoots(float _linear,float _constant,out float[] _roots){
        _roots = null;
        if (_linear == 0)
            return 0;
    
        _roots = new[] {-_constant/_linear};
        return 1;
    }
    
    public static int quadraticPolynomialRoots(float _quadratic, float _linear, float _constant,out float[] _roots)
    {
        if (_quadratic == 0) return linearPolynomialRoots(_linear, _constant,out _roots);
        
        _roots = null;
        var d = _linear * _linear - 4 * _quadratic * _constant;
        if (d < 0)
            return 0;

        var sqrtD = sqrt(d);
        var value = (-_linear + sqrtD) / (2 * _quadratic);
        
        if (d == 0)
        {
            _roots = new[] {value};
            return 1;
        }
        
        var value2 = (-_linear - sqrtD) / (2 * _quadratic);
        _roots = new[] {value,value2};
        return 2;
    }
    
    public static int cubicPolynomialRoots(float _cubic,float _quadratic, float _linear, float _constant, out float[] _roots)
    {
        if (_cubic == 0) return quadraticPolynomialRoots(_quadratic,_linear, _constant, out _roots);
        
        var t = _cubic;
        
        var a = _quadratic / t;
        var b = _linear / t;
        var c = _constant / t;

        var cMinus = a / 3; //roots = (x - a/3)
        
        var p = -kInv9 * pow2(a) + kInv3 * b;
        var q = (1f / 27) * pow3(a) - kInv6 * a * b +  kInv2 * c;
        var D = -(pow3(p) + pow2(q));

        var sqrtNegD = sqrt(-D);
        var r = math.pow(-q + sqrtNegD  , kInv3);
        var s = math.pow(abs(-q - sqrtNegD)  , kInv3);
        if (D == 0)
        {
            _roots = new[] { 2*r - cMinus, -r - cMinus};
            return 2;
        }

        if (D < 0)     //To be continued 
        {
            _roots = new[] { r+s - cMinus};
            return 1;
        }

        var omega = kInv3 * acos(-q / sqrt(-pow3(p)));
        var sqrtNegP = sqrt(-p);
        var x1 = 2 * sqrtNegP * cos(omega);
        var x2 = 2 * sqrtNegP * cos(omega + kPI2 / 3);
        var x3 = 2 * sqrtNegP * cos(omega - kPI2 / 3);
        _roots = new []{ x1 -cMinus,x2 - cMinus,x3 - cMinus};
        return 3;
    }
}