using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LightmapCollection:ScriptableObject
{
    public LightmapRenderer[] m_Renderers;

    public void Export(Transform _rootTransform)
    {
        var renderers = _rootTransform.GetComponentsInChildren<MeshRenderer>();
        m_Renderers = new LightmapRenderer[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            m_Renderers[i] = new LightmapRenderer() {index = renderers[i].lightmapIndex,scaleOffset = renderers[i].lightmapScaleOffset};
    }

    public void Apply(Transform _rootTransform)
    {
        var renderers = _rootTransform.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length != m_Renderers.Length)
            throw new Exception("Invalid Lightmap Renderer Length!");

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].lightmapIndex = m_Renderers[i].index;
            renderers[i].lightmapScaleOffset = m_Renderers[i].scaleOffset;
        }
    }
}

[Serializable]
public struct LightmapRenderer
{
    public int index;
    public Vector4 scaleOffset;
}