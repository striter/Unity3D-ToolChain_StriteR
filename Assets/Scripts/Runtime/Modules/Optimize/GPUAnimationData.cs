using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace Rendering.Optimize
{
    public enum EGPUAnimationMode
    {
        Vertex=1,
        Bone=2,
    }
    
    public class GPUAnimationData : ScriptableObject
    {
        public EGPUAnimationMode m_Mode;
        public Mesh m_BakedMesh;
        public Texture2D m_BakeTexture;
        public AnimationTickerClip[] m_AnimationClips;
        public GPUAnimationExposeBone[] m_ExposeBones;
    }

    [Serializable]
    public class GPUAnimationExposeBone
    {
        public string m_BoneName;
        public int m_BoneIndex;
        public Vector3 m_Position;
        public Vector3 m_Direction;
    }

}
