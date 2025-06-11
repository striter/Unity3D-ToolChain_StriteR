using System;
using UnityEngine;

[Serializable]
public struct ColorPalette
{
    public Color a,b,c,d;
    
    public Color Evaluate(float _value) => (a + b * UColor.Cos(kmath.kPI2*(c*_value+d))).SetA(1f);
    
    public static ColorPalette[] kPresets = {
        new () { a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0, 0.33f, 0.67f)},   
        new () { a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0, 0.1f, 0.2f)},   
        new () { a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0.3f, 0.2f, 0.2f)},   
        new () { a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, .5f), d = new Color(0.8f, 0.9f, 0.3f)},  
        new () { a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, .7f, .4f), d = new Color(0, 0.15f, 0.2f)},   
        new () { a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(2f, 1f, 1f), d = new Color(0.5f, 0.2f, 0.25f)},   
        new () { a = new Color(.8f, .5f, .4f), b = new Color(.2f, .4f, .2f), c = new Color(2f, 1f, 1f), d = new Color(0, 0.25f, 0.25f)},
    };
    public static readonly ColorPalette kDefault = new()
        {a = new Color(.5f,.5f,.5f,1f), b = new Color(.5f,.5f,.5f,1f), c = new Color(1f,1f,1f,1f), d = new Color(0,0.1f,0.2f,1f)};
}