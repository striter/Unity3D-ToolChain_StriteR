using System;
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
    public static void EnableKeywords(this Material _material, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            _material.EnableKeyword(_keywords[i], i+1 == _target);
    }
    public static void EnableGlobalKeyword(string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            EnableGlobalKeyword(_keywords[i],(i+1)==_target);
    }

    public static void EnableGlobalKeyword(string _keyword,bool _enable)
    {
        if (_enable)
            Shader.EnableKeyword(_keyword);
        else
            Shader.DisableKeyword(_keyword);
    }

    public static Material CreateMaterial(Type _type)
    {
        Shader _shader = Shader.Find("Hidden/" + _type.Name);

        if (_shader == null)
            throw new NullReferenceException("Invalid ImageEffect Shader Found:" + _type.Name);

        if (!_shader.isSupported)
            throw new NullReferenceException("Shader Not Supported:" + _type.Name);

        return new Material(_shader) { name = _type.Name, hideFlags = HideFlags.DontSave };
    }
}
public class MeshBoundsChecker
{
    Vector3 m_BoundsMin;
    Vector3 m_BoundsMax;
    public MeshBoundsChecker()
    {
        m_BoundsMin = Vector3.zero;
        m_BoundsMax = Vector3.zero;
    }
    public void CheckBounds(Vector3 vertex)
    {
        m_BoundsMin = Vector3.Min(m_BoundsMin, vertex);
        m_BoundsMax = Vector3.Max(m_BoundsMax, vertex);
    }
    public Bounds GetBounds() => new Bounds((m_BoundsMin + m_BoundsMax) / 2, m_BoundsMax - m_BoundsMin);
}
