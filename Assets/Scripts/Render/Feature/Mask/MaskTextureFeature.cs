using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline.Mask
{
    public class MaskTextureFeature : ScriptableRendererFeature
    {
        public SRD_MaskData m_Data = SRD_MaskData.kDefault;
        private MaskTexturePass m_Mask;

        public override void Create()
        {
            m_Mask = new MaskTexturePass() { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques };
        }

        public override void AddRenderPasses(ScriptableRenderer _renderer, ref RenderingData renderingData)
        {
            _renderer.EnqueuePass(m_Mask.Setup(m_Data));
        }
    }
}