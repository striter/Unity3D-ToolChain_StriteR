using UnityEngine;

public static class UColorChannel
{
    public static Color FilterColor(this EColorChannelFlags _visualize,Color _color)
    {
        switch (_visualize)
        {
            default:throw new System.Exception("Invalid Visualize Color Type:"+_visualize);
            case EColorChannelFlags.None: return Color.clear;
            case EColorChannelFlags.RGBA: return _color; 
            case EColorChannelFlags.RGB: return _color.SetA(1);
            case EColorChannelFlags.R: return Color.red * _color.r;
            case EColorChannelFlags.G: return Color.green * _color.g; 
            case EColorChannelFlags.B: return Color.blue * _color.b;
            case EColorChannelFlags.A: return Color.white * _color.a;
        }
    }

    public static Color FilterGreyScale(this EColorChannelFlags _visualize, Color _color)
    {
        switch (_visualize)
        {
            default: throw new System.Exception("Invalid Visualize Color Type:" + _visualize);
            case EColorChannelFlags.None:return Color.clear;
            case EColorChannelFlags.RGBA: return _color;
            case EColorChannelFlags.RGB: return _color.SetA(1);
            case EColorChannelFlags.R: return Color.white * _color.r;
            case EColorChannelFlags.G: return Color.white * _color.g;
            case EColorChannelFlags.B: return Color.white * _color.b;
            case EColorChannelFlags.A: return Color.white * _color.a;
        }
    }
}