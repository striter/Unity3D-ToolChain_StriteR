using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Tools.Optimize.GPUSkinning
{
    [Serializable]
    public struct GPUSkinningBoneData
    {
        public Matrix4x4 bindPose;
        public string relativePath;
        public GSphere bounds;
    }
    
    public class GPUSkinningData : ScriptableObject
    {
        public Mesh m_Mesh;
        public List<GPUSkinningBoneData> m_Bones;

        public bool Valid() => m_Mesh != null
                               && m_Bones is { Count: > 0 };
    }
}