using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline.Mask
{
    public class MaskTextureFeature : ScriptableRendererFeature
    {
        [ScriptableObjectEdit]public MaskTextureData m_Data;
        private MaskTexturePass m_Mask;

        public override void Create()
        {
            m_Mask = new MaskTexturePass() { renderPassEvent = RenderPassEvent.AfterRenderingOpaques + 1 };
        }

        public override void AddRenderPasses(ScriptableRenderer _renderer, ref RenderingData renderingData)
        {
            _renderer.EnqueuePass(m_Mask.Setup(m_Data));
        }
    }
}