using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UMath
{
    public static float AngleToRadin(float angle) => Mathf.PI * angle / 180f;
    public static float RadinToAngle(float radin) => radin / Mathf.PI * 180f;
    public static int Power(int _src, int _pow)
    {
        if (_pow == 0) return 1;
        if (_pow == 1) return _src;
        int dst = _src;
        for (int i = 0; i < _pow - 1; i++)
            dst *= _src;
        return dst;
    }
}
