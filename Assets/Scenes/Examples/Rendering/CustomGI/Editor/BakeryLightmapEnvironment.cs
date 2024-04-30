using System;
using System.Linq.Extensions;
using UnityEngine;

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

        public EBakeryLightmapFlags m_Settings = EBakeryLightmapFlags.SkyLight | EBakeryLightmapFlags.Direct | EBakeryLightmapFlags.Point;
        public override void OnExecute()
        {
            GameObject.FindObjectsOfType<BakeryDirectLight>().Traversal(p=>p.enabled = m_Settings.IsFlagEnable(EBakeryLightmapFlags.Direct));
            GameObject.FindObjectsOfType<BakeryPointLight>().Traversal(p=>p.enabled = m_Settings.IsFlagEnable(EBakeryLightmapFlags.Point));
            GameObject.FindObjectsOfType<BakerySkyLight>().Traversal(p=>p.enabled = m_Settings.IsFlagEnable(EBakeryLightmapFlags.SkyLight));
        }
    }
}
#endif