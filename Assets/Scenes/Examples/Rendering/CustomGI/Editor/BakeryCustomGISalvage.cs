using Examples.Rendering.GI.CustomGI;
using Rendering.Lightmap;
using UnityEngine;

#if BAKERY_INCLUDED
namespace UnityEditor.Extensions.ScriptableObjectBundle.Process.Lightmap.Bakery
{
    public class BakeryCustomGISalvage : EAssetPipelineProcess
    {
        protected void OnValidate() => BakeryCustomGIPreparation.Finish();
        public override bool Execute()
        {
            var gi = FindObjectOfType<CustomGI>();
            if (!gi)
            {
                Debug.LogError("Can't find CustomGI");                
                return false;
            }
            
            gi.m_Diffuse = GlobalIllumination_LightmapDiffuse.Export(gi.transform);
            EditorUtility.SetDirty(gi);
            BakeryCustomGIPreparation.Finish();
            return true;
        }
    }
}
#endif