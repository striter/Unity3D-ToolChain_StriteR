using System;
using UnityEngine;
namespace Rendering.Optimize
{
    public enum EGPUAnimationMode
    {
        _ANIM_VERTEX=1,
        _ANIM_BONE=2,
    }
    
    public class GPUAnimationData : ScriptableObject
    {
        public EGPUAnimationMode m_Mode;
        public Mesh m_BakedMesh;
        public Texture2D m_BakeTexture;
        public AnimationTickerClip[] m_AnimationClips;
        public GPUAnimationExposeBone[] m_ExposeTransforms;
    }

    [Serializable]
    public struct GPUAnimationExposeBone
    {
        public string name;
        public int index;
        public Vector3 position;
        public Vector3 direction;
    }

}
