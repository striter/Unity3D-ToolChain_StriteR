using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace Rendering.Optimize
{
    public enum EGPUAnimationMode
    {
        Vertex=1,
        Transform=2,
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
