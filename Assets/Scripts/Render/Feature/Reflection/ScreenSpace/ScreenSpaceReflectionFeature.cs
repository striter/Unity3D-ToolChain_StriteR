using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class ScreenSpaceReflectionFeature : AScriptableRendererFeature
    {
        public ScreenSpaceReflectionData m_Data = ScreenSpaceReflectionData.kDefault;
        private ScreenSpaceReflectionPass m_Pass;
        public override void Create()
        {
            m_Pass = new(m_Data) { renderPassEvent = RenderPassEvent.BeforeRenderingSkybox + 1 };
        }

        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            _renderer.EnqueuePass(m_Pass);
        }
    }
}