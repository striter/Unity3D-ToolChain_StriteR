using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Examples.Rendering.Shadows.ScreenspaceShadow
{
    [Serializable]
    public struct ScreenSpaceShadowData
    {
        public static ScreenSpaceShadowData kDefault = new() {};
    }
    public class ContactShadowFeature : ScriptableRendererFeature
    {
        [DefaultAsset("Hidden/VolumeShadowCasterPasses")] public Shader m_Shader;
        public ScreenSpaceShadowData m_Data = ScreenSpaceShadowData.kDefault;
        private ContactShadowPass m_Pass = new();
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
            m_Material = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(!m_Material)
                return;

            renderer.EnqueuePass(m_Pass.Setup(m_Data,m_Material));
        }
    }
}