using System;
using Unity.Mathematics;
using UnityEngine;

public enum EColorVisualize
{
    None,
    RGB,
    RGBA,
    R,
    G,
    B,
    A,
}
public static partial class UColor
{
    #region ColorTransform
    public static byte toColor32(float _src) => (byte)(_src * 255);
    public static float toColor(byte _color32) => _color32 / 255f;
    public static Color toColor(this Color32 _color32)=> new Color(_color32.r / 255f, _color32.g / 255f, _color32.b / 255f, _color32.a / 255f); 
    public static Color SetA(this Color _color, float _alpha) => new Color(_color.r, _color.g, _color.b, _alpha);
    //Vector
    public static Color toColor(Vector3 _colorVector) => new Color(_colorVector.x, _colorVector.y, _colorVector.z);
    public static float4 to4(this Color _color) => new float4(_color.r,_color.g,_color.b,_color.a);
    public static float3 to3(this Color _color) => new float3(_color.r,_color.g,_color.b);
    public static Vector4 toV4(this Color _color) => new Vector4(_color.r, _color.g, _color.b, _color.a);
    public static Color toColor(this Vector4 _vector)=>new Color(_vector.x,_vector.y,_vector.z,_vector.w);
    public static Color toColor(this float3 _color, float _a = 1) => new Color(_color.x, _color.y, _color.z, _a);
    public static Color toColor(this float4 _color)=> new Color(_color.x, _color.y, _color.z, _color.w);
    #endregion
    
    //RGBA32
    public static readonly Vector4 kRGBA32Max = Vector4.one * 255f;
    public static Vector4 ToRGBA32(this Color _color) => _color.toV4().mul(kRGBA32Max);
    public static Color RGB32toColor(Vector3 _rgb) => RGBA32toColor(_rgb.x, _rgb.y, _rgb.z);
    public static Color RGBA32toColor(float _r, float g, float b, float a = 255f) => RGBA32toColor(new Vector4(_r, g, b, a));
    public static Color RGBA32toColor(Vector4 _rgba) => _rgba.div(kRGBA32Max);

    //Hexademic
    public static readonly string kHEXMax = @"#FFFFFFFF";
    public static string ToHex(this Color _color)
    {
        Vector4 rgba = _color.ToRGBA32();
        return $"#{(short) rgba.x:X2}{(short) rgba.y:X2}{(short) rgba.z:X2}{(short) rgba.w:X2}";
    }
    public static Color HEXtoColor(string _hex)
    {
        try
        {
            int offset = _hex[0] == '#' ? 1 : 0;
            if (_hex.Length == offset + 6)
            {
                float br = byte.Parse(_hex.Substring(offset + 0, 2), System.Globalization.NumberStyles.HexNumber);
                float bg = byte.Parse(_hex.Substring(offset + 2, 2), System.Globalization.NumberStyles.HexNumber);
                float bb = byte.Parse(_hex.Substring(offset + 4, 2), System.Globalization.NumberStyles.HexNumber);
                return RGBA32toColor(br, bg, bb);
            }
            else if (_hex.Length == offset + 8)
            {
                float br = byte.Parse(_hex.Substring(offset + 0, 2), System.Globalization.NumberStyles.HexNumber);
                float bg = byte.Parse(_hex.Substring(offset + 2, 2), System.Globalization.NumberStyles.HexNumber);
                float bb = byte.Parse(_hex.Substring(offset + 4, 2), System.Globalization.NumberStyles.HexNumber);
                float ba = byte.Parse(_hex.Substring(offset + 6, 2), System.Globalization.NumberStyles.HexNumber);
                return RGBA32toColor(br, bg, bb, ba);
            }
            throw new System.Exception();
        }
        catch
        {
            Debug.Log("Invalid Hex Color:" + _hex);
            return Color.magenta;
        }
    }

    //HSVA
    public static readonly Vector4 kHSVMax = new Vector4(360f, 100f, 100f, 255f);
    const float kHueCellSize = 1f / 6f;
    public static Vector4 ToHSVA(this Color color) => ToHSVA_Normalized(color).mul(kHSVMax);
    public static Vector4 ToHSVA_Normalized(this Color color)
    {
        float cmax = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        float cmin = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        float offset = cmax - cmin;
        float h = 1;
        if (offset == 0)
        {
            h = 0f;
        }
        else
        {
            if (Math.Abs(cmax - color.r) < float.Epsilon)
                h = kHueCellSize * ((color.g>=color.b?0f:6f) + (color.g - color.b) / offset);
            else if (Math.Abs(cmax - color.g) < float.Epsilon)
                h = kHueCellSize * (2f + (color.b - color.r) / offset);
            else
                h = kHueCellSize * (4f + (color.r - color.g) / offset);
        }
        float s = cmax == 0 ? 0f : offset / cmax;
        float v = cmax;
        return new Vector4(h, s, v, color.a);
    }
    
    public static Color HSVAtoColor(Vector3 _hsv) => HSVAtoColor(_hsv.x, _hsv.y, _hsv.z);
    
    public static Color HSVAtoColor(float _h, float _s, float _v, float _a = 255f) => HSVAtoColor(new Vector4(_h, _s, _v, _a));
    
    public static Color HSVAtoColor(Vector4 _hsva)
    {
        Vector4 hsvaNormalized  = _hsva.div(kHSVMax);

        float h = hsvaNormalized.x;
        float s = hsvaNormalized.y;
        float v = hsvaNormalized.z;
        float a = hsvaNormalized.w;

        h /= kHueCellSize;
        float hIndex = Mathf.FloorToInt(h);
        float color =  h - hIndex;
        float p = v * (1f - s);
        float q = v * (1f - color * s);
        float t = v * (1f - (1f - color) * s);
        switch (hIndex%6f)
        {
            case 0: return new Color(v, t, p, a);
            case 1: return new Color(q, v, p, a);
            case 2: return new Color(p, v, t, a);
            case 3: return new Color(p, q, v, a);
            case 4: return new Color(t, p, v, a);
            case 5: return new Color(v, p, q, a);
        }
        Debug.LogError("Invalid HSV Color:" + _hsva);
        return Color.magenta;
    }

    public static Color FilterColor(this EColorVisualize _visualize,Color _color)
    {
        switch (_visualize)
        {
            default:throw new System.Exception("Invalid Visualize Color Type:"+_visualize);
            case EColorVisualize.None: return Color.clear;
            case EColorVisualize.RGBA: return _color; 
            case EColorVisualize.RGB: return _color.SetA(1);
            case EColorVisualize.R: return Color.red * _color.r;
            case EColorVisualize.G: return Color.green * _color.g; 
            case EColorVisualize.B: return Color.blue * _color.b;
            case EColorVisualize.A: return Color.white * _color.a;
        }
    }
    
    public static Color FilterGreyScale(this EColorVisualize _visualize, Color _color)
    {
        switch (_visualize)
        {
            default: throw new System.Exception("Invalid Visualize Color Type:" + _visualize);
            case EColorVisualize.None:return Color.clear;
            case EColorVisualize.RGBA: return _color;
            case EColorVisualize.RGB: return _color.SetA(1);
            case EColorVisualize.R: return Color.white * _color.r;
            case EColorVisualize.G: return Color.white * _color.g;
            case EColorVisualize.B: return Color.white * _color.b;
            case EColorVisualize.A: return Color.white * _color.a;
        }
    }

    public static Color Cos(Color _value)
    {
        return new Color(Mathf.Cos(_value.r), Mathf.Cos(_value.g), Mathf.Cos(_value.b), _value.a);//Mathf.Cos(_value.a));
    }
    
    public static Color IndexToColor(int _index) => (_index % 16) switch {
            0 => Color.red, 1 => Color.green, 2 => Color.blue, 3 => Color.yellow,
            4 => Color.cyan, 5 => Color.white, 6 => Color.magenta, 7 => KColor.kSandyBrown,
            8 => KColor.kPaleGreen, 9 => KColor.kPaleTurquoise, 10 => KColor.kChocolate, 11 => KColor.kCrimson,
            12 => KColor.kLime,13 => KColor.kOlive, 14 => KColor.kTeal, 15 => KColor.kTomato,
            _ => Color.grey
        };
}

[Serializable]
public struct ColorPalette
{
    public Color a,b,c,d;
    public static readonly ColorPalette kDefault = new ColorPalette()
        {a = new Color(.5f,.5f,.5f,1f), b = new Color(.5f,.5f,.5f,1f), c = new Color(1f,1f,1f,1f), d = new Color(0,0.1f,0.2f,1f)};
    
    public Color Evaluate(float _value) => (a + b * UColor.Cos(kmath.kPI2*(c*_value+d))).SetA(1f);
}
