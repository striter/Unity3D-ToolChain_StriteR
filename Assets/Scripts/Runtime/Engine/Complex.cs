using System;
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

    public static implicit operator cfloat2(float _value) => new cfloat2(_value,_value);
    public static implicit operator cfloat2(float2 _value) => new cfloat2(_value.x,_value.y);
    public static implicit operator float2(cfloat2 _value) => new float2(_value.x,_value.i);
    
    public static cfloat2 operator *(cfloat2 _c1, cfloat2 _c2) => new cfloat2(_c1.x * _c2.x - _c1.i * _c2.i, _c1.x * _c2.i + _c1.i * _c2.x );
    public static cfloat2 operator -(cfloat2 _c1, cfloat2 _c2) => new cfloat2(_c1.x - _c2.x, _c1.i - _c2.i);
    public static cfloat2 operator +(cfloat2 _c1, cfloat2 _c2) => new cfloat2(_c1.x + _c2.x, _c1.i + _c2.i);
    public static cfloat2 operator *(cfloat2 _c1, float _c2) => new cfloat2(_c1.x * _c2, _c1.i * _c2);
    public static cfloat2 operator *(float _c1, cfloat2 _c2) => new cfloat2(_c1 * _c2.x, _c1 * _c2.i);
    public static cfloat2 operator /(cfloat2 _c1, float _c2) => new cfloat2(_c1.x / _c2, _c1.i / _c2);
    

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
        var num1 = Math.Abs(_value.x);
        var num2 = Math.Abs(_value.i);
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
}
