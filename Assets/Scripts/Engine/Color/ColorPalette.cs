using System;
using UnityEngine;

[Serializable]
public struct ColorPalette
{
    public Color a,b,c,d;
    public static readonly ColorPalette kDefault = new()
        {a = new Color(.5f,.5f,.5f,1f), b = new Color(.5f,.5f,.5f,1f), c = new Color(1f,1f,1f,1f), d = new Color(0,0.1f,0.2f,1f)};
    
    public Color Evaluate(float _value) => (a + b * UColor.Cos(kmath.kPI2*(c*_value+d))).SetA(1f);
}