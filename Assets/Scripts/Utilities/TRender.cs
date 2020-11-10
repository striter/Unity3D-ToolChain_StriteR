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
