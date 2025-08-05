using System;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UString
{
    public static string CollectAllNumber(this string _src)
    {
        var var = new StringBuilder();
        foreach (var character in _src)
        {
            if (character is >= '0' and <= '9')
                var.Append(character);
        }
        return var.ToString();
    }

    public static bool LastEquals(this string _src,string _dst)
    {
        var index = _src.LastIndexOf(_dst, StringComparison.Ordinal);
        if (index < 0)
            return false;
        return index + _dst.Length == _src.Length;
    }

    public static bool ReplaceLast(this string _src, string _match,string _replace,out string _replaced)
    {
        _replaced = null;
        var index = _src.LastIndexOf(_match, StringComparison.Ordinal);
        if (index < 0)
            return false;
        _replaced = _src.Remove(index, _match.Length);
        _replaced = _replaced.Insert(index,_replace);
        return true;
    }

    public static string ToStringSigned(this float _number)
    {
        if (_number > 0)
            return string.Format($"+{_number}");
        return _number.ToString(CultureInfo.InvariantCulture);
    }

    public static string Reverse(this string _src)
    {
        if (string.IsNullOrEmpty(_src))
            return _src;

        var subProblem = _src[1..];
        var subSolution = Reverse(subProblem);
        return subSolution + _src[0];
    }
}