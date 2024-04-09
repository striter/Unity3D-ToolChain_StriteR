using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Process.Lightmap
{
    [CreateAssetMenu(menuName = "AssetPipeline/Lightmap/BakeryLightmapProcessor", fileName = "BakeryLightmapProcessor", order = 0)]
    public class BakeryLightmapProcessor : EAssetPipelineProcess 
    {
        public override bool Executing() => ftRenderLightmap.bakeInProgress;
        public override float process => ftRenderLightmap.progressBarPercent;
        public override void Begin()
        {
            if (ftRenderLightmap.instance == null)
                ftRenderLightmap.RenderLightmap();
            
            if (ftRenderLightmap.bakeInProgress)
                return;

            ftRenderLightmap.useScenePath = true;
            ftRenderLightmap.instance.unloadScenesInDeferredMode = false;
            ftRenderLightmap.instance.RenderButton();
        }

        public override void Cancel()
        {
            End();
            ftRenderLightmap.DebugLogError("Cancelled");
        }

        public override void End()
        {
            if (ftRenderLightmap.instance == null)
                return;
            
            ftRenderLightmap.instance.Close();
            ftRenderLightmap.instance = null;
        }

    }
}