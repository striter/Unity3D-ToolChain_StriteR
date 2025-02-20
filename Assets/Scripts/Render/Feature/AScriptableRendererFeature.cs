using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public abstract class AScriptableRendererFeature : ScriptableRendererFeature
    {
        public sealed override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;
            EnqueuePass(renderer,ref renderingData);
        }

        protected abstract void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData);
    }
}