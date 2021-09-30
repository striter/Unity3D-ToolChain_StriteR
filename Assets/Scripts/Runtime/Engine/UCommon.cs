using UnityEngine;
public static class UCommon
{
    public static readonly RangeFloat m_Range01 = new RangeFloat(0f, 1f);
    public static readonly RangeFloat m_RangeNeg1Pos1 = new RangeFloat(-1f, 2f);
    public static bool InRange(this RangeFloat _value, float _check) => _value.start <= _check && _check <= _value.end;
    public static float InRangeScale(this RangeFloat _value, float _check) => Mathf.InverseLerp(_value.start, _value.end, _check);
}

