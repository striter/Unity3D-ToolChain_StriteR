using System;
using Unity.Mathematics;
public static class ucomplex
{
    public static float2 mul(float2 _c1, float2 _c2)
    {
        return new float2(_c1.x * _c2.x - _c1.y * _c2.y, _c1.x * _c2.y + _c1.y * _c2.x );
    }
    
    public static float2 divide(float2 _complex1, float2 _complex2)
    {
        var real1 = _complex1.x;
        var imaginary1 = _complex1.y;
        var real2 = _complex2.x;
        var imaginary2 = _complex2.y;
        if (math.abs(imaginary2) < math.abs(real2))
        {
            var num = imaginary2 / real2;
            return new float2((real1 + imaginary1 * num) / (real2 + imaginary2 * num), (imaginary1 - real1 * num) / (real2 + imaginary2 * num));
        }
        var num1 = real2 / imaginary2;
        return new float2((imaginary1 + real1 * num1) / (imaginary2 + real2 * num1), (-real1 + imaginary1 * num1) / (imaginary2 + real2 * num1));
    }

    public static float2 pow(float2 _value, float _power)
    {
        if (_power == 0f)
            return 0f;
        if (_value.sum() == 0f)
            return 0f;
        var real1 = _value.x;
        var imaginary1 = _value.y;
        var num1 = abs(_value);
        var num2 = math.atan2(imaginary1, real1);
        var num3 = _power * num2;
        var num4 = math.pow(num1, _power) ;
        return new float2(num4 * math.cos(num3), num4 * math.sin(num3));
    }

    public static float abs(float2 _value)
    {
        if (float.IsInfinity(_value.x) || float.IsInfinity(_value.y))
            return float.PositiveInfinity;
        var num1 = Math.Abs(_value.x);
        var num2 = Math.Abs(_value.y);
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
}