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
}