using UnityEngine;
#if BAKERY_INCLUDED
namespace UnityEditor.Extensions.ScriptableObjectBundle.Process.Lightmap.Bakery
{
    public class BakeryLightmapProcessor : EditorExecutableProcess , IEditorExecutableProcessContinuous
    {
        public ftRenderLightmap.RenderMode m_RenderMode;
        public ftRenderLightmap.RenderDirMode m_RenderDirMode;
        public ftRenderLightmap.LightProbeMode m_LightProbeMode;
        
        public bool Executing() => ftRenderLightmap.bakeInProgress;
        public float process => ftRenderLightmap.progressBarPercent;
        public override bool Execute()
        {
            if (ftRenderLightmap.instance == null)
                ftRenderLightmap.RenderLightmap();
            
            if (ftRenderLightmap.bakeInProgress)
                return false;

            ftRenderLightmap.instance.settingsMode = ftRenderLightmap.SettingsMode.Advanced;
            ftRenderLightmap.instance.userRenderMode = m_RenderMode;
            ftRenderLightmap.lightProbeMode = m_LightProbeMode;
            ftRenderLightmap.renderDirMode = m_RenderDirMode;
            ftRenderLightmap.useScenePath = true;
            ftRenderLightmap.instance.unloadScenesInDeferredMode = false;
            ftRenderLightmap.instance.RenderButton();
            return true;
        }

        public void Cancel()
        {
            End();
            ftRenderLightmap.DebugLogError("Cancelled");
        }

        public void End()
        {
            if (ftRenderLightmap.instance == null)
                return;
            
            ftRenderLightmap.instance.Close();
            ftRenderLightmap.instance = null;
        }

        public void OnGUI()
        {
        }
    }
}
#endif