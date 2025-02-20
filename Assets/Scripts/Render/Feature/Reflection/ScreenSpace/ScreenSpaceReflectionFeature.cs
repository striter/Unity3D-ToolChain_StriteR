using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class ScreenSpaceReflectFeature : AScriptableRendererFeature
    {
        public ScreenSpaceReflectionData m_Data;
        private ScreenSpaceReflectionPass m_Pass ;
        public override void Create()
        {
            m_Pass = new(m_Data) { renderPassEvent = RenderPassEvent.AfterRenderingOpaques};
        }

        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            _renderer.EnqueuePass(m_Pass);
        }
    }
}