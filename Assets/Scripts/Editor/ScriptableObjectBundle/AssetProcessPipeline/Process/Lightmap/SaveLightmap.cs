using Rendering.Lightmap;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle.Process.Lightmap
{
    public class SaveLightmap : EAssetPipelineProcess
    {
        public ELightmap m_LightmapToSave = ELightmap.Color | ELightmap.Directional | ELightmap.ShadowMask;
        [EditorPath] public string m_DestinationPath;
        public override void OnExecute()
        {
            var dstDirectory = UEPath.PathRegex(m_DestinationPath);
            if (!AssetDatabase.IsValidFolder(dstDirectory))
                AssetDatabase.CreateFolder(dstDirectory.GetPathDirectory(), dstDirectory.GetPathName());
            
            var lightmaps = LightmapSettings.lightmaps;
            foreach (var data in lightmaps)
            {
                var lightmapData = (LightmapTextures)data;
                foreach (var lightmapToSave in UEnum.GetEnums<ELightmap>())
                {
                    if (!m_LightmapToSave.IsFlagEnable(lightmapToSave))
                        continue;
                    
                    var lightmap = lightmapData[lightmapToSave];
                    if (lightmap == null)
                    {
                        Debug.LogError($"Lightmap:{lightmapToSave} is null");
                        continue;
                    }

                    var lightmapPath = AssetDatabase.GetAssetPath(lightmap);
                    AssetDatabase.CopyAsset(lightmapPath, dstDirectory + "/" + AssetDatabase.GetAssetPath(lightmap).GetFileName());
                }
            }
        }
    }
}
