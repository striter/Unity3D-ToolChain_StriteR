using System;
#if BAKERY_INCLUDED
namespace UnityEditor.Extensions.ScriptableObjectBundle.Process.Lightmap.Bakery
{
    public class BakeryLightmapEnvironment : EAssetPipelineProcess
    {
        [Flags]
        public enum EBakeryLightmapFlags
        {
            SkyLight = 1,
            Direct = 1 << 1,
            Point = 1 << 2,
        }

        public bool m_OpenDirectLight = true;
        public override void OnExecute()
        {
            
        }
    }
}
#endif