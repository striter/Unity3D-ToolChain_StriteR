using System;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UString
{
    public static string CollectAllNumber(this string _src)
    {
        StringBuilder var = new StringBuilder();
        foreach (var VARIABLE in _src)
        {
            if (VARIABLE >= '0' && VARIABLE <= '9')
                var.Append(VARIABLE);
        }
        return var.ToString();
    }

    public static bool LastEquals(this string _src,string _dst)
    {
        int index = _src.LastIndexOf(_dst, StringComparison.Ordinal);
        if (index < 0)
            return false;
        return index + _dst.Length == _src.Length;
    }
}