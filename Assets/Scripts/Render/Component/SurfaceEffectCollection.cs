using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rendering.Pipeline.Component
{
    public class SurfaceEffectCollection : ScriptableObject
    {
        [Readonly] public List<EntityEffectClip> m_AnimationClips;

        [Button]
        public void InsertAnimation(string _name,AnimationClip _clip,Material _material)
        {
            if (_clip == null)
                return;
            
            m_AnimationClips.Add(new EntityEffectClip() {
                material = _material,
                curves = UnityEditor.AnimationUtility.GetCurveBindings(_clip).Select(p=> new SurfaceEffectCurve()
                {
                    paths = p.propertyName.Split('.'),
                    curve = UnityEditor.AnimationUtility.GetEditorCurve(_clip, p),
                }).ToArray(),
                name = _name,
                length =  _clip.length,
                warpMode = _clip.wrapMode,
            });
        }
    }

    [Serializable]
    public struct SurfaceEffectCurve
    {
        public string[] paths;
        public AnimationCurve curve;
    }

    [Serializable]
    public struct EntityEffectClip 
    {
        public Material material;
        public string name;
        public float length;
        public WrapMode warpMode;
        public SurfaceEffectCurve[] curves;

    }
}

