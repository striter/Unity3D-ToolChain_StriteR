using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Examples.Rendering.Shadows.Volume
{
    [Serializable]
    public struct VolumeShadowData
    {
        [Range(0,1f)]public float bias;
        [Range(0,1f)]public float normal;
        public static VolumeShadowData kDefault = new() {};
    }
    public class VolumeShadowFeature : ScriptableRendererFeature
    {
        [DefaultAsset("Hidden/VolumeShadowCasterPasses")] public Shader m_Shader;
        public VolumeShadowData m_Data;
        private VolumeShadowPass m_Pass = new();
        private Material m_Material;
        public override void Create()
        {
            m_Material = m_Shader ? CoreUtils.CreateEngineMaterial(m_Shader) : null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(m_Material)
                CoreUtils.Destroy(m_Material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(!m_Material)
                return;

            renderer.EnqueuePass(m_Pass.Setup(m_Data,m_Material));
        }
    }
}