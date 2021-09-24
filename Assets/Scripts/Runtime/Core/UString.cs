using System;
public static class UString
{
    public static bool LastEquals(this string _src,string _dst)
    {
        int index = _src.LastIndexOf(_dst, StringComparison.Ordinal);
        if (index < 0)
            return false;

        return index + _dst.Length == _src.Length;
    }
}