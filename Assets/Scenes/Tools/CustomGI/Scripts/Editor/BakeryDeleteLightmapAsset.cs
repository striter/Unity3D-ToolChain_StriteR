#if BAKERY_INCLUDED
namespace UnityEditor.Extensions.EditorExecutable.Process.Lightmap.Bakery
{
    public class BakeryDeleteLightmapAsset : EditorExecutableProcess
    {
        public ftClearMenu.SceneClearingMode m_Mode = ftClearMenu.SceneClearingMode.lightmapReferences;
        public bool m_RemoveLightmapFiles = true;
        public override bool Execute()
        {
            ftClearMenu.ClearBakedData(m_Mode,m_RemoveLightmapFiles);
            return true;
        }
    }
}
#endif