using System;
using Rendering.GI.SphericalHarmonics;
using Rendering.Lightmap;
using UnityEngine;

namespace Examples.Rendering.GI.CustomGI
{
    [Serializable]
    public class GIData
    {
        public Cubemap cubemap;
        public float shIntensity = 1f;
        [HideInInspector] public SHL2Data shData = SHL2Data.kZero;
    }
    
    [ExecuteInEditMode]
    public class CustomGI : MonoBehaviour
    {
        public GIData[] kGiData;

        public float div = 5f;

        private bool isDirty;
        public SHL2ShaderProperties kSHProperties = new SHL2ShaderProperties();
        public GlobalIllumination_LightmapDiffuse m_Diffuse;
        private void OnValidate()
        {
            isDirty = true;
        }

        [Button]
        void ValidateSH()
        {
            foreach (var giData in kGiData)
            {
                if (giData.cubemap == null)
                    return;
                giData.shData = SphericalHarmonicsExport.ExportL2Cubemap(64, giData.cubemap, giData.shIntensity,ESHSampleMode.Fibonacci);
            }
        }
        private void Update()
        {
            if (isDirty)
            {
                isDirty = false;
                ValidateSH();
            }
            
            var (start,end,interp) = kGiData.Gradient(UTime.time / div);
            var sh = SHL2Data.Interpolate(start.shData, end.shData, interp);
            kSHProperties.ApplyGlobal(sh.Output());
        }

        [Button]
        public void Output()
        {
#if UNITY_EDITOR
            m_Diffuse = GlobalIllumination_LightmapDiffuse.Export(transform);
#endif
        }

        [Button]
        public void Apply()
        {
            m_Diffuse?.Apply(transform);
        }
    }
}
