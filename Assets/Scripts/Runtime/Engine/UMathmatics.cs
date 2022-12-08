using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public static  class UMathmatics
{
    public static float4 to4(this float2 _value, float _z = 0, float _w = 0) => new float4(_value.x,_value.y,_z,_w);
}
