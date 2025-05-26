using System;
using Unity.Mathematics;

[Serializable]
public struct RangeFloat
{
    public float start;
    public float length;
    public float end => start + length;
    public RangeFloat(float _start, float _length)
    {
        start = _start;
        length = _length;
    }

    public static readonly RangeFloat k01 = new (0f,1f);
    public float Clamp(float _value)=>math.clamp(_value,start,end);
    public bool Contains(float _check) => start <= _check && _check <= end;
    public float NormalizedAmount(float _check) => umath.invLerp(start, end, _check);
    public float Evaluate(float _normalized) => start + length * _normalized;
    public static RangeFloat Minmax(float _min, float _max) => new(_min, _max - _min);
    public static RangeFloat operator*(RangeFloat _src, float _scale) => new (_src.start * _scale, _src.length * _scale);
}

[Serializable]
public struct RangeInt
{
    public int start;
    public int length;
    public int end => start + length;
    public RangeInt(int _start, int _length)
    {
        start = _start;
        length = _length;
    }

    public static RangeInt operator +(RangeInt _src, RangeInt _dst) => new RangeInt(_src.start+_dst.start,_src.length+_dst.length);
    public int GetValue(float _normalized) => (int) (start + length * _normalized);
    public int GetValueContains(float _normalized) => (int) (start + length * _normalized) + (_normalized>=.99f?1:0);
    public bool Contains(int _check) => start <= _check && _check <= end;
}