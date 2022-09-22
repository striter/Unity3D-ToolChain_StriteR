using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceEffectCollection : ScriptableObject
{
    [CullingMask] public int m_LightLayer;
    public EntityEffectClip[] m_AnimationClips;
}

[Serializable]
public class EntityEffectClip
{
    public Material material;
    public AnimationClip animation;
    public string name;
}

