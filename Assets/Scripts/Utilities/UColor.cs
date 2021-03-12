using UnityEngine;
public static class UColor
{
    public static Color SetAlpha(this Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
    //Vector
    public static Color VectorToColor(Vector3 colorVector) => new Color(colorVector.x, colorVector.y, colorVector.z);
    public static Color VectorToColor(Vector4 colorVector) => new Color(colorVector.x, colorVector.y, colorVector.z, colorVector.w);
    public static Vector4 ToVector(this Color color) => new Vector4(color.r, color.g, color.b, color.a);
    public static Color ToColor(this Vector4 vector)=>new Color(vector.x,vector.y,vector.z,vector.w);
    //RGBA32
    public static readonly Vector4 m_RGBA32_MaxValue = Vector4.one * 255f;
    public static Vector4 ToRGBA32(this Color color) => color.ToVector().Multiply(m_RGBA32_MaxValue);
    public static Color RGB32toColor(Vector3 rgb) => RGBA32toColor(rgb.x, rgb.y, rgb.z);
    public static Color RGBA32toColor(float r, float g, float b, float a = 255f) => RGBA32toColor(new Vector4(r, g, b, a));
    public static Color RGBA32toColor(Vector4 rgba) => rgba.Divide(m_RGBA32_MaxValue);

    //Hexademic
    public static readonly string m_HEX_MaxValue = @"#FFFFFFFF";
    public static string ToHex(this Color color)
    {
        Vector4 rgba = color.ToRGBA32();
        return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", (short)rgba.x, (short)rgba.y, (short)rgba.z, (short)rgba.w);
    }
    public static Color HEXtoColor(string hex)
    {
        try
        {
            int offset = hex[0] == '#' ? 1 : 0;
            if (hex.Length == offset + 6)
            {
                float br = byte.Parse(hex.Substring(offset + 0, 2), System.Globalization.NumberStyles.HexNumber);
                float bg = byte.Parse(hex.Substring(offset + 2, 2), System.Globalization.NumberStyles.HexNumber);
                float bb = byte.Parse(hex.Substring(offset + 4, 2), System.Globalization.NumberStyles.HexNumber);
                return RGBA32toColor(br, bg, bb);
            }
            else if (hex.Length == offset + 8)
            {
                float br = byte.Parse(hex.Substring(offset + 0, 2), System.Globalization.NumberStyles.HexNumber);
                float bg = byte.Parse(hex.Substring(offset + 2, 2), System.Globalization.NumberStyles.HexNumber);
                float bb = byte.Parse(hex.Substring(offset + 4, 2), System.Globalization.NumberStyles.HexNumber);
                float ba = byte.Parse(hex.Substring(offset + 6, 2), System.Globalization.NumberStyles.HexNumber);
                return RGBA32toColor(br, bg, bb, ba);
            }
            throw new System.Exception();
        }
        catch
        {
            Debug.Log("Invalid Hex Color:" + hex);
            return Color.magenta;
        }
    }

    //HSVA
    public static readonly Vector4 m_HSV_MaxValue = new Vector4(360f, 100f, 100f, 255f);
    const float m_Hue_CellSize = 1f / 6f;
    public static Vector4 ToHSVA(this Color color) => ToHSVA_Normalized(color).Multiply(m_HSV_MaxValue);
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
            if (cmax == color.r)
                h = m_Hue_CellSize * ((color.g>=color.b?0f:6f) + (color.g - color.b) / offset);
            else if (cmax == color.g)
                h = m_Hue_CellSize * (2f + (color.b - color.r) / offset);
            else
                h = m_Hue_CellSize * (4f + (color.r - color.g) / offset);
        }
        float s = cmax == 0 ? 0f : offset / cmax;
        float v = cmax;
        return new Vector4(h, s, v, color.a);
    }
    public static Color HSVAtoColor(Vector3 hsv) => HSVAtoColor(hsv.x, hsv.y, hsv.z);
    public static Color HSVAtoColor(float h, float s, float v, float a = 255f) => HSVAtoColor(new Vector4(h, s, v, a));
    public static Color HSVAtoColor(Vector4 hsva)
    {
        Vector4 nhsva = hsva.Divide(m_HSV_MaxValue);

        float h = nhsva.x;
        float s = nhsva.y;
        float v = nhsva.z;
        float a = nhsva.w;

        h /= m_Hue_CellSize;
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
        Debug.LogError("Invalid HSV Color:" + hsva);
        return Color.magenta;
    }
}
