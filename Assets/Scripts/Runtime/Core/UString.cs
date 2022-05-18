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

    public static bool ReplaceLast(this string _src, string _match,string _replace,out string _replaced)
    {
        _replaced = null;
        int index = _src.LastIndexOf(_match, StringComparison.Ordinal);
        if (index < 0)
            return false;
        _replaced = _src.Remove(index, _match.Length);
        _replaced = _replaced.Insert(index,_replace);
        return true;
    }
}