using Runtime.Geometry;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    public class ImposterData : ScriptableObject
    {
        public bool m_Interpolate;
        [MFoldout(nameof(m_Interpolate),true),Range(0,1)] public float m_Parallax;

        [Readonly] public bool m_Instanced;
        [Readonly] public ImposterInput m_Input;
        [Readonly] public GSphere m_BoundingSphere;
        [Readonly] public Material m_Material;
        [Readonly] public Mesh m_Mesh;
    }
}