using Unity.Mathematics;

public struct cfloat2
{
    public float x;
    public float i;
    public cfloat2(float _x, float _i)
    {
        x = _x;
        i = _i;
    }

    public static implicit operator cfloat2(float _value) => new(_value,0);
    public static implicit operator cfloat2(float2 _value) => new(_value.x,_value.y);
    public static implicit operator float2(cfloat2 _value) => new(_value.x,_value.i);

    public static cfloat2 operator *(cfloat2 _c1, cfloat2 _c2) => new(_c1.x * _c2.x - _c1.i * _c2.i, _c1.x * _c2.i + _c1.i * _c2.x );
    public static cfloat2 operator -(cfloat2 _c1, cfloat2 _c2) => new(_c1.x - _c2.x, _c1.i - _c2.i);
    public static cfloat2 operator +(cfloat2 _c1, cfloat2 _c2) => new(_c1.x + _c2.x, _c1.i + _c2.i);
    public static cfloat2 operator *(cfloat2 _c1, float _c2) => new(_c1.x * _c2, _c1.i * _c2);
    public static cfloat2 operator *(float _c1, cfloat2 _c2) => new(_c1 * _c2.x, _c1 * _c2.i);
    public static cfloat2 operator /(cfloat2 _c1, float _c2) => new(_c1.x / _c2, _c1.i / _c2);
    
    public static cfloat2 operator /(cfloat2 _complex1, cfloat2 _complex2)
    {
        var real1 = _complex1.x;
        var imaginary1 = _complex1.i;
        var real2 = _complex2.x;
        var imaginary2 = _complex2.i;
        if (math.abs(imaginary2) < math.abs(real2))
        {
            var num = imaginary2 / real2;
            return new cfloat2((real1 + imaginary1 * num) / (real2 + imaginary2 * num), (imaginary1 - real1 * num) / (real2 + imaginary2 * num));
        }
        var num1 = real2 / imaginary2;
        return new cfloat2((imaginary1 + real1 * num1) / (imaginary2 + real2 * num1), (-real1 + imaginary1 * num1) / (imaginary2 + real2 * num1));
    }

    public static cfloat2 exp(cfloat2 _value)
    {
        var num = math.exp(_value.x);
        return new cfloat2(num * math.cos(_value.i), num * math.sin(_value.i));
    }
    
    public static cfloat2 mul(cfloat2 _value, cfloat2 _point) => _value * _point;
    public static cfloat2 pow(cfloat2 _value, float _power)
    {
        if (_power == 0f)
            return 0f;
        if (_value.sum() == 0f)
            return 0f;
        var real1 = _value.x;
        var imaginary1 = _value.i;
        var num1 = abs(_value);
        var num2 = math.atan2(imaginary1, real1);
        var num3 = _power * num2;
        var num4 = math.pow(num1, _power) ;
        return new cfloat2(num4 * math.cos(num3), num4 * math.sin(num3));
    }
    
    public static float abs(cfloat2 _value)
    {
        if (float.IsInfinity(_value.x) || float.IsInfinity(_value.i))
            return float.PositiveInfinity;
        var num1 = math.abs(_value.x);
        var num2 = math.abs(_value.i);
        if (num1 > num2)
        {
            var num3 = num2 / num1;
            return num1 * math.sqrt(1.0f + num3 * num3);
        }
        if (num2 == 0.0)
            return num1;
        var num4 = num1 / num2;
        return num2 * math.sqrt(1.0f + num4 * num4);
    }
    
    public float sum() => x + i;
    public float sqrmagnitude() => x * x + i * i;
    public float magnitude() => math.sqrt(x * x + i * i);
    
    public static cfloat2 zero = new (0f, 0f);
    public static cfloat2 one = new (1f, 0f);
    public static cfloat2 iOne = new (0f, 1f);
    public static cfloat2 rOne = new (1f, 0f);
}

public struct cfloat2x3
{
    public cfloat2 c0, c1, c2;
    public cfloat2x3(cfloat2 _c0, cfloat2 _c1, cfloat2 _c2) => (c0, c1, c2) = (_c0, _c1, _c2);

    public static readonly cfloat2x3 zero = new(cfloat2.zero, cfloat2.zero, cfloat2.zero);

    public static cfloat2x3 operator +(cfloat2x3 _a, cfloat2x3 _b) =>
        new cfloat2x3(_a.c0 + _b.c0, _a.c1 + _b.c1, _a.c2 + _b.c2);
}

public struct cfloat2x2
{ 
    public cfloat2 c0, c1;

    public cfloat2x2(cfloat2 _c0, cfloat2 _c1) => (c0, c1) = (_c0, _c1);
    
    public static readonly cfloat2x2 zero = new(cfloat2.zero, cfloat2.zero);
    
    public static cfloat2x2 operator +(cfloat2x2 _a, cfloat2x2 _b) =>
        new cfloat2x2(_a.c0 + _b.c0, _a.c1 + _b.c1);
}