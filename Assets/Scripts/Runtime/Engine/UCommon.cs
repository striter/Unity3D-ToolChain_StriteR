using UnityEngine;
public static class UCommon
{
    public static readonly RangeFloat kRange01 = new RangeFloat(0f, 1f);
    public static readonly RangeFloat kRangeNeg1Pos1 = new RangeFloat(-1f, 2f);
    public static bool InRange(this RangeFloat _value, float _check) => _value.start <= _check && _check <= _value.end;
    public static float NormalizedAmount(this RangeFloat _range, float _check) => UMath.InvLerp(_range.start, _range.end, _check);
}

