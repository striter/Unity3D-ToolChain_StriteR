using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ColorPalette
{
    [ColorUsage(false,true)] public Color baseColor;
    public float3 amplitude,frequency,phaseShift;
    public Color Evaluate(float _value) => baseColor + (amplitude * math.cos(kmath.kPI2*(frequency*_value+phaseShift))).toColor(1f);
    
    public static readonly ColorPalette kDefault = new()
        {baseColor = new Color(.5f,.5f,.5f,1f), amplitude = new (.5f,.5f,.5f), frequency = new (1f,1f,1f), phaseShift = new (0,0.1f,0.2f)};
}