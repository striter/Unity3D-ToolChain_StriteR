using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TRender
{
    public static void EnableKeyword(this Material _material, string _keyword, bool _enable)
    {
        if (_enable)
            _material.EnableKeyword(_keyword);
        else
            _material.DisableKeyword(_keyword);
    }
    public static void EnableKeyword(this Material _material, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
        {
            _material.EnableKeyword(_keywords[i], (i + 1) == _target);
        }
    }

}