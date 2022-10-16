using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UInterpolate
{
    public float Lerp(float _a, float _b, float _t)=> (1.0f - _t) * _a + _b * _t;
    public float InvLerp(float _a, float _b, float _v)=>(_v - _a) / (_b - _a);
    public float Remap(float _a, float _b, float _v, float _ta, float _tb) => Lerp(_ta, _tb, InvLerp(_a,_b,_v));
}